namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised after a user registers successfully.
/// </summary>
public class UserRegisteredEvent : IntegrationEvent
{
    public Guid SagaId { get; init; } = Guid.NewGuid();
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
