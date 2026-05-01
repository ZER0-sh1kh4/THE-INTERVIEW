using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

/// <summary>
/// Raised when request validation fails.
/// </summary>
public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message, string? details = null)
        : base(message, StatusCodes.Status400BadRequest, details)
    {
    }
}
