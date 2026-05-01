using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

/// <summary>
/// Raised when a caller does not have permission to perform an action.
/// </summary>
public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message)
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}
