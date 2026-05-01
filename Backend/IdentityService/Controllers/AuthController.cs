using BuildingBlocks.Contracts;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IRabbitMqPublisher rabbitMqPublisher, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _rabbitMqPublisher = rabbitMqPublisher;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user, returns a JWT token, and publishes registration/email events.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Incoming register request for {Email}", request.Email);

            var result = await _authService.RegisterAsync(request);

            await _rabbitMqPublisher.PublishAsync(QueueNames.UserRegistration, new UserRegisteredEvent
            {
                UserId = result.UserId,
                FullName = request.FullName,
                Email = request.Email
            });

            await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
            {
                ToEmail = request.Email,
                ToName = request.FullName,
                Subject = "Welcome to Mock Interview Platform",
                TemplateKey = "welcome",
                Model = new Dictionary<string, string>
                {
                    ["FullName"] = request.FullName
                }
            });

            _logger.LogInformation("User {Email} registered successfully", request.Email);
            return Ok(ApiResponse<AuthResponse>.Ok(result, result.Message));
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Incoming login request for {Email}", request.Email);
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, result.Message));
        }

        /// <summary>
        /// Sends a password-reset OTP to the user's registered email address.
        /// </summary>
        [HttpPost("forgot-password/request-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestForgotPasswordOtp([FromBody] ForgotPasswordOtpRequest request)
        {
            _logger.LogInformation("Incoming forgot-password OTP request for {Email}", request.Email);

            var dispatch = await _authService.SendForgotPasswordOtpAsync(request);

            if (dispatch.ShouldSendEmail)
            {
                await _rabbitMqPublisher.PublishAsync(QueueNames.EmailNotifications, new EmailRequestedEvent
                {
                    ToEmail = dispatch.ToEmail,
                    ToName = dispatch.ToName,
                    Subject = "Your password reset OTP",
                    TemplateKey = "password-reset-otp",
                    Model = new Dictionary<string, string>
                    {
                        ["FullName"] = dispatch.ToName,
                        ["OtpCode"] = dispatch.Otp,
                        ["ExpiryMinutes"] = dispatch.ExpiryMinutes.ToString()
                    }
                });
            }

            return Ok(ApiResponse<object>.Ok(null, dispatch.Message));
        }

        /// <summary>
        /// Resets the password when the provided OTP is valid.
        /// </summary>
        [HttpPost("forgot-password/reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequest request)
        {
            _logger.LogInformation("Incoming password reset request for {Email}", request.Email);

            await _authService.ResetPasswordWithOtpAsync(request);
            return Ok(ApiResponse<object>.Ok(null, "Password reset successful."));
        }

        /// <summary>
        /// Returns the current authenticated user's claims.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var isPremium = User.FindFirst("isPremium")?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            return Ok(ApiResponse<object>.Ok(new { userId, email, role, isPremium, fullName }));
        }

        /// <summary>
        /// Refreshes the caller's token after profile changes such as premium upgrades.
        /// </summary>
        [HttpPost("refresh-claims")]
        [Authorize]
        public async Task<IActionResult> RefreshClaims()
        {
            _logger.LogInformation("Incoming refresh-claims request");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Unable to read the authenticated user.");
            }

            var result = await _authService.RefreshClaimsAsync(userId);
            return Ok(ApiResponse<AuthResponse>.Ok(result, result.Message));
        }

        /// <summary>
        /// Updates the current user's profile (display name).
        /// Returns a refreshed JWT with the updated claims.
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Unable to read the authenticated user.");
            }

            if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length < 2 || request.FullName.Trim().Length > 100)
            {
                return BadRequest(new ApiErrorResponse { Message = "Name must be between 2 and 100 characters." });
            }

            await _authService.UpdateProfileAsync(userId, request.FullName);

            // Return a refreshed token so the frontend gets updated claims immediately
            var refreshed = await _authService.RefreshClaimsAsync(userId);
            return Ok(ApiResponse<AuthResponse>.Ok(refreshed, "Profile updated successfully."));
        }

        /// <summary>
        /// Returns all users for admin review.
        /// </summary>
        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<User>>.Ok(users));
        }

        /// <summary>
        /// Returns one user by id for admins.
        /// </summary>
        [HttpGet("admin/users/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiErrorResponse { Message = "User not found." });
            }

            return Ok(ApiResponse<User>.Ok(user));
        }

        /// <summary>
        /// Updates a user's role.
        /// </summary>
        [HttpPut("admin/users/{id:int}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string role)
        {
            _logger.LogInformation("Incoming admin role update for user {UserId}", id);
            await _authService.UpdateUserRoleAsync(id, role);
            return Ok(ApiResponse<object>.Ok(null, "Role updated."));
        }

        /// <summary>
        /// Updates a user's premium flag.
        /// </summary>
        [HttpPut("admin/users/{id:int}/premium")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserPremium(int id, [FromBody] bool isPremium)
        {
            _logger.LogInformation("Incoming admin premium update for user {UserId}", id);
            await _authService.UpdateUserPremiumAsync(id, isPremium);
            return Ok(ApiResponse<object>.Ok(null, "Premium status updated."));
        }

        /// <summary>
        /// Deactivates a user account.
        /// </summary>
        [HttpPut("admin/users/{id:int}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            _logger.LogInformation("Incoming deactivate request for user {UserId}", id);
            await _authService.DeactivateUserAsync(id);
            return Ok(ApiResponse<object>.Ok(null, "User deactivated."));
        }

        /// <summary>
        /// Reactivates a user account.
        /// </summary>
        [HttpPut("admin/users/{id:int}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            _logger.LogInformation("Incoming reactivate request for user {UserId}", id);
            await _authService.ReactivateUserAsync(id);
            return Ok(ApiResponse<object>.Ok(null, "User reactivated."));
        }

        /// <summary>
        /// Internal endpoint used by SubscriptionService to update premium state.
        /// </summary>
        [HttpPut("internal/users/{id:int}/premium")]
        [AllowAnonymous]
        public async Task<IActionResult> InternalUpdatePremium(int id, [FromBody] InternalPremiumUpdateRequest request)
        {
            _logger.LogInformation("Incoming internal premium update for user {UserId}", id);

            if (!Request.Headers.TryGetValue("X-Internal-Key", out var apiKey) || apiKey != _configuration["InternalApiKey"])
            {
                _logger.LogWarning("Invalid internal key used for premium update on user {UserId}", id);
                return Unauthorized(new ApiErrorResponse { Message = "Invalid internal key." });
            }

            await _authService.UpdateUserPremiumAsync(id, request.IsPremium);
            return Ok(ApiResponse<object>.Ok(null, "Premium status updated via internal call."));
        }
    }
}
