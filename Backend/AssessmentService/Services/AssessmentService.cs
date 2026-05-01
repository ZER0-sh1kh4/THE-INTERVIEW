using BuildingBlocks.Exceptions;
using AssessmentService.Data;
using AssessmentService.DTOs;
using AssessmentService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Retry;

namespace AssessmentService.Services
{
    /// <summary>
    /// Handles assessment creation, evaluation and reporting.
    /// </summary>
    public class AssessmentSvc : IAssessmentService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<AssessmentSvc> _logger;

        public AssessmentSvc(AppDbContext context, HttpClient httpClient, IConfiguration config, ILogger<AssessmentSvc> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Starts a new assessment and returns the first batch of questions.
        /// Remaining questions are generated lazily via /next-batch endpoint.
        /// </summary>
            public async Task<StartAssessmentResponse?> StartAssessmentAsync(int userId, bool isPremium, StartAssessmentRequest request)
        {
            var domain = request.Domain.Trim();
            var requestedCount = Math.Clamp(request.QuestionCount <= 0 ? 10 : request.QuestionCount, 1, 60);
            var difficulty = string.IsNullOrWhiteSpace(request.Difficulty) ? "Medium" : request.Difficulty.Trim();

            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ValidationAppException("Domain is required.");
            }

            if (!isPremium)
            {
                var totalAttempts = await _context.Assessments.CountAsync(a => a.UserId == userId);
                _logger.LogInformation("Free user {UserId} has {TotalAttempts} total assessments (limit: 2)", userId, totalAttempts);
                if (totalAttempts >= 2) throw new ForbiddenAppException("Free users can create only 2 assessment tests. Upgrade to premium for unlimited access.");
            }

            // Fetch all cached domain questions from DB (JIT cache pool)
            var allDbQuestions = await _context.MCQQuestions
                .Where(q => q.Domain == domain)
                .ToListAsync();

            // Filter only valid ones
            allDbQuestions = allDbQuestions.Where(q => IsHighQualityAssessmentQuestion(q)).ToList();

            // Shuffle for variety — minor repetition is allowed
            var rnd = new Random();
            allDbQuestions = allDbQuestions.OrderBy(x => rnd.Next()).ToList();

            var firstBatchSize = Math.Min(3, requestedCount);
            var finalQuestions = new List<MCQQuestion>();

            // 1. Try to fill from cached DB questions first
            finalQuestions.AddRange(allDbQuestions.Take(requestedCount));

            // 2. If not enough, generate ONLY the first batch via AI (fast — just 5 questions)
            if (finalQuestions.Count < firstBatchSize)
            {
                var needed = Math.Max(firstBatchSize - finalQuestions.Count, 5);
                try
                {
                    var aiQuestions = await GenerateAiAssessmentQuestionsAsync(domain, difficulty, needed);

                    // Save ALL to DB for JIT caching
                    if (aiQuestions.Any())
                    {
                        var nextOrder = await _context.MCQQuestions.Where(q => q.Domain == domain).CountAsync() + 1;
                        foreach (var q in aiQuestions)
                        {
                            q.OrderIndex = nextOrder++;
                        }
                        _context.MCQQuestions.AddRange(aiQuestions);
                        await _context.SaveChangesAsync();
                    }

                    finalQuestions.AddRange(aiQuestions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI generation failed for first batch.");
                }
            }

            // 3. Final safety
            if (finalQuestions.Count == 0)
            {
                throw new AppException(
                    "Unable to fetch or generate any questions for the assessment. Please try again in a moment.",
                    StatusCodes.Status503ServiceUnavailable);
            }

            // Take only what we need, shuffle
            var actualCount = Math.Min(finalQuestions.Count, requestedCount);
            finalQuestions = finalQuestions.Take(actualCount).OrderBy(x => rnd.Next()).ToList();

            var previousAttempts = await _context.Assessments.CountAsync(a => a.UserId == userId && a.Domain == domain);

            var assessment = new Assessment
            {
                UserId = userId,
                Domain = domain,
                Status = "InProgress",
                AttemptNumber = previousAttempts + 1,
                TimeLimitMinutes = Math.Max(10, (int)Math.Ceiling(requestedCount * 1.5)),
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Math.Max(10, (int)Math.Ceiling(requestedCount * 1.5)))
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            var questionDtos = finalQuestions
                .Select((q, index) => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    OrderIndex = index + 1
                }).ToList();

            // If we don't have enough yet, flag that the frontend should request more
            var totalAvailable = finalQuestions.Count;
            var totalRequested = requestedCount;

            return new StartAssessmentResponse
            {
                AssessmentId = assessment.Id,
                TimeLimitMinutes = assessment.TimeLimitMinutes,
                ExpiresAt = assessment.ExpiresAt,
                Questions = questionDtos,
                TotalExpected = totalRequested,
                HasMore = totalAvailable < totalRequested
            };
        }

        /// <summary>
        /// Returns the next batch of questions for a lazy-loaded assessment.
        /// Called by the frontend while the user is answering earlier questions.
        /// </summary>
        public async Task<List<QuestionDto>> GetNextBatchAsync(int userId, int assessmentId, int currentCount, int batchSize = 5)
        {
            var assessment = await _context.Assessments.FindAsync(assessmentId);
            if (assessment == null || assessment.UserId != userId || assessment.Status != "InProgress")
                return new List<QuestionDto>();

            var domain = assessment.Domain;
            var difficulty = "Medium";

            // Pull from DB cache first
            var existingQuestions = await _context.MCQQuestions
                .Where(q => q.Domain == domain)
                .ToListAsync();

            existingQuestions = existingQuestions
                .Where(q => IsHighQualityAssessmentQuestion(q))
                .ToList();

            var rnd = new Random();
            existingQuestions = existingQuestions.OrderBy(x => rnd.Next()).ToList();

            // Skip questions the user already has
            var available = existingQuestions.Skip(currentCount).Take(batchSize).ToList();

            // If not enough in DB, generate more
            if (available.Count < batchSize)
            {
                try
                {
                    var aiQuestions = await GenerateAiAssessmentQuestionsAsync(domain, difficulty, batchSize);
                    if (aiQuestions.Any())
                    {
                        var nextOrder = await _context.MCQQuestions.Where(q => q.Domain == domain).CountAsync() + 1;
                        foreach (var q in aiQuestions)
                        {
                            q.OrderIndex = nextOrder++;
                        }
                        _context.MCQQuestions.AddRange(aiQuestions);
                        await _context.SaveChangesAsync();
                    }
                    available.AddRange(aiQuestions.Take(batchSize - available.Count));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate next batch of questions.");
                }
            }

            return available.Select((q, index) => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                OrderIndex = currentCount + index + 1
            }).ToList();
        }

        /// <summary>
        /// Evaluates a submitted assessment and returns the saved result.
        /// </summary>
        public async Task<object?> SubmitAssessmentAsync(int userId, bool isPremium, SubmitAssessmentRequest request)
        {
            if (request.AssessmentId <= 0)
            {
                throw new ValidationAppException("A valid assessment id is required.");
            }

            if (request.Answers == null || !request.Answers.Any())
            {
                throw new ValidationAppException("At least one answer must be submitted.");
            }

            var assessment = await _context.Assessments.FindAsync(request.AssessmentId);
            if (assessment == null)
            {
                throw new NotFoundAppException("Assessment not found.");
            }

            if (assessment.UserId != userId)
            {
                throw new ForbiddenAppException("You are not allowed to submit this assessment.");
            }

            if (assessment.Status != "InProgress") throw new ValidationAppException("Assessment is already completed or expired.");

            if (DateTime.UtcNow > assessment.ExpiresAt)
            {
                assessment.Status = "Expired";
                await _context.SaveChangesAsync();
                throw new ValidationAppException("Time expired.");
            }

            var submittedQuestionIds = request.Answers.Select(a => a.QuestionId).Distinct().ToList();
            var mcqQuestions = await _context.MCQQuestions.Where(q => submittedQuestionIds.Contains(q.Id)).ToListAsync();
            var validQuestionIds = mcqQuestions.Select(q => q.Id).ToHashSet();
            if (request.Answers.Any(a => !validQuestionIds.Contains(a.QuestionId)))
            {
                throw new ValidationAppException("One or more submitted question ids are invalid for this assessment.");
            }

            var existingAnswers = await _context.UserAnswers.Where(a => a.AssessmentId == assessment.Id).ToListAsync();
            if (existingAnswers.Any())
            {
                _context.UserAnswers.RemoveRange(existingAnswers);
            }

            var distinctAnswers = request.Answers.GroupBy(a => a.QuestionId).Select(g => g.First()).ToList();
            var score = 0;
            
            // Assume 1 mark per question to calculate total max score. If the assessment was generated
            // with 5 questions, the max score should be 5 regardless of how many the user answered.
            var maxScore = request.TotalExpected ?? distinctAnswers.Count;
            
            var userAnswers = new List<UserAnswer>();

            foreach (var ans in distinctAnswers)
            {
                var q = mcqQuestions.First(x => x.Id == ans.QuestionId);
                var isCorrect = string.Equals(q.CorrectOption, ans.SelectedOption, StringComparison.OrdinalIgnoreCase);
                if (isCorrect)
                {
                    score += q.Marks;
                }

                userAnswers.Add(new UserAnswer
                {
                    AssessmentId = assessment.Id,
                    QuestionId = ans.QuestionId,
                    UserId = userId,
                    SelectedOption = ans.SelectedOption,
                    IsCorrect = isCorrect
                });
            }

            _context.UserAnswers.AddRange(userAnswers);
            assessment.Status = "Completed";

            var percentage = maxScore > 0 ? ((double)score / maxScore) * 100 : 0;
            var grade = percentage >= 90 ? "A+" : percentage >= 80 ? "A" : percentage >= 70 ? "B" : percentage >= 60 ? "C" : percentage >= 50 ? "D" : "F";

            var existingResult = await _context.AssessmentResults.FirstOrDefaultAsync(r => r.AssessmentId == assessment.Id && r.UserId == userId);
            if (existingResult != null)
            {
                _context.AssessmentResults.Remove(existingResult);
            }

            var result = new AssessmentResult
            {
                AssessmentId = assessment.Id,
                UserId = userId,
                Domain = assessment.Domain,
                Score = score,
                MaxScore = maxScore,
                Percentage = percentage,
                Grade = grade,
                IsPremiumResult = isPremium
            };

            _context.AssessmentResults.Add(result);
            await _context.SaveChangesAsync();

            return await GetAssessmentResultAsync(userId, assessment.Id, isPremium);
        }

        /// <summary>
        /// Returns all results for the current user.
        /// </summary>
        public async Task<IEnumerable<AssessmentResult>> GetUserAssessmentsAsync(int userId)
        {
            return await _context.AssessmentResults.Where(r => r.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Returns the result summary, with extra review for premium users.
        /// </summary>
        public async Task<object?> GetAssessmentResultAsync(int userId, int assessmentId, bool isPremium)
        {
            if (assessmentId <= 0)
            {
                throw new ValidationAppException("A valid assessment id is required.");
            }

            var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
            if (assessment == null)
            {
                throw new NotFoundAppException("Assessment not found.");
            }

            if (assessment.UserId != userId)
            {
                throw new ForbiddenAppException("You are not allowed to view this assessment result.");
            }

            var result = await _context.AssessmentResults.FirstOrDefaultAsync(r => r.AssessmentId == assessmentId && r.UserId == userId);
            if (result == null)
            {
                throw new NotFoundAppException("Assessment result is not available yet. Submit the assessment first.");
            }

            var answers = await _context.UserAnswers.Where(a => a.AssessmentId == assessmentId).ToListAsync();
            var wrongAnswers = answers.Where(a => !a.IsCorrect).Select(a => a.QuestionId).ToList();

            if (!isPremium)
            {
                return new
                {
                    result.Score,
                    result.MaxScore,
                    result.Percentage,
                    result.Grade,
                    WrongQuestionIds = wrongAnswers
                };
            }

            var questionIds = answers.Select(a => a.QuestionId).ToList();
            var questions = await _context.MCQQuestions.Where(q => questionIds.Contains(q.Id)).ToListAsync();

            var answerReview = answers.Select(a =>
            {
                var q = questions.First(x => x.Id == a.QuestionId);
                return new
                {
                    q.Text,
                    q.Subtopic,
                    a.SelectedOption,
                    q.CorrectOption,
                    a.IsCorrect
                };
            }).ToList();

            var weakAreas = answerReview.Where(a => !a.IsCorrect).Select(a => a.Subtopic).Distinct().ToList();

            return new
            {
                result.Score,
                result.MaxScore,
                result.Percentage,
                result.Grade,
                WrongQuestionIds = wrongAnswers,
                AnswerReview = answerReview,
                WeakAreas = weakAreas,
                WeakAreasSummary = weakAreas.Any() ? "Weak areas identified from incorrect answers." : "No weak areas identified because all submitted answers were correct."
            };
        }

        /// <summary>
        /// Returns all assessment results for admins.
        /// </summary>
        public async Task<IEnumerable<AssessmentResult>> GetAllAssessmentsAsync()
        {
            return await _context.AssessmentResults.ToListAsync();
        }

        /// <summary>
        /// Returns all MCQ questions for admin question bank management.
        /// </summary>
        public async Task<IEnumerable<MCQQuestion>> GetAllQuestionsAsync()
        {
            return await _context.MCQQuestions.OrderBy(q => q.Domain).ThenBy(q => q.OrderIndex).ToListAsync();
        }

        /// <summary>
        /// Adds a new MCQ question for the specified domain.
        /// </summary>
        public async Task<MCQQuestion> AddQuestionAsync(CreateQuestionRequest request)
        {
            ValidateQuestionRequest(request.Domain, request.Text, request.OptionA, request.OptionB, request.OptionC, request.OptionD, request.CorrectOption, request.Subtopic);

            var nextOrder = await _context.MCQQuestions.Where(q => q.Domain == request.Domain).CountAsync() + 1;

            var q = new MCQQuestion
            {
                Domain = request.Domain,
                Text = request.Text,
                OptionA = request.OptionA,
                OptionB = request.OptionB,
                OptionC = request.OptionC,
                OptionD = request.OptionD,
                CorrectOption = request.CorrectOption,
                Subtopic = request.Subtopic,
                OrderIndex = nextOrder
            };

            _context.MCQQuestions.Add(q);
            await _context.SaveChangesAsync();
            return q;
        }

        /// <summary>
        /// Updates an existing MCQ question.
        /// </summary>
        public async Task<MCQQuestion?> UpdateQuestionAsync(int questionId, UpdateQuestionRequest request)
        {
            if (questionId <= 0)
            {
                throw new ValidationAppException("A valid question id is required.");
            }

            ValidateQuestionRequest(request.Domain, request.Text, request.OptionA, request.OptionB, request.OptionC, request.OptionD, request.CorrectOption, request.Subtopic);

            var question = await _context.MCQQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
            if (question == null)
            {
                throw new NotFoundAppException("Question not found.");
            }

            var oldDomain = question.Domain;
            question.Domain = request.Domain;
            question.Text = request.Text;
            question.OptionA = request.OptionA;
            question.OptionB = request.OptionB;
            question.OptionC = request.OptionC;
            question.OptionD = request.OptionD;
            question.CorrectOption = request.CorrectOption;
            question.Subtopic = request.Subtopic;

            if (!string.Equals(oldDomain, request.Domain, StringComparison.OrdinalIgnoreCase))
            {
                question.OrderIndex = await _context.MCQQuestions
                    .Where(q => q.Domain == request.Domain && q.Id != questionId)
                    .CountAsync() + 1;
            }

            await _context.SaveChangesAsync();
            await ReIndexDomainQuestionsAsync(oldDomain);
            if (!string.Equals(oldDomain, request.Domain, StringComparison.OrdinalIgnoreCase))
            {
                await ReIndexDomainQuestionsAsync(request.Domain);
            }

            return question;
        }

        /// <summary>
        /// Deletes an MCQ question and reorders the remaining domain questions.
        /// </summary>
        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            if (questionId <= 0)
            {
                throw new ValidationAppException("A valid question id is required.");
            }

            var question = await _context.MCQQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
            if (question == null)
            {
                throw new NotFoundAppException("Question not found.");
            }

            var domain = question.Domain;
            _context.MCQQuestions.Remove(question);
            await _context.SaveChangesAsync();
            await ReIndexDomainQuestionsAsync(domain);
            return true;
        }

        /// <summary>
        /// Pre-generates questions and caches them in the DB.
        /// Called by the frontend while the user reads the instructions page.
        /// </summary>
        public async Task<int> WarmUpCacheAsync(string domain, string difficulty, int targetCount = 3)
        {
            var existingCount = await _context.MCQQuestions.CountAsync(q => q.Domain == domain);
            if (existingCount >= targetCount)
            {
                _logger.LogInformation("Warm-up skipped for {Domain}: already have {Count} cached questions.", domain, existingCount);
                return existingCount;
            }

            var needed = targetCount - existingCount;
            try
            {
                var aiQuestions = await GenerateAiAssessmentQuestionsAsync(domain, difficulty, Math.Min(needed, 3));
                if (aiQuestions.Any())
                {
                    var nextOrder = existingCount + 1;
                    foreach (var q in aiQuestions)
                    {
                        q.OrderIndex = nextOrder++;
                    }
                    _context.MCQQuestions.AddRange(aiQuestions);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Warm-up generated {Count} questions for {Domain}.", aiQuestions.Count, domain);
                    return existingCount + aiQuestions.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Warm-up generation failed for {Domain}.", domain);
            }

            return existingCount;
        }

        /// <summary>
        /// Dynamic Seed sub-scenarios to force diverse question generation
        /// without sending exclusion lists. Randomized per call.
        /// </summary>
        private static readonly string[][] DomainSubScenarios = new[]
        {
            new[] { "memory leaks and garbage collection", "thread safety and race conditions", "async deadlocks" },
            new[] { "API design and REST best practices", "middleware pipeline ordering", "dependency injection lifetimes" },
            new[] { "LINQ query optimization", "Entity Framework N+1 problems", "database connection pooling" },
            new[] { "exception handling strategies", "logging and observability", "configuration management" },
            new[] { "unit testing and mocking", "integration testing patterns", "CI/CD pipeline issues" },
            new[] { "security vulnerabilities", "authentication and authorization", "input validation and XSS prevention" },
            new[] { "design patterns in practice", "SOLID principles violations", "refactoring anti-patterns" },
            new[] { "microservices communication", "event-driven architecture", "circuit breaker and retry patterns" },
        };

        private static string GetRandomSubScenario()
        {
            var rnd = new Random();
            var group = DomainSubScenarios[rnd.Next(DomainSubScenarios.Length)];
            return group[rnd.Next(group.Length)];
        }

        private async Task<List<MCQQuestion>> GenerateAiAssessmentQuestionsAsync(string domain, string difficulty, int count)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var model = _config["Gemini:Model"] ?? "gemini-2.5-flash";
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_KEY")
            {
                throw new AppException(
                    "Gemini is not configured on the assessment service. Add a valid Gemini API key before starting assessments.",
                    StatusCodes.Status503ServiceUnavailable);
            }

            // Step 1: Dynamic Seed — random sub-scenario forces diversity
            var subScenario = GetRandomSubScenario();

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            // Step 4: Use response_mime_type for constrained JSON output (no manual parsing)
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = $"Generate exactly {count} {difficulty} multiple choice questions for the domain: {domain}. Focus specifically on: {subScenario}. Questions must be practical, scenario-based, and interview-level. Each question needs 4 plausible options (A-D) with exactly one correct answer." } } }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = new
                    {
                        type = "ARRAY",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new Dictionary<string, object>
                            {
                                ["text"] = new { type = "STRING", description = "The question text" },
                                ["optionA"] = new { type = "STRING", description = "Option A" },
                                ["optionB"] = new { type = "STRING", description = "Option B" },
                                ["optionC"] = new { type = "STRING", description = "Option C" },
                                ["optionD"] = new { type = "STRING", description = "Option D" },
                                ["correctOption"] = new { type = "STRING", description = "A, B, C, or D", @enum = new[] { "A", "B", "C", "D" } },
                                ["subtopic"] = new { type = "STRING", description = "The subtopic of this question" }
                            },
                            required = new[] { "text", "optionA", "optionB", "optionC", "optionD", "correctOption", "subtopic" }
                        }
                    }
                }
            };

            var contentString = System.Text.Json.JsonSerializer.Serialize(requestBody);

            // Step 2 (Polly): Fast retries — 500ms, 1s, 2s — quick recovery from transient 429s
            var retryDelays = new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) };
            var retryPolicy = Polly.Policy
                .Handle<HttpRequestException>()
                .Or<AppException>()
                .WaitAndRetryAsync(retryDelays,
                    (exception, timeSpan, retryCount, context) => {
                        _logger.LogWarning("Retrying Gemini (attempt {RetryCount}) after {Delay}ms: {Message}", retryCount, timeSpan.TotalMilliseconds, exception.Message);
                    });

            return await retryPolicy.ExecuteAsync(async () =>
            {
                var questions = new List<MCQQuestion>();
                using var content = new StringContent(contentString, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(url, content);
                
                if ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500)
                {
                    throw new HttpRequestException($"Gemini API returned {(int)response.StatusCode}");
                }
                
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseString);
                
                var candidates = document.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() == 0)
                    throw new AppException("Gemini returned empty candidates.", StatusCodes.Status503ServiceUnavailable);

                var textResponse = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(textResponse))
                {
                    throw new AppException(
                        "Gemini returned an empty response.",
                        StatusCodes.Status503ServiceUnavailable);
                }

                // With response_mime_type, the output is already clean JSON — but clean up just in case
                textResponse = textResponse.Replace("```json", string.Empty).Replace("```", string.Empty).Trim();
                var dtos = System.Text.Json.JsonSerializer.Deserialize<List<GeminiAssessmentQuestionDto>>(textResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dtos == null)
                {
                    throw new AppException(
                        "Gemini returned an unreadable payload.",
                        StatusCodes.Status503ServiceUnavailable);
                }

                var index = 1;
                foreach (var dto in dtos)
                {
                    var correctOption = string.IsNullOrWhiteSpace(dto.correctOption) ? "A" : dto.correctOption.Trim().ToUpperInvariant()[0].ToString();
                    if (!"ABCD".Contains(correctOption)) correctOption = "A";

                    var question = new MCQQuestion
                    {
                        Domain = domain,
                        Text = dto.text?.Trim() ?? string.Empty,
                        OptionA = dto.optionA?.Trim() ?? string.Empty,
                        OptionB = dto.optionB?.Trim() ?? string.Empty,
                        OptionC = dto.optionC?.Trim() ?? string.Empty,
                        OptionD = dto.optionD?.Trim() ?? string.Empty,
                        CorrectOption = correctOption,
                        Subtopic = string.IsNullOrWhiteSpace(dto.subtopic) ? subScenario : dto.subtopic,
                        Marks = 1,
                        OrderIndex = index++
                    };

                    if (!IsHighQualityAssessmentQuestion(question) || questions.Any(q => NormalizeQuestionText(q.Text) == NormalizeQuestionText(question.Text)))
                        continue;

                    questions.Add(question);
                }

                if (questions.Count == 0)
                {
                    throw new AppException(
                        "Gemini did not return any usable assessment questions.",
                        StatusCodes.Status503ServiceUnavailable);
                }
                
                return questions;
            });
        }

        private sealed class GeminiAssessmentQuestionDto
        {
            public string? text { get; set; }
            public string? optionA { get; set; }
            public string? optionB { get; set; }
            public string? optionC { get; set; }
            public string? optionD { get; set; }
            public string? correctOption { get; set; }
            public string? subtopic { get; set; }
        }

        private async Task<HashSet<string>> GetPreviousAssessmentQuestionTextsAsync(int userId, string domain)
        {
            var priorAssessmentIds = await _context.Assessments
                .Where(a => a.UserId == userId && a.Domain == domain)
                .Select(a => a.Id)
                .ToListAsync();

            var priorQuestionIds = await _context.UserAnswers
                .Where(a => priorAssessmentIds.Contains(a.AssessmentId))
                .Select(a => a.QuestionId)
                .Distinct()
                .ToListAsync();

            var priorTexts = await _context.MCQQuestions
                .Where(q => priorQuestionIds.Contains(q.Id))
                .Select(q => q.Text)
                .ToListAsync();

            return priorTexts
                .Select(NormalizeQuestionText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToHashSet();
        }

        private static bool IsHighQualityAssessmentQuestion(MCQQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Text) ||
                string.IsNullOrWhiteSpace(question.OptionA) ||
                string.IsNullOrWhiteSpace(question.OptionB) ||
                string.IsNullOrWhiteSpace(question.OptionC) ||
                string.IsNullOrWhiteSpace(question.OptionD))
            {
                return false;
            }

            var text = question.Text.Trim();
            var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            // Only reject if extremely short (less than 5 words or 20 chars)
            if (wordCount < 5 || text.Length < 20)
            {
                return false;
            }

            var lower = text.ToLowerInvariant();
            
            // Reject only the most trivial patterns
            var rejectedPatterns = new[]
            {
                "true or false",
            };

            if (rejectedPatterns.Any(pattern => lower.StartsWith(pattern)))
            {
                return false;
            }

            // Accept all other questions — minor repetition is allowed per requirements
            return true;
        }

        private static string NormalizeQuestionText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return System.Text.RegularExpressions.Regex.Replace(text.Trim().ToLowerInvariant(), @"\s+", " ");
        }

        /// <summary>
        /// Rebuilds order indexes so each domain starts at 1 and remains sequential.
        /// </summary>
        private async Task ReIndexDomainQuestionsAsync(string domain)
        {
            var domainQuestions = await _context.MCQQuestions
                .Where(q => q.Domain == domain)
                .OrderBy(q => q.OrderIndex)
                .ThenBy(q => q.Id)
                .ToListAsync();

            for (var index = 0; index < domainQuestions.Count; index++)
            {
                domainQuestions[index].OrderIndex = index + 1;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates question input before saving it.
        /// </summary>
        private static void ValidateQuestionRequest(string domain, string text, string optionA, string optionB, string optionC, string optionD, string correctOption, string subtopic)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ValidationAppException("Domain is required.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ValidationAppException("Question text is required.");
            }

            if (string.IsNullOrWhiteSpace(optionA) || string.IsNullOrWhiteSpace(optionB) || string.IsNullOrWhiteSpace(optionC) || string.IsNullOrWhiteSpace(optionD))
            {
                throw new ValidationAppException("All four options are required.");
            }

            if (string.IsNullOrWhiteSpace(correctOption) || !"ABCD".Contains(correctOption.Trim().ToUpperInvariant()))
            {
                throw new ValidationAppException("Correct option must be one of A, B, C, or D.");
            }

            if (string.IsNullOrWhiteSpace(subtopic))
            {
                throw new ValidationAppException("Subtopic is required.");
            }
        }
    }
}







