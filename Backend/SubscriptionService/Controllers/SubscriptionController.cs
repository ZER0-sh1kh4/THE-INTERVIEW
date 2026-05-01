using BuildingBlocks.Contracts;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.DTOs;
using SubscriptionService.Services;
using System.Security.Claims;
using System.Text;

namespace SubscriptionService.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionSvc _service;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscriptionSvc service, IRabbitMqPublisher rabbitMqPublisher, ILogger<SubscriptionController> logger)
        {
            _service = service;
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Reads the current user id and email from JWT claims.
        /// </summary>
        private (int userId, string email) GetUserContext()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            int.TryParse(userIdStr, out var userId);
            return (userId, email);
        }

        /// <summary>
        /// Creates a pending subscription payment order.
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            var (userId, _) = GetUserContext();
            _logger.LogInformation("Incoming subscribe request for user {UserId}", userId);
            var result = await _service.SubscribeAsync(userId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Confirms the payment and activates premium.
        /// </summary>
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest request)
        {
            var (userId, email) = GetUserContext();
            _logger.LogInformation("Incoming payment confirmation for user {UserId} and session {SessionId}", userId, request.PaymentSessionId);

            var result = await _service.ConfirmPaymentAsync(userId, request);

            await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
            {
                ToEmail = email,
                ToName = email,
                Subject = "Payment successful",
                TemplateKey = "payment-success",
                Model = new Dictionary<string, string>
                {
                    ["Amount"] = result.GetType().GetProperty("amount")?.GetValue(result)?.ToString() ?? string.Empty,
                    ["Currency"] = "INR",
                    ["PaymentId"] = result.GetType().GetProperty("transactionId")?.GetValue(result)?.ToString() ?? string.Empty
                }
            });

            await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
            {
                ToEmail = email,
                ToName = email,
                Subject = "Premium subscription activated",
                TemplateKey = "subscription-upgrade",
                Model = new Dictionary<string, string>
                {
                    ["Plan"] = "Premium",
                    ["EndDate"] = result.GetType().GetProperty("endDate")?.GetValue(result)?.ToString() ?? string.Empty
                }
            });

            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Receives Stripe webhook callbacks, verifies the signature and updates payment state asynchronously.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhook/stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

            _logger.LogInformation("Incoming Stripe webhook received.");
            var result = await _service.HandleStripeWebhookAsync(rawBody, signature);
            return Ok(ApiResponse<object>.Ok(result, "Stripe webhook processed."));
        }

        /// <summary>
        /// Cancels the user's active premium subscription.
        /// </summary>
        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            var (userId, _) = GetUserContext();
            _logger.LogInformation("Incoming cancel subscription request for user {UserId}", userId);
            var message = await _service.CancelSubscriptionAsync(userId);
            return Ok(ApiResponse<object>.Ok(null, message));
        }

        /// <summary>
        /// Returns the current user's subscriptions.
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMySubscriptions()
        {
            var (userId, _) = GetUserContext();
            return Ok(ApiResponse<IEnumerable<SubscriptionService.Models.Subscription>>.Ok(await _service.GetMySubscriptionsAsync(userId)));
        }

        /// <summary>
        /// Returns the current user's payment history.
        /// </summary>
        [HttpGet("my/payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var (userId, _) = GetUserContext();
            return Ok(ApiResponse<IEnumerable<SubscriptionService.Models.PaymentRecord>>.Ok(await _service.GetMyPaymentsAsync(userId)));
        }

        /// <summary>
        /// Returns all subscriptions for admins.
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            return Ok(ApiResponse<IEnumerable<SubscriptionService.Models.Subscription>>.Ok(await _service.GetAllSubscriptionsAsync()));
        }

        /// <summary>
        /// Returns all payments for admins.
        /// </summary>
        [HttpGet("payments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            return Ok(ApiResponse<IEnumerable<SubscriptionService.Models.PaymentRecord>>.Ok(await _service.GetAllPaymentsAsync()));
        }
    }
}
