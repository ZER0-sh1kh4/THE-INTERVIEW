namespace BuildingBlocks.Messaging.Events;

/// <summary>
/// Requests an email to be sent by the notification worker.
/// </summary>
public class EmailRequestedEvent : IntegrationEvent
{
    public string ToEmail { get; init; } = string.Empty;
    public string ToName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public Dictionary<string, string> Model { get; init; } = [];
}
