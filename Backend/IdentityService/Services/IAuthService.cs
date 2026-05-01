using IdentityService.DTOs;
using IdentityService.Models;

namespace IdentityService.Services
{
    /// <summary>
    /// Defines the user authentication and administration operations.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<ForgotPasswordOtpDispatch> SendForgotPasswordOtpAsync(ForgotPasswordOtpRequest request);
        Task ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequest request);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(int id, string role);
        Task<bool> UpdateUserPremiumAsync(int id, bool isPremium);
        Task<bool> DeactivateUserAsync(int id);
        Task<bool> ReactivateUserAsync(int id);
        Task<AuthResponse> RefreshClaimsAsync(int id);
        Task<bool> UpdateProfileAsync(int id, string fullName);
    }
}
