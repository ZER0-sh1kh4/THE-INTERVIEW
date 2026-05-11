# Backend Viva Study Roadmap

Project: Mock Interview Platform  
Backend stack: ASP.NET Core, C#, Microservices-style architecture, Ocelot API Gateway, SQL Server, Entity Framework Core, JWT, RabbitMQ, Stripe, Gemini API, MailKit, Swagger, xUnit

This document is the complete backend study syllabus. We will study one topic at a time in depth: first the general technology concept, then how it is used in this project, why it is needed, what happens without it, alternatives, viva questions, and scenario-based answers.

---

## 1. Backend Architecture Overview

### Topics to Study
- What is backend?
- Client-server architecture
- Request-response cycle
- REST API basics
- HTTP methods: GET, POST, PUT, DELETE
- HTTP status codes: 200, 400, 401, 403, 404, 500
- JSON request and response format
- Layered backend structure
- Monolith vs modular monolith vs microservices
- Service responsibility separation
- Backend flow from Angular frontend to database

### Project Subtopics
- Overall backend folder structure
- API Gateway as entry point
- IdentityService for users and authentication
- InterviewService for mock interviews
- AssessmentService for MCQ assessments
- SubscriptionService for payment and premium access
- NotificationService for email processing
- BuildingBlocks for shared code
- How frontend communicates with backend

### Must Be Able to Explain
- Why the project has multiple backend services
- How one user action travels through backend
- Difference between frontend route and backend API route
- Which service owns which responsibility

---

## 2. ASP.NET Core Web API

### Topics to Study
- What is ASP.NET Core?
- What is Web API?
- Program.cs startup file
- WebApplication builder
- Dependency injection container
- Controllers and actions
- Route attributes
- Model binding
- FromBody, FromQuery, route parameters
- IActionResult
- Middleware pipeline
- app.UseAuthentication()
- app.UseAuthorization()
- app.MapControllers()
- Environment-based behavior

### Project Subtopics
- Program.cs in each service
- AuthController
- InterviewController
- AssessmentController
- SubscriptionController
- Controller-to-service flow
- Swagger registration
- CORS configuration
- HTTPS redirection

### Must Be Able to Explain
- What happens when an API endpoint is called
- Why controllers should not contain heavy business logic
- Why Program.cs is important
- Middleware order importance

---

## 3. C# Backend Fundamentals

### Topics to Study
- Classes and objects
- Interfaces
- Access modifiers
- Properties
- Constructors
- async and await
- Task and Task<T>
- LINQ
- Lambda expressions
- Nullable types
- Collections: List, Dictionary, HashSet
- Exception handling
- Records/classes/DTOs
- Dependency inversion

### Project Subtopics
- Service interfaces like IAuthService, IInterviewSvc
- DTO classes for requests and responses
- Model classes for database entities
- Async database calls
- LINQ queries with EF Core
- Custom exception classes

### Must Be Able to Explain
- Why async/await is used in APIs
- Why interfaces are used before service classes
- Difference between model and DTO
- Difference between List and IEnumerable

---

## 4. REST API Design

### Topics to Study
- REST principles
- Resource-based URLs
- Statelessness
- HTTP verbs
- Request body vs query string vs route parameter
- API response consistency
- Validation errors
- Protected vs public endpoints

### Project Subtopics
- /api/auth/register
- /api/auth/login
- /api/auth/me
- /api/interviews/start
- /api/interviews/{id}/begin
- /api/assessments/start
- /api/assessments/{id}/result
- /api/subscriptions/subscribe
- /api/subscriptions/webhook/stripe

### Must Be Able to Explain
- Why login is POST, not GET
- Why admin endpoints are role-protected
- Why webhook endpoint is anonymous
- Why consistent API responses are useful

---

## 5. API Gateway and Ocelot

### Topics to Study
- What is API Gateway?
- Reverse proxy concept
- Routing
- Upstream path vs downstream path
- Service ports
- Gateway-level authentication
- Gateway-level CORS
- Gateway Swagger aggregation
- Quality of Service and timeout

### Project Subtopics
- API_Gateway service
- ocelot.json
- Routes to IdentityService on port 5005
- Routes to InterviewService on port 5002
- Routes to AssessmentService on port 5003
- Routes to SubscriptionService on port 5004
- Ocelot + Polly
- SwaggerForOcelot

### Must Be Able to Explain
- Why frontend calls gateway instead of every service directly
- What upstream and downstream mean
- What happens if gateway is removed
- Alternatives to Ocelot

---

## 6. Authentication and Authorization

### Topics to Study
- Authentication vs authorization
- User identity
- Login flow
- Password verification
- Role-based authorization
- Claims-based authorization
- Public endpoints
- Protected endpoints
- Admin-only endpoints
- 401 vs 403

### Project Subtopics
- Register user
- Login user
- Get current user
- Admin user management
- Candidate role
- Admin role
- IsActive user flag
- IsPremium user claim
- Authorize attribute
- AllowAnonymous attribute

### Must Be Able to Explain
- Difference between authentication and authorization
- Why admin APIs need role protection
- How services know current user
- Why deactivated users cannot log in

---

## 7. JWT Tokens

### Topics to Study
- What is JWT?
- Header, payload, signature
- Claims
- Token signing
- Issuer and audience
- Token expiry
- Bearer token
- Stateless authentication
- Token validation
- Refreshing claims
- Security risks of storing tokens

### Project Subtopics
- JWT generation in AuthService
- Claims: userId, email, role, isPremium, FullName
- JWT validation in all protected services
- Gateway JWT validation
- RefreshClaims endpoint
- Premium claim update after subscription

### Must Be Able to Explain
- Why JWT is used
- What data is stored in the token
- Why token must be signed
- Why old token does not automatically know premium status changed

---

## 8. Password Hashing and OTP

### Topics to Study
- Plain password vs hashed password
- Hashing vs encryption
- BCrypt
- Salt
- Password verification
- Forgot password flow
- OTP generation
- OTP expiry
- Memory cache
- Security against email enumeration

### Project Subtopics
- BCrypt password hashing
- BCrypt password verification
- Forgot password OTP generation
- OTP stored in memory cache as hash
- Generic forgot-password response
- Reset password after OTP validation

### Must Be Able to Explain
- Why passwords are never stored directly
- Why OTP is hashed before storing
- Why forgot-password response should be generic
- Limitations of in-memory OTP storage

---

## 9. Entity Framework Core and SQL Server

### Topics to Study
- What is ORM?
- EF Core DbContext
- DbSet
- Entity models
- LINQ to SQL
- Migrations
- Primary key
- Foreign key concept
- Tracking vs no tracking
- SaveChangesAsync
- Database relationships
- SQL Server connection string

### Project Subtopics
- AppDbContext in IdentityService
- AppDbContext in InterviewService
- AppDbContext in AssessmentService
- AppDbContext in SubscriptionService
- User table
- Interview, Question, InterviewAnswer, InterviewResult tables
- Assessment, MCQQuestion, UserAnswer, AssessmentResult tables
- Subscription, PaymentRecord, WebhookEventLog tables
- Migration files

### Must Be Able to Explain
- Why each service has its own DbContext
- How EF Core maps C# classes to database tables
- Why async database calls are used
- What migrations do

---

## 10. Dependency Injection

### Topics to Study
- What is dependency injection?
- Constructor injection
- Interface-based design
- Service lifetimes: Singleton, Scoped, Transient
- Why DbContext is usually scoped
- Loose coupling
- Testability

### Project Subtopics
- AddScoped<IAuthService, AuthService>
- AddScoped<IInterviewSvc, InterviewSvc>
- AddScoped<IAssessmentService, AssessmentSvc>
- AddScoped<ISubscriptionSvc, SubscriptionSvc>
- AddSingleton RabbitMQ publisher
- AddHostedService consumers
- IHttpClientFactory / AddHttpClient

### Must Be Able to Explain
- Why controllers receive services through constructor
- Why interfaces improve testing
- Difference between Scoped, Singleton, and Transient
- Why singleton DbContext would be dangerous

---

## 11. Middleware and Exception Handling

### Topics to Study
- What is middleware?
- Middleware pipeline
- Global exception handling
- Custom exceptions
- Logging errors
- Standard error response
- Validation errors
- Difference between business error and system error

### Project Subtopics
- GlobalExceptionMiddleware
- AppException
- ValidationAppException
- NotFoundAppException
- ForbiddenAppException
- ApiErrorResponse
- AddApiDefaults validation response

### Must Be Able to Explain
- Why global exception middleware is used
- Why custom exceptions are better than random strings
- Difference between 400, 403, 404, 500
- How consistent error responses help frontend

---

## 12. RabbitMQ and Event-Driven Architecture

### Topics to Study
- What is message broker?
- Queue
- Exchange
- Routing key
- Producer
- Consumer
- Publish/subscribe
- Asynchronous communication
- Event-driven architecture
- Direct HTTP vs messaging
- Dead-letter queue
- Message durability

### Project Subtopics
- RabbitMqPublisher
- QueueNames
- EmailRequestedEvent
- UserRegisteredEvent
- SubscriptionLifecycleEvent
- PaymentSucceededEvent
- InterviewStartedEvent
- InterviewCompletedEvent
- AssessmentStartedEvent
- AssessmentCompletedEvent
- NotificationService consumer
- IdentityService subscription consumers
- SubscriptionService identity result consumer

### Must Be Able to Explain
- Why email is sent asynchronously
- Why services publish events instead of directly calling each other
- What happens if NotificationService is temporarily down
- RabbitMQ alternatives

---

## 13. IdentityService Deep Dive

### Topics to Study
- User registration
- User login
- Password hashing
- JWT creation
- Claims
- Forgot password OTP
- Profile update
- Role update
- Premium flag update
- Account activation/deactivation
- Internal API key usage
- Admin endpoints

### Project Subtopics
- RegisterAsync
- LoginAsync
- GenerateJwtToken
- RefreshClaimsAsync
- SendForgotPasswordOtpAsync
- ResetPasswordWithOtpAsync
- UpdateUserRoleAsync
- UpdateUserPremiumAsync
- InternalUpdatePremium endpoint
- NotificationController

### Must Be Able to Explain
- Complete login/register flow
- Why premium flag is inside IdentityService
- Why profile update returns refreshed token
- Why internal premium update uses internal key

---

## 14. InterviewService Deep Dive

### Topics to Study
- Subjective interview system
- Interview lifecycle
- Pending/InProgress/Completed states
- Free vs premium limits
- Question generation
- Question caching
- Lazy loading vs upfront loading
- AI answer evaluation
- Scoring logic
- Feedback generation
- Premium result details

### Project Subtopics
- StartInterviewAsync
- BeginInterviewAsync
- FetchMoreQuestionsAsync
- WarmUpCacheAsync
- BuildInterviewQuestionSetAsync
- Admin curated questions
- JIT global question pool
- Gemini question generation
- SubmitInterviewAsync
- EvaluateInterviewAnswersAsync
- GetResultAsync

### Must Be Able to Explain
- Why subjective answers need AI evaluation
- Why previous questions are avoided
- Why fallback exists if Gemini fails
- Difference between free and premium interview result

---

## 15. AssessmentService Deep Dive

### Topics to Study
- MCQ test flow
- Objective answer checking
- Question bank
- AI-generated MCQs
- JIT question caching
- Lazy loading batches
- Timer and expiry
- Attempt number
- Score, percentage, grade
- Premium review
- Admin question management

### Project Subtopics
- StartAssessmentAsync
- GetNextBatchAsync
- SubmitAssessmentAsync
- GetAssessmentResultAsync
- WarmUpCacheAsync
- GenerateAiAssessmentQuestionsAsync
- AddQuestionAsync
- UpdateQuestionAsync
- DeleteQuestionAsync
- ReIndexDomainQuestionsAsync

### Must Be Able to Explain
- Difference between assessment and interview
- Why MCQs can be evaluated without AI
- Why question cache is used
- Why assessment expires after time limit

---

## 16. SubscriptionService and Payments

### Topics to Study
- Subscription model
- Payment lifecycle
- Stripe checkout
- Webhooks
- Webhook signature validation
- Idempotency
- Payment record
- Premium activation
- Subscription cancellation
- Saga pattern basics
- Simulated payment mode

### Project Subtopics
- SubscribeAsync
- ConfirmPaymentAsync
- HandleStripeWebhookAsync
- EnsureActiveSubscriptionAsync
- PublishSuccessfulPaymentEventsAsync
- CancelSubscriptionAsync
- PaymentRecord
- Subscription
- WebhookEventLog
- Stripe enabled vs simulated mode

### Must Be Able to Explain
- Why webhook is needed
- Why webhook endpoint is anonymous but signature-protected
- Why duplicate webhook events are ignored
- Why premium update is event-based
- Why JWT refresh is needed after payment

---

## 17. NotificationService and Email

### Topics to Study
- Background worker service
- Hosted service
- Email templates
- SMTP
- MailKit
- MIME email
- Asynchronous email sending
- Event consumer

### Project Subtopics
- EmailNotificationConsumer
- MailKitEmailSender
- HtmlEmailTemplateRenderer
- EmailOptions
- Welcome email
- Password reset OTP email
- Payment success email
- Subscription upgrade email
- Interview completion email
- Assessment completion email

### Must Be Able to Explain
- Why NotificationService has no controller
- Why email should not block main API response
- What event triggers each email
- Alternatives to SMTP/MailKit

---

## 18. Gemini API and AI Integration

### Topics to Study
- What is external API integration?
- HttpClient
- API keys
- Request body
- JSON response parsing
- Prompting
- Structured JSON output
- Retry handling
- Rate limiting
- Fallback behavior
- Security of API keys

### Project Subtopics
- Gemini question generation in InterviewService
- Gemini answer evaluation in InterviewService
- Gemini MCQ generation in AssessmentService
- Prompt construction
- JSON parsing
- Retry with Polly
- Fallback when Gemini is unavailable
- Warm-up cache to reduce user wait

### Must Be Able to Explain
- Why Gemini is used in interview and assessment services
- What happens if Gemini returns invalid JSON
- Why retries are useful
- Why API keys should be kept in configuration, not hardcoded

---

## 19. Stripe Integration

### Topics to Study
- Payment gateway concept
- Checkout session
- Publishable key vs secret key
- Webhook secret
- Payment intent
- Webhook event types
- Signature verification
- Test mode vs live mode
- Payment success and failure states

### Project Subtopics
- Stripe checkout session creation
- checkout.session.completed
- checkout.session.expired
- PaymentRecord status: Pending, Success, Failed
- Simulated local payment mode
- Stripe webhook processing

### Must Be Able to Explain
- Why payment is not trusted only from frontend
- Why backend waits for Stripe webhook
- Difference between checkout session ID and payment intent ID
- Why webhook logs are stored

---

## 20. Swagger and API Documentation

### Topics to Study
- What is Swagger/OpenAPI?
- API testing through Swagger UI
- Security scheme in Swagger
- Bearer token testing
- XML comments
- Gateway Swagger aggregation

### Project Subtopics
- Swagger in every service
- SwaggerForOcelot in API Gateway
- Bearer security definition
- OperationFilter for authorized endpoints
- Backend/API_Documentation.md

### Must Be Able to Explain
- Why Swagger is helpful during development
- How to test protected APIs in Swagger
- Why API documentation matters

---

## 21. Configuration and Secrets

### Topics to Study
- appsettings.json
- appsettings.Example.json
- Connection strings
- Environment-specific settings
- API keys
- Secret management
- Configuration sections
- IOptions pattern
- IConfiguration

### Project Subtopics
- Jwt settings
- RabbitMq settings
- Stripe settings
- Gemini settings
- Email settings
- InternalApiKey
- SQL Server connection strings

### Must Be Able to Explain
- Why secrets should not be committed
- Why example config files are used
- How services read configuration
- What happens if config is missing

---

## 22. CORS and Frontend-Backend Communication

### Topics to Study
- What is CORS?
- Browser same-origin policy
- AllowAnyOrigin
- AllowAnyMethod
- AllowAnyHeader
- Preflight OPTIONS request
- Security risk of open CORS

### Project Subtopics
- Gateway CORS policy
- Development CORS in each service
- Angular frontend calling backend APIs
- Authorization header from frontend

### Must Be Able to Explain
- Why CORS is needed for Angular + backend
- Why OPTIONS appears in routes
- Why production CORS should be restricted

---

## 23. Security Topics

### Topics to Study
- Password hashing
- JWT signing
- Token expiry
- Role-based authorization
- Input validation
- Internal API key
- Webhook signature validation
- Avoiding email enumeration
- Secure configuration
- HTTPS
- Principle of least privilege

### Project Subtopics
- BCrypt
- JWT validation
- Authorize Roles = Admin
- Internal premium update key
- Stripe-Signature validation
- Generic forgot-password message
- User IsActive flag

### Must Be Able to Explain
- Top security features in the project
- What security risks still exist
- Why HTTPS is used
- Why frontend cannot be trusted for payment success

---

## 24. Resilience and Reliability

### Topics to Study
- Retry pattern
- Timeout
- Circuit breaker concept
- Fallback
- Idempotency
- Duplicate event handling
- Graceful degradation
- Logging

### Project Subtopics
- Polly in gateway
- Polly retry for Gemini assessment generation
- Gateway QoS timeout
- Stripe duplicate webhook handling
- Gemini fallback during evaluation
- RabbitMQ persistent messages
- WebhookEventLog

### Must Be Able to Explain
- Why external APIs may fail
- Why retry should not be infinite
- Why duplicate webhook events must be ignored
- How project handles partial failure

---

## 25. Testing

### Topics to Study
- Unit testing
- Integration testing
- xUnit
- FluentAssertions
- In-memory database
- Mocking dependencies
- Test naming
- Arrange, Act, Assert
- Code coverage

### Project Subtopics
- IdentityService.Tests
- AssessmentService.Tests
- InterviewService.Tests
- SubscriptionService.Tests
- EF Core InMemory
- Testing service methods

### Must Be Able to Explain
- Why backend tests are needed
- What should be unit tested
- What should be integration tested
- Why external APIs should be mocked in tests

---

## 26. Logging and Observability

### Topics to Study
- What is logging?
- Log levels: Information, Warning, Error
- Structured logging
- Why logs matter in production
- Debugging backend flow
- Correlation ID concept

### Project Subtopics
- ILogger usage in controllers and services
- Logging register/login
- Logging assessment/interview start
- Logging Gemini failure
- Logging webhook processing
- Logging exception middleware

### Must Be Able to Explain
- Why logs are not the same as user responses
- Why sensitive data should not be logged
- How logs help debug production issues

---

## 27. Data Models and ER Understanding

### Topics to Study
- Entity
- Attribute
- Primary key
- Foreign key
- One-to-many relationship
- Status fields
- Audit fields
- Normalization basics

### Project Subtopics
- User
- Notification
- Interview
- Question
- InterviewAnswer
- InterviewResult
- MCQQuestion
- Assessment
- UserAnswer
- AssessmentResult
- Subscription
- PaymentRecord
- WebhookEventLog

### Must Be Able to Explain
- Why separate result tables exist
- Why answers and questions are stored separately
- Why status fields are used
- How userId links data across services

---

## 28. Design Patterns Used or Related

### Topics to Study
- Dependency Injection pattern
- Repository pattern concept
- Service layer pattern
- API Gateway pattern
- Event-driven pattern
- Saga pattern
- Retry pattern
- Factory concept
- Observer/pub-sub pattern
- DTO pattern

### Project Subtopics
- Service layer in each service
- RabbitMQ events as pub-sub
- Subscription lifecycle as saga-style flow
- DTOs for API contracts
- Global exception middleware
- Interface-driven services

### Must Be Able to Explain
- Which patterns are used in the project
- Why service layer is useful
- Why saga is useful for premium activation
- Difference between direct call and event-driven flow

---

## 29. Service-by-Service Endpoint Study

### IdentityService Endpoints
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/forgot-password/request-otp
- POST /api/auth/forgot-password/reset
- GET /api/auth/me
- PUT /api/auth/me
- POST /api/auth/refresh-claims
- GET /api/auth/admin/users
- PUT /api/auth/admin/users/{id}/role
- PUT /api/auth/admin/users/{id}/premium
- PUT /api/auth/admin/users/{id}/deactivate
- PUT /api/auth/admin/users/{id}/reactivate

### InterviewService Endpoints
- POST /api/interviews/start
- POST /api/interviews/{id}/begin
- POST /api/interviews/warm-up
- POST /api/interviews/{id}/fetch-more
- POST /api/interviews/submit
- GET /api/interviews
- GET /api/interviews/{id}
- GET /api/interviews/{id}/result
- GET /api/interviews/admin/all

### AssessmentService Endpoints
- POST /api/assessments/start
- GET /api/assessments/{id}/next-batch
- POST /api/assessments/warm-up
- POST /api/assessments/submit
- GET /api/assessments
- GET /api/assessments/{id}/result
- GET /api/assessments/admin/all
- GET /api/assessments/questions
- POST /api/assessments/questions
- PUT /api/assessments/questions/{id}
- DELETE /api/assessments/questions/{id}

### SubscriptionService Endpoints
- POST /api/subscriptions/subscribe
- POST /api/subscriptions/confirm
- POST /api/subscriptions/webhook/stripe
- POST /api/subscriptions/cancel
- GET /api/subscriptions/my
- GET /api/subscriptions/my/payments
- GET /api/subscriptions/all
- GET /api/subscriptions/payments

---

## 30. Common Viva Questions to Prepare

- Explain your backend architecture.
- Why did you use microservices?
- What is the role of API Gateway?
- How does login work?
- What is JWT and how do you use it?
- What are claims?
- Difference between authentication and authorization?
- Why do you use BCrypt?
- What is Entity Framework Core?
- What is DbContext?
- What are migrations?
- What is dependency injection?
- What is middleware?
- How do you handle exceptions?
- Why use RabbitMQ?
- What is event-driven architecture?
- How does premium subscription work?
- Why do you need JWT refresh after payment?
- How does Stripe webhook work?
- How does AssessmentService calculate score?
- How does InterviewService evaluate answers?
- Why use Gemini API?
- What happens if Gemini fails?
- What happens if email service fails?
- Difference between interview and assessment modules?
- How is admin authorization handled?
- What security features are implemented?
- What improvements would you make in future?

---

## Suggested Study Order

1. Backend architecture overview
2. ASP.NET Core Web API basics
3. REST API and HTTP
4. C# backend fundamentals
5. Dependency Injection
6. Entity Framework Core and SQL Server
7. Authentication, Authorization, JWT
8. IdentityService deep dive
9. API Gateway and Ocelot
10. RabbitMQ and event-driven architecture
11. NotificationService
12. AssessmentService deep dive
13. InterviewService deep dive
14. Gemini API integration
15. SubscriptionService and Stripe
16. Exception handling and middleware
17. Security
18. Testing
19. Design patterns
20. Final viva question practice

---

## How We Will Study Each Topic

For every topic, use this format:

1. Simple real-life explanation
2. Technical definition
3. Where it appears in this project
4. Why this project uses it
5. What happens if we remove it
6. Alternatives
7. Step-by-step project flow
8. Important code files
9. Common viva questions
10. Best viva answer

