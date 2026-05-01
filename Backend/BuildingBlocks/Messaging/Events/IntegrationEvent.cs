namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Base event shared by all asynchronous integration messages.
/// </summary>
public abstract class IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
