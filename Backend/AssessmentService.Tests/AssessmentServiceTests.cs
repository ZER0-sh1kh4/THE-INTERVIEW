using AssessmentService.Data;
using AssessmentService.DTOs;
using AssessmentService.Models;
using AssessmentService.Services;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace AssessmentService.Tests;

/// <summary>
/// Verifies assessment rules such as free-tier limits, grading, and question administration.
/// </summary>
public class AssessmentServiceTests
{
    [Fact]
    public async Task StartAssessmentAsync_FreeUserThirdAttempt_ThrowsForbidden()
    {
        await using var context = CreateContext();
        context.Assessments.AddRange(
            new Assessment { UserId = 77, Domain = "C#", AttemptNumber = 1, Status = "Completed" },
            new Assessment { UserId = 77, Domain = "C#", AttemptNumber = 2, Status = "Completed" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var act = () => service.StartAssessmentAsync(77, false, new StartAssessmentRequest { Domain = "C#" });

        var exception = await act.Should().ThrowAsync<ForbiddenAppException>();
        exception.Which.Message.Should().Contain("only 2 assessment tests");
    }

    [Fact]
    public async Task StartAssessmentAsync_ReturnsQuestionsForRequestedDomain()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var response = await service.StartAssessmentAsync(5, false, new StartAssessmentRequest { Domain = "C#" });

        response.Should().NotBeNull();
        response!.AssessmentId.Should().BeGreaterThan(0);
        response.Questions.Should().HaveCount(10);
        response.Questions.Select(q => q.OrderIndex).Should().ContainInOrder(1, 2, 3, 4, 5);
        response.Questions.First().Text.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SubmitAssessmentAsync_PremiumUserResult_IncludesWeakAreasSummary()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var startResponse = await service.StartAssessmentAsync(25, true, new StartAssessmentRequest { Domain = "C#" });
        startResponse.Should().NotBeNull();

        var submitResponse = await service.SubmitAssessmentAsync(25, true, new SubmitAssessmentRequest
        {
            AssessmentId = startResponse!.AssessmentId,
            Answers = new List<AnswerSubmission>
            {
                new() { QuestionId = startResponse.Questions[0].Id, SelectedOption = "D" },
                new() { QuestionId = startResponse.Questions[1].Id, SelectedOption = "A" }
            }
        });

        submitResponse.Should().NotBeNull();
        var weakAreas = GetProperty<List<string>>(submitResponse!, "WeakAreas");
        var weakAreasSummary = GetProperty<string>(submitResponse, "WeakAreasSummary");

        weakAreas.Should().NotBeEmpty();
        weakAreasSummary.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AddQuestionAsync_AppendsQuestionAtNextDomainOrder()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created = await service.AddQuestionAsync(new CreateQuestionRequest
        {
            Domain = "Math",
            Text = "2 + 2 = ?",
            OptionA = "3",
            OptionB = "4",
            OptionC = "5",
            OptionD = "6",
            CorrectOption = "B",
            Subtopic = "Arithmetic"
        });

        created.OrderIndex.Should().Be(1);

        var createdAgain = await service.AddQuestionAsync(new CreateQuestionRequest
        {
            Domain = "Math",
            Text = "5 - 3 = ?",
            OptionA = "1",
            OptionB = "2",
            OptionC = "3",
            OptionD = "4",
            CorrectOption = "B",
            Subtopic = "Arithmetic"
        });

        createdAgain.OrderIndex.Should().Be(2);
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

    private static AssessmentSvc CreateService(AppDbContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gemini:ApiKey"] = "TEST_KEY",
                ["Gemini:Model"] = "gemini-1.5-flash"
            })
            .Build();

        return new AssessmentSvc(
            context,
            new HttpClient(new GeminiHandler()),
            configuration,
            NullLogger<AssessmentSvc>.Instance);
    }

    private static T GetProperty<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName);
        property.Should().NotBeNull($"expected property {propertyName} to exist on result");
        return (T)property!.GetValue(instance)!;
    }

    private sealed class GeminiHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var questions = Enumerable.Range(1, 10).Select(index => new
            {
                text = $"In a production C# scenario number {index}, how should a developer handle a failing service while balancing reliability and speed?",
                optionA = "Investigate telemetry, isolate the cause, apply a safe fix, and verify with tests.",
                optionB = "Restart every server and ignore the root cause until users complain again.",
                optionC = "Change unrelated code paths to see whether the issue disappears.",
                optionD = "Disable monitoring so alerts do not distract the team.",
                correctOption = "A",
                subtopic = "Production Debugging"
            });

            var responseBody = JsonSerializer.Serialize(new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[] { new { text = JsonSerializer.Serialize(questions) } }
                        }
                    }
                }
            });

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
