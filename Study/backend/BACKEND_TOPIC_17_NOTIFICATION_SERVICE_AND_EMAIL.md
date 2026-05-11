# Topic 17: NotificationService and Email

Project: Mock Interview Platform  
Focus: Understanding NotificationService, background workers, RabbitMQ email events, hosted service consumers, email templates, SMTP, MailKit, MIME email, retry/dead-letter handling, and why email is processed asynchronously.

---

## 1. What Is NotificationService?

### Simple Explanation

NotificationService is the backend service responsible for sending emails.

It does not handle login, interviews, assessments, or payments directly.

It waits for email events and sends emails in the background.

### In Your Project

NotificationService sends emails for:

- Welcome after registration
- Password reset OTP
- Payment success
- Premium subscription activation
- Assessment completion
- Interview completion

### Viva Answer

> NotificationService is a background worker service that consumes email events from RabbitMQ and sends emails using MailKit and SMTP.

---

## 2. Why NotificationService Is Separate

### Simple Explanation

Email sending can be slow or fail.

Main API response should not wait for SMTP.

### Example

During registration:

```text
User account should be created quickly.
Welcome email can be sent later.
```

### Service Responsibility

```text
IdentityService -> register user and publish email event
NotificationService -> consume event and send email
```

### Viva Answer

> NotificationService is separate so email sending happens asynchronously and does not block main APIs like register, payment, assessment, or interview submission.

---

## 3. Why NotificationService Has No Controller

### Simple Explanation

NotificationService is not called directly by frontend.

It listens to RabbitMQ queue.

### In Your Project

It is a worker service:

```text
Microsoft.NET.Sdk.Worker
```

It starts a hosted service:

```text
EmailNotificationConsumer
```

### Viva Answer

> NotificationService has no controller because it is a background worker. It receives email requests from RabbitMQ instead of HTTP endpoints.

---

## 4. Important NotificationService Files

### Program and Configuration

```text
Backend/NotificationService/Program.cs
Backend/NotificationService/appsettings.Example.json
```

### Messaging

```text
Backend/NotificationService/Messaging/EmailNotificationConsumer.cs
```

### Services

```text
Backend/NotificationService/Services/IEmailSender.cs
Backend/NotificationService/Services/MailKitEmailSender.cs
Backend/NotificationService/Services/IHtmlEmailTemplateRenderer.cs
Backend/NotificationService/Services/HtmlEmailTemplateRenderer.cs
Backend/NotificationService/Services/EmailOptions.cs
```

### Shared Event

```text
Backend/BuildingBlocks/Messaging/Events/EmailRequestedEvent.cs
```

### Viva Answer

> Important files are Program.cs, EmailNotificationConsumer, MailKitEmailSender, HtmlEmailTemplateRenderer, EmailOptions, and EmailRequestedEvent.

---

## 5. Worker Service

### Simple Explanation

A worker service runs background tasks.

It does not need HTTP request/response flow.

### In Your Project

NotificationService project uses:

```xml
<Project Sdk="Microsoft.NET.Sdk.Worker">
```

### Packages

```text
MailKit
MimeKit
Microsoft.Extensions.Hosting
RabbitMQ.Client
```

### Viva Answer

> NotificationService is a .NET Worker Service that runs background email processing using hosted services.

---

## 6. Program.cs in NotificationService

### File

```text
Backend/NotificationService/Program.cs
```

### Code

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IHtmlEmailTemplateRenderer, HtmlEmailTemplateRenderer>();
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
builder.Services.AddHostedService<EmailNotificationConsumer>();

var host = builder.Build();
host.Run();
```

### Meaning

```text
Load RabbitMQ settings
Load Email settings
Register template renderer
Register email sender
Start email consumer background service
```

### Viva Answer

> Program.cs configures RabbitMQ options, email options, email sender, template renderer, and starts EmailNotificationConsumer as hosted service.

---

## 7. EmailRequestedEvent

### File

```text
Backend/BuildingBlocks/Messaging/Events/EmailRequestedEvent.cs
```

### Code

```csharp
public class EmailRequestedEvent : IntegrationEvent
{
    public string ToEmail { get; init; } = string.Empty;
    public string ToName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public Dictionary<string, string> Model { get; init; } = [];
}
```

### Meaning

The event contains:

```text
Receiver email
Receiver name
Email subject
Template key
Template values
```

### Viva Answer

> EmailRequestedEvent is the shared event used by services to request email sending from NotificationService.

---

## 8. Email Queue

### Queue Name

```text
notifications.email.v2
```

### File

```text
Backend/BuildingBlocks/Messaging/QueueNames.cs
```

### Code

```csharp
public const string EmailNotifications = "notifications.email.v2";
```

### Viva Answer

> Email requests are sent to the RabbitMQ queue named notifications.email.v2.

---

## 9. EmailNotificationConsumer

### File

```text
Backend/NotificationService/Messaging/EmailNotificationConsumer.cs
```

### Purpose

Consumes email messages from RabbitMQ and sends email.

### Inherits

```csharp
BackgroundService
```

### Viva Answer

> EmailNotificationConsumer is a background service that listens to the email queue and processes EmailRequestedEvent messages.

---

## 10. Consumer Startup Flow

### ExecuteAsync Flow

```text
1. Read RabbitMQ options.
2. Create RabbitMQ connection.
3. Create RabbitMQ channel.
4. Declare email queue with dead-letter settings.
5. Create AsyncEventingBasicConsumer.
6. Attach ReceivedAsync handler.
7. Start consuming notifications.email.v2.
```

### Code Concept

```csharp
await _channel.BasicConsumeAsync(
    QueueNames.EmailNotifications,
    autoAck: false,
    consumer: consumer);
```

### Viva Answer

> EmailNotificationConsumer opens RabbitMQ connection/channel, declares queue, and starts consuming email messages with manual acknowledgement.

---

## 11. Handling One Email Message

### Flow

```text
1. Convert message body from bytes to JSON string.
2. Deserialize JSON into EmailRequestedEvent.
3. Create DI scope.
4. Resolve IEmailSender.
5. Resolve IHtmlEmailTemplateRenderer.
6. Render HTML body using template key and model.
7. Send email through MailKitEmailSender.
8. Acknowledge RabbitMQ message.
```

### Viva Answer

> For each email message, the consumer deserializes EmailRequestedEvent, renders HTML, sends email, and acknowledges the message after successful processing.

---

## 12. Why Create DI Scope in Consumer

### Code

```csharp
using var scope = _scopeFactory.CreateScope();
var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
var renderer = scope.ServiceProvider.GetRequiredService<IHtmlEmailTemplateRenderer>();
```

### Why

Background services are long-lived.

Creating a scope is a safe way to resolve dependencies for each message.

### Viva Answer

> The consumer creates a DI scope per message so dependencies are resolved safely inside a background worker.

---

## 13. Manual Acknowledgement

### In Your Project

Consumer uses:

```text
autoAck: false
```

and after success:

```csharp
await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
```

### Why

RabbitMQ should remove message only after email processing completes.

### Viva Answer

> Manual acknowledgement ensures RabbitMQ removes an email message only after it is successfully processed.

---

## 14. Retry Handling

### If Email Processing Fails

Consumer catches exception:

```text
Log error
Retry or dead-letter message
```

### Retry Header

```text
x-retry-count
```

### Config

```text
RabbitMq:RetryCount
RabbitMq:RetryDelaySeconds
```

### Viva Answer

> Failed email messages are retried using x-retry-count header and retry settings from RabbitMqOptions.

---

## 15. Dead-Letter Queue

### Simple Explanation

Dead-letter queue stores messages that fail after maximum retries.

### In Your Project

Dead-letter queue name:

```text
notifications.email.v2.dead
```

### Dead-Letter Exchange

```text
mockinterview.events.v2.dlx
```

### Viva Answer

> Dead-letter queue stores email messages that could not be processed after retries, so they are not lost and can be inspected later.

---

## 16. EmailOptions

### File

```text
Backend/NotificationService/Services/EmailOptions.cs
```

### Fields

```text
Enabled
Host
Port
EnableSsl
Username
Password
FromAddress
FromName
```

### Viva Answer

> EmailOptions stores SMTP configuration such as host, port, SSL, username, password, sender address, and enabled flag.

---

## 17. Email Configuration

### appsettings Example

```json
"Email": {
  "Enabled": true,
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "YOUR_EMAIL@gmail.com",
  "Password": "YOUR_GMAIL_APP_PASSWORD",
  "FromAddress": "noreply@interview.com",
  "FromName": "Mock Interview Platform"
}
```

### Important Security Note

Real email password or app password should not be committed to source code.

### Viva Answer

> Email settings configure SMTP server and sender details. Real credentials should be stored securely and not committed.

---

## 18. IEmailSender

### File

```text
Backend/NotificationService/Services/IEmailSender.cs
```

### Code

```csharp
public interface IEmailSender
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken);
}
```

### Viva Answer

> IEmailSender defines the email sending contract used by NotificationService.

---

## 19. MailKitEmailSender

### File

```text
Backend/NotificationService/Services/MailKitEmailSender.cs
```

### Purpose

Sends emails using MailKit and SMTP.

### Flow

```text
1. Check if email delivery is enabled.
2. Create MimeMessage.
3. Set From address.
4. Set To address.
5. Set subject.
6. Set HTML body.
7. Connect to SMTP server.
8. Authenticate.
9. Send email.
10. Disconnect.
11. Log success.
```

### Viva Answer

> MailKitEmailSender creates MIME email messages and sends them through SMTP using MailKit.

---

## 20. Email Enabled Flag

### In MailKitEmailSender

```csharp
if (!_options.Enabled)
{
    _logger.LogInformation("Email delivery is disabled. Skipping email to {Email}", toEmail);
    return;
}
```

### Why Useful

During development or testing, email sending can be disabled without changing business logic.

### Viva Answer

> Email Enabled flag allows the project to skip real email sending during development or testing.

---

## 21. What Is SMTP?

### Simple Explanation

SMTP means Simple Mail Transfer Protocol.

It is used to send emails.

### In Your Project

SMTP configuration uses:

```text
Host = smtp.gmail.com
Port = 587
SSL/TLS enabled
Username/password authentication
```

### Viva Answer

> SMTP is the protocol used to send emails. NotificationService uses SMTP through MailKit.

---

## 22. What Is MailKit?

### Simple Explanation

MailKit is a .NET library for sending emails through SMTP.

### In Your Project

Used by:

```text
MailKitEmailSender
```

### Why Useful

It provides:

```text
SMTP client
TLS support
Authentication
Async send APIs
```

### Viva Answer

> MailKit is a .NET email library used to connect to SMTP server and send emails asynchronously.

---

## 23. What Is MIME Email?

### Simple Explanation

MIME lets email contain structured content like HTML.

### In Your Project

Uses:

```text
MimeMessage
BodyBuilder
HtmlBody
```

### Code Concept

```csharp
message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();
```

### Viva Answer

> MIME email allows structured email content like HTML. The project uses MimeKit to build HTML email messages.

---

## 24. HTML Email Template Renderer

### File

```text
Backend/NotificationService/Services/HtmlEmailTemplateRenderer.cs
```

### Purpose

Converts template key and model values into HTML email body.

### Method

```csharp
string Render(string templateKey, IReadOnlyDictionary<string, string> model)
```

### Viva Answer

> HtmlEmailTemplateRenderer builds HTML email body from a template key and replacement values.

---

## 25. Template Keys

### Supported Templates

```text
welcome
password-reset-otp
payment-success
subscription-upgrade
assessment-complete
interview-complete
```

### Default

If template key is unknown:

```text
Mock Interview Platform Notification
```

### Viva Answer

> NotificationService supports template keys for welcome, password reset OTP, payment success, subscription upgrade, assessment completion, and interview completion emails.

---

## 26. Welcome Email

### Published By

```text
IdentityService
```

### Template Key

```text
welcome
```

### Model

```text
FullName
```

### Flow

```text
User registers
IdentityService publishes EmailRequestedEvent
NotificationService sends welcome email
```

### Viva Answer

> Welcome email is sent after registration using the welcome template.

---

## 27. Password Reset OTP Email

### Published By

```text
IdentityService
```

### Template Key

```text
password-reset-otp
```

### Model

```text
FullName
OtpCode
ExpiryMinutes
```

### Viva Answer

> Password reset OTP email sends the OTP and expiry time using password-reset-otp template.

---

## 28. Payment Success Email

### Published By

```text
SubscriptionService
```

### Template Key

```text
payment-success
```

### Model

```text
Amount
Currency
PaymentId
```

### Viva Answer

> Payment success email is sent after successful payment using payment-success template.

---

## 29. Subscription Upgrade Email

### Published By

```text
SubscriptionService
```

### Template Key

```text
subscription-upgrade
```

### Model

```text
Plan
EndDate
```

### Viva Answer

> Subscription upgrade email informs the user that premium subscription is active and shows plan and end date.

---

## 30. Assessment Completion Email

### Published By

```text
AssessmentService
```

### Template Key

```text
assessment-complete
```

### Model

```text
Domain
Percentage
Grade
```

### Viva Answer

> Assessment completion email tells the user their assessment result is ready with score percentage and grade.

---

## 31. Interview Completion Email

### Published By

```text
InterviewService
```

### Template Key

```text
interview-complete
```

### Model

```text
Domain
Percentage
Grade
```

### Viva Answer

> Interview completion email tells the user their interview result is ready with score percentage and grade.

---

## 32. Complete Flow: Welcome Email

```text
1. User calls POST /api/auth/register.
2. IdentityService creates user.
3. IdentityService publishes EmailRequestedEvent with TemplateKey = welcome.
4. RabbitMQ stores message in notifications.email.v2 queue.
5. EmailNotificationConsumer receives message.
6. Consumer renders welcome HTML.
7. MailKitEmailSender sends email through SMTP.
8. Consumer acknowledges RabbitMQ message.
```

### Viva Explanation

> Welcome email is asynchronous. Registration does not wait for SMTP; NotificationService sends email after consuming RabbitMQ event.

---

## 33. Complete Flow: Password Reset OTP Email

```text
1. User requests forgot-password OTP.
2. IdentityService generates secure OTP.
3. IdentityService hashes OTP and stores it in memory cache.
4. IdentityService publishes EmailRequestedEvent with OTP model.
5. NotificationService consumes the event.
6. Renderer creates password reset OTP email.
7. MailKit sends OTP email.
8. User submits OTP to reset password.
```

### Viva Explanation

> OTP email is sent asynchronously while IdentityService securely stores hashed OTP for later verification.

---

## 34. Complete Flow: Payment Emails

```text
1. Payment succeeds in simulated mode or Stripe webhook mode.
2. SubscriptionService publishes payment-success EmailRequestedEvent.
3. SubscriptionService publishes subscription-upgrade EmailRequestedEvent.
4. NotificationService consumes both messages.
5. Renderer builds HTML emails.
6. MailKit sends payment success and premium activation emails.
```

### Viva Explanation

> Payment and premium emails are triggered by SubscriptionService after successful payment.

---

## 35. Complete Flow: Assessment Completion Email

```text
1. User submits assessment.
2. AssessmentService calculates result.
3. AssessmentController publishes EmailRequestedEvent with assessment-complete template.
4. NotificationService consumes event.
5. Email is rendered with domain, percentage, and grade.
6. MailKit sends email.
```

### Viva Explanation

> Assessment completion email is sent after result calculation through RabbitMQ.

---

## 36. Complete Flow: Interview Completion Email

```text
1. User submits interview answers.
2. InterviewService evaluates answers and creates result.
3. InterviewController publishes EmailRequestedEvent with interview-complete template.
4. NotificationService consumes event.
5. Email is rendered with domain, percentage, and grade.
6. MailKit sends email.
```

### Viva Explanation

> Interview completion email is sent after interview result is generated.

---

## 37. What Happens If NotificationService Is Down?

### Scenario

IdentityService publishes welcome email event.

NotificationService is down.

### Result

```text
User registration still succeeds.
Email message remains in RabbitMQ queue.
When NotificationService starts, it consumes the message.
Email is sent later.
```

### Viva Answer

> If NotificationService is down, main API flow still succeeds and RabbitMQ keeps email messages until the service comes back.

---

## 38. What Happens If SMTP Fails?

### Scenario

SMTP server is unavailable or credentials are wrong.

### Project Behavior

```text
MailKitEmailSender throws exception.
EmailNotificationConsumer catches it.
Error is logged.
Message is retried.
After max retries, message moves to dead-letter queue.
```

### Viva Answer

> If SMTP fails, the consumer retries the email message and eventually moves it to dead-letter queue if it keeps failing.

---

## 39. Why Email Should Not Block Main API

### Problem

SMTP can be slow.

Email provider can fail.

Network can timeout.

### If Blocking

Registration or payment API could become slow or fail because email failed.

### Project Solution

```text
Publish EmailRequestedEvent
Return API response
Send email in background
```

### Viva Answer

> Email should not block main API because SMTP is slow and unreliable. RabbitMQ lets email be processed asynchronously.

---

## 40. Logging in NotificationService

### Logged Events

```text
Email disabled
Email sent successfully
Failed to process email message
Retry failures
```

### Why Useful

Logs help debug:

```text
SMTP credential problems
RabbitMQ connection issues
Template problems
Failed emails
```

### Viva Answer

> NotificationService logs email success, skipped emails, and failures to help debug email delivery issues.

---

## 41. Security Considerations

### Important Points

- Do not commit real SMTP password
- Use app password for Gmail
- Do not log OTP values unnecessarily
- Do not log email password
- Use TLS/SSL for SMTP
- Store credentials in environment variables or secret manager
- Validate email event data if needed

### In Your Project

EmailOptions includes:

```text
Username
Password
EnableSsl
```

### Viva Answer

> Email credentials should be stored securely, SMTP should use TLS, and sensitive data like passwords or OTPs should not be logged.

---

## 42. Alternatives to SMTP and MailKit

### Alternatives

```text
SendGrid
Amazon SES
Mailgun
Postmark
Azure Communication Services
Resend
Brevo
SMTP with another provider
```

### MailKit Benefit

Works with standard SMTP providers.

### SaaS Provider Benefit

Better deliverability, analytics, templates, bounce handling.

### Viva Answer

> Alternatives include SendGrid, Amazon SES, Mailgun, Postmark, Azure Communication Services, and other email APIs. MailKit is suitable for SMTP-based email sending.

---

## 43. Difference Between In-App Notification and Email

### In-App Notification

Stored in IdentityService database.

Shown inside application UI.

Example:

```text
Interview result ready bell notification
```

### Email Notification

Sent by NotificationService through SMTP.

Delivered to user's mailbox.

### Viva Answer

> In-app notifications are stored in database and shown inside the app, while email notifications are sent externally through NotificationService.

---

## 44. Why TemplateKey Is Used

### Simple Explanation

Producer services should not build full HTML email.

They only send:

```text
TemplateKey
Model values
```

NotificationService decides how email looks.

### Benefit

Email design remains centralized.

### Viva Answer

> TemplateKey allows services to request a specific email type without embedding HTML in every service.

---

## 45. What Happens If TemplateKey Is Unknown?

### In Renderer

Unknown key uses default title:

```text
Mock Interview Platform Notification
```

and default body:

```text
You have a new notification from Mock Interview Platform.
```

### Viva Answer

> If template key is unknown, the renderer returns a default notification email template.

---

## 46. What Happens If Email Delivery Is Disabled?

### In Config

```json
"Enabled": false
```

### Behavior

MailKitEmailSender logs and skips sending.

RabbitMQ message is still acknowledged after processing.

### Viva Answer

> If email delivery is disabled, NotificationService skips real SMTP sending but still processes the message.

---

## 47. Complete Request-to-Email Architecture

```text
Producer Service
    |
    | Publishes EmailRequestedEvent
    v
RabbitMQ Exchange mockinterview.events.v2
    |
    | Routing key notifications.email.v2
    v
Queue notifications.email.v2
    |
    | Consumed by EmailNotificationConsumer
    v
HtmlEmailTemplateRenderer
    |
    v
MailKitEmailSender
    |
    v
SMTP Server
    |
    v
User Email Inbox
```

### Viva Explanation

> Producer services publish email events, RabbitMQ queues them, NotificationService consumes them, renders HTML, and sends email through SMTP.

---

## 48. Limitations and Improvements

### Current Limitations

- Templates are hardcoded in C# strings
- No email delivery status table
- No unsubscribe/preferences system
- No provider-specific bounce tracking
- No rate limiting for email sending
- No localization
- No attachment support
- No plain text alternative body
- No template preview tool
- No centralized email audit history

### Possible Improvements

- Store templates in files or database
- Add email delivery log table
- Add user notification preferences
- Add plain-text email fallback
- Add SendGrid/Amazon SES provider
- Add bounce/failure tracking
- Add rate limiting
- Add template versioning
- Add localization support
- Add correlation id to email events

### Balanced Viva Answer

> Current NotificationService supports asynchronous HTML email sending with RabbitMQ, MailKit, retry, and dead-letter queue. Future improvements could include template files, delivery logs, user preferences, provider APIs, bounce tracking, and rate limiting.

---

## 49. Best Full Viva Answer for Topic 17

> NotificationService is a .NET Worker Service responsible for sending emails asynchronously. It has no controllers because it is not called by frontend. Other services publish EmailRequestedEvent messages to RabbitMQ queue notifications.email.v2. EmailNotificationConsumer runs as a BackgroundService, consumes these messages, deserializes the event, renders HTML using HtmlEmailTemplateRenderer, and sends the email using MailKitEmailSender through SMTP. The service supports templates like welcome, password-reset-otp, payment-success, subscription-upgrade, assessment-complete, and interview-complete. It uses manual RabbitMQ acknowledgement, retry count headers, and dead-letter queue for failed messages. This design prevents slow or failed email sending from blocking main APIs like registration, payment, assessment, or interview submission.

---

## 50. Common Viva Questions and Answers

### Q1. What is NotificationService?

NotificationService is a background worker that consumes email events and sends emails using MailKit and SMTP.

### Q2. Why does NotificationService have no controller?

Because it is not called by frontend; it listens to RabbitMQ messages.

### Q3. What is EmailRequestedEvent?

It is an event containing recipient, subject, template key, and model values for sending email.

### Q4. Which queue is used for email?

notifications.email.v2.

### Q5. What is EmailNotificationConsumer?

It is a BackgroundService that consumes email messages from RabbitMQ.

### Q6. Why is email sent asynchronously?

Because SMTP can be slow or fail, and main APIs should not wait for email delivery.

### Q7. What is MailKit?

MailKit is a .NET library used to send emails through SMTP.

### Q8. What is MimeKit?

MimeKit is used to build MIME email messages, including HTML email body.

### Q9. What is SMTP?

SMTP is the protocol used to send email.

### Q10. What is EmailOptions?

EmailOptions stores SMTP configuration like host, port, username, password, from address, and enabled flag.

### Q11. What happens if email delivery is disabled?

The sender logs and skips sending email.

### Q12. What templates are supported?

welcome, password-reset-otp, payment-success, subscription-upgrade, assessment-complete, and interview-complete.

### Q13. Who publishes welcome email?

IdentityService after registration.

### Q14. Who publishes password reset OTP email?

IdentityService after generating OTP.

### Q15. Who publishes payment emails?

SubscriptionService after successful payment.

### Q16. Who publishes assessment completion email?

AssessmentService after assessment submission.

### Q17. Who publishes interview completion email?

InterviewService after interview submission.

### Q18. What happens if NotificationService is down?

RabbitMQ keeps email messages in queue until NotificationService comes back.

### Q19. What happens if SMTP fails?

The message is retried and then moved to dead-letter queue after max retries.

### Q20. Why use manual acknowledgement?

To remove message from queue only after successful processing.

### Q21. What is dead-letter queue?

A queue for messages that failed after retries.

### Q22. Why use TemplateKey?

To centralize email HTML generation in NotificationService.

### Q23. What are alternatives to MailKit SMTP?

SendGrid, Amazon SES, Mailgun, Postmark, Azure Communication Services, Resend, and other email APIs.

### Q24. What security precautions are needed for email?

Do not commit SMTP passwords, use TLS, store secrets securely, and avoid logging sensitive data.

### Q25. What improvements can be made?

Template files, delivery logs, user preferences, bounce tracking, provider API, rate limiting, and localization.

---

## 51. Quick Revision Summary

- NotificationService sends emails.
- It is a Worker Service, not Web API.
- It has no controller.
- It consumes RabbitMQ messages.
- Email queue is notifications.email.v2.
- Email event is EmailRequestedEvent.
- EmailNotificationConsumer is a BackgroundService.
- Consumer deserializes event JSON.
- HtmlEmailTemplateRenderer builds HTML body.
- MailKitEmailSender sends through SMTP.
- EmailOptions stores SMTP settings.
- MailKit handles SMTP sending.
- MimeKit builds MIME HTML email.
- Manual ack is used.
- Failed messages are retried.
- Dead-letter queue stores repeatedly failed emails.
- Supported templates include welcome, OTP, payment, subscription, assessment, interview.
- Email sending is asynchronous.
- Main APIs do not wait for SMTP.
- If NotificationService is down, messages remain in RabbitMQ.
- If SMTP fails, retries/dead-letter handling applies.
- Future improvements include template files, delivery logs, provider APIs, and user preferences.

