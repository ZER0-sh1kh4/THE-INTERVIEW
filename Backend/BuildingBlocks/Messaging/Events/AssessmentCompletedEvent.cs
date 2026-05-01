namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised after an assessment is completed.
/// </summary>
public class AssessmentCompletedEvent : IntegrationEvent
{
    public int UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public double Percentage { get; init; }
    public string Grade { get; init; } = string.Empty;
}
