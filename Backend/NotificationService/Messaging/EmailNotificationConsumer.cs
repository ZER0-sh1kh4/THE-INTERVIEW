using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Services;

namespace NotificationService.Messaging;

/// <summary>
/// Consumes email requests from RabbitMQ and sends emails through MailKit.
/// </summary>
public class EmailNotificationConsumer : BackgroundService
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailNotificationConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public EmailNotificationConsumer(IOptions<RabbitMqOptions> rabbitMqOptions, IServiceScopeFactory scopeFactory, ILogger<EmailNotificationConsumer> logger)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates RabbitMQ topology and begins consuming email messages.
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

        await DeclareQueueWithDeadLetterAsync(QueueNames.EmailNotifications, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            await HandleMessageAsync(eventArgs, stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueNames.EmailNotifications, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }

    /// <summary>
    /// Processes one email request and acknowledges it when finished.
    /// </summary>
    private async Task HandleMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var emailEvent = JsonSerializer.Deserialize<EmailRequestedEvent>(json);
            if (emailEvent is null)
            {
                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var renderer = scope.ServiceProvider.GetRequiredService<IHtmlEmailTemplateRenderer>();

            var htmlBody = renderer.Render(emailEvent.TemplateKey, emailEvent.Model);
            await sender.SendAsync(emailEvent.ToEmail, emailEvent.ToName, emailEvent.Subject, htmlBody, cancellationToken);

            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email message.");
            await RetryOrDeadLetterAsync(QueueNames.EmailNotifications, eventArgs, cancellationToken);
        }
    }

    /// <summary>
    /// Declares the queue and a matching dead-letter queue for failed email messages.
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
    /// Retries a failed email event a limited number of times before moving it to the dead-letter queue.
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
    /// Disposes RabbitMQ resources when the worker stops.
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
