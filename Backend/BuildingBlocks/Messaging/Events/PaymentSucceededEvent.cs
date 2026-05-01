namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Raised when a payment succeeds.
/// </summary>
public class PaymentSucceededEvent : IntegrationEvent
{
    public Guid SagaId { get; init; } = Guid.NewGuid();
    public int UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "INR";
    public string PaymentId { get; init; } = string.Empty;
    public string OrderId { get; init; } = string.Empty;
}
