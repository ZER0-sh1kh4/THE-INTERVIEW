# Topic 10: Dependency Injection

Project: Mock Interview Platform  
Focus: Understanding how ASP.NET Core creates and provides dependencies like services, DbContext, loggers, RabbitMQ publisher, HttpClient, and hosted consumers without manually creating objects everywhere.

---

## 1. Why Dependency Injection Is Needed

### Simple Explanation

In backend code, one class often needs help from another class.

Example:

```text
AuthController needs AuthService
AuthService needs AppDbContext
InterviewController needs InterviewSvc
AssessmentSvc needs HttpClient
SubscriptionSvc needs AppDbContext
```

Instead of manually creating these objects using `new`, ASP.NET Core creates them and injects them automatically.

This is called dependency injection.

### Practical Scenario

When frontend calls:

```text
POST /api/auth/login
```

ASP.NET Core creates `AuthController`.

But `AuthController` needs:

```text
IAuthService
IRabbitMqPublisher
ILogger<AuthController>
IConfiguration
```

Dependency injection provides these automatically through the constructor.

### Viva Answer

> Dependency injection is needed so classes can receive their required dependencies from ASP.NET Core instead of manually creating them. This keeps code loosely coupled, testable, and easier to maintain.

---

## 2. What Is Dependency?

### Simple Explanation

A dependency is any object that a class needs to do its work.

### Example

`AuthController` depends on `IAuthService`.

```csharp
private readonly IAuthService _authService;
```

Without `IAuthService`, controller cannot register or login users.

### In Your Project

Examples of dependencies:

```text
AuthController depends on IAuthService
InterviewController depends on IInterviewSvc
AssessmentController depends on IAssessmentService
SubscriptionController depends on ISubscriptionSvc
AuthService depends on AppDbContext
AuthService depends on IMemoryCache
Services depend on ILogger
Services depend on IConfiguration
Controllers depend on IRabbitMqPublisher
```

### Viva Answer

> A dependency is an object required by another class. For example, AuthController depends on IAuthService to perform authentication operations.

---

## 3. What Is Dependency Injection?

### Technical Definition

Dependency Injection, or DI, is a design pattern where dependencies are provided to a class from outside instead of being created inside the class.

### Without DI

```csharp
public class AuthController
{
    private AuthService _authService = new AuthService();
}
```

Problem:

```text
Controller is tightly connected to AuthService
Testing becomes difficult
Changing implementation becomes harder
Constructor of AuthService may need many dependencies
```

### With DI

```csharp
public AuthController(IAuthService authService)
{
    _authService = authService;
}
```

Now ASP.NET Core provides the object.

### Viva Answer

> Dependency injection is a pattern where required objects are injected from outside, usually through the constructor. ASP.NET Core has a built-in DI container that creates and manages these objects.

---

## 4. What Is DI Container?

### Simple Explanation

DI container is a service registry.

It knows:

```text
Which interface maps to which class
How long object should live
How to create object with its dependencies
```

### In ASP.NET Core

The DI container is configured using:

```csharp
builder.Services
```

Example:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

Meaning:

```text
When someone asks for IAuthService,
create and provide AuthService.
```

### Viva Answer

> DI container is the built-in ASP.NET Core system that stores service registrations and creates dependency objects when needed.

---

## 5. Constructor Injection

### Simple Explanation

Constructor injection means dependencies are passed through the class constructor.

### Example: AuthController

File:

```text
Backend/IdentityService/Controllers/AuthController.cs
```

Code:

```csharp
public AuthController(
    IAuthService authService,
    IRabbitMqPublisher rabbitMqPublisher,
    ILogger<AuthController> logger,
    IConfiguration configuration)
{
    _authService = authService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
    _configuration = configuration;
}
```

### Why Constructor Injection Is Good

- Required dependencies are clear
- Class cannot be created without required dependencies
- Easy to replace dependencies in tests
- Avoids hidden object creation

### Viva Answer

> Constructor injection passes dependencies through the constructor. My controllers receive service interfaces, loggers, configuration, and RabbitMQ publisher through constructor injection.

---

## 6. Interface-Based Design

### Simple Explanation

Controllers depend on interfaces, not concrete classes.

Example:

```text
Controller depends on IAuthService
Actual implementation is AuthService
```

### In Your Project

Interfaces:

```text
IAuthService
IInterviewSvc
IAssessmentService
ISubscriptionSvc
IRabbitMqPublisher
IEmailSender
IHtmlEmailTemplateRenderer
```

Implementations:

```text
AuthService
InterviewSvc
AssessmentSvc
SubscriptionSvc
RabbitMqPublisher
MailKitEmailSender
HtmlEmailTemplateRenderer
```

### Why Interface Is Used

If tomorrow implementation changes, controller does not need to change.

Example:

```text
IEmailSender -> MailKitEmailSender today
IEmailSender -> SendGridEmailSender tomorrow
```

The consumer can still depend on `IEmailSender`.

### Viva Answer

> Interface-based design keeps classes loosely coupled. Controllers depend on interfaces like IAuthService, while Program.cs decides the actual implementation.

---

## 7. Registering Services in Program.cs

### Simple Explanation

Before ASP.NET Core can inject a dependency, it must be registered.

Registration happens in `Program.cs`.

### IdentityService

File:

```text
Backend/IdentityService/Program.cs
```

Code:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

### InterviewService

File:

```text
Backend/InterviewService/Program.cs
```

Code:

```csharp
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
```

### AssessmentService

File:

```text
Backend/AssessmentService/Program.cs
```

Code:

```csharp
builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>();
```

This registers `AssessmentSvc` as a typed HttpClient service.

### SubscriptionService

File:

```text
Backend/SubscriptionService/Program.cs
```

Code:

```csharp
builder.Services.AddScoped<ISubscriptionSvc, SubscriptionSvc>();
```

### Viva Answer

> Services are registered in Program.cs using builder.Services. For example, AddScoped<IAuthService, AuthService>() tells ASP.NET Core to inject AuthService whenever IAuthService is required.

---

## 8. Service Lifetimes

### Simple Explanation

Service lifetime decides how long an object lives.

ASP.NET Core has three common lifetimes:

```text
Singleton
Scoped
Transient
```

### Quick Difference

| Lifetime | Object Created | Best For |
|---|---|---|
| Singleton | Once for entire application | Shared stateless services, configuration-based services |
| Scoped | Once per HTTP request | DbContext, business services |
| Transient | Every time requested | Lightweight stateless objects |

### Viva Answer

> Service lifetime controls how long dependency objects are reused. Singleton lives for the entire application, scoped lives for one request, and transient is created every time it is requested.

---

## 9. Scoped Lifetime

### Simple Explanation

Scoped means one object is created per HTTP request.

During one request:

```text
Same scoped object is reused
```

For a different request:

```text
New scoped object is created
```

### In Your Project

Business services are scoped:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
builder.Services.AddScoped<ISubscriptionSvc, SubscriptionSvc>();
```

`DbContext` is also scoped by default:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(...));
```

### Why Scoped Is Used

An API request usually represents one unit of work.

Example:

```text
Login request starts
AuthService uses AppDbContext
Database query runs
Response returns
Scoped objects are disposed
```

### Viva Answer

> Scoped lifetime creates one object per HTTP request. It is suitable for business services and DbContext because they work within one request flow.

---

## 10. Singleton Lifetime

### Simple Explanation

Singleton means only one object is created for the full application lifetime.

Same object is shared by all requests.

### In Your Project

RabbitMQ publisher is registered as singleton:

File:

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

Code:

```csharp
services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
```

NotificationService also uses singleton services:

```csharp
builder.Services.AddSingleton<IHtmlEmailTemplateRenderer, HtmlEmailTemplateRenderer>();
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
```

### Why Singleton Is Used

RabbitMQ publisher and email helper services can be reused.

They do not represent one user's database transaction.

### Viva Answer

> Singleton creates one instance for the entire application. My project uses singleton for shared services like RabbitMqPublisher and NotificationService email helpers.

---

## 11. Transient Lifetime

### Simple Explanation

Transient means a new object is created every time it is requested.

### Example

```csharp
builder.Services.AddTransient<IMyService, MyService>();
```

### In Your Project

Your main service registrations mostly use scoped and singleton.

Transient is not heavily used because most project services either belong to request flow or are shared infrastructure.

### When Transient Is Useful

Use transient for lightweight stateless services where new instance creation is cheap.

### Viva Answer

> Transient lifetime creates a new object every time it is requested. My project mainly uses scoped and singleton because business services are request-based and shared infrastructure is reusable.

---

## 12. Why DbContext Is Scoped

### Simple Explanation

DbContext represents one database session/unit of work.

It tracks loaded entities and pending changes.

Because each HTTP request has its own database work, DbContext should be scoped.

### In Your Project

Each service registers DbContext:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

This registers `AppDbContext` as scoped by default.

### Why Not Singleton DbContext?

Singleton DbContext would be dangerous because:

- It would be shared by many users at the same time
- DbContext is not thread-safe
- Entity tracking data could mix across requests
- Memory usage could grow
- One user's changes could affect another request

### Viva Answer

> DbContext is scoped because each request needs its own database session. Singleton DbContext is dangerous because DbContext is not thread-safe and tracks entity state.

---

## 13. Dependency Injection Flow in AuthController

### Complete Flow

```text
1. Application starts.
2. Program.cs registers IAuthService -> AuthService.
3. Program.cs registers AppDbContext.
4. Frontend calls /api/auth/login.
5. ASP.NET Core needs to create AuthController.
6. AuthController constructor asks for IAuthService.
7. DI container creates AuthService.
8. AuthService constructor asks for AppDbContext, IConfiguration, ILogger, IMemoryCache.
9. DI container provides those dependencies.
10. AuthController calls _authService.LoginAsync(request).
```

### Viva Explanation

> During login, ASP.NET Core creates AuthController and injects IAuthService. Then it creates AuthService with AppDbContext, configuration, logger, and memory cache. This allows login logic to run without manual object creation.

---

## 14. Dependency Injection Flow in InterviewService

### Registered Dependencies

In `Backend/InterviewService/Program.cs`:

```csharp
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddHttpClient();
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
builder.Services.AddRabbitMqMessaging(builder.Configuration);
```

### Flow

```text
1. User starts interview.
2. InterviewController is created.
3. DI injects IInterviewSvc.
4. DI injects IRabbitMqPublisher.
5. InterviewSvc receives AppDbContext, IHttpClientFactory/HttpClient-related dependencies, logger, and configuration as needed.
6. Service creates interview questions, saves records, and may call Gemini API.
7. Controller publishes interview event using RabbitMQ publisher.
```

### Viva Answer

> InterviewService uses DI to inject InterviewSvc into the controller, AppDbContext into the service, HttpClient support for Gemini calls, and RabbitMQ publisher for events.

---

## 15. Dependency Injection Flow in AssessmentService

### Typed HttpClient Registration

In `Backend/AssessmentService/Program.cs`:

```csharp
builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>();
```

### Meaning

This registers:

```text
IAssessmentService -> AssessmentSvc
HttpClient for AssessmentSvc
```

### Why It Is Useful

AssessmentService uses Gemini API to generate MCQs.

HttpClient is needed for external HTTP calls.

Using `AddHttpClient` avoids manually creating `new HttpClient()` repeatedly.

### Viva Answer

> AssessmentService uses AddHttpClient<IAssessmentService, AssessmentSvc>() so the service can be injected and also receive a managed HttpClient for Gemini API calls.

---

## 16. Dependency Injection Flow in SubscriptionService

### Registered Dependencies

In `Backend/SubscriptionService/Program.cs`:

```csharp
builder.Services.AddScoped<ISubscriptionSvc, SubscriptionSvc>();
builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddHostedService<IdentityResultConsumer>();
```

### Flow

```text
1. User calls subscribe endpoint.
2. SubscriptionController is created.
3. DI injects ISubscriptionSvc.
4. DI injects IRabbitMqPublisher.
5. SubscriptionSvc uses AppDbContext to save payment/subscription records.
6. RabbitMQ publisher publishes payment or lifecycle events.
7. Hosted consumer listens for identity update results.
```

### Viva Answer

> SubscriptionService uses DI for SubscriptionSvc, DbContext, RabbitMQ publisher, and hosted consumer registration. This supports payment and premium lifecycle processing.

---

## 17. Shared RabbitMQ Dependency Registration

### Simple Explanation

RabbitMQ publisher is used by multiple services.

Instead of repeating code in every service, project uses a shared extension method.

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

> RabbitMQ dependencies are registered using a shared extension method AddRabbitMqMessaging. It registers RabbitMqOptions and singleton IRabbitMqPublisher for event publishing.

---

## 18. Options Pattern with DI

### Simple Explanation

Options pattern binds configuration sections to C# classes.

### Example

RabbitMQ:

```csharp
services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
```

Email:

```csharp
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email"));
```

### Why It Is Useful

Instead of reading raw configuration keys everywhere, services can receive strongly typed options.

Example:

```text
RabbitMqOptions
EmailOptions
```

### Viva Answer

> Options pattern uses DI to provide strongly typed configuration classes like RabbitMqOptions and EmailOptions to services.

---

## 19. ILogger Injection

### Simple Explanation

ASP.NET Core automatically provides logger objects.

### Example

```csharp
private readonly ILogger<AuthController> _logger;
```

### In Your Project

Loggers are injected into:

```text
Controllers
Services
Middleware
Consumers
```

### Why Logger Is Injected

Logger helps record:

- Register/login flow
- Assessment/interview start
- Gemini failures
- Stripe webhook processing
- RabbitMQ consumer errors
- Unexpected exceptions

### Viva Answer

> ILogger is injected by ASP.NET Core DI and is used to log important backend events, warnings, and errors.

---

## 20. IConfiguration Injection

### Simple Explanation

`IConfiguration` gives access to app settings.

### Example

AuthService needs JWT settings:

```text
Jwt:SecretKey
Jwt:Issuer
Jwt:Audience
```

### In Your Project

Configuration is used for:

```text
Connection strings
JWT settings
RabbitMQ settings
Gemini API key
Stripe settings
Email settings
Internal API key
```

### Viva Answer

> IConfiguration is injected to read settings from appsettings.json, environment variables, and other configuration sources.

---

## 21. IMemoryCache Injection

### Simple Explanation

`IMemoryCache` stores temporary data in memory.

### In Your Project

IdentityService registers:

```csharp
builder.Services.AddMemoryCache();
```

AuthService receives:

```csharp
public AuthService(
    AppDbContext context,
    IConfiguration config,
    ILogger<AuthService> logger,
    IMemoryCache memoryCache)
```

### Used For

Forgot password OTP flow:

```text
Generate OTP
Hash OTP
Store hashed OTP temporarily in memory cache
Expire OTP after configured time
```

### Viva Answer

> IMemoryCache is injected into AuthService for temporary OTP storage during forgot-password flow.

---

## 22. HttpClient and IHttpClientFactory

### Problem with Manual HttpClient

Creating `new HttpClient()` repeatedly can cause socket exhaustion and poor connection reuse.

### ASP.NET Core Solution

Use:

```csharp
builder.Services.AddHttpClient();
```

or:

```csharp
builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>();
```

### In Your Project

Used for Gemini API calls in:

```text
InterviewService
AssessmentService
```

### Viva Answer

> AddHttpClient registers HttpClient support through ASP.NET Core DI. It is better than manually creating HttpClient repeatedly and is used for Gemini API integration.

---

## 23. Hosted Services and DI

### Simple Explanation

Hosted services are background workers started by the application.

They also use DI.

### In Your Project

IdentityService:

```csharp
builder.Services.AddHostedService<SubscriptionEventConsumer>();
builder.Services.AddHostedService<UserNotificationEventConsumer>();
```

SubscriptionService:

```csharp
builder.Services.AddHostedService<IdentityResultConsumer>();
```

NotificationService:

```csharp
builder.Services.AddHostedService<EmailNotificationConsumer>();
```

### Why Hosted Services Are Used

They listen to RabbitMQ queues in background.

Example:

```text
NotificationService waits for EmailRequestedEvent
IdentityService waits for subscription events
SubscriptionService waits for identity update result
```

### Viva Answer

> Hosted services are background workers registered with DI using AddHostedService. My project uses them as RabbitMQ consumers.

---

## 24. Service Layer and DI

### Simple Explanation

Controllers should not contain heavy business logic.

They receive service classes through DI and call them.

### Controller Responsibility

```text
Receive HTTP request
Validate route/body binding
Call service method
Return response
Publish simple event if needed
```

### Service Responsibility

```text
Business rules
Database operations
Payment logic
AI evaluation logic
Question generation logic
Result calculation
```

### In Your Project

```text
AuthController -> IAuthService
InterviewController -> IInterviewSvc
AssessmentController -> IAssessmentService
SubscriptionController -> ISubscriptionSvc
```

### Viva Answer

> DI supports service layer architecture because controllers receive service interfaces and delegate business logic to service classes.

---

## 25. Loose Coupling

### Simple Explanation

Loose coupling means classes are not tightly dependent on exact implementations.

### Example

Controller depends on:

```text
IAuthService
```

not:

```text
AuthService
```

### Why It Helps

If implementation changes:

```text
AuthService -> NewAuthService
```

Controller code can remain the same.

Only registration changes:

```csharp
builder.Services.AddScoped<IAuthService, NewAuthService>();
```

### Viva Answer

> Loose coupling means classes depend on abstractions instead of concrete implementations. DI helps achieve loose coupling by injecting interfaces.

---

## 26. Testability

### Simple Explanation

DI makes testing easier because dependencies can be replaced with fake or mock objects.

### Example

During testing, instead of real RabbitMQ:

```text
Use fake IRabbitMqPublisher
```

Instead of real SQL Server:

```text
Use EF Core InMemory database
```

### In Your Project

Test projects:

```text
IdentityService.Tests
InterviewService.Tests
AssessmentService.Tests
SubscriptionService.Tests
```

### Why Interfaces Help Testing

Code depends on interfaces.

So tests can provide:

```text
Mock IAuthService
Fake IEmailSender
Fake IRabbitMqPublisher
InMemory AppDbContext
```

### Viva Answer

> DI improves testability because dependencies can be replaced with mock or fake implementations during unit tests.

---

## 27. What Happens If We Do Not Use DI?

### Without DI

Classes manually create dependencies:

```csharp
var service = new AuthService();
```

### Problems

- Tight coupling
- Difficult testing
- Repeated object creation
- Hard to manage lifetimes
- Hard to share infrastructure services
- Controllers become harder to maintain
- DbContext creation/disposal becomes risky

### Viva Answer

> Without DI, classes would manually create dependencies, causing tight coupling, difficult testing, lifetime problems, and less maintainable code.

---

## 28. Common DI Error

### Error

```text
Unable to resolve service for type IAuthService while attempting to activate AuthController
```

### Meaning

ASP.NET Core tried to create `AuthController`, but no registration exists for `IAuthService`.

### Fix

Register the dependency:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

### Viva Answer

> This error means a required dependency was not registered in the DI container. The fix is to register the interface and implementation in Program.cs.

---

## 29. Lifetime Mismatch Problem

### Simple Explanation

A longer-lived service should not directly depend on a shorter-lived service.

### Bad Example

```text
Singleton service depends on Scoped DbContext
```

### Why Bad

Singleton lives for whole application.

Scoped DbContext lives for one request.

This can cause invalid object reuse and threading issues.

### In Your Project

This is why business services and DbContext are scoped.

RabbitMQ publisher is singleton because it does not directly represent one request's DbContext.

### Viva Answer

> Lifetime mismatch happens when a singleton depends on scoped service like DbContext. This is dangerous because scoped services are request-specific.

---

## 30. Dependency Injection in NotificationService

### Simple Explanation

NotificationService is a worker service, not a Web API service.

It still uses DI.

### Program.cs

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IHtmlEmailTemplateRenderer, HtmlEmailTemplateRenderer>();
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
builder.Services.AddHostedService<EmailNotificationConsumer>();
```

### Meaning

```text
Register RabbitMQ settings
Register email settings
Register template renderer
Register email sender
Start background email consumer
```

### Viva Answer

> NotificationService uses DI even though it has no controllers. It registers email sender, template renderer, configuration options, and hosted RabbitMQ consumer.

---

## 31. Dependency Injection and Middleware

### Simple Explanation

Middleware can also receive dependencies.

### In Your Project

Global exception handling is added through shared extension methods:

```text
Backend/BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs
Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs
```

Middleware commonly uses:

```text
ILogger
RequestDelegate
```

### Why It Matters

Exception middleware can log errors and return consistent error responses.

### Viva Answer

> ASP.NET Core middleware can also use DI. My project uses shared global exception middleware for consistent error handling.

---

## 32. Dependency Injection and API Defaults

### Simple Explanation

The project also uses extension methods to register common API behavior.

### File

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Code

```csharp
builder.Services.AddApiDefaults();
```

### What It Does

It configures validation error responses using:

```csharp
services.Configure<ApiBehaviorOptions>(options => ...)
```

### Viva Answer

> AddApiDefaults is a custom extension method that registers shared API behavior in the DI service collection, such as consistent validation responses.

---

## 33. Important Code Files

### Program.cs Files

```text
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
Backend/NotificationService/Program.cs
```

### Interfaces

```text
Backend/IdentityService/Services/IAuthService.cs
Backend/InterviewService/Services/IInterviewSvc.cs
Backend/AssessmentService/Services/IAssessmentService.cs
Backend/SubscriptionService/Services/ISubscriptionSvc.cs
Backend/BuildingBlocks/Messaging/IRabbitMqPublisher.cs
Backend/NotificationService/Services/IEmailSender.cs
Backend/NotificationService/Services/IHtmlEmailTemplateRenderer.cs
```

### Implementations

```text
Backend/IdentityService/Services/AuthService.cs
Backend/InterviewService/Services/InterviewService.cs
Backend/AssessmentService/Services/AssessmentService.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
Backend/BuildingBlocks/Messaging/RabbitMqPublisher.cs
Backend/NotificationService/Services/MailKitEmailSender.cs
Backend/NotificationService/Services/HtmlEmailTemplateRenderer.cs
```

### Controllers

```text
Backend/IdentityService/Controllers/AuthController.cs
Backend/InterviewService/Controllers/InterviewController.cs
Backend/AssessmentService/Controllers/AssessmentController.cs
Backend/SubscriptionService/Controllers/SubscriptionController.cs
```

### Shared Extensions

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
Backend/BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs
```

---

## 34. Complete Flow: Login with Dependency Injection

```text
1. Application starts.
2. IdentityService Program.cs registers controllers, DbContext, memory cache, IAuthService, RabbitMQ, JWT, and hosted consumers.
3. User sends login request.
4. ASP.NET Core creates AuthController.
5. DI injects IAuthService, IRabbitMqPublisher, ILogger, and IConfiguration.
6. DI creates AuthService.
7. DI injects AppDbContext, IConfiguration, ILogger<AuthService>, and IMemoryCache into AuthService.
8. AuthController calls LoginAsync.
9. AuthService uses AppDbContext to find user.
10. AuthService verifies password and creates JWT.
11. Controller returns response.
12. Request ends and scoped services are disposed.
```

### Viva Explanation

> Login uses DI from controller to service. The controller receives IAuthService, and AuthService receives DbContext and other dependencies. This keeps the login flow clean and loosely coupled.

---

## 35. Complete Flow: Assessment with Typed HttpClient

```text
1. Application starts.
2. AssessmentService registers AddHttpClient<IAssessmentService, AssessmentSvc>().
3. User starts assessment.
4. ASP.NET Core creates AssessmentController.
5. DI injects IAssessmentService.
6. DI creates AssessmentSvc with managed HttpClient and other dependencies.
7. AssessmentSvc can call Gemini API if more MCQs are needed.
8. AssessmentSvc uses AppDbContext to save assessment data.
```

### Viva Explanation

> AssessmentService uses typed HttpClient registration so AssessmentSvc is injectable and can also make external Gemini API calls through managed HttpClient.

---

## 36. Complete Flow: Email Notification Worker

```text
1. NotificationService starts as a worker.
2. Program.cs registers EmailOptions, RabbitMqOptions, IEmailSender, template renderer, and EmailNotificationConsumer.
3. Hosted service starts automatically.
4. EmailNotificationConsumer listens to RabbitMQ queue.
5. When EmailRequestedEvent arrives, consumer uses IEmailSender.
6. MailKitEmailSender sends email using SMTP settings.
```

### Viva Explanation

> NotificationService uses DI to run background email processing. The hosted consumer receives email dependencies and processes RabbitMQ events asynchronously.

---

## 37. Dependency Injection Alternatives

### Manual Object Creation

Using `new` everywhere.

Problem:

```text
Tight coupling
Hard testing
Manual lifetime management
```

### Service Locator Pattern

Class asks a global provider for dependencies.

Problem:

```text
Hidden dependencies
Harder to understand required objects
```

### Third-Party DI Containers

Examples:

```text
Autofac
Ninject
Simple Injector
```

### In Your Project

ASP.NET Core built-in DI is enough because the dependency graph is straightforward.

### Viva Answer

> Alternatives include manual object creation, service locator, or third-party DI containers like Autofac. My project uses built-in ASP.NET Core DI because it is simple, integrated, and sufficient.

---

## 38. Possible Improvements

### Improvements

- Remove unclear comments like "FRESHLY ADDED" from production code
- Use typed HttpClient consistently for Gemini services
- Add named HttpClient policies for Gemini and internal service calls
- Add retry/timeout policies with Polly for external APIs
- Avoid injecting IConfiguration everywhere by using strongly typed options
- Add more interfaces for external API clients
- Add fake implementations for tests
- Validate service registrations during startup
- Keep singleton services stateless or thread-safe

### Balanced Viva Answer

> Current DI setup is clean because services, DbContext, RabbitMQ, HttpClient, hosted consumers, logging, and configuration are registered centrally. Future improvements could include stronger typed options, consistent typed HttpClient usage, and retry policies for external dependencies.

---

## 39. Best Full Viva Answer for Topic 10

> Dependency Injection is a design pattern where required objects are provided from outside instead of being manually created inside classes. ASP.NET Core has a built-in DI container configured in Program.cs using builder.Services. In my project, controllers receive service interfaces through constructor injection, such as IAuthService, IInterviewSvc, IAssessmentService, and ISubscriptionSvc. Program.cs maps these interfaces to implementations using AddScoped. DbContext is also registered with AddDbContext and is scoped because each request needs its own database session. Shared services like RabbitMqPublisher and email helpers are singleton because they can be reused. Hosted services like EmailNotificationConsumer are registered using AddHostedService for background RabbitMQ processing. DI makes the backend loosely coupled, testable, and easier to maintain.

---

## 40. Common Viva Questions and Answers

### Q1. What is dependency injection?

Dependency injection is a pattern where a class receives its required dependencies from outside instead of creating them manually.

### Q2. What is a dependency?

A dependency is an object required by a class to perform its work, such as IAuthService required by AuthController.

### Q3. What is DI container?

DI container is ASP.NET Core's service registry that creates and provides dependencies.

### Q4. Where do you register dependencies in ASP.NET Core?

Dependencies are registered in Program.cs using builder.Services.

### Q5. What is constructor injection?

Constructor injection means dependencies are passed through the class constructor.

### Q6. Why use interfaces in DI?

Interfaces reduce tight coupling and make it easier to replace implementations or mock dependencies in tests.

### Q7. What is AddScoped?

AddScoped creates one service instance per HTTP request.

### Q8. What is AddSingleton?

AddSingleton creates one service instance for the whole application lifetime.

### Q9. What is AddTransient?

AddTransient creates a new service instance every time it is requested.

### Q10. Why is DbContext scoped?

DbContext is scoped because it represents one request's database unit of work and is not thread-safe.

### Q11. Why should DbContext not be singleton?

A singleton DbContext would be shared across requests, causing threading issues, tracking conflicts, memory growth, and data safety problems.

### Q12. Which services are scoped in your project?

IAuthService, IInterviewSvc, ISubscriptionSvc, and DbContext are scoped. AssessmentService is registered as a typed HttpClient service.

### Q13. Which services are singleton in your project?

IRabbitMqPublisher, IEmailSender, and IHtmlEmailTemplateRenderer are registered as singleton services.

### Q14. What is AddHostedService?

AddHostedService registers a background worker that starts with the application.

### Q15. Where is AddHostedService used in your project?

It is used for RabbitMQ consumers like EmailNotificationConsumer, SubscriptionEventConsumer, UserNotificationEventConsumer, and IdentityResultConsumer.

### Q16. What is AddHttpClient?

AddHttpClient registers managed HttpClient support through IHttpClientFactory.

### Q17. Why is AddHttpClient better than new HttpClient?

It manages connection reuse and avoids problems like socket exhaustion.

### Q18. What is loose coupling?

Loose coupling means classes depend on abstractions like interfaces instead of concrete implementations.

### Q19. How does DI improve testing?

It allows real dependencies to be replaced with mocks, fakes, or in-memory implementations during tests.

### Q20. What happens if a dependency is not registered?

ASP.NET Core throws an error like "Unable to resolve service for type..." when creating the dependent class.

### Q21. What is lifetime mismatch?

Lifetime mismatch happens when a long-lived service like singleton depends on a shorter-lived scoped service like DbContext.

### Q22. Why does NotificationService use DI?

It uses DI to register email sender, template renderer, options, and hosted RabbitMQ consumer.

### Q23. How is RabbitMQ registered?

RabbitMQ is registered using AddRabbitMqMessaging, which configures RabbitMqOptions and registers IRabbitMqPublisher as singleton.

### Q24. What is Options pattern?

Options pattern binds configuration sections to strongly typed classes and provides them through DI.

### Q25. What is the main benefit of DI in your project?

DI keeps controllers, services, database access, messaging, logging, and external API clients cleanly separated and maintainable.

---

## 41. Quick Revision Summary

- Dependency means an object needed by another class.
- DI provides dependencies from outside.
- ASP.NET Core has built-in DI container.
- Dependencies are registered in Program.cs.
- Controllers use constructor injection.
- Controllers depend on interfaces, not concrete classes.
- IAuthService maps to AuthService.
- IInterviewSvc maps to InterviewSvc.
- ISubscriptionSvc maps to SubscriptionSvc.
- IAssessmentService maps to AssessmentSvc through typed HttpClient registration.
- AddScoped creates one instance per request.
- AddSingleton creates one instance for the app lifetime.
- AddTransient creates a new instance every time.
- DbContext is scoped by default.
- Singleton DbContext is dangerous.
- RabbitMqPublisher is singleton.
- NotificationService email helpers are singleton.
- Hosted consumers are registered with AddHostedService.
- HttpClient is registered with AddHttpClient.
- ILogger and IConfiguration are injected automatically.
- DI improves loose coupling, maintainability, and testing.

