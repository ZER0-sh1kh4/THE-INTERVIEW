namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised when an assessment is created.
/// </summary>
public class AssessmentStartedEvent : IntegrationEvent
{
    public int UserId { get; init; }
    public string Domain { get; init; } = string.Empty;
    public int AssessmentId { get; init; }
}
