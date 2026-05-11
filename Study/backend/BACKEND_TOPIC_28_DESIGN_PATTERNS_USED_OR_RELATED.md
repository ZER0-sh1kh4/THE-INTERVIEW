# Topic 28: Design Patterns Used or Related

Project: Mock Interview Platform  
Focus: Understanding design patterns and architectural patterns used in the backend, including MVC, Service Layer, Dependency Injection, Options, Middleware, API Gateway, Publisher-Subscriber, Background Worker, Saga, DTO, Repository/Unit of Work concepts through EF Core, Adapter/Facade wrappers, and standard response patterns.

---

## 1. What Is a Design Pattern?

### Simple Explanation

A design pattern is a reusable solution to a common software design problem.

It is not a copy-paste code snippet. It is an idea or structure that helps organize code.

### Example

If many classes need dependencies, instead of creating objects manually everywhere, we use Dependency Injection.

### Viva Answer

> A design pattern is a proven reusable way to solve a common design problem in software development.

---

## 2. Why Design Patterns Are Useful

### Benefits

- Make code easier to understand
- Reduce duplication
- Improve maintainability
- Improve testability
- Separate responsibilities
- Make large systems easier to extend
- Provide common vocabulary for developers

### In Your Project

Patterns help because the backend has multiple services:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
NotificationService
API Gateway
BuildingBlocks
```

Without patterns, business logic, database logic, authentication, messaging, and HTTP routing would become mixed together.

### Viva Answer

> Design patterns are useful because they provide tested ways to structure code, reduce coupling, and make the system easier to maintain and extend.

---

## 3. Architectural Pattern: Microservices

### Simple Explanation

Microservices architecture divides a large application into smaller independent services.

Each service owns a specific business capability.

### In Your Project

```text
IdentityService      -> Authentication, users, roles, premium flag
InterviewService     -> Mock interviews and interview results
AssessmentService    -> MCQ assessments and assessment results
SubscriptionService  -> Payments and subscriptions
NotificationService  -> Email notifications
API_Gateway          -> Single entry point and routing
```

### Why It Is a Pattern

Microservices is an architectural pattern, not a GoF design pattern.

It solves the problem of separating a large system into independently understandable and deployable services.

### Viva Answer

> The project follows a microservices-style architecture where different backend services handle separate business capabilities such as identity, interviews, assessments, subscriptions, and notifications.

---

## 4. MVC Pattern

### Simple Explanation

MVC means Model-View-Controller.

In Web API projects, there is usually no server-rendered View. The controller returns JSON to the frontend.

### Parts

| Part | Role in Web API |
|---|---|
| Model | Entity classes such as User, Interview, AssessmentResult |
| Controller | Receives HTTP requests and returns responses |
| View | Angular frontend displays the data |

### In Your Project

Controllers:

```text
AuthController
InterviewController
AssessmentController
SubscriptionController
NotificationController
```

Models:

```text
User
Interview
AssessmentResult
Subscription
PaymentRecord
Notification
```

### Viva Answer

> The backend uses the MVC/Web API pattern where controllers receive HTTP requests, models represent data, and JSON responses are consumed by the Angular frontend.

---

## 5. Controller Pattern

### Simple Explanation

A controller acts as an entry point for HTTP requests.

It should not contain heavy business logic.

### In Your Project

Example:

```text
POST /api/auth/login
```

goes to:

```text
AuthController.Login()
```

The controller calls:

```text
IAuthService.LoginAsync()
```

### Controller Responsibilities

- Read request body
- Read route/query parameters
- Read user claims
- Call service layer
- Return HTTP response
- Publish simple events when needed

### Viva Answer

> Controllers act as API entry points. They receive HTTP requests, call services for business logic, and return JSON responses.

---

## 6. Service Layer Pattern

### Simple Explanation

The Service Layer pattern keeps business logic in service classes instead of controllers.

### In Your Project

Service interfaces and implementations:

```text
IAuthService        -> AuthService
IInterviewSvc       -> InterviewSvc
IAssessmentService  -> AssessmentSvc
ISubscriptionSvc    -> SubscriptionSvc
IEmailSender        -> MailKitEmailSender
```

### Example

`InterviewController` does not directly generate questions or calculate results.

It calls:

```text
InterviewSvc
```

### Benefits

- Controllers stay thin
- Business logic is reusable
- Unit testing becomes easier
- Dependencies are centralized
- Code is easier to maintain

### Viva Answer

> The Service Layer pattern is used by placing business logic in service classes such as AuthService, InterviewSvc, AssessmentSvc, and SubscriptionSvc, while controllers mainly handle HTTP concerns.

---

## 7. Interface-Based Design

### Simple Explanation

Interfaces define what a service can do without depending on the exact implementation.

### Example

```csharp
public interface IInterviewSvc
{
    Task<Interview> StartInterviewAsync(int userId, bool isPremium, StartInterviewRequest request);
    Task<object?> SubmitInterviewAsync(int userId, bool isPremium, SubmitInterviewRequest request);
}
```

### In Your Project

Interfaces are used for:

```text
IAuthService
IInterviewSvc
IAssessmentService
ISubscriptionSvc
IRabbitMqPublisher
IEmailSender
IHtmlEmailTemplateRenderer
```

### Why It Helps

- Reduces coupling
- Supports dependency injection
- Makes mocking easier in tests
- Allows implementation replacement

### Viva Answer

> Interface-based design allows controllers and services to depend on abstractions instead of concrete classes, improving testability and flexibility.

---

## 8. Dependency Injection Pattern

### Simple Explanation

Dependency Injection means a class receives its dependencies from outside instead of creating them itself.

### In Your Project

Example:

```csharp
public InterviewController(IInterviewSvc service, IRabbitMqPublisher rabbitMqPublisher, ILogger<InterviewController> logger)
```

The controller does not create these objects manually.

ASP.NET Core DI provides them.

### Registration Example

```csharp
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
builder.Services.AddRabbitMqMessaging(builder.Configuration);
```

### Benefits

- Loose coupling
- Easier unit testing
- Centralized object creation
- Clean constructor dependencies

### Viva Answer

> Dependency Injection is used throughout the project. Controllers and services receive dependencies such as service interfaces, DbContext, loggers, configuration, and RabbitMQ publishers through constructors.

---

## 9. Inversion of Control

### Simple Explanation

Inversion of Control means the framework controls object creation and lifecycle instead of the class doing it manually.

### In Your Project

ASP.NET Core creates:

- Controllers
- Services
- DbContext
- Loggers
- Background services
- Options objects

### Relation With DI

Dependency Injection is one way to implement Inversion of Control.

### Viva Answer

> Inversion of Control means ASP.NET Core manages object creation and lifetimes. Dependency Injection is the mechanism used to achieve it.

---

## 10. Options Pattern

### Simple Explanation

The Options pattern maps configuration sections to strongly typed C# classes.

### In Your Project

Used for:

```text
RabbitMqOptions
EmailOptions
```

Registration:

```csharp
services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
```

Usage:

```csharp
public EmailNotificationConsumer(IOptions<RabbitMqOptions> rabbitMqOptions, ...)
```

### Benefits

- Type-safe configuration
- Cleaner than repeated string lookups
- Centralized defaults
- Easier testing

### Viva Answer

> The Options pattern is used to bind configuration sections like RabbitMq and Email to strongly typed classes such as RabbitMqOptions and EmailOptions.

---

## 11. Middleware Pattern

### Simple Explanation

Middleware is a component in the HTTP request pipeline.

Each middleware can process the request before and after the next component.

### In Your Project

Global exception handling uses middleware:

```text
GlobalExceptionMiddleware
```

It is added using:

```csharp
app.UseGlobalExceptionHandling();
```

### Flow

```text
Request
GlobalExceptionMiddleware
Authentication
Authorization
Controller
Response
```

### Viva Answer

> Middleware pattern is used for cross-cutting concerns. In this project, GlobalExceptionMiddleware catches exceptions and converts them into consistent JSON error responses.

---

## 12. Extension Method Pattern

### Simple Explanation

Extension methods add reusable helper methods to existing types.

### In Your Project

Shared registration methods:

```text
AddRabbitMqMessaging()
AddApiDefaults()
UseGlobalExceptionHandling()
```

Example:

```csharp
builder.Services.AddRabbitMqMessaging(builder.Configuration);
app.UseGlobalExceptionHandling();
```

### Why It Helps

- Reduces repeated setup code
- Keeps Program.cs cleaner
- Makes common behavior consistent across services

### Viva Answer

> Extension methods are used to package common setup code, such as RabbitMQ registration and global exception middleware registration.

---

## 13. Repository Pattern: Related Through EF Core

### Simple Explanation

Repository pattern hides data access behind methods like `Add`, `Find`, `GetAll`, and `Save`.

### In Your Project

There is no separate custom repository class.

Instead, EF Core `DbContext` and `DbSet` provide repository-like behavior.

Example:

```csharp
_context.Users.Add(user);
await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
await _context.SaveChangesAsync();
```

### Important Viva Point

Do not say:

```text
We fully implemented custom Repository pattern.
```

Better answer:

```text
We use EF Core DbContext and DbSet, which already provide repository-like data access.
```

### Viva Answer

> The project does not implement separate repository classes. EF Core DbContext and DbSet act as repository-like abstractions for querying and saving entities.

---

## 14. Unit of Work Pattern: Related Through DbContext

### Simple Explanation

Unit of Work tracks multiple changes and commits them together.

### In Your Project

EF Core `DbContext` works like a Unit of Work.

Example:

```csharp
_context.Users.Add(user);
_context.Notifications.Add(notification);
await _context.SaveChangesAsync();
```

`SaveChangesAsync()` commits tracked changes in one database transaction.

### Viva Answer

> EF Core DbContext acts as a Unit of Work because it tracks entity changes and commits them together using SaveChangesAsync.

---

## 15. DTO Pattern

### Simple Explanation

DTO means Data Transfer Object.

DTOs define the shape of data sent between frontend and backend.

### In Your Project

Examples:

```text
RegisterRequest
LoginRequest
StartInterviewRequest
SubmitInterviewRequest
StartAssessmentRequest
SubmitAssessmentRequest
ConfirmPaymentRequest
```

### Why DTOs Are Used

- Avoid exposing database entities directly
- Validate incoming data
- Keep API contracts stable
- Send only required fields
- Separate external API shape from internal model

### Viva Answer

> DTOs are used to transfer data between frontend and backend without exposing internal entity models directly.

---

## 16. Standard Response Wrapper Pattern

### Simple Explanation

A response wrapper gives all successful API responses a consistent structure.

### In Your Project

`ApiResponse<T>` is used:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
}
```

Example response:

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "..."
  }
}
```

### Viva Answer

> The project uses a standard API response wrapper so frontend receives consistent success, message, and data fields from different services.

---

## 17. Standard Error Response Pattern

### Simple Explanation

Error responses should also be consistent.

### In Your Project

`GlobalExceptionMiddleware` converts exceptions to:

```text
ApiErrorResponse
```

It handles:

- Application exceptions
- Unauthorized access
- Unexpected exceptions

### Benefits

- Frontend can handle errors consistently
- Controllers do not need repeated try-catch everywhere
- Logs are centralized

### Viva Answer

> The project uses global exception middleware to convert thrown exceptions into consistent JSON error responses.

---

## 18. Publisher-Subscriber Pattern

### Simple Explanation

Publisher-Subscriber means one part publishes an event, and other parts consume it without direct coupling.

### In Your Project

RabbitMQ implements this pattern.

Publishers:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

Consumers:

```text
NotificationService
IdentityService background consumers
SubscriptionService background consumers
```

### Example

After user registration:

```text
IdentityService publishes UserRegisteredEvent
NotificationService consumes email event
```

### Benefits

- Services are loosely coupled
- One service does not directly call every other service
- Background processing becomes possible
- Failures can be retried

### Viva Answer

> RabbitMQ is used to implement a Publisher-Subscriber style pattern where services publish events and other services consume them asynchronously.

---

## 19. Observer Pattern: Related Concept

### Simple Explanation

Observer pattern means observers react when a subject changes or emits an event.

### Relation to Your Project

RabbitMQ event consumers behave like distributed observers.

Example:

```text
Payment succeeded event -> Subscription/Notification behavior reacts
Interview completed event -> Notification email reacts
Assessment completed event -> Notification email reacts
```

### Viva Answer

> The project uses an event-driven approach related to the Observer pattern, where consumers react to events published through RabbitMQ.

---

## 20. Background Worker Pattern

### Simple Explanation

A background worker runs continuously outside normal HTTP request handling.

### In Your Project

Background workers inherit from:

```text
BackgroundService
```

Examples:

```text
EmailNotificationConsumer
IdentityResultConsumer
SubscriptionEventConsumer
UserNotificationEventConsumer
```

### Why It Is Used

- Process RabbitMQ messages
- Send emails asynchronously
- Complete subscription saga steps
- Update notifications
- Avoid blocking HTTP requests

### Viva Answer

> BackgroundService is used for long-running message consumers that process RabbitMQ events outside the normal HTTP request-response flow.

---

## 21. Producer-Consumer Pattern

### Simple Explanation

Producer-Consumer means one part creates work and another part processes it.

### In Your Project

Producer:

```text
Controller or service publishes RabbitMQ event
```

Consumer:

```text
BackgroundService reads event and processes it
```

### Example

```text
AssessmentController publishes EmailRequestedEvent
EmailNotificationConsumer sends email
```

### Viva Answer

> RabbitMQ messaging follows a Producer-Consumer pattern where services produce events and background consumers process them asynchronously.

---

## 22. Saga Pattern

### Simple Explanation

Saga pattern manages a business process that spans multiple services.

Instead of one database transaction across services, each service performs a local transaction and publishes events.

### In Your Project

Subscription premium activation is saga-like.

Flow:

```text
Payment succeeds
SubscriptionService creates/updates subscription
SubscriptionService publishes premium activation event
IdentityService updates user's premium flag
IdentityService publishes result event
SubscriptionService updates SagaState
```

### Saga State

Subscription records store state such as:

```text
PendingIdentityUpdate
Completed
CancelledCompleted
CompensationRequired
Failed
```

### Viva Answer

> The subscription flow uses a saga-like pattern because premium activation spans SubscriptionService and IdentityService through local updates and RabbitMQ events instead of one distributed transaction.

---

## 23. API Gateway Pattern

### Simple Explanation

API Gateway provides one entry point for frontend requests.

### In Your Project

The Angular frontend calls:

```text
http://localhost:5190/api
```

The gateway routes to downstream services using Ocelot.

### Benefits

- Frontend does not need to know every service port
- Central place for routing
- Central JWT validation for protected routes
- Gateway Swagger aggregation
- Supports cross-cutting policies

### Viva Answer

> The API Gateway pattern is used through Ocelot. The frontend calls one gateway, and the gateway forwards requests to the correct backend microservice.

---

## 24. Proxy Pattern

### Simple Explanation

Proxy pattern means one object or component stands in front of another and controls access to it.

### In Your Project

The API Gateway acts like a reverse proxy.

```text
Frontend -> API Gateway -> IdentityService
Frontend -> API Gateway -> InterviewService
```

The frontend does not directly call every microservice.

### Viva Answer

> The API Gateway works like a reverse proxy by accepting frontend requests and forwarding them to downstream services.

---

## 25. Adapter Pattern

### Simple Explanation

Adapter pattern wraps an external library or API behind your own interface.

### In Your Project

Examples:

```text
IEmailSender -> MailKitEmailSender
IRabbitMqPublisher -> RabbitMqPublisher
```

`MailKitEmailSender` adapts MailKit to your application's email interface.

`RabbitMqPublisher` adapts RabbitMQ.Client to your application's publisher interface.

### Why It Helps

- Hides third-party library details
- Makes code easier to test
- Allows replacement later
- Keeps controllers/services cleaner

### Viva Answer

> Adapter pattern is used conceptually where classes like MailKitEmailSender and RabbitMqPublisher wrap external libraries behind application-specific interfaces.

---

## 26. Facade Pattern

### Simple Explanation

Facade pattern provides a simpler interface over complex operations.

### In Your Project

Service classes act as facades for complex business flows.

Example:

```text
SubscriptionSvc.SubscribeAsync()
```

internally handles:

- Stripe configuration
- Checkout session creation
- Payment record creation
- Database save
- RabbitMQ event publish

The controller only calls one method.

### Viva Answer

> Service classes act like facades by exposing simple methods to controllers while hiding complex workflows such as Stripe checkout or interview evaluation.

---

## 27. Strategy Pattern: Related Concept

### Simple Explanation

Strategy pattern means choosing between different algorithms or behaviors at runtime.

### In Your Project

There is not a full formal Strategy implementation with multiple strategy classes.

But there are strategy-like decisions:

```text
Stripe enabled -> real Stripe checkout
Stripe disabled -> simulated local payment
```

Also:

```text
Premium user -> more detailed result access
Free user -> limited access
```

### Viva Answer

> The project has strategy-like conditional behavior, such as real Stripe mode versus simulated mode, but it does not implement a full Strategy pattern with separate strategy classes.

---

## 28. Factory Pattern: Related Through DI

### Simple Explanation

Factory pattern centralizes object creation.

### In Your Project

There are no explicit custom factory classes for most services.

ASP.NET Core DI container acts as an object factory.

Example:

```text
Controller needs IInterviewSvc
DI creates InterviewSvc and injects it
```

### Viva Answer

> The project does not use many custom factory classes, but the ASP.NET Core DI container acts as a factory for controllers, services, DbContext, loggers, and background workers.

---

## 29. Singleton-Like Shared Services

### Simple Explanation

Singleton means one instance is reused for the application lifetime.

### In Your Project

RabbitMQ publisher is registered as singleton:

```csharp
services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
```

This makes sense because the publisher manages RabbitMQ connection/channel resources.

### Important Note

Singleton should be used carefully. A singleton should not directly depend on scoped services like `DbContext`.

### Viva Answer

> RabbitMqPublisher is registered as a singleton so the application can reuse RabbitMQ publishing infrastructure instead of creating it for every request.

---

## 30. Scoped Service Lifetime Pattern

### Simple Explanation

Scoped services are created once per HTTP request.

### In Your Project

Business services and DbContext are generally scoped:

```csharp
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
builder.Services.AddDbContext<AppDbContext>(...);
```

### Why Scoped Is Good for DbContext

Each request gets its own DbContext, avoiding concurrency issues between requests.

### Viva Answer

> Scoped lifetime is used for request-level services such as business services and DbContext, so each request gets its own safe working instance.

---

## 31. Template Method: Related Through BackgroundService

### Simple Explanation

Template Method defines a base workflow and lets derived classes implement specific steps.

### In Your Project

`BackgroundService` provides the hosting lifecycle.

Your consumers override:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
```

### Viva Answer

> BackgroundService is related to the Template Method pattern because the framework controls the worker lifecycle and our class overrides ExecuteAsync to define the actual background work.

---

## 32. Decorator Pattern: Related Through Middleware

### Simple Explanation

Decorator pattern adds behavior around another object without changing it.

### Relation to Middleware

ASP.NET Core middleware wraps request handling.

Example:

```text
GlobalExceptionMiddleware wraps controller execution
```

It adds exception handling around the rest of the pipeline.

### Viva Answer

> Middleware is related to the Decorator pattern because each middleware wraps the next component and adds behavior like exception handling, authentication, or CORS.

---

## 33. State Pattern: Related Through SagaState

### Simple Explanation

State pattern changes behavior based on an object's current state.

### In Your Project

Subscription records store a `SagaState`.

Examples:

```text
PendingIdentityUpdate
Completed
CompensationRequired
CancelledCompleted
```

The code updates the subscription based on event results.

### Important Point

This is not a full formal State pattern with separate state classes.

It is state-based workflow logic.

### Viva Answer

> The subscription saga uses state-based workflow values like PendingIdentityUpdate and Completed, which is related to the State pattern, but not a full State pattern implementation.

---

## 34. Idempotency Pattern

### Simple Explanation

Idempotency means processing the same request or event multiple times should not create duplicate harmful effects.

### In Your Project

Stripe webhooks can be delivered more than once.

`SubscriptionService` stores webhook event logs:

```text
WebhookEventLogs
```

If an event was already processed, duplicate webhook events are ignored.

### Viva Answer

> The Stripe webhook flow uses idempotency by storing processed webhook event IDs and ignoring duplicate events.

---

## 35. Retry and Dead-Letter Pattern

### Simple Explanation

Retry pattern tries failed work again.

Dead-letter pattern stores messages that cannot be processed after retries.

### In Your Project

RabbitMQ consumers retry failed messages using:

```text
x-retry-count
RetryCount
RetryDelaySeconds
```

After max retries, messages go to a dead-letter queue.

### Viva Answer

> RabbitMQ consumers use retry and dead-letter handling so temporary failures can be retried and permanently failing messages are not lost silently.

---

## 36. Separation of Concerns

### Simple Explanation

Separation of Concerns means each part of the system has one clear responsibility.

### In Your Project

```text
Controller        -> HTTP handling
Service           -> Business logic
DbContext         -> Database access
Middleware        -> Cross-cutting errors
Gateway           -> Routing
RabbitMQ          -> Messaging
NotificationSvc   -> Email sending
DTOs              -> API contracts
```

### Viva Answer

> The project follows separation of concerns by keeping HTTP logic, business logic, data access, messaging, configuration, and error handling in different layers or components.

---

## 37. Patterns Actually Used vs Related

### Actually Used Clearly

```text
Microservices architecture
MVC/Web API
Service Layer
Dependency Injection
Options Pattern
Middleware
API Gateway
Publisher-Subscriber
Producer-Consumer
Background Worker
DTO
Response Wrapper
Unit of Work through EF Core DbContext
Adapter-like wrappers
Saga-like subscription workflow
Retry and Dead-Letter messaging
```

### Related but Not Fully Formal

```text
Repository Pattern through EF Core DbSet
Strategy-like mode switching
Factory through DI container
State-like saga state values
Decorator-like middleware pipeline
Observer-like event consumers
Template Method through BackgroundService
```

### Viva Answer

> Some patterns are directly implemented, such as DI, Service Layer, Middleware, Options, API Gateway, and Pub-Sub. Others are related concepts, such as EF Core acting like Repository and Unit of Work, or RabbitMQ consumers behaving like distributed observers.

---

## 38. Important Files in Your Project

### Service Layer and Interfaces

```text
Backend/IdentityService/Services/IAuthService.cs
Backend/IdentityService/Services/AuthService.cs
Backend/InterviewService/Services/IInterviewSvc.cs
Backend/InterviewService/Services/InterviewService.cs
Backend/AssessmentService/Services/IAssessmentService.cs
Backend/AssessmentService/Services/AssessmentService.cs
Backend/SubscriptionService/Services/ISubscriptionService.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
```

### Middleware and Shared Contracts

```text
Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs
Backend/BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs
Backend/BuildingBlocks/Contracts/ApiResponse.cs
```

### Messaging

```text
Backend/BuildingBlocks/Messaging/RabbitMqPublisher.cs
Backend/BuildingBlocks/Messaging/IRabbitMqPublisher.cs
Backend/BuildingBlocks/Messaging/Events
Backend/NotificationService/Messaging/EmailNotificationConsumer.cs
Backend/SubscriptionService/Messaging/IdentityResultConsumer.cs
```

### Gateway

```text
Backend/API_Gateway/Program.cs
Backend/API_Gateway/ocelot.json
```

---

## 39. Best Full Viva Answer

> In our Mock Interview Platform, we use several design and architectural patterns. At the architecture level, the system follows a microservices style, with separate services for identity, interviews, assessments, subscriptions, notifications, and an API Gateway. The API Gateway pattern is implemented using Ocelot, so the frontend calls one entry point and the gateway routes requests to downstream services.
>
> Inside each service, we use the Web API/MVC pattern with controllers, models, DTOs, and services. Controllers handle HTTP requests and delegate business logic to the Service Layer, such as AuthService, InterviewSvc, AssessmentSvc, and SubscriptionSvc. These services are injected through interfaces using Dependency Injection, which improves loose coupling and testability.
>
> EF Core DbContext acts like Repository and Unit of Work because it exposes DbSet collections for querying and tracks changes until SaveChangesAsync commits them. We also use the Options pattern for strongly typed configuration like RabbitMqOptions and EmailOptions, and Middleware pattern for global exception handling.
>
> For asynchronous communication, RabbitMQ implements Publisher-Subscriber and Producer-Consumer patterns. Services publish events, and background workers consume them using BackgroundService. The subscription premium activation process is saga-like because it spans SubscriptionService and IdentityService through local updates and events instead of one distributed transaction.
>
> Some patterns are directly implemented, while others are related concepts. For example, we do not have custom repository classes, but EF Core provides repository-like behavior. We do not have formal Strategy classes, but configuration chooses between real Stripe mode and simulated mode.

---

## 40. Common Viva Questions and Answers

### Q1. What is a design pattern?

A design pattern is a reusable solution to a common software design problem.

### Q2. Which design patterns are used in your project?

The project uses Service Layer, Dependency Injection, Options, Middleware, API Gateway, Publisher-Subscriber, Producer-Consumer, DTO, Response Wrapper, Background Worker, and Saga-like workflow patterns.

### Q3. Is microservices a design pattern?

Microservices is an architectural pattern, not a small object-oriented design pattern.

### Q4. What is the Service Layer pattern?

It places business logic in service classes instead of controllers.

### Q5. Why use interfaces with services?

Interfaces reduce coupling and make services easier to mock or replace.

### Q6. What is Dependency Injection?

Dependency Injection means classes receive their dependencies from the framework instead of creating them manually.

### Q7. How does EF Core relate to Repository pattern?

EF Core DbSet provides repository-like behavior, so the project does not need separate repository classes for basic CRUD.

### Q8. How does EF Core relate to Unit of Work?

DbContext tracks changes and commits them together using SaveChangesAsync, which is Unit of Work behavior.

### Q9. Which pattern is used for RabbitMQ messaging?

RabbitMQ supports Publisher-Subscriber and Producer-Consumer patterns.

### Q10. Which pattern is used for global exception handling?

Middleware pattern is used through GlobalExceptionMiddleware.

### Q11. What is the API Gateway pattern?

It provides a single entry point that routes client requests to internal microservices.

### Q12. What is the Saga pattern in your project?

The subscription activation flow is saga-like because it updates subscription state and user premium status across services using events.

### Q13. What is the Adapter pattern example?

MailKitEmailSender adapts MailKit behind IEmailSender, and RabbitMqPublisher adapts RabbitMQ.Client behind IRabbitMqPublisher.

### Q14. What is the DTO pattern?

DTOs define request and response shapes without exposing internal database entities directly.

### Q15. What is idempotency in your project?

Stripe webhook processing stores event IDs so duplicate webhook events are ignored.

---

## 41. Quick Revision Summary

```text
Design pattern = reusable solution to common design problem.

Microservices = architectural pattern.

MVC/Web API = controllers + models + JSON responses.

Service Layer = business logic in services.

Interfaces = depend on abstractions.

DI = framework injects dependencies.

Options Pattern = bind config to classes.

Middleware = cross-cutting HTTP pipeline behavior.

EF DbSet = repository-like.

EF DbContext = unit-of-work-like.

DTO = API data contract.

ApiResponse<T> = standard success response wrapper.

GlobalExceptionMiddleware = standard error response.

RabbitMQ = pub-sub and producer-consumer.

BackgroundService = async consumers.

Subscription flow = saga-like.

API Gateway = single frontend entry point.

Adapter examples = RabbitMqPublisher, MailKitEmailSender.

Retry + DLQ = reliability pattern.
```

---

## 42. One-Line Viva Answer

> The project uses patterns like Service Layer, Dependency Injection, Options, Middleware, API Gateway, DTOs, Pub-Sub messaging, Background Workers, EF Core Unit of Work behavior, and a saga-like subscription flow to keep the backend modular, testable, and maintainable.
