namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised after an interview is completed.
/// </summary>
public class InterviewCompletedEvent : IntegrationEvent
{
    public int UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public double Percentage { get; init; }
    public string Grade { get; init; } = string.Empty;
}
