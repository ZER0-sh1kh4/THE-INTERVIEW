# Topic 21: Configuration and Secrets

Project: Mock Interview Platform  
Focus: Understanding appsettings files, IConfiguration, Options pattern, connection strings, JWT settings, RabbitMQ settings, Stripe keys, Gemini keys, email SMTP settings, internal API keys, and safe secret handling.

---

## 1. What Is Configuration?

### Simple Explanation

Configuration means values that control how the application runs without changing source code.

Examples:

- Database connection string
- JWT secret key
- RabbitMQ host and password
- Stripe API keys
- Gemini API key
- Email SMTP username and password
- API gateway routes
- Token expiry time

### In Your Project

Your backend services read configuration from `appsettings.json` style files.

Example files:

```text
IdentityService/appsettings.Example.json
InterviewService/appsettings.Example.json
AssessmentService/appsettings.Example.json
SubscriptionService/appsettings.Example.json
NotificationService/appsettings.Example.json
API_Gateway/appsettings.Example.json
API_Gateway/ocelot.json
```

### Viva Answer

> Configuration stores environment-dependent values outside the source code so the same application can run in development, testing, and production with different settings.

---

## 2. Why Configuration Is Needed

### Simple Explanation

Hardcoding values directly in code is risky and inflexible.

For example, if the database server changes, we should update configuration, not recompile the project.

### Benefits

- Keeps code clean
- Supports different environments
- Avoids recompilation for simple setting changes
- Separates sensitive values from business logic
- Makes deployment easier
- Allows feature toggles like `Stripe:Enabled`

### Viva Answer

> Configuration is needed because values like database paths, API keys, token settings, and external service URLs change between environments and should not be hardcoded in the application.

---

## 3. appsettings.json and appsettings.Example.json

### Simple Explanation

`appsettings.json` normally stores actual runtime configuration.

`appsettings.Example.json` shows the required structure using placeholder values.

### In Your Project

Your repository contains example files such as:

```text
Backend/IdentityService/appsettings.Example.json
Backend/InterviewService/appsettings.Example.json
Backend/SubscriptionService/appsettings.Example.json
```

These example files contain placeholders like:

```json
{
  "Jwt": {
    "SecretKey": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS_HERE!"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  }
}
```

### Why Example Files Are Useful

- They document required keys
- They help a developer create their local config
- They avoid exposing real secrets
- They make setup easier for viva/demo

### Viva Answer

> `appsettings.Example.json` is a template that shows which configuration keys are required, while actual secret values should be placed in local or environment-specific configuration and not committed to source control.

---

## 4. IConfiguration

### Simple Explanation

`IConfiguration` is ASP.NET Core's built-in interface for reading configuration values.

It allows code to read values using keys.

### Example

```csharp
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];
```

### In Your Project

`IConfiguration` is used for:

- JWT token creation
- JWT validation
- Database connection string
- Gemini API key
- Stripe checkout settings
- OTP expiry minutes
- Internal API key

### Viva Answer

> `IConfiguration` provides access to configuration values from sources like JSON files, environment variables, command-line arguments, and secret stores.

---

## 5. Configuration Sections

### Simple Explanation

A configuration section is a grouped set of related settings.

Example:

```json
{
  "Jwt": {
    "SecretKey": "secret",
    "Issuer": "MockInterviewApp",
    "Audience": "MockInterviewApp",
    "ExpiryMinutes": 120
  }
}
```

Here, `Jwt` is a section.

### Accessing Nested Values

```csharp
_config["Jwt:SecretKey"]
_config["Jwt:Issuer"]
_config["Jwt:Audience"]
```

### Viva Answer

> Configuration sections group related values together, and nested values can be read using colon-separated keys like `Jwt:SecretKey`.

---

## 6. ConnectionStrings Configuration

### Simple Explanation

Connection strings tell Entity Framework Core how to connect to SQL Server.

### Example

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER\\SQLEXPRESS;Initial Catalog=MockInterview_IdentityDb;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### In Your Project

Each database-backed service has its own database:

```text
IdentityService      -> MockInterview_IdentityDb
InterviewService     -> MockInterview_InterviewDb
AssessmentService    -> MockInterview_AssessmentDb
SubscriptionService  -> MockInterview_SubscriptionDb
```

### Code Usage

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Viva Answer

> The `ConnectionStrings:DefaultConnection` setting is used by EF Core to connect each service to its SQL Server database.

---

## 7. JWT Configuration

### Simple Explanation

JWT configuration controls how tokens are created and validated.

### JWT Settings

```json
{
  "Jwt": {
    "SecretKey": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS_HERE!",
    "Issuer": "MockInterviewApp",
    "Audience": "MockInterviewApp",
    "ExpiryMinutes": 120
  }
}
```

### Meaning of Each Key

| Key | Meaning |
|---|---|
| `SecretKey` | Symmetric signing key used to sign and validate JWTs |
| `Issuer` | Application that issued the token |
| `Audience` | Application/API that should accept the token |
| `ExpiryMinutes` | Token lifetime |

### In Your Project

`IdentityService` uses JWT settings to create tokens.

Other services and the API Gateway use the same JWT settings to validate tokens.

### Important Point

The JWT `SecretKey`, `Issuer`, and `Audience` must match across services.

If they do not match:

- Login may still generate a token
- Other services may reject the token
- Gateway authorization may fail
- Protected endpoints return `401 Unauthorized`

### Viva Answer

> JWT settings define the signing key, issuer, audience, and expiry time. In this project, IdentityService creates the JWT, and the API Gateway plus other services validate it using matching JWT configuration.

---

## 8. JWT Secret Key

### Simple Explanation

The JWT secret key is used to sign tokens.

If an attacker gets this key, they may be able to forge valid tokens.

### Why It Is Sensitive

- It protects authentication
- It proves that the token was created by the backend
- It must be long and random
- It must not be committed publicly

### In Your Project

The example files use:

```text
YOUR_JWT_SECRET_KEY_MIN_32_CHARS_HERE!
```

This should be replaced with a real strong secret in local or production configuration.

### Viva Answer

> The JWT secret key is a sensitive signing key. It should be strong, private, and consistent across services that need to validate the same token.

---

## 9. RabbitMQ Configuration

### Simple Explanation

RabbitMQ configuration tells services how to connect to the message broker.

### Example

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "RetryCount": 3,
    "RetryDelaySeconds": 15
  }
}
```

### In Your Project

RabbitMQ is used for:

- User registration events
- Email notification events
- Subscription lifecycle events
- Payment succeeded events
- Identity update result events

### Strongly Typed Options

Your shared building block defines:

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

And registers it using:

```csharp
services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
```

### Viva Answer

> RabbitMQ settings define how services connect to the message broker. In this project, RabbitMQ configuration is bound to `RabbitMqOptions` using the Options pattern.

---

## 10. Options Pattern

### Simple Explanation

The Options pattern maps configuration sections to C# classes.

Instead of reading strings manually everywhere, we create a class that represents the section.

### Example

```csharp
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
```

Then a service can receive:

```csharp
IOptions<EmailOptions>
```

### In Your Project

Options pattern is used for:

- `RabbitMqOptions`
- `EmailOptions`

### Why It Is Useful

- Type-safe access
- Cleaner code
- Easier testing
- Centralized defaults
- Better maintainability

### Viva Answer

> The Options pattern binds configuration sections to strongly typed classes, making configuration cleaner and safer than repeatedly reading string keys.

---

## 11. IConfiguration vs Options Pattern

### IConfiguration

Used when:

- Only a few values are needed
- Values are read directly
- Logic is simple

Example:

```csharp
_config["Gemini:ApiKey"]
```

### Options Pattern

Used when:

- A full section is needed
- Many related values exist
- Multiple classes use the same settings
- Type safety is helpful

Example:

```csharp
IOptions<RabbitMqOptions>
```

### In Your Project

| Usage | Approach |
|---|---|
| JWT settings | `IConfiguration` |
| Database connection | `IConfiguration` |
| Gemini settings | `IConfiguration` |
| Stripe settings | `IConfiguration` |
| RabbitMQ settings | Options pattern |
| Email settings | Options pattern |

### Viva Answer

> `IConfiguration` reads values directly, while the Options pattern maps a configuration section to a typed class. Options are better for grouped settings such as RabbitMQ and Email.

---

## 12. Gemini Configuration

### Simple Explanation

Gemini configuration stores the API key and model used for AI-generated questions and evaluation.

### Example

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  }
}
```

### In Your Project

Gemini is used in:

```text
InterviewService
AssessmentService
```

It supports:

- Interview question generation
- Assessment question generation
- Answer evaluation
- Rubric-based feedback

### Missing Configuration Behavior

Your services check whether the Gemini API key is missing or still contains a placeholder.

If missing, the services return a service unavailable style error instead of silently failing.

### Viva Answer

> Gemini configuration stores the AI API key and model name. InterviewService and AssessmentService read these settings to call Gemini for question generation and evaluation.

---

## 13. Stripe Configuration

### Simple Explanation

Stripe configuration controls payment behavior.

### Example

```json
{
  "Stripe": {
    "Enabled": true,
    "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY_HERE",
    "PublishableKey": "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY_HERE",
    "WebhookSecret": "whsec_YOUR_STRIPE_WEBHOOK_SECRET_HERE",
    "SuccessUrl": "http://localhost:4200/subscription/success?session_id={CHECKOUT_SESSION_ID}",
    "CancelUrl": "http://localhost:4200/premium",
    "Currency": "INR",
    "Amount": 49900
  }
}
```

### Meaning of Each Key

| Key | Meaning |
|---|---|
| `Enabled` | Enables real Stripe checkout mode |
| `SecretKey` | Server-side Stripe API key |
| `PublishableKey` | Client-side key returned to frontend |
| `WebhookSecret` | Used to verify Stripe webhook signatures |
| `SuccessUrl` | Redirect URL after payment success |
| `CancelUrl` | Redirect URL after payment cancellation |
| `Currency` | Payment currency |
| `Amount` | Amount in minor units, for example paise |

### In Your Project

`SubscriptionService` uses Stripe config to:

- Decide between real Stripe mode and simulated local mode
- Create Checkout sessions
- Return publishable key to frontend
- Verify webhook signatures
- Set amount, currency, success URL, and cancel URL

### Important Code Behavior

If Stripe is disabled, the service creates a simulated checkout session for local demo testing.

Real Stripe mode requires:

- `Stripe:Enabled = true`
- `Stripe:SecretKey`
- `Stripe:PublishableKey`
- `Stripe:WebhookSecret`

### Viva Answer

> Stripe configuration controls whether real payment mode is enabled and provides the API keys, webhook secret, amount, currency, and redirect URLs used by SubscriptionService.

---

## 14. Stripe Webhook Secret

### Simple Explanation

The webhook secret proves that a webhook request actually came from Stripe.

### In Your Project

`SubscriptionService` reads:

```csharp
_config["Stripe:WebhookSecret"]
```

Then validates the `Stripe-Signature` header.

### Why It Matters

Without webhook signature validation, anyone could send a fake payment success request.

### Viva Answer

> The Stripe webhook secret is used to validate incoming webhook signatures so that fake payment success events cannot activate premium subscriptions.

---

## 15. Email Configuration

### Simple Explanation

Email configuration controls SMTP delivery.

### Example

```json
{
  "Email": {
    "Enabled": true,
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "YOUR_EMAIL_USERNAME",
    "Password": "YOUR_EMAIL_APP_PASSWORD",
    "FromAddress": "noreply@example.com",
    "FromName": "Mock Interview Platform"
  }
}
```

### In Your Project

`NotificationService` uses email settings to send:

- Welcome emails
- Password reset OTP emails
- Payment success emails
- Subscription upgrade emails
- Interview completion emails
- Assessment completion emails

### Strongly Typed EmailOptions

```csharp
public class EmailOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Mock Interview Platform";
}
```

### Viva Answer

> Email configuration stores SMTP settings. NotificationService binds the `Email` section to `EmailOptions` and uses those values to send email through MailKit.

---

## 16. InternalApiKey Configuration

### Simple Explanation

An internal API key protects internal service-to-service endpoints.

### In Your Project

`IdentityService` has an internal premium update endpoint:

```text
PUT /api/auth/internal/users/{id}/premium
```

It checks the request header:

```text
X-Internal-Key
```

against:

```text
InternalApiKey
```

### Code Idea

```csharp
if (!Request.Headers.TryGetValue("X-Internal-Key", out var apiKey)
    || apiKey != _configuration["InternalApiKey"])
{
    return Unauthorized(...);
}
```

### Why It Matters

Without this key, an unauthorized caller could try to update premium status directly.

### Viva Answer

> `InternalApiKey` is a shared secret used to protect internal service endpoints, such as the IdentityService premium update endpoint.

---

## 17. PasswordResetOtp Configuration

### Simple Explanation

Password reset OTP expiry is configurable.

### Example

```json
{
  "PasswordResetOtp": {
    "ExpiryMinutes": 10
  }
}
```

### In Your Project

`IdentityService` reads:

```csharp
_config["PasswordResetOtp:ExpiryMinutes"]
```

If the value is missing or invalid, the code defaults to `10` minutes.

### Viva Answer

> Password reset OTP expiry is stored in configuration so the timeout can be changed without modifying authentication code.

---

## 18. ServiceUrls Configuration

### Simple Explanation

Service URLs define where another backend service can be reached.

### Example

```json
{
  "ServiceUrls": {
    "IdentityService": "http://localhost:5005"
  }
}
```

### In Your Project

`SubscriptionService` example configuration contains the IdentityService URL.

This kind of setting is useful when one service needs to call another service directly.

### Viva Answer

> `ServiceUrls` stores addresses of other backend services so they can be changed per environment without changing code.

---

## 19. API Gateway and Ocelot Configuration

### Simple Explanation

The API Gateway uses configuration for routing and JWT validation.

### In Your Project

The gateway has:

```text
API_Gateway/appsettings.Example.json
API_Gateway/ocelot.json
```

`appsettings` contains JWT settings.

`ocelot.json` contains route mappings from gateway paths to downstream services.

### Example Concept

```text
Frontend request -> API Gateway route -> Downstream microservice
```

### Viva Answer

> The API Gateway uses configuration to validate JWT tokens and Ocelot route configuration to forward frontend requests to the correct backend service.

---

## 20. Secrets

### Simple Explanation

Secrets are configuration values that must be kept private.

### Examples in Your Project

- JWT secret key
- Database password, if SQL authentication is used
- RabbitMQ password
- Gemini API key
- Stripe secret key
- Stripe webhook secret
- Email password or app password
- Internal API key

### Not Secrets

- `AllowedHosts`
- `Logging:LogLevel`
- Public frontend URLs
- Swagger title/version
- Stripe publishable key

Note: Stripe publishable key is designed to be public, but it should still be handled intentionally.

### Viva Answer

> Secrets are sensitive configuration values such as API keys, passwords, signing keys, and webhook secrets. They should not be committed to the repository.

---

## 21. Why Secrets Should Not Be Committed

### Simple Explanation

If secrets are pushed to GitHub, anyone with access to the repository can misuse them.

### Risks

- Fake JWT creation
- Unauthorized database access
- Payment API misuse
- Email account abuse
- AI API billing misuse
- Fake internal service calls
- Production compromise

### Better Approaches

- Environment variables
- ASP.NET Core User Secrets for local development
- Cloud secret managers
- CI/CD secret variables
- Key Vault style storage in production

### Viva Answer

> Secrets should not be committed because exposed secrets can allow attackers to access databases, forge tokens, call paid APIs, or impersonate internal services.

---

## 22. Environment Variables

### Simple Explanation

Environment variables allow configuration to be supplied outside JSON files.

They are commonly used in production and CI/CD.

### ASP.NET Core Nested Key Format

For nested configuration, environment variables commonly use double underscores.

Example:

```text
Jwt__SecretKey=real-production-secret
Stripe__WebhookSecret=real-webhook-secret
Gemini__ApiKey=real-gemini-key
```

ASP.NET Core maps `Jwt__SecretKey` to:

```text
Jwt:SecretKey
```

### Viva Answer

> Environment variables are a secure and deployment-friendly way to provide configuration values without storing secrets in JSON files.

---

## 23. User Secrets

### Simple Explanation

ASP.NET Core User Secrets are useful for local development.

They store secrets outside the project folder.

### Example Commands

```text
dotnet user-secrets set "Gemini:ApiKey" "real-key"
dotnet user-secrets set "Jwt:SecretKey" "local-secret"
```

### Important

User Secrets are for development only, not production.

### Viva Answer

> User Secrets are used during development to store sensitive settings outside the repository while still allowing the application to read them through `IConfiguration`.

---

## 24. Configuration Source Priority

### Simple Explanation

ASP.NET Core can load configuration from multiple sources.

Common sources:

- `appsettings.json`
- `appsettings.Development.json`
- User Secrets
- Environment variables
- Command-line arguments

### Important Idea

Later configuration sources can override earlier values.

For example, production environment variables can override placeholder JSON settings.

### Viva Answer

> ASP.NET Core combines configuration from multiple sources, and later sources such as environment variables can override JSON file values.

---

## 25. Development vs Production Configuration

### Development

Usually uses:

- Local SQL Server
- Local RabbitMQ
- Test Stripe keys
- Test Gemini key
- Local frontend URLs
- Swagger enabled

### Production

Should use:

- Production database
- Production message broker
- Strong JWT secret
- Secure secret store
- HTTPS URLs
- Restricted CORS
- Real payment webhooks
- Careful logging

### Viva Answer

> Development configuration is optimized for local testing, while production configuration must use secure secrets, production infrastructure, HTTPS URLs, and stricter access control.

---

## 26. What Happens If Configuration Is Missing?

### Possible Results

- Database connection fails
- JWT token generation fails
- JWT validation rejects requests
- RabbitMQ publishing fails
- Gemini features return unavailable errors
- Stripe checkout falls back to simulated mode or fails webhook validation
- Email delivery is skipped or fails
- Internal service call returns unauthorized

### In Your Project

Some settings have defaults:

- OTP expiry defaults to `10` minutes
- RabbitMQ options have default values
- Email options have default host/port/SSL values
- Stripe amount defaults to `49900`
- Stripe currency defaults to `INR`

Other settings are required:

- JWT secret key
- Valid connection string
- Real Gemini key for AI features
- Stripe secret and webhook secret for real payment mode

### Viva Answer

> Missing configuration can cause startup failure, authentication failure, external API failure, or degraded local-demo behavior depending on which setting is missing.

---

## 27. Feature Toggles in Configuration

### Simple Explanation

A feature toggle is a setting that turns a feature on or off.

### In Your Project

`Stripe:Enabled` works like a feature toggle.

When true:

```text
SubscriptionService creates real Stripe Checkout sessions.
```

When false:

```text
SubscriptionService creates simulated local payment sessions.
```

`Email:Enabled` also controls whether emails are actually sent.

### Viva Answer

> Feature toggles allow features like Stripe payment mode or email delivery to be enabled or disabled through configuration without changing code.

---

## 28. Complete Flow: JWT Configuration

### Flow

```text
User logs in
IdentityService reads Jwt settings
IdentityService signs JWT using SecretKey
Frontend stores token
Frontend sends token to API Gateway
Gateway validates token using same Jwt settings
Downstream service also validates token
Controller reads claims from token
```

### Viva Answer

> JWT configuration must be shared correctly because one service generates the token and other services validate it.

---

## 29. Complete Flow: Stripe Configuration

### Flow

```text
User clicks subscribe
SubscriptionService reads Stripe settings
If Stripe is enabled, it creates Checkout session
Frontend redirects user to Stripe
Stripe sends webhook after payment
SubscriptionService verifies webhook using WebhookSecret
SubscriptionService activates subscription and publishes events
```

### Viva Answer

> Stripe configuration controls checkout creation, redirect URLs, webhook verification, and payment amount in the subscription flow.

---

## 30. Complete Flow: Gemini Configuration

### Flow

```text
User requests interview or assessment questions
Service reads Gemini:ApiKey and Gemini:Model
Service calls Gemini API
Gemini returns generated content
Backend parses and saves usable questions
```

### Viva Answer

> Gemini configuration allows AI features to be controlled externally by changing the API key or model without modifying service code.

---

## 31. Complete Flow: Email Configuration

### Flow

```text
Service publishes EmailRequestedEvent
NotificationService consumes message from RabbitMQ
NotificationService reads EmailOptions
MailKit connects to SMTP server
Email is sent if Email:Enabled is true
```

### Viva Answer

> Email configuration is used by NotificationService to connect to SMTP and send email notifications based on RabbitMQ events.

---

## 32. Configuration Security Improvements

### Improvements You Can Mention

- Move all real secrets to environment variables or User Secrets
- Validate required config at startup
- Use strongly typed options for JWT, Stripe, and Gemini too
- Add data annotations validation for options
- Rotate leaked keys immediately
- Use different secrets for development and production
- Do not log secret values
- Use HTTPS URLs in production
- Restrict production Swagger access
- Use cloud secret manager in deployment

### Viva Answer

> A good improvement would be to validate important configuration at startup and move all real secrets to environment variables or a secret manager.

---

## 33. Configuration Validation

### Simple Explanation

Configuration validation checks whether required settings exist before the app starts serving requests.

### Example Checks

- JWT secret is not empty
- JWT secret has enough length
- Connection string exists
- Stripe webhook secret exists when Stripe is enabled
- Gemini API key is not a placeholder
- Email password exists when email is enabled

### Why It Helps

Without validation, the application may start successfully but fail later during an important user action.

### Viva Answer

> Configuration validation catches missing or invalid settings early at startup instead of failing later during runtime.

---

## 34. Important Configuration Files in Your Project

### Service Config Templates

```text
Backend/IdentityService/appsettings.Example.json
Backend/InterviewService/appsettings.Example.json
Backend/AssessmentService/appsettings.Example.json
Backend/SubscriptionService/appsettings.Example.json
Backend/NotificationService/appsettings.Example.json
Backend/API_Gateway/appsettings.Example.json
```

### Gateway Routing

```text
Backend/API_Gateway/ocelot.json
```

### Shared Configuration Classes

```text
Backend/BuildingBlocks/Messaging/RabbitMqOptions.cs
Backend/NotificationService/Services/EmailOptions.cs
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Important Services

```text
Backend/IdentityService/Services/AuthService.cs
Backend/InterviewService/Services/InterviewService.cs
Backend/AssessmentService/Services/AssessmentService.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
Backend/NotificationService/Services/MailKitEmailSender.cs
```

---

## 35. Best Full Viva Answer

> In our Mock Interview Platform, configuration is used to keep environment-specific and sensitive values outside business logic. Each microservice has an appsettings example file that shows required settings such as `ConnectionStrings`, `Jwt`, `RabbitMq`, `Gemini`, `Stripe`, `Email`, and `InternalApiKey`.
>
> The services use `IConfiguration` to read direct values like JWT settings, Gemini keys, Stripe keys, OTP expiry, and connection strings. For grouped settings such as RabbitMQ and Email, the project uses the Options pattern by binding sections to `RabbitMqOptions` and `EmailOptions`.
>
> JWT configuration is shared across IdentityService, API Gateway, and protected services so that tokens generated by IdentityService can be validated everywhere. Database configuration is used by EF Core through `DefaultConnection`. RabbitMQ settings are used by publishers and background consumers. Stripe settings control real checkout mode, amount, redirect URLs, and webhook verification. Gemini settings control AI question generation and evaluation. Email settings control SMTP delivery in NotificationService.
>
> Secrets such as JWT secret key, Stripe secret key, webhook secret, Gemini API key, email password, RabbitMQ password, and internal API key should not be committed to source control. In production, they should be provided through environment variables, CI/CD secrets, or a secure secret manager. A good improvement is to validate required configuration at startup so missing secrets are caught early.

---

## 36. Common Viva Questions and Answers

### Q1. What is configuration in ASP.NET Core?

Configuration is the system used to read application settings from sources like JSON files, environment variables, user secrets, and command-line arguments.

### Q2. What is `appsettings.json`?

It is a JSON configuration file used by ASP.NET Core to store application settings such as logging, connection strings, JWT settings, and external service keys.

### Q3. Why do we use `appsettings.Example.json`?

It documents required configuration keys using placeholder values without exposing real secrets.

### Q4. What is `IConfiguration`?

`IConfiguration` is an interface used to read configuration values in ASP.NET Core.

### Q5. What is the Options pattern?

The Options pattern binds a configuration section to a strongly typed C# class.

### Q6. Where is the database connection string stored?

It is stored under `ConnectionStrings:DefaultConnection`.

### Q7. Why must JWT settings match across services?

Because IdentityService signs the token and the API Gateway plus downstream services validate it. If the secret, issuer, or audience does not match, validation fails.

### Q8. Which configuration values are secrets?

JWT secret key, API keys, database passwords, RabbitMQ password, Stripe secret key, Stripe webhook secret, email password, and internal API key are secrets.

### Q9. What is `Stripe:Enabled`?

It is a configuration toggle that controls whether SubscriptionService uses real Stripe checkout or simulated local payment mode.

### Q10. Why is `Stripe:WebhookSecret` important?

It verifies that webhook requests actually came from Stripe and prevents fake payment success requests.

### Q11. What is `Gemini:ApiKey` used for?

It authenticates calls to Gemini for AI question generation and answer evaluation.

### Q12. What is `Email:Enabled` used for?

It controls whether NotificationService actually sends emails or skips delivery.

### Q13. What is `InternalApiKey`?

It is a shared secret used to protect internal endpoints, such as the IdentityService internal premium update endpoint.

### Q14. What happens if configuration is missing?

The application may fail to start, reject JWTs, fail database connection, skip email, fail external API calls, or return service unavailable errors.

### Q15. How should production secrets be stored?

Production secrets should be stored in environment variables, CI/CD secret variables, or a secure secret manager, not in source code.

---

## 37. Quick Revision Summary

```text
Configuration = external settings used by the app.

appsettings.Example.json = template, not real secrets.

IConfiguration = reads values directly.

Options pattern = binds config sections to C# classes.

ConnectionStrings:DefaultConnection = SQL Server connection.

Jwt = token secret, issuer, audience, expiry.

RabbitMq = broker host, port, username, password, retry settings.

Gemini = AI API key and model.

Stripe = payment keys, webhook secret, amount, URLs, enabled flag.

Email = SMTP host, port, username, password, sender details.

InternalApiKey = protects internal service endpoints.

Secrets must not be committed.

Production should use environment variables or secret manager.
```

---

## 38. One-Line Viva Answer

> Configuration in this project stores environment-specific values like database connections, JWT settings, RabbitMQ, Stripe, Gemini, Email, and internal keys, while secrets are protected values that should be supplied securely outside source control.
