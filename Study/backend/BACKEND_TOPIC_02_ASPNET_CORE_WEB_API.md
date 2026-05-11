# Topic 2: ASP.NET Core Web API

Project: Mock Interview Platform  
Focus: Understanding how ASP.NET Core receives API requests, routes them to controllers, runs middleware, injects services, and returns responses.

---

## 1. What Is ASP.NET Core?

### Simple Explanation

ASP.NET Core is a framework used to build backend applications using C#.

Think of it as the engine that helps your backend:

- Receive HTTP requests
- Run authentication
- Call controllers
- Use database services
- Return JSON responses
- Handle errors
- Serve Swagger documentation

### Practical Scenario

When a user clicks Login in Angular:

```text
Angular sends HTTP request
        ↓
ASP.NET Core backend receives it
        ↓
ASP.NET Core matches it to AuthController.Login()
        ↓
AuthService checks credentials
        ↓
ASP.NET Core sends JSON response
```

### Technical Definition

ASP.NET Core is a cross-platform, high-performance framework for building web applications, APIs, microservices, and backend services using .NET and C#.

### In Your Project

Your backend services are ASP.NET Core Web API projects:

```text
Backend/API_Gateway
Backend/IdentityService
Backend/InterviewService
Backend/AssessmentService
Backend/SubscriptionService
```

NotificationService is slightly different because it is a worker/background service, not a controller-based Web API.

### Viva Answer

> ASP.NET Core is the backend framework used in my project to build Web APIs. It handles request routing, controllers, dependency injection, middleware, authentication, Swagger documentation, and JSON responses.

---

## 2. What Is Web API?

### Simple Explanation

Web API is a backend interface that frontend can call over HTTP.

It exposes endpoints such as:

```text
POST /api/auth/login
POST /api/interviews/start
GET /api/assessments/{id}/result
POST /api/subscriptions/subscribe
```

The frontend sends data. The backend processes it and returns data.

### Practical Example

Login API:

```text
Frontend sends:
email + password

Backend returns:
JWT token + userId + message
```

Assessment API:

```text
Frontend sends:
domain + question count + difficulty

Backend returns:
assessmentId + questions + time limit
```

### Why Web API Is Used

Because Angular frontend and ASP.NET backend are separate applications.

They communicate using HTTP APIs and JSON.

### What If We Do Not Use API?

Frontend would not have a proper way to ask backend for login, questions, results, or payment actions.

### Viva Answer

> Web API exposes backend functionality through HTTP endpoints. In my project, Angular calls these APIs to register users, login, start interviews, attempt assessments, buy subscriptions, and fetch results.

---

## 3. Program.cs

### Simple Explanation

`Program.cs` is the startup file of an ASP.NET Core application.

It decides:

- Which services are registered
- Which middleware will run
- Which authentication is used
- Which database is connected
- Whether Swagger is enabled
- Which controllers are mapped

### Real-Life Analogy

Imagine opening a coaching center every morning.

Before students arrive, you prepare:

- Reception desk
- Classrooms
- Teachers
- Security guard
- Attendance system
- Notice board

Similarly, `Program.cs` prepares the backend before requests arrive.

### In Your Project

Every API service has its own `Program.cs`.

Examples:

```text
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
Backend/API_Gateway/Program.cs
```

### Common Things in Your Program.cs Files

Most services do these things:

```csharp
builder.Services.AddControllers();
builder.Services.AddApiDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

### Viva Answer

> Program.cs is the entry point of each ASP.NET Core service. It registers dependencies like controllers, DbContext, services, JWT authentication, Swagger, and middleware, then starts the application.

---

## 4. WebApplication Builder

### Simple Explanation

`WebApplication.CreateBuilder(args)` creates a builder object.

This builder is used to configure the application before it starts.

### In Your Project

Most services start like this:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

API Gateway has a slightly custom setup:

```csharp
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
```

### Why Builder Is Used

Builder allows us to register services:

```csharp
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddAuthentication();
```

Then we build the app:

```csharp
var app = builder.Build();
```

### Simple Flow

```text
Create builder
        ↓
Register services
        ↓
Build app
        ↓
Add middleware
        ↓
Run app
```

### Viva Answer

> WebApplication builder is used to configure services and application settings before the ASP.NET Core app starts.

---

## 5. Dependency Injection Container

### Simple Explanation

ASP.NET Core has a built-in dependency injection container.

It stores information about which class should be provided when another class asks for it.

### Practical Scenario

`AuthController` needs `IAuthService`.

It does not create `AuthService` manually.

ASP.NET Core provides it automatically.

### In Your Project

IdentityService registers:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

InterviewService registers:

```csharp
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
```

AssessmentService registers:

```csharp
builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>();
```

SubscriptionService registers:

```csharp
builder.Services.AddScoped<ISubscriptionSvc, SubscriptionSvc>();
```

### Why This Is Useful

It keeps controllers clean.

Controller only says:

```csharp
private readonly IAuthService _authService;

public AuthController(IAuthService authService)
{
    _authService = authService;
}
```

ASP.NET Core provides the object.

### What If We Do Not Use Dependency Injection?

Controllers would create services manually:

```csharp
var service = new AuthService(...);
```

This becomes difficult because `AuthService` also needs DbContext, configuration, logger, and memory cache.

Testing also becomes harder.

### Viva Answer

> ASP.NET Core has built-in dependency injection. My project registers service interfaces and implementations in Program.cs, and controllers receive those services through constructor injection.

---

## 6. Controllers

### Simple Explanation

Controllers are classes that receive API requests.

They are like counters in an office.

Different counters handle different work:

- AuthController handles login/register
- InterviewController handles interviews
- AssessmentController handles MCQ assessments
- SubscriptionController handles payments

### In Your Project

Important controllers:

```text
Backend/IdentityService/Controllers/AuthController.cs
Backend/InterviewService/Controllers/InterviewController.cs
Backend/AssessmentService/Controllers/AssessmentController.cs
Backend/SubscriptionService/Controllers/SubscriptionController.cs
```

### Controller Attributes

Example:

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
}
```

`[ApiController]` means this class behaves as a Web API controller.

`[Route("api/auth")]` means all endpoints inside this controller start with:

```text
/api/auth
```

### Practical Example

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
```

Final endpoint:

```text
POST /api/auth/login
```

### Controller Responsibility

Controller should:

- Receive request
- Read route/body/query data
- Read user claims if needed
- Call service layer
- Return response

Controller should not contain heavy business logic.

### Viva Answer

> Controllers expose API endpoints. They receive HTTP requests, call service classes for business logic, and return IActionResult responses to the frontend.

---

## 7. Actions

### Simple Explanation

An action is a method inside a controller that handles one endpoint.

### Example

In `AuthController`:

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
```

This action handles:

```text
POST /api/auth/login
```

### Project Examples

```text
Register action       → POST /api/auth/register
Login action          → POST /api/auth/login
Start action          → POST /api/interviews/start
Submit action         → POST /api/assessments/submit
Subscribe action      → POST /api/subscriptions/subscribe
```

### Why Actions Return IActionResult

`IActionResult` allows the controller to return different HTTP responses:

```csharp
return Ok(...);
return NotFound(...);
return BadRequest(...);
return Unauthorized(...);
```

### Viva Answer

> An action is a controller method mapped to an API endpoint. It processes one specific request and returns an IActionResult response.

---

## 8. Routing

### Simple Explanation

Routing means matching the URL to the correct controller and action.

### Practical Example

Request:

```text
POST /api/auth/login
```

ASP.NET Core checks:

```text
Controller route = api/auth
Action route = login
HTTP method = POST
```

So it calls:

```text
AuthController.Login()
```

### Route Parameters

Example:

```csharp
[HttpGet("{id}/result")]
public async Task<IActionResult> GetResult(int id)
```

Request:

```text
GET /api/assessments/15/result
```

Here:

```text
id = 15
```

### In Your Project

Interview result endpoint:

```text
GET /api/interviews/{id}/result
```

Assessment result endpoint:

```text
GET /api/assessments/{id}/result
```

Admin update role endpoint:

```text
PUT /api/auth/admin/users/{id}/role
```

### Viva Answer

> Routing maps incoming URLs and HTTP methods to specific controller actions. ASP.NET Core uses route attributes like Route, HttpGet, HttpPost, HttpPut, and HttpDelete.

---

## 9. Model Binding

### Simple Explanation

Model binding means ASP.NET Core automatically reads data from the request and puts it into C# objects or parameters.

### FromBody

Used when data comes from JSON request body.

Example:

```csharp
public async Task<IActionResult> Login([FromBody] LoginRequest request)
```

Request body:

```json
{
  "email": "student@gmail.com",
  "password": "123456"
}
```

ASP.NET Core converts this JSON into `LoginRequest`.

### FromQuery

Used when data comes from URL query string.

Example:

```csharp
public async Task<IActionResult> NextBatch(
    int id,
    [FromQuery] int currentCount = 0,
    [FromQuery] int batchSize = 5)
```

Request:

```text
GET /api/assessments/10/next-batch?currentCount=5&batchSize=5
```

### Route Parameter

Used when data comes from URL path.

Example:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetInterview(int id)
```

Request:

```text
GET /api/interviews/7
```

Here:

```text
id = 7
```

### Viva Answer

> Model binding automatically maps request data from body, query string, or route parameters into C# method parameters or DTO objects.

---

## 10. DTOs

### Simple Explanation

DTO means Data Transfer Object.

DTOs are used to transfer data between frontend and backend.

### Why DTOs Are Needed

Database model may contain fields that frontend should not send or receive.

Example:

User model contains:

```text
PasswordHash
Role
IsActive
CreatedAt
```

Frontend login should only send:

```text
Email
Password
```

So we use DTOs like `LoginRequest`.

### In Your Project

DTO folders:

```text
Backend/IdentityService/DTOs
Backend/InterviewService/DTOs
Backend/AssessmentService/DTOs
Backend/SubscriptionService/DTOs
```

### Practical Examples

Identity DTOs:

```text
RegisterRequest
LoginRequest
AuthResponse
ForgotPasswordOtpRequest
```

Interview DTOs:

```text
StartInterviewRequest
SubmitInterviewRequest
QuestionResponseDto
```

Assessment DTOs:

```text
StartAssessmentRequest
SubmitAssessmentRequest
QuestionDto
```

Subscription DTOs:

```text
ConfirmPaymentRequest
```

### Viva Answer

> DTOs are used to transfer only required data between frontend and backend. They prevent exposing internal database models directly and keep API contracts clean.

---

## 11. Middleware

### Simple Explanation

Middleware is code that runs between receiving the request and returning the response.

It is like checkpoints on a road.

Request passes through multiple checkpoints:

```text
Request
  ↓
Exception middleware
  ↓
HTTPS redirection
  ↓
Authentication
  ↓
Authorization
  ↓
Controller
  ↓
Response
```

### In Your Project

Common middleware:

```csharp
app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
```

### Why Middleware Order Matters

Authentication must happen before authorization.

Correct order:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

If authorization runs before authentication, the app may not know who the user is.

### Practical Scenario

User calls protected API:

```text
GET /api/interviews
```

Middleware flow:

```text
JWT token is read
        ↓
User identity is created
        ↓
Authorization checks access
        ↓
Controller action runs
```

### Viva Answer

> Middleware is a component in the ASP.NET Core request pipeline. It can handle cross-cutting concerns like exception handling, authentication, authorization, HTTPS redirection, CORS, and Swagger.

---

## 12. Authentication Middleware

### Simple Explanation

Authentication middleware checks who the user is.

In your project, it reads JWT token from:

```text
Authorization: Bearer token
```

### In Program.cs

```csharp
app.UseAuthentication();
```

### What It Does

It validates:

- Token signature
- Token issuer
- Token audience
- Token expiry
- Signing key

If valid, it creates user identity and claims.

### Why It Is Needed

Without authentication middleware, `[Authorize]` endpoints would not know the current user.

### Viva Answer

> Authentication middleware validates the JWT token and creates the current user identity with claims.

---

## 13. Authorization Middleware

### Simple Explanation

Authorization middleware checks what the user is allowed to do.

Authentication asks:

```text
Who are you?
```

Authorization asks:

```text
Are you allowed to access this?
```

### In Program.cs

```csharp
app.UseAuthorization();
```

### In Controllers

Protected endpoint:

```csharp
[Authorize]
```

Admin-only endpoint:

```csharp
[Authorize(Roles = "Admin")]
```

Public endpoint:

```csharp
[AllowAnonymous]
```

### Practical Examples

Login is public:

```csharp
[AllowAnonymous]
[HttpPost("login")]
```

My interviews is protected:

```csharp
[Authorize]
[HttpGet]
```

Admin users is admin-only:

```csharp
[Authorize(Roles = "Admin")]
[HttpGet("admin/users")]
```

### Viva Answer

> Authorization middleware checks whether the authenticated user has permission to access an endpoint. My project uses Authorize for protected APIs and role-based authorization for admin APIs.

---

## 14. app.MapControllers()

### Simple Explanation

`app.MapControllers()` tells ASP.NET Core to activate controller routes.

Without it, controller endpoints will not work.

### In Your Project

Most services end with:

```csharp
app.MapControllers();
app.Run();
```

### What If We Remove It?

Even if controllers exist, requests will not reach them.

Example:

```text
POST /api/auth/login
```

would not be mapped to `AuthController.Login()`.

### Viva Answer

> app.MapControllers maps attribute-routed controller actions into the ASP.NET Core request pipeline, allowing API endpoints to work.

---

## 15. Swagger

### Simple Explanation

Swagger is a browser-based API documentation and testing tool.

It shows:

- API endpoints
- Request body format
- Response format
- Required parameters
- Authentication option

### In Your Project

Each service registers Swagger:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

Then enables UI:

```csharp
app.UseSwagger();
app.UseSwaggerUI();
```

Your API Gateway also uses SwaggerForOcelot to combine service docs.

### Why Swagger Is Useful

During viva or testing, you can open Swagger and show APIs without frontend.

You can test:

- Register
- Login
- Start assessment
- Submit answers
- View result

### Viva Answer

> Swagger provides interactive API documentation. In my project, each service exposes Swagger so APIs can be tested and understood easily during development.

---

## 16. CORS

### Simple Explanation

CORS means Cross-Origin Resource Sharing.

It controls whether frontend from one origin can call backend from another origin.

### Practical Scenario

Angular may run on:

```text
http://localhost:4200
```

Backend may run on:

```text
http://localhost:5190
```

Browser sees these as different origins.

CORS allows frontend to call backend.

### In Your Project

API Gateway config:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

Development services also allow CORS:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
```

### Why CORS Is Needed

Without CORS, browser blocks frontend API calls even if backend is running correctly.

### Production Note

`AllowAnyOrigin` is okay for local development, but production should restrict allowed origins.

### Viva Answer

> CORS allows Angular frontend running on a different origin to call ASP.NET Core backend APIs. My project enables open CORS mainly for development.

---

## 17. HTTPS Redirection

### Simple Explanation

HTTPS encrypts communication between client and server.

`app.UseHttpsRedirection()` redirects HTTP requests to HTTPS.

### In Your Project

Most API services use:

```csharp
app.UseHttpsRedirection();
```

### Why It Is Needed

Sensitive data like:

- Passwords
- JWT tokens
- Payment-related data
- OTPs

should not travel over plain HTTP in production.

### Viva Answer

> HTTPS redirection improves security by ensuring API communication uses encrypted HTTPS instead of plain HTTP.

---

## 18. Environment-Based Behavior

### Simple Explanation

Applications can behave differently in development and production.

Development may allow:

- Open CORS
- Detailed Swagger
- Debug-friendly settings

Production should be stricter.

### In Your Project

Example:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
```

### Why It Is Useful

Local development needs flexibility. Production needs security.

### Viva Answer

> ASP.NET Core supports environment-based configuration. My project uses development checks to enable permissive CORS locally.

---

## 19. Complete Request Flow in ASP.NET Core

### Example: Login

```text
1. Angular sends POST /api/auth/login
2. API Gateway routes request to IdentityService
3. ASP.NET Core receives request
4. Middleware pipeline runs
5. Routing finds AuthController.Login
6. Model binding converts JSON to LoginRequest
7. Controller calls AuthService.LoginAsync
8. AuthService checks database and password
9. JWT token is generated
10. Controller returns Ok response
11. ASP.NET Core serializes response to JSON
12. Frontend receives response
```

### Example: Protected Interview API

```text
1. Angular sends GET /api/interviews with Bearer token
2. Gateway routes request to InterviewService
3. Authentication middleware validates JWT
4. Authorization middleware checks [Authorize]
5. Controller reads userId from claims
6. Service fetches user's interviews
7. Controller returns response
```

---

## 20. What Happens If Important Pieces Are Missing?

| Missing Piece | Result |
|---|---|
| AddControllers | Controllers may not be registered |
| MapControllers | Routes will not reach controller actions |
| UseAuthentication | JWT token will not be validated |
| UseAuthorization | Access rules will not be enforced |
| AddDbContext | Database access will fail |
| AddScoped service registration | Controller cannot get service dependency |
| Swagger setup | API documentation/testing UI unavailable |
| CORS | Browser may block frontend requests |
| Global exception middleware | Error responses become inconsistent |

---

## 21. Best Full Viva Answer for Topic 2

> ASP.NET Core Web API is used in my project to build backend services. Each service has a Program.cs file where controllers, database context, services, Swagger, JWT authentication, authorization, CORS, and middleware are configured. Controllers expose endpoints like login, start assessment, submit interview, and subscribe. Requests pass through middleware such as exception handling, authentication, and authorization before reaching controller actions. Controllers use dependency injection to call service classes, and service classes perform business logic and database operations. Finally, the backend returns JSON responses to the Angular frontend.

---

## 22. Common Viva Questions and Answers

### Q1. What is ASP.NET Core?

ASP.NET Core is a framework for building web applications, APIs, and microservices using C# and .NET.

### Q2. What is Web API?

Web API exposes backend functionality through HTTP endpoints that frontend can call.

### Q3. What is Program.cs?

Program.cs is the startup file where services and middleware are configured before the application runs.

### Q4. What is a controller?

A controller is a class that receives HTTP requests and exposes API endpoints.

### Q5. What is an action method?

An action method is a method inside a controller that handles one specific API endpoint.

### Q6. What is routing?

Routing maps incoming URLs and HTTP methods to controller actions.

### Q7. What is model binding?

Model binding automatically converts request body, route values, or query string values into C# objects or parameters.

### Q8. What is middleware?

Middleware is code that runs in the request pipeline before or after the controller action.

### Q9. Why is middleware order important?

Because some middleware depends on previous middleware. For example, authentication must run before authorization.

### Q10. What is dependency injection?

Dependency injection provides required service objects to classes automatically, usually through constructors.

### Q11. Why use IActionResult?

IActionResult allows controller actions to return different HTTP responses like Ok, BadRequest, NotFound, or Unauthorized.

### Q12. What is Swagger?

Swagger is an API documentation and testing tool used to view and test backend endpoints.

### Q13. What is CORS?

CORS allows a frontend running on one origin to call backend APIs running on another origin.

### Q14. What does app.MapControllers do?

It maps controller routes into the ASP.NET Core request pipeline so API endpoints can work.

### Q15. Why use DTOs?

DTOs transfer only required data between frontend and backend and avoid exposing internal database models directly.

---

## 23. Quick Revision Summary

- ASP.NET Core is the backend framework.
- Web API exposes HTTP endpoints.
- Program.cs configures services and middleware.
- Controllers receive requests.
- Actions handle specific endpoints.
- Routing maps URL to action.
- Model binding converts request data to C# objects.
- DTOs define API request/response shapes.
- Middleware runs before/after controller actions.
- Authentication checks user identity.
- Authorization checks permissions.
- Dependency injection provides services automatically.
- Swagger documents and tests APIs.
- CORS allows Angular to call backend.
- app.MapControllers enables controller routing.

