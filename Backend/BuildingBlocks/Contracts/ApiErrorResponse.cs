namespace BuildingBlocks.Contracts;

/// <summary>
/// Standard error response contract used by all APIs.
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
}
