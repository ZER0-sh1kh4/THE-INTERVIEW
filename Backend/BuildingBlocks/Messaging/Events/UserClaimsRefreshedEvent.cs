namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised when a user refreshes their claims.
/// </summary>
public class UserClaimsRefreshedEvent : IntegrationEvent
{
    public int UserId { get; init; }
}
