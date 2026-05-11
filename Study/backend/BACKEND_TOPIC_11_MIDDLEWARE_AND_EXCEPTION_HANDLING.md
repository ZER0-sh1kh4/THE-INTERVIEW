# Topic 11: Middleware and Exception Handling

Project: Mock Interview Platform  
Focus: Understanding ASP.NET Core middleware pipeline, global exception handling, custom exceptions, logging, validation errors, and consistent API error responses.

---

## 1. Why Middleware Is Needed

### Simple Explanation

Middleware is code that runs between the incoming HTTP request and the final API response.

Every request passes through a pipeline.

Each middleware can:

- Read the request
- Modify the request
- Stop the request
- Call the next middleware
- Modify the response
- Handle errors

### Practical Scenario

When frontend calls:

```text
GET /api/assessments/10/result
```

The request may pass through:

```text
Exception middleware
CORS middleware
Authentication middleware
Authorization middleware
Controller endpoint
```

If something goes wrong, global exception middleware catches the error and returns a proper JSON error response.

### Viva Answer

> Middleware is needed to process HTTP requests and responses in a reusable pipeline. It handles cross-cutting concerns like exception handling, authentication, authorization, CORS, and routing.

---

## 2. What Is Middleware?

### Technical Definition

Middleware is a component in ASP.NET Core request pipeline that receives an `HttpContext`, performs some work, and either passes control to the next middleware or short-circuits the request.

### Simple Flow

```text
Request -> Middleware 1 -> Middleware 2 -> Controller -> Response
```

### Example

Exception middleware wraps the next middleware:

```csharp
try
{
    await _next(context);
}
catch (Exception ex)
{
    // Convert exception into JSON response
}
```

### Viva Answer

> Middleware is a pipeline component that processes HTTP requests and responses. It can call the next middleware or stop the request and return a response itself.

---

## 3. What Is Middleware Pipeline?

### Simple Explanation

Middleware pipeline is the ordered chain of middleware configured in `Program.cs`.

Order matters.

### Example

```csharp
app.UseGlobalExceptionHandling();
app.UseCors(...);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### Meaning

```text
First handle exceptions
Then handle CORS
Then authenticate user
Then authorize user
Then execute controller action
```

### Viva Answer

> Middleware pipeline is the ordered sequence of middleware through which every request passes. The order is important because each middleware depends on previous middleware behavior.

---

## 4. Middleware in Your Project

### In Your Services

The Web API services use middleware in `Program.cs`:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

Common middleware:

```text
UseGlobalExceptionHandling
UseHttpsRedirection
UseCors
UseSwagger
UseAuthentication
UseAuthorization
MapControllers
```

### Notable Difference

`NotificationService` is a worker service.

It does not expose HTTP controllers, so it does not use the same Web API middleware pipeline.

### Viva Answer

> My Web API services use middleware for exception handling, CORS, Swagger, authentication, authorization, and controller routing. NotificationService is a worker service, so it mainly uses hosted services instead of HTTP middleware.

---

## 5. What Is Exception Handling?

### Simple Explanation

Exception handling means managing runtime errors properly.

Instead of crashing or returning random error messages, backend should return a clear response.

### Example Error

Assessment result not found:

```text
Assessment result is not available yet. Submit the assessment first.
```

Backend should return:

```text
404 Not Found
JSON error response
```

### Viva Answer

> Exception handling means catching runtime errors and converting them into meaningful HTTP responses instead of allowing the application to crash or expose raw errors.

---

## 6. Why Global Exception Handling Is Used

### Simple Explanation

Without global exception handling, every controller action would need repeated try-catch blocks.

### Without Global Middleware

```csharp
try
{
    var result = await _service.GetResultAsync(id);
    return Ok(result);
}
catch (Exception ex)
{
    return StatusCode(500, ex.Message);
}
```

This becomes repetitive and inconsistent.

### With Global Middleware

Service throws exception:

```csharp
throw new NotFoundAppException("Assessment not found.");
```

Middleware catches it and returns consistent response.

### Viva Answer

> Global exception handling is used to centralize error handling. It avoids repeated try-catch blocks and ensures all APIs return consistent error responses.

---

## 7. GlobalExceptionMiddleware

### File

```text
Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs
```

### Purpose

It catches exceptions thrown from controllers, services, or later middleware.

### Important Code

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (AppException ex)
    {
        _logger.LogWarning(ex, "Handled application exception for {Path}", context.Request.Path);
        await WriteErrorAsync(context, ex.StatusCode, ex.Message, ex.Details);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access for {Path}", context.Request.Path);
        await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized access.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
        await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "Internal server error.", ex.Message);
    }
}
```

### Viva Answer

> GlobalExceptionMiddleware wraps the request pipeline in try-catch. It catches custom AppException, unauthorized errors, and unexpected exceptions, then returns a standard JSON error response.

---

## 8. Registering Global Exception Middleware

### File

```text
Backend/BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs
```

### Code

```csharp
public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
{
    return app.UseMiddleware<GlobalExceptionMiddleware>();
}
```

### Used In

```text
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
```

### Code

```csharp
app.UseGlobalExceptionHandling();
```

### Viva Answer

> The global exception middleware is registered through the shared extension method UseGlobalExceptionHandling and used in each Web API service's Program.cs.

---

## 9. Why Middleware Order Matters

### Simple Explanation

Middleware runs in the order it is added.

If exception middleware is placed early, it can catch errors from later middleware and controllers.

### Good Order

```csharp
app.UseGlobalExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### Why This Is Good

If controller or service throws an exception, exception middleware can catch it.

### Bad Order

```csharp
app.MapControllers();
app.UseGlobalExceptionHandling();
```

This is wrong because controller execution may happen before exception middleware is active.

### Viva Answer

> Middleware order matters because each request passes through middleware in sequence. Exception middleware should be early so it can catch errors from later middleware and controllers.

---

## 10. AppException

### File

```text
Backend/BuildingBlocks/Exceptions/AppException.cs
```

### Purpose

`AppException` is the base class for controlled business errors.

### Code

```csharp
public class AppException : Exception
{
    public int StatusCode { get; }
    public string? Details { get; }

    public AppException(
        string message,
        int statusCode = StatusCodes.Status400BadRequest,
        string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }
}
```

### Why It Is Useful

It carries:

```text
Error message
HTTP status code
Optional details
```

### Viva Answer

> AppException is a custom base exception for expected business errors. It stores an HTTP status code and optional details so middleware can return the correct API response.

---

## 11. Custom Exception Classes

### In Your Project

Custom exceptions:

```text
AppException
ValidationAppException
NotFoundAppException
ForbiddenAppException
```

### Why Custom Exceptions Are Used

They make errors meaningful.

Instead of:

```csharp
throw new Exception("Question not found");
```

Use:

```csharp
throw new NotFoundAppException("Question not found.");
```

Now middleware knows it should return:

```text
404 Not Found
```

### Viva Answer

> Custom exceptions represent different types of business errors. They help middleware convert errors into correct HTTP status codes.

---

## 12. ValidationAppException

### File

```text
Backend/BuildingBlocks/Exceptions/ValidationAppException.cs
```

### Purpose

Used when user input or request state is invalid.

### Code

```csharp
public sealed class ValidationAppException : AppException
{
    public ValidationAppException(string message, string? details = null)
        : base(message, StatusCodes.Status400BadRequest, details)
    {
    }
}
```

### Project Examples

```csharp
throw new ValidationAppException("Domain is required.");
throw new ValidationAppException("At least one answer must be submitted.");
throw new ValidationAppException("Correct option must be one of A, B, C, or D.");
```

### Viva Answer

> ValidationAppException is used for bad input or invalid request conditions and returns HTTP 400 Bad Request.

---

## 13. NotFoundAppException

### File

```text
Backend/BuildingBlocks/Exceptions/NotFoundAppException.cs
```

### Purpose

Used when requested data does not exist.

### Code

```csharp
public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(string message)
        : base(message, StatusCodes.Status404NotFound)
    {
    }
}
```

### Project Examples

```csharp
throw new NotFoundAppException("User not found.");
throw new NotFoundAppException("Assessment not found.");
throw new NotFoundAppException("Interview not found.");
throw new NotFoundAppException("Payment record not found.");
```

### Viva Answer

> NotFoundAppException is used when a requested record is missing and returns HTTP 404 Not Found.

---

## 14. ForbiddenAppException

### File

```text
Backend/BuildingBlocks/Exceptions/ForbiddenAppException.cs
```

### Purpose

Used when user is authenticated but not allowed to perform an action.

### Code

```csharp
public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message)
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}
```

### Project Examples

```csharp
throw new ForbiddenAppException("You are not allowed to submit this assessment.");
throw new ForbiddenAppException("You are not allowed to view this assessment result.");
throw new ForbiddenAppException("Missing Stripe-Signature header.");
```

### Viva Answer

> ForbiddenAppException is used when the caller is not allowed to perform an operation and returns HTTP 403 Forbidden.

---

## 15. UnauthorizedAccessException

### Simple Explanation

Unauthorized means the user identity is missing or invalid.

### In Your Project

AuthController throws:

```csharp
throw new UnauthorizedAccessException("Unable to read the authenticated user.");
```

Global middleware catches it:

```csharp
catch (UnauthorizedAccessException ex)
{
    await WriteErrorAsync(
        context,
        StatusCodes.Status401Unauthorized,
        "Unauthorized access.");
}
```

### Viva Answer

> UnauthorizedAccessException is handled by the global middleware and converted into HTTP 401 Unauthorized.

---

## 16. 400 vs 401 vs 403 vs 404 vs 500

### Status Code Meaning

| Status Code | Meaning | Project Example |
|---|---|---|
| 400 | Bad request or validation failure | Domain is required |
| 401 | Not authenticated or invalid credentials | Invalid login token |
| 403 | Authenticated but not allowed | Free user limit reached |
| 404 | Resource not found | Assessment not found |
| 500 | Unexpected server error | Unhandled bug |

### Viva Answer

> 400 means invalid request, 401 means unauthenticated, 403 means authenticated but not allowed, 404 means resource not found, and 500 means unexpected server error.

---

## 17. ApiErrorResponse

### File

```text
Backend/BuildingBlocks/Contracts/ApiErrorResponse.cs
```

### Code

```csharp
public class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
}
```

### Example Response

```json
{
  "success": false,
  "message": "Assessment not found.",
  "details": null
}
```

### Why It Is Useful

Frontend can handle errors consistently.

It always knows:

```text
success
message
details
```

### Viva Answer

> ApiErrorResponse is the standard error response format used by APIs. It gives frontend a consistent structure for displaying errors.

---

## 18. WriteErrorAsync

### File

```text
Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs
```

### Code

```csharp
private static async Task WriteErrorAsync(
    HttpContext context,
    int statusCode,
    string message,
    string? details = null)
{
    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/json";

    var payload = new ApiErrorResponse
    {
        Message = message,
        Details = details
    };

    await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
}
```

### Meaning

This method:

```text
Sets HTTP status code
Sets JSON content type
Creates ApiErrorResponse
Serializes it to JSON
Writes it to response body
```

### Viva Answer

> WriteErrorAsync converts an exception into a JSON API error response with proper status code and content type.

---

## 19. Logging Errors

### Simple Explanation

Logging records what happened inside backend.

It helps developers debug issues.

### In Middleware

Handled application errors:

```csharp
_logger.LogWarning(ex, "Handled application exception for {Path}", context.Request.Path);
```

Unexpected errors:

```csharp
_logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
```

### Why Different Levels

```text
Warning: Expected business error
Error: Unexpected system failure
```

### Viva Answer

> The middleware logs expected business exceptions as warnings and unexpected exceptions as errors. Logs help debug backend failures without exposing internal details to users.

---

## 20. Business Error vs System Error

### Business Error

Expected condition caused by application rules.

Examples:

```text
Free user reached assessment limit
Assessment not found
Invalid OTP
Time expired
Payment already confirmed
```

Usually handled with:

```text
AppException or derived custom exception
```

### System Error

Unexpected technical problem.

Examples:

```text
NullReferenceException
Database connection failure
Bug in code
External service crash
Serialization failure
```

Usually returns:

```text
500 Internal Server Error
```

### Viva Answer

> Business errors are expected rule-based errors like invalid input or not found. System errors are unexpected technical failures and usually return 500.

---

## 21. Validation Errors from Model Binding

### Simple Explanation

ASP.NET Core can automatically validate request DTOs.

If model validation fails, project returns a standard error response.

### File

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Code

```csharp
services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = string.Join("; ", context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? "Invalid request."
                : e.ErrorMessage));

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Message = "Validation failed.",
            Details = errors
        });
    };
});
```

### Viva Answer

> The project customizes model validation errors through AddApiDefaults so invalid request DTOs return a consistent ApiErrorResponse with 400 Bad Request.

---

## 22. AddApiDefaults

### File

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Purpose

Registers shared API behavior for all services.

### Used In

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Code

```csharp
builder.Services.AddApiDefaults();
```

### What It Configures

Currently it configures consistent validation error response.

### Viva Answer

> AddApiDefaults is a shared extension method that configures common API behavior, especially consistent validation error responses.

---

## 23. Exception Handling in IdentityService

### Examples

Invalid login:

```csharp
throw new AppException("Invalid credentials.", StatusCodes.Status401Unauthorized);
```

Deactivated account:

```csharp
throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
```

Invalid OTP:

```csharp
throw new AppException("Invalid or expired OTP.", StatusCodes.Status400BadRequest);
```

User not found:

```csharp
throw new NotFoundAppException("User not found.");
```

### Viva Answer

> IdentityService uses custom exceptions for invalid credentials, deactivated accounts, invalid OTP, and missing users. Global middleware converts them into proper HTTP responses.

---

## 24. Exception Handling in AssessmentService

### Examples

Missing domain:

```csharp
throw new ValidationAppException("Domain is required.");
```

Free limit reached:

```csharp
throw new ForbiddenAppException(
    "Free users can create only 2 assessment tests. Upgrade to premium for unlimited access.");
```

Assessment not found:

```csharp
throw new NotFoundAppException("Assessment not found.");
```

User not owner:

```csharp
throw new ForbiddenAppException("You are not allowed to view this assessment result.");
```

### Viva Answer

> AssessmentService uses validation, not found, and forbidden exceptions to handle invalid assessment flow, ownership checks, and free user limits.

---

## 25. Exception Handling in InterviewService

### Examples

Missing domain:

```csharp
throw new ValidationAppException("Domain is required.");
```

Free limit reached:

```csharp
throw new ForbiddenAppException(
    "Free users can create only 1 interview. Upgrade to premium to attempt more interviews.");
```

Interview not found:

```csharp
throw new NotFoundAppException("Interview not found.");
```

Gemini unavailable:

```csharp
throw new AppException(
    "Gemini API key is not configured.",
    StatusCodes.Status503ServiceUnavailable);
```

### Viva Answer

> InterviewService uses custom exceptions for domain validation, free user limits, missing interviews, invalid interview states, and Gemini service failures.

---

## 26. Exception Handling in SubscriptionService

### Examples

Manual confirm disabled:

```csharp
throw new ValidationAppException(
    "Manual confirm is disabled when Stripe mode is enabled. Wait for Stripe webhook callback.");
```

Payment not found:

```csharp
throw new NotFoundAppException("Payment record not found.");
```

Missing Stripe signature:

```csharp
throw new ForbiddenAppException("Missing Stripe-Signature header.");
```

Invalid Stripe webhook:

```csharp
throw new ForbiddenAppException(
    "Invalid Stripe webhook signature or webhook secret.");
```

### Viva Answer

> SubscriptionService uses custom exceptions to handle payment validation, missing records, Stripe configuration issues, and invalid webhook signatures.

---

## 27. Why Controllers Avoid Try-Catch

### Simple Explanation

Controllers should stay clean.

They should focus on HTTP request and response.

### Controller Should Do

```text
Receive request
Call service
Return result
```

### Service Should Do

```text
Apply business rules
Throw meaningful exceptions
```

### Middleware Should Do

```text
Catch exceptions
Log errors
Return standard error response
```

### Viva Answer

> Controllers avoid repetitive try-catch because global exception middleware handles errors centrally. This keeps controllers cleaner and more maintainable.

---

## 28. Protected Endpoint Error Flow

### Scenario

User calls protected endpoint without token:

```text
GET /api/auth/me
```

### Flow

```text
1. Request enters middleware pipeline.
2. Authentication middleware checks JWT.
3. Token is missing or invalid.
4. Request is treated as unauthorized.
5. API returns 401 Unauthorized.
```

### Important Note

Authentication/authorization failures are often handled by ASP.NET Core authentication middleware itself.

Business-level unauthorized errors can still be handled by `GlobalExceptionMiddleware`.

### Viva Answer

> Protected endpoint authentication failures are handled by authentication and authorization middleware, while custom unauthorized business errors can be caught by global exception middleware.

---

## 29. Ownership Error Flow

### Scenario

User tries to view another user's assessment result.

### Flow

```text
1. User sends GET /api/assessments/{id}/result.
2. Controller calls AssessmentSvc.
3. Service loads assessment from database.
4. Service compares assessment.UserId with logged-in user id.
5. If mismatch, service throws ForbiddenAppException.
6. GlobalExceptionMiddleware catches it.
7. Middleware returns 403 JSON response.
```

### Viva Explanation

> Ownership errors are handled at service layer using ForbiddenAppException. The middleware converts it into a 403 response.

---

## 30. Not Found Error Flow

### Scenario

User requests interview that does not exist.

### Flow

```text
1. User calls GET /api/interviews/{id}.
2. InterviewService searches database.
3. Record is null.
4. Service throws NotFoundAppException.
5. Middleware catches AppException.
6. Response status becomes 404.
7. ApiErrorResponse is returned.
```

### Viva Explanation

> Missing records are represented using NotFoundAppException and returned as 404 through global exception middleware.

---

## 31. Validation Error Flow

### Scenario

User starts assessment without domain.

### Flow

```text
1. Frontend sends start assessment request.
2. AssessmentSvc checks domain.
3. Domain is empty.
4. Service throws ValidationAppException.
5. Middleware returns 400 Bad Request.
```

### Viva Explanation

> Invalid business input is handled using ValidationAppException and converted into 400 Bad Request.

---

## 32. Unexpected Error Flow

### Scenario

A coding bug causes `NullReferenceException`.

### Flow

```text
1. Request reaches service method.
2. Unexpected exception occurs.
3. It is not AppException.
4. GlobalExceptionMiddleware catches generic Exception.
5. Logs it as error.
6. Returns 500 Internal Server Error.
```

### Viva Answer

> Unexpected exceptions are caught by the generic catch block, logged as errors, and returned as 500 Internal Server Error.

---

## 33. Why Consistent Error Responses Help Frontend

### Simple Explanation

Frontend should not guess error format.

If every backend error uses the same shape, frontend can display it easily.

### Standard Shape

```json
{
  "success": false,
  "message": "Validation failed.",
  "details": "Domain is required."
}
```

### Frontend Benefit

Frontend can always read:

```text
response.message
response.details
```

### Viva Answer

> Consistent error responses help frontend show user-friendly messages without writing different parsing logic for each API.

---

## 34. Security in Exception Handling

### Important Rule

Do not expose sensitive internal details to users.

### Risky Details

```text
Database connection string
Stack trace
Secret keys
SQL query details
Internal file paths
Payment secret details
```

### In Your Middleware

Unexpected exception response uses:

```csharp
await WriteErrorAsync(
    context,
    StatusCodes.Status500InternalServerError,
    "Internal server error.",
    ex.Message);
```

### Improvement Note

Returning `ex.Message` as details is useful during development, but in production it is safer to hide internal details.

### Viva Answer

> Exception handling should avoid exposing stack traces or secrets. The current middleware returns a standard internal server error, and production systems should avoid sending raw exception details to frontend.

---

## 35. UseHttpsRedirection Middleware

### Simple Explanation

HTTPS redirection sends HTTP requests to HTTPS.

### In Your Project

Services use:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

### Meaning

HTTPS redirection is enabled outside development.

### Viva Answer

> UseHttpsRedirection redirects HTTP traffic to HTTPS. In my project it is enabled outside development to improve transport security.

---

## 36. CORS Middleware

### Simple Explanation

CORS controls which frontend origins can call backend APIs from browser.

### In Your Project

Development CORS:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors(options =>
        options.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
}
```

### Why It Is Needed

Angular frontend may run on a different port than backend.

Browser blocks cross-origin calls unless backend allows them.

### Viva Answer

> CORS middleware allows browser-based frontend requests from different origins. In development, my services allow any origin, method, and header.

---

## 37. Authentication and Authorization Middleware

### Authentication

Checks who the user is.

```csharp
app.UseAuthentication();
```

### Authorization

Checks what the user is allowed to do.

```csharp
app.UseAuthorization();
```

### Correct Order

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Authentication must run before authorization.

### Viva Answer

> Authentication middleware validates the JWT and builds user identity. Authorization middleware checks roles and policies. Authentication must run before authorization.

---

## 38. Swagger Middleware

### Simple Explanation

Swagger middleware exposes API documentation and testing UI.

### In Your Project

Development mode:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Viva Answer

> Swagger middleware provides API documentation and testing UI during development.

---

## 39. MapControllers

### Simple Explanation

`MapControllers()` maps controller routes to endpoints.

### In Your Project

```csharp
app.MapControllers();
```

### Meaning

It enables routes like:

```text
/api/auth/login
/api/interviews/start
/api/assessments/start
/api/subscriptions/subscribe
```

### Viva Answer

> MapControllers connects attribute-routed controller actions to the ASP.NET Core endpoint routing system.

---

## 40. Complete Request Pipeline Example

### Scenario

User submits assessment.

```text
POST /api/assessments/submit
```

### Flow

```text
1. Request enters AssessmentService.
2. GlobalExceptionMiddleware starts try-catch.
3. CORS middleware handles browser cross-origin rules if needed.
4. Authentication middleware validates JWT.
5. Authorization middleware checks protected endpoint access.
6. MapControllers routes request to AssessmentController.
7. Controller calls AssessmentSvc.
8. AssessmentSvc validates request and ownership.
9. If error occurs, service throws custom exception.
10. Exception bubbles back to GlobalExceptionMiddleware.
11. Middleware logs the error.
12. Middleware returns ApiErrorResponse with correct status code.
```

### Viva Explanation

> The request passes through middleware before reaching the controller. If AssessmentSvc throws a custom exception, global middleware catches it and returns a consistent JSON error response.

---

## 41. What Happens If Global Exception Middleware Is Removed?

### Problems

- Repeated try-catch in controllers
- Inconsistent error response format
- Raw framework errors may reach frontend
- Frontend error handling becomes harder
- Logs may become inconsistent
- Business exceptions may not map cleanly to status codes

### Viva Answer

> Without global exception middleware, each controller would need manual error handling and responses could become inconsistent. It would also be harder to log and standardize errors.

---

## 42. Alternatives to Custom Exception Middleware

### Built-In Exception Handler

ASP.NET Core provides:

```csharp
app.UseExceptionHandler(...)
```

### Exception Filters

MVC filters can handle controller/action exceptions.

### Problem Details

ASP.NET Core can return RFC 7807 ProblemDetails responses.

### Third-Party Logging/Monitoring

Examples:

```text
Serilog
Seq
Application Insights
Sentry
ELK Stack
```

### In Your Project

Custom middleware is simple and reusable across services through `BuildingBlocks`.

### Viva Answer

> Alternatives include UseExceptionHandler, exception filters, ProblemDetails, and monitoring tools. My project uses custom middleware for a simple shared error format across services.

---

## 43. Possible Improvements

### Improvements

- Hide raw exception details in production
- Add correlation ID to each error response
- Use ProblemDetails standard format
- Add request path and trace ID to error response
- Add centralized logging using Serilog or Application Insights
- Add separate handling for external service failures
- Add validation error dictionary instead of joined string
- Add rate limiting middleware
- Add request/response logging middleware with sensitive data masking
- Add health check endpoints

### Balanced Viva Answer

> The current middleware gives consistent error handling across services. Future improvements could include production-safe error details, correlation IDs, centralized logging, and ProblemDetails support.

---

## 44. Important Code Files

### Middleware

```text
Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs
```

### Middleware Extension

```text
Backend/BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs
```

### API Defaults

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Error Response Contract

```text
Backend/BuildingBlocks/Contracts/ApiErrorResponse.cs
```

### Custom Exceptions

```text
Backend/BuildingBlocks/Exceptions/AppException.cs
Backend/BuildingBlocks/Exceptions/ValidationAppException.cs
Backend/BuildingBlocks/Exceptions/NotFoundAppException.cs
Backend/BuildingBlocks/Exceptions/ForbiddenAppException.cs
```

### Program.cs Usage

```text
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
```

---

## 45. Best Full Viva Answer for Topic 11

> Middleware in ASP.NET Core is a request pipeline component that processes HTTP requests and responses. In my project, middleware is used for global exception handling, CORS, Swagger, authentication, authorization, and controller routing. The most important custom middleware is GlobalExceptionMiddleware in BuildingBlocks. It wraps the pipeline in try-catch, catches AppException, UnauthorizedAccessException, and unexpected exceptions, logs them, and returns a standard ApiErrorResponse JSON object. Custom exceptions like ValidationAppException, NotFoundAppException, and ForbiddenAppException represent 400, 404, and 403 errors. This keeps controllers clean, avoids repeated try-catch blocks, and gives frontend consistent error responses.

---

## 46. Common Viva Questions and Answers

### Q1. What is middleware?

Middleware is a component in ASP.NET Core request pipeline that processes HTTP requests and responses.

### Q2. What is middleware pipeline?

It is the ordered chain of middleware configured in Program.cs.

### Q3. Why does middleware order matter?

Because requests pass through middleware in the order they are registered, and some middleware depends on earlier middleware.

### Q4. What is global exception handling?

It is centralized error handling that catches exceptions from controllers and services and returns standard responses.

### Q5. Why use global exception middleware?

It avoids repeated try-catch blocks and makes error responses consistent.

### Q6. Which file contains your global exception middleware?

`Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs`.

### Q7. How is global exception middleware registered?

Using `app.UseGlobalExceptionHandling()` in each Web API service's Program.cs.

### Q8. What is AppException?

AppException is the base custom exception for controlled business errors and includes an HTTP status code.

### Q9. What is ValidationAppException?

It represents validation or bad request errors and returns HTTP 400.

### Q10. What is NotFoundAppException?

It represents missing resources and returns HTTP 404.

### Q11. What is ForbiddenAppException?

It represents permission or ownership errors and returns HTTP 403.

### Q12. What is ApiErrorResponse?

It is the standard JSON error response containing success, message, and details.

### Q13. Difference between 401 and 403?

401 means user is not authenticated. 403 means user is authenticated but not allowed to perform the action.

### Q14. Difference between 400 and 404?

400 means invalid request. 404 means requested resource does not exist.

### Q15. What status code is used for unexpected errors?

500 Internal Server Error.

### Q16. Why log exceptions?

Logs help developers debug backend failures without exposing internal details to users.

### Q17. Why should raw exception details not be exposed?

They may reveal sensitive information like stack traces, internal paths, SQL details, or secrets.

### Q18. What is AddApiDefaults?

It is a shared extension method that configures consistent validation error responses.

### Q19. What is UseAuthentication?

It validates the JWT and creates the authenticated user identity.

### Q20. What is UseAuthorization?

It checks whether the authenticated user has permission to access an endpoint.

### Q21. Which comes first, authentication or authorization?

Authentication must come before authorization.

### Q22. What is UseCors?

It configures browser cross-origin access rules for frontend-backend communication.

### Q23. What is UseSwagger?

It enables Swagger API documentation and testing UI.

### Q24. What is MapControllers?

It maps controller route attributes to API endpoints.

### Q25. What happens if global exception middleware is removed?

Controllers would need manual try-catch blocks and API error responses could become inconsistent.

---

## 47. Quick Revision Summary

- Middleware processes HTTP requests and responses.
- Middleware pipeline order matters.
- Exception middleware should be early in the pipeline.
- GlobalExceptionMiddleware catches service/controller exceptions.
- AppException is base class for controlled business errors.
- ValidationAppException returns 400.
- UnauthorizedAccessException returns 401.
- ForbiddenAppException returns 403.
- NotFoundAppException returns 404.
- Generic unexpected exceptions return 500.
- ApiErrorResponse gives consistent error format.
- AddApiDefaults customizes model validation responses.
- Controllers stay clean because services throw meaningful exceptions.
- Logs are used for debugging.
- Business errors are expected rule failures.
- System errors are unexpected technical failures.
- Authentication middleware validates JWT.
- Authorization middleware checks permissions.
- CORS middleware allows browser frontend calls.
- Swagger middleware enables API docs.
- MapControllers connects controller routes to endpoints.
- Future improvements include correlation IDs and hiding raw exception details in production.

