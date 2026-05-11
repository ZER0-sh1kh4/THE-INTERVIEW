# Backend Study Table of Contents

This file is the study roadmap for the backend. We will use it to go service by service, file by file, and then line by line.

## 0. How To Study This Backend

### 0.1 First Pass: Big Picture
- What problem the backend solves
- Microservice layout and responsibilities
- Request flow from frontend to API gateway to service
- Authentication, authorization, JWT, and admin-only endpoints
- Database access through Entity Framework Core
- RabbitMQ event-driven communication
- Background consumers and notification flow
- Error handling and standard API responses
- Tests and how they protect service behavior

### 0.2 Second Pass: Each Service Internals
- `Program.cs` startup pipeline
- Controllers and HTTP routes
- DTOs and request/response shapes
- Service interfaces and implementations
- EF Core models and `DbContext`
- Messaging publishers/consumers
- Configuration files
- Migrations
- Unit tests

### 0.3 Suggested Reading Order
1. Solution and dependency setup
2. Shared `BuildingBlocks`
3. `API_Gateway`
4. `IdentityService`
5. `SubscriptionService`
6. `AssessmentService`
7. `InterviewService`
8. `NotificationService`
9. Tests
10. Migrations and generated files

## 1. Backend Solution Overview

### 1.1 Solution Files
- `SPRINT_INTERVIEW.sln` - Visual Studio solution containing backend projects.
- `SPRINT_INTERVIEW.slnx` - newer solution format.
- `Directory.Packages.props` - centralized NuGet package versions.
- `.gitignore` - ignored backend files.

### 1.2 Main Projects
| Project | Type | Responsibility |
|---|---|---|
| `API_Gateway` | ASP.NET Core gateway | Routes frontend API calls to backend services through Ocelot. |
| `IdentityService` | ASP.NET Core API | Registration, login, JWT claims, profile, admin user management, notifications. |
| `SubscriptionService` | ASP.NET Core API | Premium subscription lifecycle, Stripe payment confirmation, webhooks, payment history. |
| `AssessmentService` | ASP.NET Core API | MCQ assessments, question generation, submissions, results, admin question CRUD. |
| `InterviewService` | ASP.NET Core API | Mock interview creation, AI-generated questions, answers, evaluation, results. |
| `NotificationService` | Worker service | Consumes email notification events and sends email through MailKit. |
| `BuildingBlocks` | Class library | Shared contracts, exceptions, middleware, RabbitMQ publisher, integration events. |

### 1.3 Test Projects
| Project | Covers |
|---|---|
| `IdentityService.Tests` | Authentication service behavior. |
| `SubscriptionService.Tests` | Subscription/payment service behavior and event publishing. |
| `AssessmentService.Tests` | Assessment service behavior and AI question generation helpers. |
| `InterviewService.Tests` | Interview service behavior and AI question/evaluation helpers. |

## 2. Cross-Cutting Architecture Topics

### 2.1 HTTP Request Lifecycle
- Gateway receives request.
- Ocelot matches route in `ocelot.json`.
- Target service validates JWT when needed.
- Controller validates route/body/user claims.
- Service layer performs business logic.
- EF Core reads/writes SQL Server.
- RabbitMQ events are published when needed.
- Global exception middleware formats failures.

### 2.2 Authentication And Authorization
- JWT bearer authentication in each API service.
- Shared issuer, audience, and signing key configuration.
- `[Authorize]` and admin authorization paths.
- Swagger bearer token support through `AuthorizeOperationFilter`.
- Premium claim refresh flow after subscription updates.

### 2.3 Data Access
- EF Core `DbContext` per service.
- SQL Server provider.
- Service-specific tables and migrations.
- In-memory EF Core provider in unit tests.

### 2.4 Messaging
- Shared `RabbitMqPublisher`.
- Queue names centralized in `QueueNames`.
- Integration events in `BuildingBlocks/Messaging/Events`.
- Background consumers for subscriptions, identity updates, user notifications, and email.
- Dead-letter queue handling in consumers.

### 2.5 Error Handling
- `AppException` base class.
- Typed app exceptions for validation, not found, and forbidden cases.
- `GlobalExceptionMiddleware`.
- Standard response contracts.

## 3. BuildingBlocks Project

### 3.1 Project And Purpose
- `BuildingBlocks/BuildingBlocks.csproj` - shared class library project file.

### 3.2 Contracts
- `BuildingBlocks/Contracts/ApiResponse.cs` - generic successful API response wrapper.
- `BuildingBlocks/Contracts/ApiErrorResponse.cs` - error response shape.

### 3.3 Exceptions
- `BuildingBlocks/Exceptions/AppException.cs` - base application exception.
- `BuildingBlocks/Exceptions/ValidationAppException.cs` - validation failure exception.
- `BuildingBlocks/Exceptions/NotFoundAppException.cs` - missing resource exception.
- `BuildingBlocks/Exceptions/ForbiddenAppException.cs` - authorization/business permission exception.

### 3.4 Middleware
- `BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs` - catches exceptions and maps them to HTTP responses.

### 3.5 Extensions
- `BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs` - registers global exception middleware in the app pipeline.
- `BuildingBlocks/Extensions/ServiceCollectionExtensions.cs` - registers shared API defaults and RabbitMQ services.

### 3.6 Messaging Core
- `BuildingBlocks/Messaging/IRabbitMqPublisher.cs` - publisher abstraction.
- `BuildingBlocks/Messaging/RabbitMqPublisher.cs` - RabbitMQ exchange/queue publisher implementation.
- `BuildingBlocks/Messaging/RabbitMqOptions.cs` - RabbitMQ configuration options.
- `BuildingBlocks/Messaging/QueueNames.cs` - exchange, queues, and dead-letter names.

### 3.7 Integration Events
- `BuildingBlocks/Messaging/Events/IntegrationEvent.cs` - base event contract.
- `BuildingBlocks/Messaging/Events/UserRegisteredEvent.cs` - user registration event.
- `BuildingBlocks/Messaging/Events/UserClaimsRefreshedEvent.cs` - updated user claims event.
- `BuildingBlocks/Messaging/Events/SubscriptionLifecycleEvent.cs` - subscription state event.
- `BuildingBlocks/Messaging/Events/PaymentSucceededEvent.cs` - payment success event.
- `BuildingBlocks/Messaging/Events/AssessmentStartedEvent.cs` - assessment started event.
- `BuildingBlocks/Messaging/Events/AssessmentCompletedEvent.cs` - assessment completed event.
- `BuildingBlocks/Messaging/Events/InterviewStartedEvent.cs` - interview started event.
- `BuildingBlocks/Messaging/Events/InterviewCompletedEvent.cs` - interview completed event.
- `BuildingBlocks/Messaging/Events/EmailRequestedEvent.cs` - email request event.

## 4. API_Gateway Project

### 4.1 Gateway Purpose
- Central entry point for frontend API calls.
- JWT validation at gateway level.
- Ocelot route forwarding.
- Swagger aggregation through SwaggerForOcelot.
- CORS configuration.

### 4.2 Files To Study
- `API_Gateway/API_Gateway.csproj` - gateway dependencies: Ocelot, Polly, Swagger, JWT bearer.
- `API_Gateway/Program.cs` - gateway startup, authentication, CORS, SwaggerForOcelot, Ocelot middleware.
- `API_Gateway/ocelot.json` - route table from gateway paths to downstream services.
- `API_Gateway/appsettings.json` - runtime gateway configuration.
- `API_Gateway/appsettings.Example.json` - safe sample configuration.
- `API_Gateway/appsettings.Development.json` - development settings.
- `API_Gateway/Properties/launchSettings.json` - local launch profile.
- `API_Gateway/WeatherForecast.cs` - template leftover/simple model.

### 4.3 Subtopics
- Ocelot route matching.
- Downstream host/port mapping.
- Authentication scheme naming.
- Swagger endpoint aggregation.
- CORS policy.

## 5. IdentityService Project

### 5.1 Service Responsibility
- User registration and login.
- Password hashing and verification.
- JWT creation and claim refresh.
- Forgot-password OTP flow.
- User profile updates.
- Admin user management.
- In-app notification storage.
- Subscription lifecycle event consumption.
- Assessment/interview event consumption for notifications.

### 5.2 Startup And Configuration
- `IdentityService/IdentityService.csproj` - dependencies: BCrypt, JWT bearer, EF Core SQL Server, RabbitMQ, Swagger.
- `IdentityService/Program.cs` - DI registration, EF Core, auth, hosted consumers, Swagger, middleware pipeline.
- `IdentityService/appsettings.json` - local runtime configuration.
- `IdentityService/appsettings.Example.json` - safe sample configuration.
- `IdentityService/appsettings.Development.json` - development settings.
- `IdentityService/Properties/launchSettings.json` - launch profile.

### 5.3 Controllers
- `IdentityService/Controllers/AuthController.cs` - auth, profile, admin user, internal premium endpoints.
- `IdentityService/Controllers/NotificationController.cs` - user notification read/list endpoints.

### 5.4 Services
- `IdentityService/Services/IAuthService.cs` - authentication service contract.
- `IdentityService/Services/AuthService.cs` - auth logic, password hashing, JWT tokens, OTP cache, profile/admin operations.
- `IdentityService/Services/AuthorizeOperationFilter.cs` - Swagger auth lock icon/support.

### 5.5 Data And Models
- `IdentityService/Data/AppDbContext.cs` - EF Core user and notification database context.
- `IdentityService/Models/User.cs` - user entity.
- `IdentityService/Models/Notification.cs` - notification entity.
- `IdentityService/DTOs/AuthDTOs.cs` - auth, profile, OTP, admin DTOs.

### 5.6 Messaging
- `IdentityService/Messaging/SubscriptionEventConsumer.cs` - consumes subscription lifecycle events, updates premium state, publishes results.
- `IdentityService/Messaging/UserNotificationEventConsumer.cs` - consumes interview/assessment events and creates user notifications.

### 5.7 Migrations
- `IdentityService/Migrations/20260430185917_InitialCase.cs`
- `IdentityService/Migrations/20260430185917_InitialCase.Designer.cs`
- `IdentityService/Migrations/20260501190807_AddNotificationsTable.cs`
- `IdentityService/Migrations/20260501190807_AddNotificationsTable.Designer.cs`
- `IdentityService/Migrations/AppDbContextModelSnapshot.cs`

### 5.8 Key Study Subtopics
- Register flow and event publishing.
- Login flow and JWT payload.
- OTP generation and validation.
- Admin role checks.
- Premium claim refresh.
- RabbitMQ consumer retry/dead-letter logic.
- Notification creation and read tracking.

## 6. SubscriptionService Project

### 6.1 Service Responsibility
- Premium subscription creation.
- Stripe checkout/session confirmation.
- Stripe webhook processing.
- Payment records.
- Subscription cancellation.
- Admin subscription/payment listing.
- Subscription saga interaction with IdentityService.
- Email notification event publishing.

### 6.2 Startup And Configuration
- `SubscriptionService/SubscriptionService.csproj` - dependencies: JWT bearer, EF Core SQL Server, Stripe, RabbitMQ, Swagger.
- `SubscriptionService/Program.cs` - DI registration, EF Core, auth, hosted identity result consumer, Swagger, middleware.
- `SubscriptionService/appsettings.json` - local runtime configuration.
- `SubscriptionService/appsettings.Example.json` - safe sample configuration.
- `SubscriptionService/Properties/launchSettings.json` - launch profile.

### 6.3 Controllers
- `SubscriptionService/Controllers/SubscriptionController.cs` - subscribe, confirm, webhook, cancel, current user, admin endpoints.

### 6.4 Services
- `SubscriptionService/Services/ISubscriptionSvc.cs` - subscription service contract.
- `SubscriptionService/Services/SubscriptionService.cs` - subscription, payment, Stripe, webhook, cancellation, event logic.
- `SubscriptionService/Services/AuthorizeOperationFilter.cs` - Swagger auth support.

### 6.5 Data And Models
- `SubscriptionService/Data/AppDbContext.cs` - subscription database context.
- `SubscriptionService/Models/SubscriptionModels.cs` - `Subscription`, `PaymentRecord`, `WebhookEventLog`.
- `SubscriptionService/DTOs/SubscriptionDTOs.cs` - payment confirmation request DTO.

### 6.6 Messaging
- `SubscriptionService/Messaging/IdentityResultConsumer.cs` - consumes identity update results for subscription saga.

### 6.7 Migrations
- `SubscriptionService/Migrations/20260331172252_InitialCreate.cs`
- `SubscriptionService/Migrations/20260331172252_InitialCreate.Designer.cs`
- `SubscriptionService/Migrations/20260402174536_AddSagaStateToSubscription.cs`
- `SubscriptionService/Migrations/20260402174536_AddSagaStateToSubscription.Designer.cs`
- `SubscriptionService/Migrations/20260406042540_AddRazorpayWebhookSupport.cs`
- `SubscriptionService/Migrations/20260406042540_AddRazorpayWebhookSupport.Designer.cs`
- `SubscriptionService/Migrations/20260406061159_AddStripeSupport.cs`
- `SubscriptionService/Migrations/20260406061159_AddStripeSupport.Designer.cs`
- `SubscriptionService/Migrations/AppDbContextModelSnapshot.cs`

### 6.8 Key Study Subtopics
- Payment lifecycle.
- Stripe session confirmation.
- Webhook idempotency.
- Subscription state transitions.
- Saga state and identity update result.
- Email events after payment/cancel actions.

## 7. AssessmentService Project

### 7.1 Service Responsibility
- Start assessment sessions.
- Serve MCQ question batches.
- Generate warm-up/AI questions.
- Submit answers.
- Calculate scores and results.
- Publish assessment lifecycle and email events.
- Admin assessment list and question CRUD.

### 7.2 Startup And Configuration
- `AssessmentService/AssessmentService.csproj` - dependencies: JWT bearer, EF Core SQL Server, Polly, RabbitMQ, Swagger.
- `AssessmentService/Program.cs` - DI registration, EF Core, HTTP client, seed questions, auth, Swagger, middleware.
- `AssessmentService/appsettings.json` - local runtime configuration.
- `AssessmentService/appsettings.Example.json` - safe sample configuration.
- `AssessmentService/appsettings.Development.json` - development settings.
- `AssessmentService/Properties/launchSettings.json` - launch profile.

### 7.3 Controllers
- `AssessmentService/Controllers/AssessmentController.cs` - start, next batch, warm-up, submit, history/result, admin and question endpoints.

### 7.4 Services
- `AssessmentService/Services/IAssessmentService.cs` - assessment service contract.
- `AssessmentService/Services/AssessmentService.cs` - assessment session, question generation, scoring, result logic.
- `AssessmentService/Services/AuthorizeOperationFilter.cs` - Swagger auth support.

### 7.5 Data And Models
- `AssessmentService/Data/AppDbContext.cs` - assessment database context.
- `AssessmentService/Models/AssessmentModels.cs` - question, assessment, answer, result entities.
- `AssessmentService/DTOs/AssessmentDtos.cs` - start, question, submit, answer, admin question DTOs.

### 7.6 Migrations
- `AssessmentService/Migrations/20260331173743_InitialCreate.cs`
- `AssessmentService/Migrations/20260331173743_InitialCreate.Designer.cs`
- `AssessmentService/Migrations/AppDbContextModelSnapshot.cs`

### 7.7 Key Study Subtopics
- Assessment session creation.
- Batch question delivery.
- AI/Gemini question generation.
- Answer evaluation.
- Score and percentage calculation.
- Admin question management.
- Event publishing after start and completion.

## 8. InterviewService Project

### 8.1 Service Responsibility
- Create pending interviews.
- Begin interviews.
- Fetch/generate interview questions.
- Submit answers.
- Evaluate answers.
- Store results.
- Publish interview lifecycle and email events.
- Admin interview listing.

### 8.2 Startup And Configuration
- `InterviewService/InterviewService.csproj` - dependencies: JWT bearer, EF Core SQL Server, Polly, RabbitMQ, Swagger.
- `InterviewService/Program.cs` - DI registration, EF Core, HTTP client, auth, Swagger, middleware.
- `InterviewService/appsettings.json` - local runtime configuration.
- `InterviewService/appsettings.Example.json` - safe sample configuration.
- `InterviewService/appsettings.Development.json` - development settings.
- `InterviewService/Properties/launchSettings.json` - launch profile.

### 8.3 Controllers
- `InterviewService/Controllers/InterviewController.cs` - start, begin, warm-up, fetch-more, submit, history/result, admin endpoints.

### 8.4 Services
- `InterviewService/Services/IInterviewSvc.cs` - interview service contract.
- `InterviewService/Services/InterviewService.cs` - interview orchestration, question generation, answer evaluation, scoring, result logic.
- `InterviewService/Services/AuthorizeOperationFilter.cs` - Swagger auth support.

### 8.5 Data And Models
- `InterviewService/Data/AppDbContext.cs` - interview database context.
- `InterviewService/Models/InterviewModels.cs` - interview, question, global question, answer, result entities.
- `InterviewService/Models/Interview.cs` - one-line model file to inspect for duplication/leftover purpose.
- `InterviewService/DTOs/InterviewDtos.cs` - start, warm-up, question, submit, answer, Gemini DTOs.

### 8.6 Migrations
- `InterviewService/Migrations/20260331172650_InitialCreate.cs`
- `InterviewService/Migrations/20260331172650_InitialCreate.Designer.cs`
- `InterviewService/Migrations/20260402200107_updated.cs`
- `InterviewService/Migrations/20260402200107_updated.Designer.cs`
- `InterviewService/Migrations/20260402202242_new.cs`
- `InterviewService/Migrations/20260402202242_new.Designer.cs`
- `InterviewService/Migrations/20260429180000_AddGlobalInterviewQuestions.cs`
- `InterviewService/Migrations/20260429180000_AddGlobalInterviewQuestions.Designer.cs`
- `InterviewService/Migrations/AppDbContextModelSnapshot.cs`

### 8.7 Key Study Subtopics
- Pending interview creation vs actual begin flow.
- Question generation and fallback questions.
- Fetch-more behavior.
- AI answer evaluation.
- Scoring and result persistence.
- Admin access patterns.
- Event publishing after start and completion.

## 9. NotificationService Project

### 9.1 Service Responsibility
- Runs as a background worker.
- Consumes email notification events.
- Renders HTML email templates.
- Sends email using MailKit.
- Handles retries and dead-letter routing.

### 9.2 Startup And Configuration
- `NotificationService/NotificationService.csproj` - dependencies: worker hosting, RabbitMQ, MailKit, MimeKit.
- `NotificationService/Program.cs` - worker DI registration and hosted consumer setup.
- `NotificationService/appsettings.json` - local runtime configuration.
- `NotificationService/appsettings.Example.json` - safe sample configuration.
- `NotificationService/appsettings.Development.json` - development settings.
- `NotificationService/Properties/launchSettings.json` - launch profile.

### 9.3 Messaging
- `NotificationService/Messaging/EmailNotificationConsumer.cs` - email queue consumer, retry, ack/nack, dead-letter behavior.

### 9.4 Services
- `NotificationService/Services/IEmailSender.cs` - email sending abstraction.
- `NotificationService/Services/MailKitEmailSender.cs` - SMTP email sender implementation.
- `NotificationService/Services/IHtmlEmailTemplateRenderer.cs` - template renderer abstraction.
- `NotificationService/Services/HtmlEmailTemplateRenderer.cs` - HTML template renderer.
- `NotificationService/Services/EmailOptions.cs` - email configuration model.

### 9.5 Key Study Subtopics
- Worker-service startup.
- Queue consumption.
- Email rendering.
- SMTP configuration.
- Retry headers and dead-letter queues.

## 10. API Endpoints By Service

### 10.1 IdentityService Endpoints
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/forgot-password/request-otp`
- `POST /api/auth/forgot-password/reset`
- `GET /api/auth/me`
- `POST /api/auth/refresh-claims`
- `PUT /api/auth/me`
- `GET /api/auth/admin/users`
- `GET /api/auth/admin/users/{id}`
- `PUT /api/auth/admin/users/{id}/role`
- `PUT /api/auth/admin/users/{id}/premium`
- `PUT /api/auth/admin/users/{id}/deactivate`
- `PUT /api/auth/admin/users/{id}/reactivate`
- `PUT /api/auth/internal/users/{id}/premium`
- `GET /api/auth/notifications`
- `PUT /api/auth/notifications/read`

### 10.2 SubscriptionService Endpoints
- `POST /api/subscriptions/subscribe`
- `POST /api/subscriptions/confirm`
- `POST /api/subscriptions/webhook/stripe`
- `POST /api/subscriptions/cancel`
- `GET /api/subscriptions/my`
- `GET /api/subscriptions/my/payments`
- `GET /api/subscriptions/all`
- `GET /api/subscriptions/payments`

### 10.3 AssessmentService Endpoints
- `POST /api/assessments/start`
- `GET /api/assessments/{id}/next-batch`
- `POST /api/assessments/warm-up`
- `POST /api/assessments/submit`
- `GET /api/assessments`
- `GET /api/assessments/{id}/result`
- `GET /api/assessments/admin/all`
- `GET /api/assessments/questions`
- `POST /api/assessments/questions`
- `PUT /api/assessments/questions/{id}`
- `DELETE /api/assessments/questions/{id}`

### 10.4 InterviewService Endpoints
- `POST /api/interviews/start`
- `POST /api/interviews/{id}/begin`
- `POST /api/interviews/warm-up`
- `POST /api/interviews/{id}/fetch-more`
- `POST /api/interviews/submit`
- `GET /api/interviews`
- `GET /api/interviews/{id}`
- `GET /api/interviews/{id}/result`
- `GET /api/interviews/admin/all`

## 11. Event Flow Table

| Publisher | Event | Queue | Consumer | Purpose |
|---|---|---|---|---|
| `AuthController` | `UserRegisteredEvent` | `UserRegistration` | currently no obvious consumer in source | User registration integration hook. |
| `AuthController` | `EmailRequestedEvent` | `EmailNotifications` | `EmailNotificationConsumer` | Welcome and OTP emails. |
| `SubscriptionService` | `SubscriptionLifecycleEvent` | `SubscriptionLifecycle` | `SubscriptionEventConsumer` | Update user premium status in IdentityService. |
| `SubscriptionEventConsumer` | `UserClaimsRefreshedEvent` | `SubscriptionResults` | `IdentityResultConsumer` | Let SubscriptionService know identity update succeeded/failed. |
| `SubscriptionService` | `PaymentSucceededEvent` | `PaymentEvents` | currently no obvious consumer in source | Payment integration hook. |
| `SubscriptionService` | `EmailRequestedEvent` | `EmailNotifications` | `EmailNotificationConsumer` | Payment/subscription email. |
| `AssessmentController` | `AssessmentStartedEvent` | `AssessmentEvents` | `UserNotificationEventConsumer` | Create in-app assessment notification. |
| `AssessmentController` | `AssessmentCompletedEvent` | `AssessmentEvents` | `UserNotificationEventConsumer` | Create in-app assessment completion notification. |
| `AssessmentController` | `EmailRequestedEvent` | `EmailNotifications` | `EmailNotificationConsumer` | Assessment result email. |
| `InterviewController` | `InterviewStartedEvent` | `InterviewEvents` | `UserNotificationEventConsumer` | Create in-app interview notification. |
| `InterviewController` | `InterviewCompletedEvent` | `InterviewEvents` | `UserNotificationEventConsumer` | Create in-app interview completion notification. |
| `InterviewController` | `EmailRequestedEvent` | `EmailNotifications` | `EmailNotificationConsumer` | Interview result email. |

## 12. Test Projects

### 12.1 IdentityService.Tests
- `IdentityService.Tests/IdentityService.Tests.csproj` - test dependencies.
- `IdentityService.Tests/AuthServiceTests.cs` - unit tests for authentication service.

### 12.2 SubscriptionService.Tests
- `SubscriptionService.Tests/SubscriptionService.Tests.csproj` - test dependencies.
- `SubscriptionService.Tests/SubscriptionServiceTests.cs` - unit tests for subscription logic and publisher behavior.

### 12.3 AssessmentService.Tests
- `AssessmentService.Tests/AssessmentService.Tests.csproj` - test dependencies.
- `AssessmentService.Tests/AssessmentServiceTests.cs` - unit tests for assessment logic and fake Gemini handler.

### 12.4 InterviewService.Tests
- `InterviewService.Tests/InterviewService.Tests.csproj` - test dependencies.
- `InterviewService.Tests/InterviewServiceTests.cs` - unit tests for interview logic and fake Gemini handler.

### 12.5 Testing Subtopics
- xUnit structure.
- FluentAssertions usage.
- EF Core InMemory provider.
- Fake RabbitMQ publisher.
- Fake HTTP message handlers for AI/Gemini calls.
- Arrange/Act/Assert reading style.

## 13. Configuration Files

### 13.1 Central Package Management
- `Directory.Packages.props` - package versions for all backend projects.

### 13.2 App Settings
- `API_Gateway/appsettings*.json`
- `IdentityService/appsettings*.json`
- `SubscriptionService/appsettings*.json`
- `AssessmentService/appsettings*.json`
- `InterviewService/appsettings*.json`
- `NotificationService/appsettings*.json`

### 13.3 Important Configuration Topics
- Connection strings.
- JWT issuer/audience/signing key.
- RabbitMQ host/user/password.
- Stripe keys/webhook secrets.
- Gemini/API keys.
- SMTP host/port/credentials.
- Service ports for gateway routing.

## 14. Generated, Support, And Documentation Files

### 14.1 Generated Build Folders To Skip During Study
- `bin/`
- `obj/`
- `.dotnet/`

### 14.2 Runtime Logs
- `API_Gateway.run.log`
- `API_Gateway.err.log`
- `IdentityService.run.log`
- `IdentityService.err.log`
- `SubscriptionService.run.log`
- `SubscriptionService.err.log`
- `AssessmentService.run.log`
- `AssessmentService.err.log`
- `InterviewService.run.log`
- `InterviewService.err.log`

### 14.3 Backend Documentation And Support Files
- `API_Documentation.md` - API-level documentation.
- `ANGULAR_FRONTEND_CONTEXT.md` - frontend/backend integration context.
- `All Docs/API_EXPLANATION_GUIDE.md` - long API explanation guide.
- `All Docs/SMOKE_TEST_README.md` - smoke test guide.
- `All Docs/walkthrough.md` - walkthrough notes.
- `All Docs/MockInterviewPlatform.postman_collection.json` - Postman collection.
- `All Docs/*.ps1` - demo and smoke test scripts.
- `All Docs/PROJECT_ACTIVITY_DIAGRAM.*` - activity diagram source/export.
- `All Docs/PROJECT_REPORT_WORD_FORMAT.*` - report artifacts.

## 15. Line-By-Line Study Checklist

### 15.1 Shared Foundation
- [ ] `Directory.Packages.props`
- [ ] `BuildingBlocks/BuildingBlocks.csproj`
- [ ] `BuildingBlocks/Contracts/ApiResponse.cs`
- [ ] `BuildingBlocks/Contracts/ApiErrorResponse.cs`
- [ ] `BuildingBlocks/Exceptions/AppException.cs`
- [ ] `BuildingBlocks/Exceptions/ValidationAppException.cs`
- [ ] `BuildingBlocks/Exceptions/NotFoundAppException.cs`
- [ ] `BuildingBlocks/Exceptions/ForbiddenAppException.cs`
- [ ] `BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs`
- [ ] `BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs`
- [ ] `BuildingBlocks/Extensions/ServiceCollectionExtensions.cs`
- [ ] `BuildingBlocks/Messaging/QueueNames.cs`
- [ ] `BuildingBlocks/Messaging/RabbitMqOptions.cs`
- [ ] `BuildingBlocks/Messaging/IRabbitMqPublisher.cs`
- [ ] `BuildingBlocks/Messaging/RabbitMqPublisher.cs`
- [ ] All files in `BuildingBlocks/Messaging/Events`

### 15.2 Gateway
- [ ] `API_Gateway/API_Gateway.csproj`
- [ ] `API_Gateway/Program.cs`
- [ ] `API_Gateway/ocelot.json`
- [ ] `API_Gateway/appsettings.Example.json`
- [ ] `API_Gateway/Properties/launchSettings.json`
- [ ] `API_Gateway/WeatherForecast.cs`

### 15.3 IdentityService
- [ ] `IdentityService/IdentityService.csproj`
- [ ] `IdentityService/Program.cs`
- [ ] `IdentityService/Controllers/AuthController.cs`
- [ ] `IdentityService/Controllers/NotificationController.cs`
- [ ] `IdentityService/Services/IAuthService.cs`
- [ ] `IdentityService/Services/AuthService.cs`
- [ ] `IdentityService/Services/AuthorizeOperationFilter.cs`
- [ ] `IdentityService/Data/AppDbContext.cs`
- [ ] `IdentityService/Models/User.cs`
- [ ] `IdentityService/Models/Notification.cs`
- [ ] `IdentityService/DTOs/AuthDTOs.cs`
- [ ] `IdentityService/Messaging/SubscriptionEventConsumer.cs`
- [ ] `IdentityService/Messaging/UserNotificationEventConsumer.cs`
- [ ] `IdentityService/appsettings.Example.json`
- [ ] `IdentityService/Properties/launchSettings.json`
- [ ] `IdentityService/Migrations/*`

### 15.4 SubscriptionService
- [ ] `SubscriptionService/SubscriptionService.csproj`
- [ ] `SubscriptionService/Program.cs`
- [ ] `SubscriptionService/Controllers/SubscriptionController.cs`
- [ ] `SubscriptionService/Services/ISubscriptionSvc.cs`
- [ ] `SubscriptionService/Services/SubscriptionService.cs`
- [ ] `SubscriptionService/Services/AuthorizeOperationFilter.cs`
- [ ] `SubscriptionService/Data/AppDbContext.cs`
- [ ] `SubscriptionService/Models/SubscriptionModels.cs`
- [ ] `SubscriptionService/DTOs/SubscriptionDTOs.cs`
- [ ] `SubscriptionService/Messaging/IdentityResultConsumer.cs`
- [ ] `SubscriptionService/appsettings.Example.json`
- [ ] `SubscriptionService/Properties/launchSettings.json`
- [ ] `SubscriptionService/Migrations/*`

### 15.5 AssessmentService
- [ ] `AssessmentService/AssessmentService.csproj`
- [ ] `AssessmentService/Program.cs`
- [ ] `AssessmentService/Controllers/AssessmentController.cs`
- [ ] `AssessmentService/Services/IAssessmentService.cs`
- [ ] `AssessmentService/Services/AssessmentService.cs`
- [ ] `AssessmentService/Services/AuthorizeOperationFilter.cs`
- [ ] `AssessmentService/Data/AppDbContext.cs`
- [ ] `AssessmentService/Models/AssessmentModels.cs`
- [ ] `AssessmentService/DTOs/AssessmentDtos.cs`
- [ ] `AssessmentService/appsettings.Example.json`
- [ ] `AssessmentService/Properties/launchSettings.json`
- [ ] `AssessmentService/Migrations/*`

### 15.6 InterviewService
- [ ] `InterviewService/InterviewService.csproj`
- [ ] `InterviewService/Program.cs`
- [ ] `InterviewService/Controllers/InterviewController.cs`
- [ ] `InterviewService/Services/IInterviewSvc.cs`
- [ ] `InterviewService/Services/InterviewService.cs`
- [ ] `InterviewService/Services/AuthorizeOperationFilter.cs`
- [ ] `InterviewService/Data/AppDbContext.cs`
- [ ] `InterviewService/Models/InterviewModels.cs`
- [ ] `InterviewService/Models/Interview.cs`
- [ ] `InterviewService/DTOs/InterviewDtos.cs`
- [ ] `InterviewService/appsettings.Example.json`
- [ ] `InterviewService/Properties/launchSettings.json`
- [ ] `InterviewService/Migrations/*`

### 15.7 NotificationService
- [ ] `NotificationService/NotificationService.csproj`
- [ ] `NotificationService/Program.cs`
- [ ] `NotificationService/Messaging/EmailNotificationConsumer.cs`
- [ ] `NotificationService/Services/IEmailSender.cs`
- [ ] `NotificationService/Services/MailKitEmailSender.cs`
- [ ] `NotificationService/Services/IHtmlEmailTemplateRenderer.cs`
- [ ] `NotificationService/Services/HtmlEmailTemplateRenderer.cs`
- [ ] `NotificationService/Services/EmailOptions.cs`
- [ ] `NotificationService/appsettings.Example.json`
- [ ] `NotificationService/Properties/launchSettings.json`

### 15.8 Tests
- [ ] `IdentityService.Tests/AuthServiceTests.cs`
- [ ] `SubscriptionService.Tests/SubscriptionServiceTests.cs`
- [ ] `AssessmentService.Tests/AssessmentServiceTests.cs`
- [ ] `InterviewService.Tests/InterviewServiceTests.cs`

## 16. First Session Plan

When we start studying, begin here:

1. `Directory.Packages.props`
2. `BuildingBlocks/Contracts/ApiResponse.cs`
3. `BuildingBlocks/Contracts/ApiErrorResponse.cs`
4. `BuildingBlocks/Exceptions/AppException.cs`
5. `BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs`
6. `BuildingBlocks/Extensions/ServiceCollectionExtensions.cs`
7. `BuildingBlocks/Messaging/QueueNames.cs`
8. `BuildingBlocks/Messaging/RabbitMqPublisher.cs`

After that, move to `API_Gateway/Program.cs` and `API_Gateway/ocelot.json`, then start `IdentityService`.
