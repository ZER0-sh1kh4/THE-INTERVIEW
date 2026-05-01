using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SubscriptionService.Data;
using System.Text;
using System.Text.Json;

namespace SubscriptionService.Messaging;

/// <summary>
/// Consumes identity acknowledgements and completes the local saga state.
/// </summary>
public class IdentityResultConsumer : BackgroundService
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdentityResultConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public IdentityResultConsumer(IOptions<RabbitMqOptions> rabbitMqOptions, IServiceScopeFactory scopeFactory, ILogger<IdentityResultConsumer> logger)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Opens RabbitMQ resources and starts listening for saga results.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            Port = _rabbitMqOptions.Port,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await DeclareQueueWithDeadLetterAsync(QueueNames.SubscriptionResults, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) => await HandleDeliveryAsync(eventArgs, stoppingToken);

        await _channel.BasicConsumeAsync(QueueNames.SubscriptionResults, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }

    /// <summary>
    /// Processes one identity result message and retries or dead-letters it on failure.
    /// </summary>
    private async Task HandleDeliveryAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var message = JsonSerializer.Deserialize<SubscriptionLifecycleEvent>(json);
            if (message is null)
            {
                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
                return;
            }

            await HandleMessageAsync(message, cancellationToken);
            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process subscription result event.");
            await RetryOrDeadLetterAsync(QueueNames.SubscriptionResults, eventArgs, cancellationToken);
        }
    }

    /// <summary>
    /// Updates the latest subscription saga state based on the identity result.
    /// </summary>
    private async Task HandleMessageAsync(SubscriptionLifecycleEvent message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var subscription = await dbContext.Subscriptions
            .Where(s => s.UserId == message.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning("Received saga result but no subscription was found for user {UserId}", message.UserId);
            return;
        }

        subscription.SagaState = message.Status == "Completed"
            ? (message.Action == "Activate" ? "Completed" : "CancelledCompleted")
            : "CompensationRequired";

        if (message.Status != "Completed" && message.Action == "Activate")
        {
            subscription.Status = "Failed";
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated subscription saga state for user {UserId} to {SagaState}", message.UserId, subscription.SagaState);
    }

    /// <summary>
    /// Declares the queue and a matching dead-letter queue for failed messages.
    /// </summary>
    private async Task DeclareQueueWithDeadLetterAsync(string queueName, CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = QueueNames.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = QueueNames.DeadLetterQueue(queueName)
        };

        await _channel!.ExchangeDeclareAsync(QueueNames.Exchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(QueueNames.DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(queueName, QueueNames.Exchange, queueName, cancellationToken: cancellationToken);

        var deadLetterQueue = QueueNames.DeadLetterQueue(queueName);
        await _channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(deadLetterQueue, QueueNames.DeadLetterExchange, deadLetterQueue, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retries a failed message a limited number of times before sending it to the dead-letter queue.
    /// </summary>
    private async Task RetryOrDeadLetterAsync(string queueName, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(eventArgs) + 1;

        if (retryCount <= _rabbitMqOptions.RetryCount)
        {
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = new Dictionary<string, object?>
                {
                    ["x-retry-count"] = retryCount
                }
            };

            await Task.Delay(TimeSpan.FromSeconds(_rabbitMqOptions.RetryDelaySeconds), cancellationToken);
            await _channel!.BasicPublishAsync(QueueNames.Exchange, queueName, false, properties, eventArgs.Body, cancellationToken);
        }
        else
        {
            var deadLetterProperties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await _channel!.BasicPublishAsync(
                QueueNames.DeadLetterExchange,
                QueueNames.DeadLetterQueue(queueName),
                false,
                deadLetterProperties,
                eventArgs.Body,
                cancellationToken);
        }

        await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
    }

    /// <summary>
    /// Reads the current retry count header from the RabbitMQ message.
    /// </summary>
    private static int GetRetryCount(BasicDeliverEventArgs eventArgs)
    {
        if (eventArgs.BasicProperties.Headers is null || !eventArgs.BasicProperties.Headers.TryGetValue("x-retry-count", out var rawValue) || rawValue is null)
        {
            return 0;
        }

        return rawValue switch
        {
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
            int intValue => intValue,
            long longValue => (int)longValue,
            _ => 0
        };
    }

    /// <summary>
    /// Releases RabbitMQ resources when the host stops.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
