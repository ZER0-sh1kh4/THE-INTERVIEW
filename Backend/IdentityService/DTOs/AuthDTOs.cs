using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(2)]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordWithOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$")]
        public string Otp { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ForgotPasswordOtpDispatch
    {
        public bool ShouldSendEmail { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class InternalPremiumUpdateRequest
    {
        [Required]
        public bool IsPremium { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}
