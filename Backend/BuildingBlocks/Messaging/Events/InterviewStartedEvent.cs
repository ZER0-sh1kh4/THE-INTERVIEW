namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised when an interview is created.
/// </summary>
public class InterviewStartedEvent : IntegrationEvent
{
    public int UserId { get; init; }
    public string Domain { get; init; } = string.Empty;
    public int InterviewId { get; init; }
}
