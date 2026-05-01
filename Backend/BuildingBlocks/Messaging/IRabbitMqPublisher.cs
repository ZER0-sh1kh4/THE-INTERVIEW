namespace BuildingBlocks.Messaging;

/// <summary>
/// Defines message publishing operations shared by all services.
/// </summary>
public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default);
}
