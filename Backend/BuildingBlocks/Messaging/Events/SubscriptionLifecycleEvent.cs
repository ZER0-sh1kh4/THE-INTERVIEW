namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised when premium subscription state changes and must be propagated.
/// </summary>
public class SubscriptionLifecycleEvent : IntegrationEvent
{
    public Guid SagaId { get; init; } = Guid.NewGuid();
    public int UserId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
