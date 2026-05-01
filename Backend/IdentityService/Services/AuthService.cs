using BuildingBlocks.Exceptions;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services
{
    /// <summary>
        /// Implements authentication, token generation and admin user management.
        /// </summary>
        public class AuthService : IAuthService
        {
            private readonly AppDbContext _context;
            private readonly IConfiguration _config;
            private readonly ILogger<AuthService> _logger;
            private readonly IMemoryCache _memoryCache;

            public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger, IMemoryCache memoryCache)
            {
                _context = context;
                _config = config;
                _logger = logger;
                _memoryCache = memoryCache;
            }

        /// <summary>
        /// Registers a new user and returns a JWT for immediate login.
        /// </summary>
            public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    existingUser.FullName = request.FullName;
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    existingUser.IsActive = true;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Existing user {Email} was updated through registration flow", existingUser.Email);

                    var existingToken = GenerateJwtToken(existingUser);
                    return new AuthResponse
                    {
                        UserId = existingUser.Id,
                        Token = existingToken,
                        Message = "Existing user updated successfully"
                    };
                }

                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "Candidate",
                    IsPremium = false,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Email} registered successfully", user.Email);

                var token = GenerateJwtToken(user);
                return new AuthResponse { UserId = user.Id, Token = token, Message = "Registration successful" };
            }

        /// <summary>
        /// Authenticates an existing user and returns a JWT token.
        /// </summary>
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    throw new AppException("Invalid credentials.", StatusCodes.Status401Unauthorized);
                }

                if (!user.IsActive)
                {
                    throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
                }

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                var token = GenerateJwtToken(user);
                return new AuthResponse { UserId = user.Id, Token = token, Message = "Login successful" };
            }

        /// <summary>
        /// Issues a short-lived OTP for password reset without revealing whether an email exists.
        /// </summary>
        public async Task<ForgotPasswordOtpDispatch> SendForgotPasswordOtpAsync(ForgotPasswordOtpRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            var expiryMinutes = GetOtpExpiryMinutes();
            const string genericMessage = "If the account exists, an OTP has been sent to the registered email address.";

            if (user is null)
            {
                _logger.LogInformation("Forgot-password OTP requested for non-existing or inactive email {Email}", request.Email);
                return new ForgotPasswordOtpDispatch
                {
                    ShouldSendEmail = false,
                    Message = genericMessage,
                    ExpiryMinutes = expiryMinutes
                };
            }

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var cacheKey = GetPasswordResetCacheKey(user.Email);

            _memoryCache.Set(
                cacheKey,
                new PasswordResetOtpCacheEntry
                {
                    OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes)
                },
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes)
                });

            _logger.LogInformation("Forgot-password OTP issued for {Email}", user.Email);

            return new ForgotPasswordOtpDispatch
            {
                ShouldSendEmail = true,
                ToEmail = user.Email,
                ToName = user.FullName,
                Otp = otp,
                ExpiryMinutes = expiryMinutes,
                Message = genericMessage
            };
        }

        /// <summary>
        /// Resets the user's password after a valid OTP check.
        /// </summary>
        public async Task ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            var cacheKey = GetPasswordResetCacheKey(request.Email);

            if (user is null || !_memoryCache.TryGetValue<PasswordResetOtpCacheEntry>(cacheKey, out var otpEntry) || otpEntry is null)
            {
                throw new AppException("Invalid or expired OTP.", StatusCodes.Status400BadRequest);
            }

            if (otpEntry.ExpiresAtUtc <= DateTime.UtcNow || !BCrypt.Net.BCrypt.Verify(request.Otp, otpEntry.OtpHash))
            {
                throw new AppException("Invalid or expired OTP.", StatusCodes.Status400BadRequest);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            _memoryCache.Remove(cacheKey);

            _logger.LogInformation("Password reset completed for {Email}", user.Email);
        }

        /// <summary>
        /// Refreshes JWT claims after profile changes such as premium updates.
        /// </summary>
        public async Task<AuthResponse> RefreshClaimsAsync(int id)
        {
                var user = await _context.Users.FindAsync(id);
                if (user == null || !user.IsActive)
                {
                    throw new AppException("User not found or inactive.", StatusCodes.Status401Unauthorized);
                }

                var token = GenerateJwtToken(user);
                return new AuthResponse { UserId = user.Id, Token = token, Message = "Token refreshed successfully" };
            }

        /// <summary>
        /// Gets one user by primary key.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Returns all users for admin review.
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Updates a user's role.
        /// </summary>
            public async Task<bool> UpdateUserRoleAsync(int id, string role)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) throw new NotFoundAppException("User not found.");

                user.Role = role;
                await _context.SaveChangesAsync();
                return true;
        }

        /// <summary>
        /// Updates premium state for a user.
        /// </summary>
            public async Task<bool> UpdateUserPremiumAsync(int id, bool isPremium)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) throw new NotFoundAppException("User not found.");

                user.IsPremium = isPremium;
                await _context.SaveChangesAsync();
                return true;
        }

        /// <summary>
        /// Deactivates a user account without deleting it.
        /// </summary>
        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundAppException("User not found.");

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Reactivates a previously deactivated user account.
        /// </summary>
        public async Task<bool> ReactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundAppException("User not found.");

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Updates the user's display name (FullName).
        /// </summary>
        public async Task<bool> UpdateProfileAsync(int id, string fullName)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new NotFoundAppException("User not found.");

            user.FullName = fullName.Trim();
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Id} updated profile. New name: {Name}", id, user.FullName);
            return true;
        }

        /// <summary>
        /// Creates a signed JWT token using the current user state.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("isPremium", user.IsPremium.ToString().ToLower()),
                new Claim("FullName", user.FullName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetOtpExpiryMinutes()
        {
            return int.TryParse(_config["PasswordResetOtp:ExpiryMinutes"], out var minutes) && minutes > 0
                ? minutes
                : 10;
        }

        private static string GetPasswordResetCacheKey(string email) =>
            $"password-reset-otp:{email.Trim().ToLowerInvariant()}";

        private sealed class PasswordResetOtpCacheEntry
        {
            public string OtpHash { get; init; } = string.Empty;
            public DateTime ExpiresAtUtc { get; init; }
        }
    }
}
