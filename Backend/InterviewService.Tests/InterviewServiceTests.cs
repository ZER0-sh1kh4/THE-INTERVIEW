using BuildingBlocks.Exceptions;
using FluentAssertions;
using InterviewService.Data;
using InterviewService.DTOs;
using InterviewService.Models;
using InterviewService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace InterviewService.Tests;

/// <summary>
/// Exercises interview business rules, question numbering, and result shaping.
/// </summary>
public class InterviewServiceTests
{
    [Fact]
    public async Task StartInterviewAsync_FreeUserSecondInterview_ThrowsForbidden()
    {
        await using var context = CreateContext();
        context.Interviews.Add(new Interview
        {
            UserId = 91,
            Title = "Existing interview",
            Domain = "C#",
            Status = "Completed"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var act = () => service.StartInterviewAsync(91, false, new StartInterviewRequest
        {
            Title = "Second interview",
            Domain = "C#"
        });

        var exception = await act.Should().ThrowAsync<ForbiddenAppException>();
        exception.Which.Message.Should().Contain("only 1 interview");
    }

    [Fact]
    public async Task BeginInterviewAsync_ReturnsQuestionNumbersStartingAtOne()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var interview = await service.StartInterviewAsync(12, false, new StartInterviewRequest
        {
            Title = "C# Basics",
            Domain = "C#"
        });

        var response = await service.BeginInterviewAsync(12, false, interview.Id);
        var questions = GetProperty<List<QuestionResponseDto>>(response, "Questions");

        questions.Should().HaveCount(5);
        questions.Select(q => q.Id).Should().ContainInOrder(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task SubmitInterviewAsync_InvalidQuestionNumber_ThrowsValidationError()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var interview = await service.StartInterviewAsync(13, false, new StartInterviewRequest
        {
            Title = "C# Basics",
            Domain = "C#"
        });
        await service.BeginInterviewAsync(13, false, interview.Id);

        var act = () => service.SubmitInterviewAsync(13, false, new SubmitInterviewRequest
        {
            InterviewId = interview.Id,
            Answers = new List<InterviewAnswerSubmission>
            {
                new() { QuestionId = 99, AnswerText = "B" }
            }
        });

        var exception = await act.Should().ThrowAsync<ValidationAppException>();
        exception.Which.Message.Should().Contain("question ids returned by the begin endpoint");
    }

    [Fact]
    public async Task SubmitInterviewAsync_PremiumResponse_IncludesBreakdown()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var interview = await service.StartInterviewAsync(14, true, new StartInterviewRequest
        {
            Title = "Premium C#",
            Domain = "C#"
        });
        await service.BeginInterviewAsync(14, true, interview.Id);

        var result = await service.SubmitInterviewAsync(14, true, new SubmitInterviewRequest
        {
            InterviewId = interview.Id,
            Answers = new List<InterviewAnswerSubmission>
            {
                new() { QuestionId = 1, AnswerText = "B" },
                new() { QuestionId = 2, AnswerText = "B" },
                new() { QuestionId = 3, AnswerText = "B" },
                new() { QuestionId = 4, AnswerText = "Language" },
                new() { QuestionId = 5, AnswerText = "Exception" }
            }
        });

        var breakdown = GetProperty<object>(result!, "Breakdown");
        breakdown.Should().NotBeNull();
        GetProperty<List<int>>(result!, "WrongQuestionIds").Should().NotBeNull();
        GetProperty<List<string>>(result!, "Suggestions").Should().NotBeNull();
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    private static InterviewSvc CreateService(AppDbContext context)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gemini:ApiKey"] = "TEST_KEY",
                ["Gemini:Model"] = "gemini-2.5-flash"
            })
            .Build();

        return new InterviewSvc(context, new HttpClient(new GeminiHandler()), config, NullLogger<InterviewSvc>.Instance);
    }

    private static T GetProperty<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName);
        property.Should().NotBeNull($"expected property {propertyName} to exist on result");
        return (T)property!.GetValue(instance)!;
    }

    private sealed class GeminiHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            var prompt = JsonDocument.Parse(requestBody).RootElement
                .GetProperty("contents")[0]
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            var text = prompt.Contains("Score each answer", StringComparison.OrdinalIgnoreCase)
                ? BuildEvaluationJson()
                : BuildQuestionJson();

            var responseBody = JsonSerializer.Serialize(new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[] { new { text } }
                        }
                    }
                }
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }

        private static string BuildQuestionJson()
        {
            var questions = Enumerable.Range(1, 5).Select(index => new
            {
                text = $"How would you handle a production C# service scenario number {index} where latency rises and users report intermittent failures?",
                questionType = "Subjective",
                idealAnswer = "A strong answer should discuss investigation, metrics, trade-offs, rollout safety, and prevention.",
                evaluationCriteria = "Score clarity, practical debugging, production reasoning, and trade-off awareness.",
                subtopic = "Production Debugging"
            });

            return JsonSerializer.Serialize(questions);
        }

        private static string BuildEvaluationJson()
        {
            var evaluations = Enumerable.Range(1, 5).Select(index => new
            {
                questionId = index,
                score = 8,
                isStrong = true,
                idealAnswer = "A strong answer should include context, action, reasoning, and measurable outcomes.",
                followUpQuestion = "How would you raise the difficulty by handling scale, security, and edge-case trade-offs?"
            });

            return JsonSerializer.Serialize(new
            {
                overallFeedback = "Gemini evaluated the interview answers with a practical rubric.",
                questionEvaluations = evaluations
            });
        }
    }
}
