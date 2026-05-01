namespace BuildingBlocks.Messaging;

/// <summary>
/// Strongly typed RabbitMQ settings shared by all services.
/// </summary>
public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 15;
}
