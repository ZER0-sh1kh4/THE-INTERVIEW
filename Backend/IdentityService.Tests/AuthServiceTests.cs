using FluentAssertions;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IdentityService.Tests;

/// <summary>
/// Covers the main authentication service behaviors without spinning up the full API host.
/// </summary>
public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_NewUser_CreatesUserAndReturnsToken()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var request = new RegisterRequest
        {
            FullName = "Test Candidate",
            Email = "candidate@test.com",
            Password = "Pass@123"
        };

        var response = await service.RegisterAsync(request);

        response.UserId.Should().BeGreaterThan(0);
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.Message.Should().Be("Registration successful");

        var savedUser = await context.Users.SingleAsync(x => x.Email == request.Email);
        savedUser.FullName.Should().Be(request.FullName);
        savedUser.Role.Should().Be("Candidate");
        savedUser.IsPremium.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_UpdatesExistingUser()
    {
        await using var context = CreateContext();
        var existing = new User
        {
            FullName = "Old Name",
            Email = "existing@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Old@123"),
            IsActive = false
        };

        context.Users.Add(existing);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var response = await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Updated Name",
            Email = existing.Email,
            Password = "New@12345"
        });

        response.Message.Should().Be("Existing user updated successfully");

        var updatedUser = await context.Users.SingleAsync(x => x.Email == existing.Email);
        updatedUser.FullName.Should().Be("Updated Name");
        updatedUser.IsActive.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("New@12345", updatedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedError()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            FullName = "Candidate",
            Email = "candidate@login.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct@123"),
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var act = () => service.LoginAsync(new LoginRequest
        {
            Email = "candidate@login.com",
            Password = "Wrong@123"
        });

        var exception = await act.Should().ThrowAsync<BuildingBlocks.Exceptions.AppException>();
        exception.Which.StatusCode.Should().Be(401);
        exception.Which.Message.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task RefreshClaimsAsync_InactiveUser_ThrowsUnauthorizedError()
    {
        await using var context = CreateContext();
        var user = new User
        {
            FullName = "Inactive Candidate",
            Email = "inactive@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
            IsActive = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var act = () => service.RefreshClaimsAsync(user.Id);

        var exception = await act.Should().ThrowAsync<BuildingBlocks.Exceptions.AppException>();
        exception.Which.StatusCode.Should().Be(401);
        exception.Which.Message.Should().Be("User not found or inactive.");
    }

    [Fact]
    public async Task SendForgotPasswordOtpAsync_ExistingUser_ReturnsDispatchForEmail()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            FullName = "Otp Candidate",
            Email = "otp@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var response = await service.SendForgotPasswordOtpAsync(new ForgotPasswordOtpRequest
        {
            Email = "otp@test.com"
        });

        response.ShouldSendEmail.Should().BeTrue();
        response.ToEmail.Should().Be("otp@test.com");
        response.ToName.Should().Be("Otp Candidate");
        response.Otp.Should().MatchRegex("^\\d{6}$");
        response.Message.Should().Be("If the account exists, an OTP has been sent to the registered email address.");
    }

    [Fact]
    public async Task ResetPasswordWithOtpAsync_ValidOtp_UpdatesPasswordAndClearsOtp()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            FullName = "Reset Candidate",
            Email = "reset@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Old@123"),
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dispatch = await service.SendForgotPasswordOtpAsync(new ForgotPasswordOtpRequest
        {
            Email = "reset@test.com"
        });

        await service.ResetPasswordWithOtpAsync(new ResetPasswordWithOtpRequest
        {
            Email = "reset@test.com",
            Otp = dispatch.Otp,
            NewPassword = "New@12345"
        });

        var user = await context.Users.SingleAsync(x => x.Email == "reset@test.com");
        BCrypt.Net.BCrypt.Verify("New@12345", user.PasswordHash).Should().BeTrue();

        var act = () => service.ResetPasswordWithOtpAsync(new ResetPasswordWithOtpRequest
        {
            Email = "reset@test.com",
            Otp = dispatch.Otp,
            NewPassword = "Another@123"
        });

        var exception = await act.Should().ThrowAsync<BuildingBlocks.Exceptions.AppException>();
        exception.Which.StatusCode.Should().Be(400);
        exception.Which.Message.Should().Be("Invalid or expired OTP.");
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

    private static AuthService CreateService(AppDbContext context)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "super-secret-key-for-tests-only-1234567890",
                ["Jwt:Issuer"] = "MockInterview.Tests",
                ["Jwt:Audience"] = "MockInterview.Tests.Users",
                ["Jwt:ExpiryMinutes"] = "60",
                ["PasswordResetOtp:ExpiryMinutes"] = "10"
            })
            .Build();

        return new AuthService(context, config, NullLogger<AuthService>.Instance, new MemoryCache(new MemoryCacheOptions()));
    }
}
