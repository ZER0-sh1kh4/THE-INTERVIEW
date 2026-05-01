using BuildingBlocks.Exceptions;
using InterviewService.Data;
using InterviewService.DTOs;
using InterviewService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Polly;

namespace InterviewService.Services
{
    /// <summary>
    /// Creates interview sessions, loads questions and calculates results.
    /// </summary>
    public class InterviewSvc : IInterviewSvc
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<InterviewSvc> _logger;

        public InterviewSvc(AppDbContext context, HttpClient httpClient, IConfiguration config, ILogger<InterviewSvc> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Creates a pending interview record for the user.
        /// </summary>
        public async Task<Interview> StartInterviewAsync(int userId, bool isPremium, StartInterviewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Domain))
            {
                throw new ValidationAppException("Domain is required.");
            }

            if (!isPremium)
            {
                var totalInterviews = await _context.Interviews.CountAsync(i => i.UserId == userId);
                if (totalInterviews >= 1)
                {
                    throw new ForbiddenAppException("Free users can create only 1 interview. Upgrade to premium to attempt more interviews.");
                }
            }

            var interview = new Interview
            {
                UserId = userId,
                Title = request.Title,
                Domain = request.Domain,
                Type = isPremium ? "Premium" : "Normal",
                Status = "Pending"
            };

            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();
            return interview;
        }

        /// <summary>
        /// Starts an interview and returns question numbers starting from 1 for client usage.
        /// Idempotent: if the interview is already InProgress, returns the existing questions.
        /// </summary>
        public async Task<object> BeginInterviewAsync(int userId, bool isPremium, int interviewId)
        {
            var interview = await _context.Interviews.FirstOrDefaultAsync(i => i.Id == interviewId && i.UserId == userId);
            if (interview == null) throw new NotFoundAppException("Interview not found.");

            // Already completed — don't allow re-start
            if (interview.Status == "Completed")
                throw new ValidationAppException("Interview already completed.");

            // Idempotent: if already InProgress, return the existing saved questions
            if (interview.Status == "InProgress")
            {
                var existingQuestions = await _context.Questions
                    .Where(q => q.InterviewId == interviewId)
                    .OrderBy(q => q.OrderIndex)
                    .Select(q => new QuestionResponseDto
                    {
                        Id = q.OrderIndex,
                        Text = q.Text,
                        QuestionType = q.QuestionType,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        OrderIndex = q.OrderIndex
                    })
                    .ToListAsync();

                return new
                {
                    InterviewId = interviewId,
                    Message = "Session resumed. Questions loaded from previous begin call.",
                    Questions = existingQuestions
                };
            }

            var previousQuestions = await GetPreviousUserQuestionTextsAsync(userId, interview.Domain);
            var newQuestions = await BuildInterviewQuestionSetAsync(interview.Domain, interviewId, previousQuestions);
            if (!newQuestions.Any()) throw new AppException(
                "Gemini could not generate interview questions right now. Please try again in a moment.",
                StatusCodes.Status503ServiceUnavailable);

            interview.Status = "InProgress";
            interview.StartedAt = DateTime.UtcNow;

            _context.Questions.AddRange(newQuestions);
            await _context.SaveChangesAsync();

            var responseQuestions = newQuestions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuestionResponseDto
                {
                    Id = q.OrderIndex,
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    OrderIndex = q.OrderIndex
                })
                .ToList();

            return new
            {
                InterviewId = interviewId,
                Message = "Use the question ids returned here when calling submit. These ids are interview question numbers and start from 1.",
                Questions = responseQuestions
            };
        }

        /// <summary>
        /// Pre-generates questions for a domain and saves them to the global JIT pool.
        /// Called by the frontend to reduce startup latency.
        /// </summary>
        public async Task<int> WarmUpCacheAsync(string domain, int targetCount = 3)
        {
            if (string.IsNullOrWhiteSpace(domain)) return 0;

            // Check if we already have enough questions in the global pool for this specific domain
            var existingCount = await _context.GlobalInterviewQuestions.CountAsync(q => q.Domain == domain);
            if (existingCount >= targetCount) return existingCount;

            try
            {
                var needed = targetCount - existingCount;
                // Generate a small batch of 3 questions specifically for the warm-up
                var aiQuestions = await GenerateAiQuestionsAsync(domain, 0, Math.Min(needed, 3));
                return existingCount + aiQuestions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Warm-up failed for domain {Domain}", domain);
                return existingCount;
            }
        }

        /// <summary>
        /// Builds the question set using validated admin questions first, then Gemini-generated questions.
        /// No static or local fallback questions are allowed.
        /// </summary>
        private async Task<List<Question>> BuildInterviewQuestionSetAsync(string domain, int interviewId, HashSet<string> previousQuestions)
        {
            var questionCount = ExtractQuestionCount(domain);
            var selectedQuestions = new List<Question>();

            // 1. Try Admin Curated Questions First
            var adminQuestions = await GetAdminCuratedQuestionsAsync(domain, interviewId, previousQuestions, questionCount);
            selectedQuestions.AddRange(adminQuestions);

            // 2. JIT Check: Try Global Pool for the remaining questions
            if (selectedQuestions.Count < questionCount)
            {
                try
                {
                    var neededFromPool = questionCount - selectedQuestions.Count;
                    
                    // IMPROVED: Try to match by partial domain (Role or Tech) if the specific domain has no questions
                    var role = domain.Split('|').FirstOrDefault()?.Trim();
                    
                    var poolQuestions = await _context.GlobalInterviewQuestions
                        .Where(q => q.Domain == domain || (role != null && q.Domain.Contains(role)))
                        .OrderBy(q => Guid.NewGuid()) // Random shuffle
                        .Take(neededFromPool * 3)
                        .ToListAsync();

                    var validPool = poolQuestions
                        .Where(q => !previousQuestions.Contains(NormalizeQuestionText(q.Text)))
                        .Take(neededFromPool)
                        .Select(q => new Question
                        {
                            InterviewId = interviewId,
                            Text = q.Text,
                            QuestionType = q.QuestionType,
                            CorrectAnswer = q.IdealAnswer,
                            Subtopic = q.Subtopic,
                            Source = "JIT_Pool"
                        })
                        .ToList();

                    selectedQuestions.AddRange(validPool);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "JIT pool query failed. Falling through to AI generation.");
                }
            }

            // 3. AI Generation: If still not enough, generate fresh questions
            if (selectedQuestions.Count < questionCount)
            {
                var neededFromAi = questionCount - selectedQuestions.Count;
                var aiQuestions = await GenerateAiQuestionsAsync(domain, interviewId, neededFromAi);
                selectedQuestions.AddRange(aiQuestions);
            }

            var finalQuestions = selectedQuestions
                .GroupBy(q => NormalizeQuestionText(q.Text))
                .Select(g => g.First())
                .Take(questionCount)
                .ToList();

            if (finalQuestions.Count < questionCount)
            {
                throw new AppException(
                    "Unable to generate enough unique, interviewer-quality questions. Please try again.",
                    StatusCodes.Status503ServiceUnavailable);
            }

            for (var index = 0; index < finalQuestions.Count; index++)
            {
                finalQuestions[index].InterviewId = interviewId;
                finalQuestions[index].OrderIndex = index + 1;
            }

            return finalQuestions;
        }

        /// <summary>
        /// Returns admin-curated interview questions when they pass the same quality and duplicate checks as AI questions.
        /// </summary>
        private async Task<List<Question>> GetAdminCuratedQuestionsAsync(string domain, int interviewId, HashSet<string> previousQuestions, int count)
        {
            var profileTerms = domain.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var curatedQuestions = await _context.Questions
                .Where(q => q.InterviewId == 0 && q.Source == "Admin")
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

            return curatedQuestions
                .Where(q => profileTerms.Length == 0 || profileTerms.Any(term => q.Subtopic.Contains(term, StringComparison.OrdinalIgnoreCase) || q.Text.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Where(q => IsHighQualityInterviewQuestion(q.Text))
                .Where(q => !previousQuestions.Contains(NormalizeQuestionText(q.Text)))
                .Take(count)
                .Select(q => new Question
                {
                    InterviewId = interviewId,
                    Text = q.Text,
                    QuestionType = "Subjective",
                    CorrectAnswer = q.CorrectAnswer,
                    Subtopic = q.Subtopic,
                    Source = "Admin"
                })
                .ToList();
        }

        /// <summary>
        /// Calls Gemini for interview-style questions using:
        /// - Dynamic seed sub-scenarios for variety (no exclude list needed)
        /// - Response schema in generationConfig for instant, perfectly formatted JSON
        /// - JIT batches of 3 questions (faster, avoids rate limits)
        /// - Polly retry policy with exponential backoff + 429 handling
        /// </summary>
        private async Task<List<Question>> GenerateAiQuestionsAsync(string domain, int interviewId, int questionCount)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var model = NormalizeGeminiModel(_config["Gemini:Model"] ?? "gemini-2.5-flash");

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_KEY")
            {
                throw new AppException("Gemini API key is not configured.", StatusCodes.Status503ServiceUnavailable);
            }

            var allQuestions = new List<Question>();
            var currentBatchSize = 3; // Small batches avoid gateway timeouts (504) and rate limits
            var scenarios = new[] { "System Architecture", "Debugging & Root-Cause Analysis", "Performance Optimization", 
                                    "Security & Threat Modeling", "Scalability & Load Handling", "Team Collaboration & Code Review",
                                    "CI/CD & DevOps", "Database Design", "API Design & Integration", "Error Handling & Resilience" };
            var rng = new Random();

            // Polly retry policy: handles 429 (rate limit) and transient failures
            var retryPolicy = Polly.Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == (System.Net.HttpStatusCode)429 || (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt - 1)),
                    onRetry: (outcome, delay, attempt, _) =>
                    {
                        _logger.LogWarning("Gemini retry #{Attempt} after {Delay}ms. Status: {Status}",
                            attempt, delay.TotalMilliseconds, outcome.Result?.StatusCode.ToString() ?? outcome.Exception?.Message);
                    });

            int loopAttempts = 0;
            const int maxLoopAttempts = 5; // Escape Hatch

            while (allQuestions.Count < questionCount && loopAttempts < maxLoopAttempts)
            {
                loopAttempts++;
                var needed = Math.Min(currentBatchSize, questionCount - allQuestions.Count);
                var scenario = scenarios[rng.Next(scenarios.Length)];

                try
                {
                    // Sync API Pattern: pass key in URL
                    var url = $"https://generativelanguage.googleapis.com/v1beta/{model}:generateContent?key={apiKey}";

                    var prompt = $"You are an Elite Technical Interviewer. Generate exactly {needed} concise scenario-based mock interview questions for: {domain}. " +
                                 $"Focus on this sub-scenario: {scenario}. " +
                                 "Rules: Each question MUST be 2-3 lines maximum (under 50 words). Be direct and specific. " +
                                 "Use format: 'How would you [action] when [specific situation]?' " +
                                 "NO long preambles. NO multi-part questions. NO theory-only questions. NO 'What is X'. One clear scenario per question.";

                    // Response schema in generationConfig — guarantees perfect JSON structure
                    var requestBody = new
                    {
                        contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } },
                        generationConfig = new
                        {
                            responseMimeType = "application/json",
                            temperature = 0.9,
                            responseSchema = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    questions = new
                                    {
                                        type = "ARRAY",
                                        items = new
                                        {
                                            type = "OBJECT",
                                            properties = new
                                            {
                                                question = new { type = "STRING" },
                                                subtopic = new { type = "STRING" },
                                                idealAnswer = new { type = "STRING" }
                                            },
                                            required = new[] { "question", "subtopic", "idealAnswer" }
                                        }
                                    }
                                },
                                required = new[] { "questions" }
                            }
                        }
                    };

                    // Execute with Polly retry policy
                    var response = await retryPolicy.ExecuteAsync(() =>
                    {
                        var req = new HttpRequestMessage(HttpMethod.Post, url);
                        // Header still added for redundancy, but URL key is the primary auth
                        req.Headers.Add("x-goog-api-key", apiKey);
                        req.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                        return _httpClient.SendAsync(req);
                    });

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Gemini batch failed with status {Status} after retries.", response.StatusCode);
                        currentBatchSize = 3; // Fallback to smaller batch
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    
                    // Safe JSON Extraction
                    if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    {
                        _logger.LogWarning("AI Safety Filter triggered or empty response.");
                        currentBatchSize = 3; // Fallback to smaller batch
                        continue; 
                    }

                    var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                    if (string.IsNullOrWhiteSpace(text)) 
                    {
                        currentBatchSize = 3;
                        continue;
                    }

                    var aiData = JsonSerializer.Deserialize<AiQuestionList>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (aiData?.Questions == null || !aiData.Questions.Any()) 
                    {
                        currentBatchSize = 3;
                        continue;
                    }

                    // Save to JIT pool (graceful if table doesn't exist yet)
                    try
                    {
                        var globalQuestions = aiData.Questions
                            .Where(q => !string.IsNullOrWhiteSpace(q.Question))
                            .Select(q => new GlobalInterviewQuestion
                            {
                                Domain = domain,
                                Text = q.Question ?? "",
                                Subtopic = q.Subtopic ?? "General",
                                IdealAnswer = q.IdealAnswer ?? "",
                                QuestionType = "Subjective",
                                Difficulty = "Medium",
                                Source = "Gemini_JIT",
                                CreatedAt = DateTime.UtcNow
                            }).ToList();

                        _context.GlobalInterviewQuestions.AddRange(globalQuestions);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception poolEx)
                    {
                        _logger.LogWarning(poolEx, "Failed to save batch to JIT pool. Questions still returned.");
                    }

                    var batchQuestions = aiData.Questions
                        .Where(q => !string.IsNullOrWhiteSpace(q.Question))
                        .Select(q => new Question
                        {
                            InterviewId = interviewId,
                            Text = q.Question ?? "",
                            QuestionType = "Subjective",
                            CorrectAnswer = q.IdealAnswer ?? "",
                            Subtopic = q.Subtopic ?? "General",
                            Source = "Gemini_AI"
                        }).ToList();

                    allQuestions.AddRange(batchQuestions);
                    _logger.LogInformation("Gemini batch returned {Count} questions (scenario: {Scenario}). Total: {Total}/{Target}",
                        batchQuestions.Count, scenario, allQuestions.Count, questionCount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gemini batch generation failed for scenario '{Scenario}'.", scenario);
                    currentBatchSize = 3; // Fallback to smaller batch
                }
            }

            if (allQuestions.Count == 0)
            {
                throw new AppException("Gemini failed to generate questions after all batch attempts. Please try again.", StatusCodes.Status503ServiceUnavailable);
            }

            return allQuestions.Take(questionCount).ToList();
        }

        private class AiQuestionList { public List<AiQuestionItem> Questions { get; set; } = new(); }
        private class AiQuestionItem { public string Question { get; set; } = ""; public string Subtopic { get; set; } = ""; public string IdealAnswer { get; set; } = ""; }


        /// <summary>
        /// Saves user answers and uses Gemini-backed rubric grading for interview-style questions.
        /// </summary>
        public async Task<object?> SubmitInterviewAsync(int userId, bool isPremium, SubmitInterviewRequest request)
        {
            var interview = await _context.Interviews.FindAsync(request.InterviewId);
            if (interview == null || interview.UserId != userId) return null;
            if (interview.Status != "InProgress") throw new ValidationAppException("Interview is not in progress.");

            var questions = await _context.Questions
                .Where(q => q.InterviewId == request.InterviewId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

            if (!questions.Any())
            {
                throw new NotFoundAppException("No questions found for this interview.");
            }

            var validQuestionNumbers = questions.Select(q => q.OrderIndex).ToHashSet();
            if (request.Answers.Any(a => !validQuestionNumbers.Contains(a.QuestionId)))
            {
                throw new ValidationAppException("Submit only the question ids returned by the begin endpoint.");
            }

            var existingAnswers = await _context.InterviewAnswers.Where(a => a.InterviewId == request.InterviewId).ToListAsync();
            if (existingAnswers.Any())
            {
                _context.InterviewAnswers.RemoveRange(existingAnswers);
            }

            var submittedAnswers = request.Answers
                .GroupBy(a => a.QuestionId)
                .Select(g => g.First())
                .ToList();

            InterviewEvaluationResult evaluations;
            try
            {
                evaluations = await EvaluateInterviewAnswersAsync(questions, submittedAnswers);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini evaluation failed (likely 429). Falling back to default evaluation to save user answers.");
                evaluations = new InterviewEvaluationResult
                {
                    OverallFeedback = "The AI evaluation service is currently experiencing high traffic. Your answers have been securely saved, but detailed scoring could not be generated at this exact moment.",
                    QuestionEvaluations = questions.Select(q => new InterviewQuestionEvaluation
                    {
                        QuestionId = q.OrderIndex,
                        Score = 0,
                        IsStrong = false,
                        IdealAnswer = q.CorrectAnswer ?? "AI evaluation unavailable.",
                        FollowUpQuestion = ""
                    }).ToList()
                };
            }
            
            var evaluationByQuestion = evaluations.QuestionEvaluations.ToDictionary(x => x.QuestionId, x => x);
            var totalScore = 0;
            var maxScore = Math.Max(questions.Count * 10, 10);

            foreach (var question in questions)
            {
                var submitted = submittedAnswers.FirstOrDefault(a => a.QuestionId == question.OrderIndex);
                var answerText = submitted?.AnswerText ?? string.Empty;
                var evaluation = evaluationByQuestion.TryGetValue(question.OrderIndex, out var foundEvaluation)
                    ? foundEvaluation
                    : new InterviewQuestionEvaluation
                    {
                        QuestionId = question.OrderIndex,
                        Score = 0,
                        IdealAnswer = question.CorrectAnswer ?? "No model answer was available.",
                        IsStrong = false
                    };

                var interviewAnswer = new InterviewAnswer
                {
                    InterviewId = request.InterviewId,
                    QuestionId = question.Id,
                    UserId = userId,
                    AnswerText = answerText,
                    SubmittedAt = DateTime.UtcNow
                };

                interviewAnswer.Score = Math.Clamp(evaluation.Score, 0, 10);
                interviewAnswer.IsCorrect = evaluation.IsStrong;
                question.CorrectAnswer = string.IsNullOrWhiteSpace(evaluation.IdealAnswer) ? question.CorrectAnswer : evaluation.IdealAnswer;
                totalScore += interviewAnswer.Score;

                _context.InterviewAnswers.Add(interviewAnswer);
            }

            interview.Status = "Completed";
            interview.CompletedAt = DateTime.UtcNow;

            var percentage = maxScore > 0 ? ((double)totalScore / maxScore) * 100 : 0;
            var grade = percentage >= 90 ? "A+" : percentage >= 80 ? "A" : percentage >= 70 ? "B" : percentage >= 60 ? "C" : percentage >= 50 ? "D" : "F";
            var followUpPlan = string.Join(" ", evaluations.QuestionEvaluations
                .OrderBy(e => e.QuestionId)
                .Where(e => !string.IsNullOrWhiteSpace(e.FollowUpQuestion))
                .Select(e => $"Follow-up Q{e.QuestionId}: {e.FollowUpQuestion}"));

            var existingResult = await _context.InterviewResults.FirstOrDefaultAsync(r => r.InterviewId == request.InterviewId && r.UserId == userId);
            if (existingResult != null)
            {
                _context.InterviewResults.Remove(existingResult);
            }

            var result = new InterviewResult
            {
                InterviewId = interview.Id,
                UserId = userId,
                TotalScore = totalScore,
                MaxScore = maxScore,
                Percentage = percentage,
                Grade = grade,
                Feedback = string.IsNullOrWhiteSpace(followUpPlan)
                    ? evaluations.OverallFeedback
                    : $"{evaluations.OverallFeedback} {followUpPlan}".Trim(),
                IsPremiumResult = isPremium
            };

            _context.InterviewResults.Add(result);
            await _context.SaveChangesAsync();

            return await GetResultAsync(userId, isPremium, request.InterviewId);
        }

        /// <summary>
        /// Returns all interviews created by the user.
        /// </summary>
        public async Task<IEnumerable<Interview>> GetMyInterviewsAsync(int userId)
        {
            return await _context.Interviews.Where(i => i.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Returns one interview with its saved questions.
        /// </summary>
        public async Task<object?> GetInterviewByIdAsync(int userId, int interviewId)
        {
            var interview = await _context.Interviews.FirstOrDefaultAsync(i => i.Id == interviewId && i.UserId == userId);
            if (interview == null) return null;

            var questions = await _context.Questions.Where(q => q.InterviewId == interviewId)
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuestionResponseDto
                {
                    Id = q.OrderIndex,
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    OrderIndex = q.OrderIndex
                }).ToListAsync();

            return new { Interview = interview, Questions = questions };
        }

        /// <summary>
        /// Returns result summary and premium-only review details.
        /// </summary>
        public async Task<object?> GetResultAsync(int userId, bool isPremium, int interviewId)
        {
            var result = await _context.InterviewResults.FirstOrDefaultAsync(r => r.InterviewId == interviewId && r.UserId == userId);
            if (result == null) return null;

            var answers = await _context.InterviewAnswers.Where(a => a.InterviewId == interviewId).ToListAsync();
            var questionIds = answers.Select(a => a.QuestionId).ToList();
            var questions = await _context.Questions.Where(q => questionIds.Contains(q.Id)).ToListAsync();
            var questionNumberMap = questions.ToDictionary(q => q.Id, q => q.OrderIndex);
            var wrongAnswers = answers
                .Where(a => a.IsCorrect == false)
                .Select(a => questionNumberMap.TryGetValue(a.QuestionId, out var questionNumber) ? questionNumber : a.QuestionId)
                .ToList();

            if (!isPremium)
            {
                return new
                {
                    result.TotalScore,
                    result.MaxScore,
                    result.Percentage,
                    result.Grade,
                    WrongQuestionIds = wrongAnswers,
                    result.Feedback
                };
            }

            var breakdown = answers.Select(a =>
            {
                var q = questions.First(x => x.Id == a.QuestionId);
                return new
                {
                    q.Text,
                    q.Subtopic,
                    YourAnswer = a.AnswerText,
                    q.CorrectAnswer,
                    a.IsCorrect,
                    a.Score
                };
            }).ToList();

            var weakAreas = breakdown.Where(a => a.IsCorrect == false).Select(a => a.Subtopic).Distinct().ToList();
            var strengths = breakdown.Where(a => a.Score >= 8).Select(a => a.Subtopic).Distinct().ToList();
            var suggestions = weakAreas.Select(area => $"Practice more concise, example-driven answers for {area}.").Distinct().ToList();

            return new
            {
                result.TotalScore,
                result.MaxScore,
                result.Percentage,
                result.Grade,
                WrongQuestionIds = wrongAnswers,
                Breakdown = breakdown,
                Strengths = strengths,
                WeakAreas = weakAreas,
                Suggestions = suggestions,
                result.Feedback
            };
        }

        /// <summary>
        /// Returns all interviews for admin usage.
        /// </summary>
        public async Task<IEnumerable<Interview>> GetAllInterviewsAsync()
        {
            return await _context.Interviews.ToListAsync();
        }

        private static int ExtractQuestionCount(string domain)
        {
            var match = Regex.Match(domain, @"(\d+)\s+Questions", RegexOptions.IgnoreCase);
            return match.Success && int.TryParse(match.Groups[1].Value, out var count)
                ? Math.Clamp(count, 3, 12)
                : 5;
        }

        private static string NormalizeGeminiModel(string model)
        {
            var trimmed = model.Trim();
            return trimmed.StartsWith("models/", StringComparison.OrdinalIgnoreCase) ? trimmed : $"models/{trimmed}";
        }

        private async Task<HashSet<string>> GetPreviousUserQuestionTextsAsync(int userId, string domain)
        {
            var priorInterviewIds = await _context.Interviews
                .Where(i => i.UserId == userId && i.Domain == domain)
                .Select(i => i.Id)
                .ToListAsync();

            var priorTexts = await _context.Questions
                .Where(q => priorInterviewIds.Contains(q.InterviewId))
                .Select(q => q.Text)
                .ToListAsync();

            return priorTexts
                .Select(NormalizeQuestionText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToHashSet();
        }

        private static bool IsHighQualityInterviewQuestion(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.Trim();
            var wordCount = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount < 5 || trimmed.Length < 25)
            {
                return false;
            }

            return true;
        }

        private static string NormalizeQuestionText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return Regex.Replace(text.Trim().ToLowerInvariant(), @"\s+", " ");
        }

        private async Task<InterviewEvaluationResult> EvaluateInterviewAnswersAsync(
            List<Question> questions,
            List<InterviewAnswerSubmission> submittedAnswers)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var model = NormalizeGeminiModel(_config["Gemini:Model"] ?? "gemini-2.5-flash");
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_KEY")
            {
                throw new AppException(
                    "Gemini is not configured on the interview service.",
                    StatusCodes.Status503ServiceUnavailable);
            }

            const int batchSize = 4;
            var allEvaluations = new List<InterviewQuestionEvaluation>();
            var overallFeedbackParts = new List<string>();

            // Split questions into batches of 4 to avoid gateway timeouts
            var batches = questions
                .Select((q, i) => new { Question = q, Index = i })
                .GroupBy(x => x.Index / batchSize)
                .Select(g => g.Select(x => x.Question).ToList())
                .ToList();

            _logger.LogInformation("Evaluating {Total} questions in {BatchCount} batches of up to {BatchSize}.",
                questions.Count, batches.Count, batchSize);

            foreach (var batch in batches)
            {
                try
                {
                    var batchResult = await EvaluateBatchAsync(batch, submittedAnswers, apiKey, model);
                    if (batchResult != null)
                    {
                        allEvaluations.AddRange(batchResult.QuestionEvaluations);
                        if (!string.IsNullOrWhiteSpace(batchResult.OverallFeedback))
                            overallFeedbackParts.Add(batchResult.OverallFeedback);
                    }
                    else
                    {
                        // Batch returned null — add zero-score fallback entries
                        allEvaluations.AddRange(batch.Select(q => new InterviewQuestionEvaluation
                        {
                            QuestionId = q.OrderIndex, Score = 0, IsStrong = false,
                            IdealAnswer = q.CorrectAnswer ?? "Evaluation unavailable.",
                            FollowUpQuestion = ""
                        }));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Batch evaluation failed for questions {Ids}.",
                        string.Join(", ", batch.Select(q => q.OrderIndex)));
                    allEvaluations.AddRange(batch.Select(q => new InterviewQuestionEvaluation
                    {
                        QuestionId = q.OrderIndex, Score = 0, IsStrong = false,
                        IdealAnswer = q.CorrectAnswer ?? "Evaluation unavailable.",
                        FollowUpQuestion = ""
                    }));
                }
            }

            return new InterviewEvaluationResult
            {
                OverallFeedback = overallFeedbackParts.Any()
                    ? string.Join(" ", overallFeedbackParts)
                    : "Evaluation completed.",
                QuestionEvaluations = allEvaluations
            };
        }

        /// <summary>
        /// Evaluates a single batch of questions against Gemini with retry logic.
        /// </summary>
        private async Task<InterviewEvaluationResult?> EvaluateBatchAsync(
            List<Question> batchQuestions,
            List<InterviewAnswerSubmission> submittedAnswers,
            string apiKey, string model)
        {
            var evaluationPayload = batchQuestions.Select(question =>
            {
                var answer = submittedAnswers.FirstOrDefault(a => a.QuestionId == question.OrderIndex)?.AnswerText ?? string.Empty;
                return new { questionId = question.OrderIndex, question = question.Text, subtopic = question.Subtopic, idealAnswer = question.CorrectAnswer, answer };
            });

            var prompt = $$"""
You are evaluating a mock interview. Score each answer out of 10.
Return ONLY valid JSON using this shape:
{
  "overallFeedback": "string",
  "questionEvaluations": [
    { "questionId": 1, "score": 0, "isStrong": false, "idealAnswer": "string", "followUpQuestion": "string" }
  ]
}

Scoring parameters (use these exact criteria):
- Clarity (0-2): Clear, specific, well-structured?
- Technical Accuracy (0-3): Correct technical knowledge?
- Practical Depth (0-3): Real-world reasoning, trade-offs, examples?
- Completeness (0-2): Fully addressed the scenario?

Rules:
- Sum the 4 scores for total (max 10).
- overallFeedback: 1-2 sentences max for this batch.
- idealAnswer: concise 2-3 sentence model answer.
- followUpQuestion: one short question (under 20 words).
- questionId values must match provided ids exactly.
- Empty answer = score 0, say "No answer provided."

Interview data:
{{JsonSerializer.Serialize(evaluationPayload)}}
""";

            var url = $"https://generativelanguage.googleapis.com/v1beta/{model}:generateContent";
            var requestBody = new
            {
                contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } }
            };
            var jsonRequest = JsonSerializer.Serialize(requestBody);

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                if (attempt > 1) await Task.Delay(TimeSpan.FromSeconds(2 * attempt));

                try
                {
                    using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    using var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.Add("x-goog-api-key", apiKey);
                    request.Content = content;

                    using var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseString);
                    var textResponse = document.RootElement.GetProperty("candidates")[0]
                        .GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                    if (string.IsNullOrWhiteSpace(textResponse)) continue;

                    textResponse = textResponse.Replace("```json", "").Replace("```", "").Trim();
                    var parsed = JsonSerializer.Deserialize<InterviewEvaluationResult>(textResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (parsed?.QuestionEvaluations?.Any() == true)
                    {
                        _logger.LogInformation("Batch scored {Count} questions.", parsed.QuestionEvaluations.Count);
                        return parsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gemini batch eval attempt {Attempt} failed.", attempt);
                }
            }

            return null;
        }

        private sealed class InterviewEvaluationResult
        {
            public string OverallFeedback { get; set; } = string.Empty;
            public List<InterviewQuestionEvaluation> QuestionEvaluations { get; set; } = new();
        }

        private sealed class InterviewQuestionEvaluation
        {
            public int QuestionId { get; set; }
            public int Score { get; set; }
            public bool IsStrong { get; set; }
            public string IdealAnswer { get; set; } = string.Empty;
            public string FollowUpQuestion { get; set; } = string.Empty;
        }
    }
}
