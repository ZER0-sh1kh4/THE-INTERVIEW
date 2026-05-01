using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent API error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Executes the next middleware and formats any thrown exception.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Handled application exception for {Path}", context.Request.Path);
            await WriteErrorAsync(context, ex.StatusCode, ex.Message, ex.Details);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access for {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized access.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "Internal server error.", ex.Message);
        }
    }

    /// <summary>
    /// Writes the standard API error payload to the response body.
    /// </summary>
    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message, string? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ApiErrorResponse
        {
            Message = message,
            Details = details
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
