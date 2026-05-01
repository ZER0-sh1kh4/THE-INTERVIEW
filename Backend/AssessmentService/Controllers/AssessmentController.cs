using AssessmentService.DTOs;
using AssessmentService.Services;
using BuildingBlocks.Contracts;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssessmentService.Controllers
{
    [ApiController]
    [Route("api/assessments")]
    [Authorize]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _service;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(IAssessmentService service, IRabbitMqPublisher rabbitMqPublisher, ILogger<AssessmentController> logger)
        {
            _service = service;
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Reads current user details from JWT claims.
        /// </summary>
        private (int userId, bool isPremium, string email) GetUserDetails()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isPremiumStr = User.FindFirst("isPremium")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            int.TryParse(userIdStr, out var userId);
            var isPremium = string.Equals(isPremiumStr, "true", StringComparison.OrdinalIgnoreCase);
            return (userId, isPremium, email);
        }

        /// <summary>
        /// Starts a new assessment for the current user.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartAssessmentRequest request)
        {
            var (userId, isPremium, _) = GetUserDetails();
            _logger.LogInformation("Incoming assessment start request for user {UserId} and domain {Domain}", userId, request.Domain);

            // FRESHLY ADDED: pass the full request so question count/difficulty can be honored by the backend.
            var response = await _service.StartAssessmentAsync(userId, isPremium, request);

            await _rabbitMqPublisher.PublishAsync(QueueNames.AssessmentEvents, new AssessmentStartedEvent
            {
                UserId = userId,
                Domain = request.Domain,
                AssessmentId = response!.AssessmentId
            });

            return Ok(ApiResponse<StartAssessmentResponse>.Ok(response));
        }

        /// <summary>
        /// Returns the next batch of questions for a running assessment (lazy loading).
        /// </summary>
        [HttpGet("{id}/next-batch")]
        public async Task<IActionResult> NextBatch(int id, [FromQuery] int currentCount = 0, [FromQuery] int batchSize = 5)
        {
            var (userId, _, _) = GetUserDetails();
            var questions = await _service.GetNextBatchAsync(userId, id, currentCount, batchSize);
            return Ok(ApiResponse<List<QuestionDto>>.Ok(questions));
        }

        /// <summary>
        /// Pre-warms the question cache for a domain while the user reads instructions.
        /// Called automatically by the frontend on the assessment start page.
        /// </summary>
        [HttpPost("warm-up")]
        public async Task<IActionResult> WarmUp([FromBody] WarmUpRequest request)
        {
            _logger.LogInformation("Warm-up request for domain {Domain}", request.Domain);
            var cachedCount = await _service.WarmUpCacheAsync(
                request.Domain?.Trim() ?? string.Empty,
                request.Difficulty?.Trim() ?? "Medium",
                request.TargetCount <= 0 ? 3 : request.TargetCount
            );
            return Ok(ApiResponse<object>.Ok(new { cachedCount }, "Warm-up complete."));
        }

        /// <summary>
        /// Submits assessment answers and calculates the result.
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitAssessmentRequest request)
        {
            var (userId, isPremium, email) = GetUserDetails();
            _logger.LogInformation("Incoming assessment submit request for user {UserId} and assessment {AssessmentId}", userId, request.AssessmentId);

            var response = await _service.SubmitAssessmentAsync(userId, isPremium, request);
            if (response == null)
            {
                return NotFound(new ApiErrorResponse { Message = "Assessment not found." });
            }

            var result = await _service.GetAssessmentResultAsync(userId, request.AssessmentId, isPremium);
            if (result is not null)
            {
                var percentage = (double)(result.GetType().GetProperty("Percentage")?.GetValue(result) ?? 0d);
                var grade = result.GetType().GetProperty("Grade")?.GetValue(result)?.ToString() ?? "N/A";
                var assessment = (await _service.GetUserAssessmentsAsync(userId)).FirstOrDefault(r => r.AssessmentId == request.AssessmentId);
                var domain = assessment?.Domain ?? "Assessment";

                await _rabbitMqPublisher.PublishAsync(QueueNames.AssessmentEvents, new AssessmentCompletedEvent
                {
                    UserId = userId,
                    UserEmail = email,
                    Domain = domain,
                    Percentage = percentage,
                    Grade = grade
                });

                await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
                {
                    ToEmail = email,
                    ToName = email,
                    Subject = "Assessment completed",
                    TemplateKey = "assessment-complete",
                    Model = new Dictionary<string, string>
                    {
                        ["Domain"] = domain,
                        ["Percentage"] = percentage.ToString("0.##"),
                        ["Grade"] = grade
                    }
                });
            }

            return Ok(ApiResponse<object>.Ok(response));
        }

        /// <summary>
        /// Returns the user's past assessment results.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyAssessments()
        {
            var (userId, _, _) = GetUserDetails();
            _logger.LogInformation("Fetching assessments for user {UserId}", userId);
            var results = await _service.GetUserAssessmentsAsync(userId);
            return Ok(ApiResponse<IEnumerable<AssessmentService.Models.AssessmentResult>>.Ok(results));
        }

        /// <summary>
        /// Returns one assessment result.
        /// </summary>
        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetResult(int id)
        {
            var (userId, isPremium, _) = GetUserDetails();
            _logger.LogInformation("Fetching result for assessment {AssessmentId} (User: {UserId}, Premium: {IsPremium})", id, userId, isPremium);
            var response = await _service.GetAssessmentResultAsync(userId, id, isPremium);
            _logger.LogInformation("Successfully fetched result for assessment {AssessmentId}", id);
            return Ok(ApiResponse<object>.Ok(response));
        }

        /// <summary>
        /// Returns all assessment results for admins.
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAssessments()
        {
            return Ok(ApiResponse<IEnumerable<AssessmentService.Models.AssessmentResult>>.Ok(await _service.GetAllAssessmentsAsync()));
        }

        /// <summary>
        /// Returns all MCQ questions for admin question bank management.
        /// </summary>
        [HttpGet("questions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllQuestions()
        {
            return Ok(ApiResponse<IEnumerable<AssessmentService.Models.MCQQuestion>>.Ok(await _service.GetAllQuestionsAsync()));
        }

        /// <summary>
        /// Adds a new question to the assessment bank.
        /// </summary>
        [HttpPost("questions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddQuestion([FromBody] CreateQuestionRequest request)
        {
            _logger.LogInformation("Incoming add-question request for domain {Domain}", request.Domain);
            var question = await _service.AddQuestionAsync(request);
            return Ok(ApiResponse<AssessmentService.Models.MCQQuestion>.Ok(question, "Question added successfully."));
        }

        /// <summary>
        /// Updates an existing question in the assessment bank.
        /// </summary>
        [HttpPut("questions/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionRequest request)
        {
            _logger.LogInformation("Incoming update-question request for question {QuestionId}", id);
            var updatedQuestion = await _service.UpdateQuestionAsync(id, request);
            return Ok(ApiResponse<AssessmentService.Models.MCQQuestion?>.Ok(updatedQuestion, "Question updated successfully."));
        }

        /// <summary>
        /// Deletes an existing question from the assessment bank.
        /// </summary>
        [HttpDelete("questions/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            _logger.LogInformation("Incoming delete-question request for question {QuestionId}", id);
            await _service.DeleteQuestionAsync(id);
            return Ok(ApiResponse<object>.Ok(null, "Question deleted successfully."));
        }
    }
}

