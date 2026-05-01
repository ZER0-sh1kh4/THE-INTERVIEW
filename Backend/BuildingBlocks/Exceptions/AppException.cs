using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

/// <summary>
/// Base exception for controlled business errors.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = StatusCodes.Status400BadRequest, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public string? Details { get; }
}
