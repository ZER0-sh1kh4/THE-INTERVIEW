using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using InterviewService.DTOs;
using InterviewService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InterviewService.Controllers
{
    [ApiController]
    [Route("api/interviews")]
    [Authorize]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewSvc _service;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<InterviewController> _logger;

        public InterviewController(IInterviewSvc service, IRabbitMqPublisher rabbitMqPublisher, ILogger<InterviewController> logger)
        {
            _service = service;
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Executes an action and converts controlled business exceptions into API responses.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Interview API returned a controlled error for {Path}", HttpContext.Request.Path);
                return StatusCode(ex.StatusCode, new ApiErrorResponse
                {
                    Message = ex.Message,
                    Details = ex.Details
                });
            }
        }

        /// <summary>
        /// Reads current user id, premium state, and email from JWT claims.
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
        /// Creates a new interview entry in pending state.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartInterviewRequest request)
        {
            return await ExecuteAsync(async () =>
            {
                var (userId, isPremium, _) = GetUserDetails();
                _logger.LogInformation("Incoming interview start request for user {UserId} and domain {Domain}", userId, request.Domain);

                var interview = await _service.StartInterviewAsync(userId, isPremium, request);

                await _rabbitMqPublisher.PublishAsync(QueueNames.InterviewEvents, new InterviewStartedEvent
                {
                    UserId = userId,
                    Domain = request.Domain,
                    InterviewId = interview.Id
                });

                return Ok(ApiResponse<InterviewService.Models.Interview>.Ok(interview, "Interview created successfully."));
            });
        }

        /// <summary>
        /// Starts the interview and returns the exact questions to answer.
        /// </summary>
        [HttpPost("{id}/begin")]
        public async Task<IActionResult> Begin(int id)
        {
            return await ExecuteAsync(async () =>
            {
                var (userId, isPremium, _) = GetUserDetails();
                _logger.LogInformation("Incoming interview begin request for user {UserId} and interview {InterviewId}", userId, id);
                var questions = await _service.BeginInterviewAsync(userId, isPremium, id);
                return Ok(ApiResponse<object>.Ok(questions));
            });
        }

        /// <summary>
        /// Pre-warms the question cache for an interview domain while the user chooses parameters.
        /// </summary>
        [HttpPost("warm-up")]
        public async Task<IActionResult> WarmUp([FromBody] WarmUpRequest request)
        {
            _logger.LogInformation("Incoming interview warm-up request for domain {Domain}", request.Domain);
            var count = await _service.WarmUpCacheAsync(request.Domain, request.TargetCount);
            return Ok(ApiResponse<object>.Ok(new { count }, "Warm-up triggered."));
        }

        /// <summary>
        /// Submits interview answers and calculates the result.
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitInterviewRequest request)
        {
            return await ExecuteAsync(async () =>
            {
                var (userId, isPremium, email) = GetUserDetails();
                _logger.LogInformation("Incoming interview submit request for user {UserId} and interview {InterviewId}", userId, request.InterviewId);

                var result = await _service.SubmitInterviewAsync(userId, isPremium, request);
                if (result == null)
                {
                    return NotFound(new ApiErrorResponse { Message = "Interview not found." });
                }

                var details = await _service.GetInterviewByIdAsync(userId, request.InterviewId);
                var interview = details?.GetType().GetProperty("Interview")?.GetValue(details);
                var domain = interview?.GetType().GetProperty("Domain")?.GetValue(interview)?.ToString() ?? "Interview";
                var percentage = (double)(result.GetType().GetProperty("Percentage")?.GetValue(result) ?? 0d);
                var grade = result.GetType().GetProperty("Grade")?.GetValue(result)?.ToString() ?? "N/A";

                await _rabbitMqPublisher.PublishAsync(QueueNames.InterviewEvents, new InterviewCompletedEvent
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
                    Subject = "Interview completed",
                    TemplateKey = "interview-complete",
                    Model = new Dictionary<string, string>
                    {
                        ["Domain"] = domain,
                        ["Percentage"] = percentage.ToString("0.##"),
                        ["Grade"] = grade
                    }
                });

                return Ok(ApiResponse<object>.Ok(result));
            });
        }

        /// <summary>
        /// Returns all interviews created by the current user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyInterviews()
        {
            return await ExecuteAsync(async () =>
            {
                var (userId, _, _) = GetUserDetails();
                return Ok(ApiResponse<IEnumerable<InterviewService.Models.Interview>>.Ok(await _service.GetMyInterviewsAsync(userId)));
            });
        }

        /// <summary>
        /// Returns one interview with its questions.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInterview(int id)
        {
            return await ExecuteAsync(async () =>
            {
                var (userId, _, _) = GetUserDetails();
                var interview = await _service.GetInterviewByIdAsync(userId, id);
                if (interview == null)
                {
                    return NotFound(new ApiErrorResponse { Message = "Interview not found." });
                }

                return Ok(ApiResponse<object>.Ok(interview));
            });
        }

        /// <summary>
        /// Returns the result for a completed interview.
        /// </summary>
        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetResult(int id)
        {
            return await ExecuteAsync(async () =>
            {
                var (userId, isPremium, _) = GetUserDetails();
                var result = await _service.GetResultAsync(userId, isPremium, id);
                if (result == null)
                {
                    return NotFound(new ApiErrorResponse { Message = "Interview result not found." });
                }

                return Ok(ApiResponse<object>.Ok(result));
            });
        }

        /// <summary>
        /// Returns all interviews for admin users.
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllInterviews()
        {
            return await ExecuteAsync(async () =>
            {
                return Ok(ApiResponse<IEnumerable<InterviewService.Models.Interview>>.Ok(await _service.GetAllInterviewsAsync()));
            });
        }
    }
}
