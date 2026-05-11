using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace IdentityService.Messaging;

/// <summary>
/// Stores user-facing bell notifications from domain events.
/// </summary>
public class UserNotificationEventConsumer : BackgroundService
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserNotificationEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public UserNotificationEventConsumer(
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IServiceScopeFactory scopeFactory,
        ILogger<UserNotificationEventConsumer> logger)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

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

        await DeclareQueueWithDeadLetterAsync(QueueNames.InterviewEvents, stoppingToken);
        await DeclareQueueWithDeadLetterAsync(QueueNames.AssessmentEvents, stoppingToken);

        await StartConsumerAsync(QueueNames.InterviewEvents, HandleInterviewEventAsync, stoppingToken);
        await StartConsumerAsync(QueueNames.AssessmentEvents, HandleAssessmentEventAsync, stoppingToken);
    }

    private async Task StartConsumerAsync(
        string queueName,
        Func<string, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                await handler(json, cancellationToken);
                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification from {QueueName}.", queueName);
                await RetryOrDeadLetterAsync(queueName, eventArgs, cancellationToken);
            }
        };

        await _channel!.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
    }

    private async Task HandleInterviewEventAsync(string json, CancellationToken cancellationToken)
    {
        if (!IsEventType(json, nameof(InterviewCompletedEvent)))
        {
            return;
        }

        var message = JsonSerializer.Deserialize<InterviewCompletedEvent>(json);
        if (message is null || message.UserId <= 0)
        {
            return;
        }

        await AddNotificationAsync(new Notification
        {
            UserId = message.UserId,
            Title = "Interview completed",
            Message = $"Your {TrimDomain(message.Domain)} interview result is ready. Grade: {message.Grade} ({message.Percentage:0.#}%).",
            ActionUrl = "/interviews/history",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task HandleAssessmentEventAsync(string json, CancellationToken cancellationToken)
    {
        if (!IsEventType(json, nameof(AssessmentCompletedEvent)))
        {
            return;
        }

        var message = JsonSerializer.Deserialize<AssessmentCompletedEvent>(json);
        if (message is null || message.UserId <= 0)
        {
            return;
        }

        await AddNotificationAsync(new Notification
        {
            UserId = message.UserId,
            Title = "Assessment completed",
            Message = $"Your {TrimDomain(message.Domain)} assessment result is ready. Grade: {message.Grade} ({message.Percentage:0.#}%).",
            ActionUrl = "/assessments/history",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == notification.UserId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("Skipped notification for missing user {UserId}.", notification.UserId);
            return;
        }

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

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

    private async Task RetryOrDeadLetterAsync(string queueName, BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(eventArgs) + 1;

        if (retryCount <= _rabbitMqOptions.RetryCount)
        {
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = new Dictionary<string, object?> { ["x-retry-count"] = retryCount }
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

    private static int GetRetryCount(BasicDeliverEventArgs eventArgs)
    {
        if (eventArgs.BasicProperties.Headers is null ||
            !eventArgs.BasicProperties.Headers.TryGetValue("x-retry-count", out var rawValue) ||
            rawValue is null)
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

    private static string TrimDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return "practice";
        }

        var firstPart = domain.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(firstPart) ? domain : firstPart;
    }

    private static bool IsEventType(string json, string expectedType)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("EventType", out var eventType) &&
                !document.RootElement.TryGetProperty("eventType", out eventType))
            {
                return false;
            }

            return string.Equals(eventType.GetString(), expectedType, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

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
