# Topic 12: RabbitMQ and Event-Driven Architecture

Project: Mock Interview Platform  
Focus: Understanding RabbitMQ, message brokers, queues, exchanges, routing keys, producers, consumers, asynchronous communication, event-driven design, retries, dead-letter queues, and how services communicate through events.

---

## 1. Why RabbitMQ Is Needed

### Simple Explanation

Some backend work should not block the main API response.

Example:

```text
User registers
Backend should create account quickly
Welcome email can be sent in background
```

If email sending is done directly inside registration API, user may wait longer.

RabbitMQ helps by placing background work into a queue.

### Practical Scenario

When user registers:

```text
1. IdentityService creates user.
2. IdentityService returns response to frontend.
3. IdentityService publishes email event to RabbitMQ.
4. NotificationService consumes event later.
5. NotificationService sends email.
```

The user does not wait for email sending.

### Viva Answer

> RabbitMQ is needed for asynchronous communication between services. It allows one service to publish events and another service to process them in the background without blocking the main API response.

---

## 2. What Is RabbitMQ?

### Simple Explanation

RabbitMQ is a message broker.

It receives messages from one service and delivers them to another service.

### Real-Life Analogy

Think of RabbitMQ like a post office.

```text
Producer writes message
RabbitMQ stores/routes message
Consumer receives message
```

### In Your Project

RabbitMQ connects:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
NotificationService
```

### Viva Answer

> RabbitMQ is a message broker used to send messages between backend services asynchronously.

---

## 3. What Is Message Broker?

### Simple Explanation

A message broker is middleware that transfers messages between services.

Instead of service A calling service B directly, service A sends a message to broker.

### Direct Call

```text
IdentityService -> NotificationService
```

### Broker-Based Call

```text
IdentityService -> RabbitMQ -> NotificationService
```

### Why Broker Helps

If NotificationService is temporarily down, RabbitMQ can keep the message in queue until consumer is available.

### Viva Answer

> A message broker is a system that receives, stores, routes, and delivers messages between services. RabbitMQ is the broker used in my project.

---

## 4. What Is Event-Driven Architecture?

### Simple Explanation

Event-driven architecture means services communicate by publishing and consuming events.

An event represents something that already happened.

Examples:

```text
UserRegisteredEvent
InterviewCompletedEvent
AssessmentCompletedEvent
PaymentSucceededEvent
SubscriptionLifecycleEvent
```

### Important Idea

Producer does not need to know exactly who consumes the event.

It only publishes:

```text
Something happened.
```

Interested consumers react.

### Viva Answer

> Event-driven architecture is a design where services communicate using events. One service publishes an event when something happens, and other services consume it asynchronously.

---

## 5. Direct HTTP vs Messaging

### Direct HTTP

Service calls another service immediately.

```text
SubscriptionService -> HTTP call -> IdentityService
```

### Messaging

Service publishes event.

```text
SubscriptionService -> RabbitMQ -> IdentityService
```

### Comparison

| Feature | Direct HTTP | RabbitMQ Messaging |
|---|---|---|
| Communication | Immediate | Asynchronous |
| Sender waits? | Yes | No |
| Receiver must be online? | Usually yes | Not always |
| Best for | Immediate data/query | Background processing/events |
| Coupling | Higher | Lower |

### Viva Answer

> Direct HTTP is synchronous and the caller waits for response. RabbitMQ messaging is asynchronous and useful for background work like emails, notifications, and subscription lifecycle events.

---

## 6. Producer

### Simple Explanation

Producer is the service that sends message to RabbitMQ.

### In Your Project

Producers include:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Example

IdentityService publishes event after registration:

```csharp
await _rabbitMqPublisher.PublishAsync(
    QueueNames.UserRegistration,
    new UserRegisteredEvent { ... });
```

### Viva Answer

> Producer is the service that publishes messages to RabbitMQ. In my project, services like IdentityService and SubscriptionService act as producers.

---

## 7. Consumer

### Simple Explanation

Consumer is the service that reads messages from RabbitMQ and processes them.

### In Your Project

Consumers:

```text
EmailNotificationConsumer
SubscriptionEventConsumer
UserNotificationEventConsumer
IdentityResultConsumer
```

### Example

NotificationService consumes email requests:

```text
EmailRequestedEvent -> EmailNotificationConsumer -> MailKitEmailSender
```

### Viva Answer

> Consumer is the service or background worker that receives messages from RabbitMQ and performs work based on them.

---

## 8. Queue

### Simple Explanation

A queue stores messages until a consumer processes them.

Messages wait in queue.

### In Your Project

Queue names:

```text
notifications.email.v2
identity.user.registration.v2
subscription.lifecycle.v2
subscription.results.v2
payment.events.v2
assessment.events.v2
interview.events.v2
```

### File

```text
Backend/BuildingBlocks/Messaging/QueueNames.cs
```

### Viva Answer

> A queue stores messages until a consumer receives and processes them. My project defines queue names centrally in QueueNames.cs.

---

## 9. Exchange

### Simple Explanation

An exchange receives messages from producers and routes them to queues.

Producer publishes to exchange.

Exchange routes message based on routing key.

### In Your Project

Main exchange:

```csharp
public const string Exchange = "mockinterview.events.v2";
```

Dead-letter exchange:

```csharp
public const string DeadLetterExchange = "mockinterview.events.v2.dlx";
```

### Viva Answer

> An exchange receives messages and routes them to queues. My project uses a direct exchange named mockinterview.events.v2.

---

## 10. Direct Exchange

### Simple Explanation

A direct exchange routes messages by exact routing key match.

### Example

Message is published with routing key:

```text
notifications.email.v2
```

RabbitMQ sends it to queue bound with same routing key:

```text
notifications.email.v2
```

### In Your Project

Publisher declares direct exchange:

```csharp
await _channel.ExchangeDeclareAsync(
    exchange: QueueNames.Exchange,
    type: ExchangeType.Direct,
    durable: true,
    autoDelete: false);
```

### Viva Answer

> A direct exchange routes messages to queues using exact routing key matching. My project uses a direct exchange for service event queues.

---

## 11. Routing Key

### Simple Explanation

Routing key tells RabbitMQ where the message should go.

### In Your Project

The routing key is usually same as queue name.

Example:

```csharp
await _rabbitMqPublisher.PublishAsync(
    QueueNames.EmailNotifications,
    new EmailRequestedEvent { ... });
```

Routing key:

```text
notifications.email.v2
```

### Viva Answer

> Routing key is used by the exchange to decide which queue should receive the message. In my project, queue names are used as routing keys.

---

## 12. Publish/Subscribe Concept

### Simple Explanation

Publish/subscribe means one service publishes a message and one or more consumers can react.

### In Your Project

AssessmentService publishes:

```text
AssessmentCompletedEvent
```

Consumers can:

```text
Create bell notification
Send email notification
```

### Important Note

Your project mainly uses direct routing queues, but the design idea is event-driven publish/consume.

### Viva Answer

> Publish/subscribe means services publish events without directly depending on consumers. Consumers subscribe to relevant queues and process events.

---

## 13. Integration Event

### Simple Explanation

Integration event is a message shared between services.

It usually represents something important that happened in one service.

### File

```text
Backend/BuildingBlocks/Messaging/Events/IntegrationEvent.cs
```

### Code

```csharp
public abstract class IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
```

### Viva Answer

> IntegrationEvent is the base class for messages shared across services. It includes EventId, OccurredOnUtc, and EventType.

---

## 14. EventId

### Simple Explanation

`EventId` uniquely identifies an event.

### Why Useful

It helps with:

```text
Tracing messages
Debugging
Duplicate detection
Idempotency design
```

### Example

```csharp
public Guid EventId { get; init; } = Guid.NewGuid();
```

### Viva Answer

> EventId uniquely identifies each integration event and helps trace or detect duplicate messages.

---

## 15. OccurredOnUtc

### Simple Explanation

`OccurredOnUtc` stores when event was created.

### Why UTC

UTC avoids timezone confusion between services.

### Example

```csharp
public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
```

### Viva Answer

> OccurredOnUtc records when the event occurred using UTC time, which is safer for distributed systems.

---

## 16. RabbitMqOptions

### File

```text
Backend/BuildingBlocks/Messaging/RabbitMqOptions.cs
```

### Code

```csharp
public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 15;
}
```

### Meaning

RabbitMQ configuration includes:

```text
Host
Port
Username
Password
Retry count
Retry delay
```

### Viva Answer

> RabbitMqOptions stores RabbitMQ connection and retry settings as strongly typed configuration.

---

## 17. Registering RabbitMQ in DI

### File

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Code

```csharp
public static IServiceCollection AddRabbitMqMessaging(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
    services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
    return services;
}
```

### Used In

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Viva Answer

> RabbitMQ is registered using AddRabbitMqMessaging. It binds RabbitMqOptions from configuration and registers IRabbitMqPublisher as a singleton.

---

## 18. IRabbitMqPublisher

### File

```text
Backend/BuildingBlocks/Messaging/IRabbitMqPublisher.cs
```

### Code

```csharp
public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(
        string routingKey,
        T message,
        CancellationToken cancellationToken = default);
}
```

### Purpose

It defines a common message publishing contract.

Controllers and services depend on the interface, not directly on RabbitMQ implementation.

### Viva Answer

> IRabbitMqPublisher is the abstraction used by services to publish events without depending directly on RabbitMQ implementation.

---

## 19. RabbitMqPublisher

### File

```text
Backend/BuildingBlocks/Messaging/RabbitMqPublisher.cs
```

### Purpose

It serializes event object to JSON and publishes it to RabbitMQ.

### Important Code

```csharp
var payload = JsonSerializer.Serialize(message);
var body = Encoding.UTF8.GetBytes(payload);
```

Then:

```csharp
await _channel.BasicPublishAsync(
    exchange: QueueNames.Exchange,
    routingKey: routingKey,
    mandatory: false,
    basicProperties: properties,
    body: body);
```

### Viva Answer

> RabbitMqPublisher serializes integration events to JSON and publishes them to the configured RabbitMQ direct exchange using a routing key.

---

## 20. Persistent Messages

### Simple Explanation

Persistent messages are marked to survive broker restarts when queues/exchanges are durable.

### In Your Project

Publisher sets:

```csharp
var properties = new BasicProperties
{
    Persistent = true,
    ContentType = "application/json",
    Type = typeof(T).Name
};
```

### Why Useful

Email or subscription events should not disappear easily if RabbitMQ restarts.

### Viva Answer

> Persistent messages tell RabbitMQ to store messages more durably. My publisher marks messages as persistent and declares durable exchanges/queues.

---

## 21. Durable Exchange and Queue

### Simple Explanation

Durable means RabbitMQ keeps exchange/queue metadata after broker restart.

### In Your Project

Exchange:

```csharp
await _channel.ExchangeDeclareAsync(
    QueueNames.Exchange,
    ExchangeType.Direct,
    durable: true,
    autoDelete: false);
```

Queue:

```csharp
await _channel.QueueDeclareAsync(
    queueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: arguments);
```

### Viva Answer

> Durable exchanges and queues survive RabbitMQ restarts. My project declares RabbitMQ topology as durable.

---

## 22. Message Acknowledgement

### Simple Explanation

Acknowledgement tells RabbitMQ that message was processed successfully.

### In Your Project

Consumers use:

```csharp
await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
```

They also use:

```csharp
autoAck: false
```

### Meaning

RabbitMQ should not remove the message automatically.

The consumer removes it only after successful processing or after republishing it for retry/dead-letter.

### Viva Answer

> Message acknowledgement confirms successful processing. My consumers use manual acknowledgement with autoAck false for safer processing.

---

## 23. BackgroundService Consumers

### Simple Explanation

Consumers run in the background, continuously listening to RabbitMQ.

### In Your Project

Consumers inherit:

```csharp
BackgroundService
```

Examples:

```text
EmailNotificationConsumer
SubscriptionEventConsumer
UserNotificationEventConsumer
IdentityResultConsumer
```

### Registration

```csharp
builder.Services.AddHostedService<EmailNotificationConsumer>();
```

### Viva Answer

> RabbitMQ consumers are implemented as BackgroundService hosted workers. They start with the application and continuously listen to queues.

---

## 24. EmailNotificationConsumer

### File

```text
Backend/NotificationService/Messaging/EmailNotificationConsumer.cs
```

### Purpose

Consumes:

```text
EmailRequestedEvent
```

From queue:

```text
notifications.email.v2
```

### Flow

```text
1. Reads JSON message.
2. Deserializes EmailRequestedEvent.
3. Creates DI scope.
4. Gets IEmailSender and IHtmlEmailTemplateRenderer.
5. Renders email template.
6. Sends email using MailKit.
7. Acknowledges message.
```

### Viva Answer

> EmailNotificationConsumer listens to the email notification queue and sends emails asynchronously using MailKit.

---

## 25. SubscriptionEventConsumer

### File

```text
Backend/IdentityService/Messaging/SubscriptionEventConsumer.cs
```

### Purpose

Consumes:

```text
SubscriptionLifecycleEvent
```

From queue:

```text
subscription.lifecycle.v2
```

### What It Does

It updates user's premium status in IdentityService.

Actions:

```text
Activate
Cancel
```

Then publishes result back to:

```text
subscription.results.v2
```

### Viva Answer

> SubscriptionEventConsumer listens for subscription lifecycle events and updates the user's premium status in IdentityService.

---

## 26. IdentityResultConsumer

### File

```text
Backend/SubscriptionService/Messaging/IdentityResultConsumer.cs
```

### Purpose

Consumes identity result messages from:

```text
subscription.results.v2
```

### What It Does

It updates subscription saga state:

```text
Completed
CancelledCompleted
CompensationRequired
```

### Viva Answer

> IdentityResultConsumer listens for premium update results from IdentityService and updates the subscription saga state in SubscriptionService.

---

## 27. UserNotificationEventConsumer

### File

```text
Backend/IdentityService/Messaging/UserNotificationEventConsumer.cs
```

### Purpose

Consumes:

```text
InterviewCompletedEvent
AssessmentCompletedEvent
```

From queues:

```text
interview.events.v2
assessment.events.v2
```

### What It Does

Creates in-app bell notifications in IdentityService database.

Examples:

```text
Interview completed notification
Assessment completed notification
```

### Viva Answer

> UserNotificationEventConsumer consumes interview and assessment completion events and stores user-facing notifications.

---

## 28. QueueNames

### File

```text
Backend/BuildingBlocks/Messaging/QueueNames.cs
```

### Purpose

It centralizes all RabbitMQ queue and exchange names.

### Code

```csharp
public const string Exchange = "mockinterview.events.v2";
public const string DeadLetterExchange = "mockinterview.events.v2.dlx";
public const string EmailNotifications = "notifications.email.v2";
public const string UserRegistration = "identity.user.registration.v2";
public const string SubscriptionLifecycle = "subscription.lifecycle.v2";
public const string SubscriptionResults = "subscription.results.v2";
public const string PaymentEvents = "payment.events.v2";
public const string AssessmentEvents = "assessment.events.v2";
public const string InterviewEvents = "interview.events.v2";
```

### Why Useful

Avoids hardcoding queue names in many files.

### Viva Answer

> QueueNames keeps exchange and queue names in one shared place, reducing hardcoded strings and routing mistakes.

---

## 29. EmailRequestedEvent

### Purpose

Used when any service wants NotificationService to send an email.

### Fields

```text
ToEmail
ToName
Subject
TemplateKey
Model
```

### Used For

```text
Welcome email
Forgot password OTP email
Payment success email
Subscription upgrade email
Interview completion email
Assessment completion email
```

### Viva Answer

> EmailRequestedEvent is published when a service wants NotificationService to send an email asynchronously.

---

## 30. UserRegisteredEvent

### Purpose

Published after user registration.

### Fields

```text
SagaId
UserId
FullName
Email
```

### In Your Project

AuthController publishes it after successful register.

### Viva Answer

> UserRegisteredEvent is published after a new user registers and can be used by other services for registration-related workflows.

---

## 31. Interview Events

### Events

```text
InterviewStartedEvent
InterviewCompletedEvent
```

### Published By

```text
InterviewService
```

### Used For

```text
Tracking interview lifecycle
Creating notifications
Sending completion emails
```

### Viva Answer

> InterviewService publishes interview started and completed events so other services can react asynchronously, such as creating notifications or sending emails.

---

## 32. Assessment Events

### Events

```text
AssessmentStartedEvent
AssessmentCompletedEvent
```

### Published By

```text
AssessmentService
```

### Used For

```text
Tracking assessment lifecycle
Creating notifications
Sending result emails
```

### Viva Answer

> AssessmentService publishes assessment events to notify other parts of the system when an assessment starts or completes.

---

## 33. PaymentSucceededEvent

### Purpose

Published when payment succeeds.

### Fields

```text
SagaId
UserId
Amount
Currency
PaymentId
OrderId
```

### Used For

```text
Payment success notification
Subscription lifecycle tracking
Audit/debugging
```

### Viva Answer

> PaymentSucceededEvent represents a successful payment and allows other services to react asynchronously.

---

## 34. SubscriptionLifecycleEvent

### Purpose

Used to activate or cancel premium status across services.

### Fields

```text
SagaId
UserId
Action
Status
Message
```

### Flow

```text
SubscriptionService publishes Activate/Cancel
IdentityService consumes and updates IsPremium
IdentityService publishes Completed/Failed result
SubscriptionService consumes result
```

### Viva Answer

> SubscriptionLifecycleEvent coordinates premium activation or cancellation between SubscriptionService and IdentityService.

---

## 35. Saga Pattern Basics

### Simple Explanation

Saga pattern coordinates a multi-step process across services.

Each service completes its own local transaction and publishes the next event.

### In Your Project

Premium activation is saga-style:

```text
1. Payment succeeds in SubscriptionService.
2. SubscriptionService marks subscription/payment locally.
3. SubscriptionService publishes SubscriptionLifecycleEvent.
4. IdentityService updates user IsPremium.
5. IdentityService publishes result event.
6. SubscriptionService updates SagaState.
```

### Why Saga Is Useful

There is no single database transaction across services.

Events coordinate the workflow.

### Viva Answer

> Saga pattern manages multi-service workflows using local transactions and events. My project uses a saga-style flow for premium activation.

---

## 36. Dead-Letter Queue

### Simple Explanation

Dead-letter queue stores messages that failed repeatedly.

### In Your Project

Dead-letter queue name:

```csharp
public static string DeadLetterQueue(string queueName) => $"{queueName}.dead";
```

Example:

```text
notifications.email.v2.dead
subscription.lifecycle.v2.dead
```

### Why Useful

Failed messages are not lost.

Developers can inspect them later.

### Viva Answer

> Dead-letter queue stores messages that could not be processed after retries. It helps avoid message loss and supports debugging.

---

## 37. Retry Handling

### Simple Explanation

If message processing fails, consumer retries before dead-lettering.

### In Your Project

Retry config:

```text
RetryCount = 3
RetryDelaySeconds = 15
```

Retry count is stored in message header:

```csharp
Headers = new Dictionary<string, object?>
{
    ["x-retry-count"] = retryCount
}
```

### Flow

```text
1. Consumer fails to process message.
2. Logs error.
3. Increments x-retry-count.
4. Waits RetryDelaySeconds.
5. Republishes message.
6. After max retries, publishes to dead-letter exchange.
7. Acknowledges original message.
```

### Viva Answer

> Consumers retry failed messages using x-retry-count header. After configured retries, message is moved to dead-letter queue.

---

## 38. What Happens If NotificationService Is Down?

### Scenario

IdentityService publishes welcome email event.

NotificationService is temporarily down.

### Expected Behavior

```text
Message remains in RabbitMQ queue
User registration still succeeds
When NotificationService starts again, it consumes queued email event
Email is sent later
```

### Viva Answer

> If NotificationService is down, RabbitMQ keeps the email event in queue. Main API flow still succeeds, and email is processed when NotificationService comes back.

---

## 39. Why Email Is Sent Asynchronously

### Simple Explanation

Email sending can be slow or fail because SMTP is external infrastructure.

Main API should not wait for it.

### In Your Project

Instead of direct email sending:

```text
IdentityService -> RabbitMQ -> NotificationService -> SMTP
```

### Benefits

- Faster API response
- Better user experience
- Email failure does not break registration/payment
- Retry and dead-letter support

### Viva Answer

> Email is sent asynchronously because SMTP can be slow or fail. RabbitMQ allows the API to respond quickly while NotificationService sends email in background.

---

## 40. Complete Flow: User Registration Email

```text
1. Frontend calls POST /api/auth/register.
2. IdentityService creates user in database.
3. AuthController publishes UserRegisteredEvent.
4. AuthController publishes EmailRequestedEvent to notifications.email.v2.
5. RabbitMQ stores/routes the email event.
6. EmailNotificationConsumer receives event.
7. Consumer renders welcome email template.
8. MailKitEmailSender sends email.
9. Consumer acknowledges message.
```

### Viva Explanation

> Registration uses RabbitMQ to send welcome email asynchronously. IdentityService publishes EmailRequestedEvent, and NotificationService consumes it later.

---

## 41. Complete Flow: Assessment Completion

```text
1. User submits assessment.
2. AssessmentService calculates result.
3. AssessmentController publishes AssessmentCompletedEvent.
4. AssessmentController publishes EmailRequestedEvent.
5. UserNotificationEventConsumer consumes AssessmentCompletedEvent.
6. IdentityService stores bell notification.
7. EmailNotificationConsumer consumes EmailRequestedEvent.
8. NotificationService sends result email.
```

### Viva Explanation

> Assessment completion publishes events for notification and email so other services can react without blocking assessment submission.

---

## 42. Complete Flow: Premium Activation

```text
1. User completes payment.
2. SubscriptionService records successful payment.
3. SubscriptionService publishes SubscriptionLifecycleEvent with Action = Activate.
4. IdentityService SubscriptionEventConsumer consumes event.
5. IdentityService updates User.IsPremium = true.
6. IdentityService creates premium notification.
7. IdentityService publishes SubscriptionLifecycleEvent result with Status = Completed.
8. SubscriptionService IdentityResultConsumer consumes result.
9. SubscriptionService updates SagaState = Completed.
10. User refreshes JWT claims to see premium access.
```

### Viva Explanation

> Premium activation is event-based. SubscriptionService publishes an activation event, IdentityService updates premium status, and then publishes a result event back.

---

## 43. Idempotency and Duplicate Messages

### Simple Explanation

Message systems may sometimes deliver duplicates.

Consumers should ideally handle duplicate messages safely.

### In Your Project

The project has useful foundations:

```text
EventId on IntegrationEvent
SagaId on subscription/payment events
WebhookEventLog in SubscriptionService
SagaState in Subscription
```

### Improvement Note

Consumers can be improved by storing processed `EventId` values to avoid duplicate side effects.

### Viva Answer

> RabbitMQ consumers should be idempotent because duplicate delivery can happen. My project includes EventId and SagaId, and future improvement can store processed event IDs.

---

## 44. Message Durability vs Idempotency

### Message Durability

Helps message survive broker restart.

Uses:

```text
Durable queue/exchange
Persistent message
```

### Idempotency

Ensures processing same message twice does not cause wrong result.

Examples:

```text
Do not send duplicate payment success processing
Do not activate premium twice incorrectly
Do not create many duplicate notifications
```

### Viva Answer

> Durability protects message storage, while idempotency protects business correctness when the same message is processed more than once.

---

## 45. What Happens If RabbitMQ Is Removed?

### Problems

- Services must call each other directly
- Email sending may block API response
- NotificationService downtime can break main flows
- More tight coupling between services
- Harder retry handling
- Harder background processing
- Premium activation flow becomes more fragile

### Viva Answer

> Without RabbitMQ, services would need direct synchronous calls, making the system more tightly coupled and less reliable for background work like emails and premium activation.

---

## 46. RabbitMQ Alternatives

### Alternatives

```text
Apache Kafka
Azure Service Bus
Amazon SQS/SNS
Google Pub/Sub
Redis Streams
MassTransit over RabbitMQ
NServiceBus
Direct HTTP with retry
Background job tools like Hangfire
```

### RabbitMQ vs Kafka

RabbitMQ is good for task queues and routing.

Kafka is better for high-volume event streaming and replay.

### Viva Answer

> Alternatives include Kafka, Azure Service Bus, Amazon SQS/SNS, Google Pub/Sub, Redis Streams, MassTransit, and Hangfire. RabbitMQ is suitable here because the project needs reliable queue-based background processing.

---

## 47. RabbitMQ Security

### Important Points

- Do not expose RabbitMQ publicly without protection
- Use strong username/password outside development
- Store credentials in configuration or secret manager
- Use TLS in production if needed
- Restrict queue permissions per service
- Do not log sensitive message contents

### In Your Project

Configuration is represented by:

```text
RabbitMqOptions
```

### Viva Answer

> RabbitMQ credentials should be stored securely, access should be restricted, and sensitive message data should not be logged.

---

## 48. Possible Improvements

### Improvements

- Add processed-event table for idempotency
- Use MassTransit to reduce repeated consumer boilerplate
- Add correlation ID across events
- Add centralized monitoring for queue depth
- Add health checks for RabbitMQ connection
- Add exponential backoff instead of fixed retry delay
- Add separate queues for started/completed events if scaling demands it
- Add structured event versioning beyond queue name suffix
- Add alerting for dead-letter queues
- Add outbox pattern for reliable publish after database save

### Balanced Viva Answer

> The current RabbitMQ setup supports asynchronous events, retries, dead-letter queues, and service decoupling. Future improvements could include idempotency storage, outbox pattern, correlation IDs, monitoring, and MassTransit.

---

## 49. Important Code Files

### Shared Messaging

```text
Backend/BuildingBlocks/Messaging/IRabbitMqPublisher.cs
Backend/BuildingBlocks/Messaging/RabbitMqPublisher.cs
Backend/BuildingBlocks/Messaging/RabbitMqOptions.cs
Backend/BuildingBlocks/Messaging/QueueNames.cs
```

### Event Contracts

```text
Backend/BuildingBlocks/Messaging/Events/IntegrationEvent.cs
Backend/BuildingBlocks/Messaging/Events/EmailRequestedEvent.cs
Backend/BuildingBlocks/Messaging/Events/UserRegisteredEvent.cs
Backend/BuildingBlocks/Messaging/Events/InterviewStartedEvent.cs
Backend/BuildingBlocks/Messaging/Events/InterviewCompletedEvent.cs
Backend/BuildingBlocks/Messaging/Events/AssessmentStartedEvent.cs
Backend/BuildingBlocks/Messaging/Events/AssessmentCompletedEvent.cs
Backend/BuildingBlocks/Messaging/Events/PaymentSucceededEvent.cs
Backend/BuildingBlocks/Messaging/Events/SubscriptionLifecycleEvent.cs
Backend/BuildingBlocks/Messaging/Events/UserClaimsRefreshedEvent.cs
```

### Consumers

```text
Backend/NotificationService/Messaging/EmailNotificationConsumer.cs
Backend/IdentityService/Messaging/SubscriptionEventConsumer.cs
Backend/IdentityService/Messaging/UserNotificationEventConsumer.cs
Backend/SubscriptionService/Messaging/IdentityResultConsumer.cs
```

### Publisher Usage

```text
Backend/IdentityService/Controllers/AuthController.cs
Backend/InterviewService/Controllers/InterviewController.cs
Backend/AssessmentService/Controllers/AssessmentController.cs
Backend/SubscriptionService/Controllers/SubscriptionController.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
```

---

## 50. Best Full Viva Answer for Topic 12

> My project uses RabbitMQ for asynchronous event-driven communication between services. RabbitMQ acts as a message broker where producer services publish integration events and consumer services process them in background. The project defines a direct exchange called mockinterview.events.v2 and queue names in QueueNames.cs. RabbitMqPublisher serializes events to JSON and publishes persistent messages with routing keys. NotificationService consumes EmailRequestedEvent to send emails asynchronously. IdentityService consumes subscription lifecycle events to update premium status and consumes interview/assessment events to create user notifications. SubscriptionService consumes identity result events to update saga state. Consumers use manual acknowledgements, retry headers, and dead-letter queues for reliability. This keeps services loosely coupled and prevents slow operations like email sending from blocking API responses.

---

## 51. Common Viva Questions and Answers

### Q1. What is RabbitMQ?

RabbitMQ is a message broker used to send messages between services asynchronously.

### Q2. What is a message broker?

A message broker receives, stores, routes, and delivers messages between producers and consumers.

### Q3. What is event-driven architecture?

It is an architecture where services communicate by publishing and consuming events.

### Q4. What is a producer?

A producer is a service that publishes messages to RabbitMQ.

### Q5. What is a consumer?

A consumer is a service or background worker that reads and processes messages from RabbitMQ.

### Q6. What is a queue?

A queue stores messages until consumers process them.

### Q7. What is an exchange?

An exchange receives messages from producers and routes them to queues.

### Q8. What is a routing key?

A routing key is used by the exchange to decide which queue should receive a message.

### Q9. What type of exchange is used in your project?

A direct exchange is used.

### Q10. Why is email sent asynchronously?

Because email sending can be slow or fail, and it should not block the main API response.

### Q11. What happens if NotificationService is down?

Messages stay in RabbitMQ queue and are processed when NotificationService comes back.

### Q12. What is IRabbitMqPublisher?

It is the interface used by services to publish messages.

### Q13. What does RabbitMqPublisher do?

It serializes events to JSON and publishes them to RabbitMQ exchange with a routing key.

### Q14. What is IntegrationEvent?

It is the base event class containing EventId, OccurredOnUtc, and EventType.

### Q15. What is EventId used for?

It uniquely identifies an event and helps with tracing and duplicate handling.

### Q16. What is dead-letter queue?

A dead-letter queue stores messages that failed after retries.

### Q17. What is message acknowledgement?

Acknowledgement tells RabbitMQ that a message was processed and can be removed.

### Q18. Why use autoAck false?

To manually acknowledge messages only after successful processing.

### Q19. How does retry work in your project?

Consumers increment x-retry-count, wait for retry delay, republish the message, and dead-letter it after max retries.

### Q20. What is saga pattern?

Saga coordinates a multi-service workflow using local transactions and events.

### Q21. Where is saga-style flow used?

Premium activation between SubscriptionService and IdentityService.

### Q22. What is PaymentSucceededEvent?

It represents a successful payment event.

### Q23. What is SubscriptionLifecycleEvent?

It coordinates premium activation or cancellation.

### Q24. What is QueueNames.cs?

It is the central file that stores exchange and queue names.

### Q25. What are RabbitMQ alternatives?

Kafka, Azure Service Bus, Amazon SQS/SNS, Google Pub/Sub, Redis Streams, MassTransit, NServiceBus, and Hangfire.

---

## 52. Quick Revision Summary

- RabbitMQ is a message broker.
- Message broker connects producers and consumers.
- Event-driven architecture uses events for communication.
- Producer publishes messages.
- Consumer processes messages.
- Queue stores messages.
- Exchange routes messages.
- Routing key decides queue destination.
- Project uses direct exchange.
- Main exchange is mockinterview.events.v2.
- Dead-letter exchange is mockinterview.events.v2.dlx.
- Queue names are centralized in QueueNames.cs.
- RabbitMqPublisher publishes JSON messages.
- Messages are persistent.
- Queues and exchanges are durable.
- Consumers are BackgroundService workers.
- Manual acknowledgement is used with autoAck false.
- Failed messages are retried.
- x-retry-count tracks retry attempts.
- Failed messages eventually move to dead-letter queue.
- EmailNotificationConsumer sends emails.
- SubscriptionEventConsumer updates premium status.
- IdentityResultConsumer updates subscription saga state.
- UserNotificationEventConsumer creates bell notifications.
- RabbitMQ keeps email and notification work asynchronous.
- Saga-style premium activation uses subscription lifecycle events.
- Future improvements include outbox pattern and idempotency table.

