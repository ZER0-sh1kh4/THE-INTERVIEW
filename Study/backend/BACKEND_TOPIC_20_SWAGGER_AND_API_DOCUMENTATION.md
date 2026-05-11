# Topic 20: Swagger and API Documentation

Project: Mock Interview Platform  
Focus: Understanding Swagger/OpenAPI, Swagger UI, API testing, Bearer token security scheme, XML comments, service-level Swagger, API Gateway Swagger aggregation, and why API documentation matters.

---

## 1. What Is Swagger?

### Simple Explanation

Swagger is a tool that shows API documentation in a browser.

It lists:

- API endpoints
- HTTP methods
- Request body
- Response format
- Required parameters
- Authentication requirements

### In Your Project

Swagger is used in:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
API_Gateway
```

### Viva Answer

> Swagger is used to document and test backend APIs through a browser-based UI.

---

## 2. What Is OpenAPI?

### Simple Explanation

OpenAPI is the standard specification that describes REST APIs.

Swagger UI reads OpenAPI JSON and displays it as interactive documentation.

### Example URL

```text
/swagger/v1/swagger.json
```

This JSON describes the API.

### Viva Answer

> OpenAPI is the standard API description format, and Swagger UI displays that OpenAPI document in a human-friendly interface.

---

## 3. Why Swagger Is Helpful

### Benefits

- Easy API testing
- Shows endpoint routes
- Shows request/response DTOs
- Helps frontend developers
- Helps backend debugging
- Helps viva explanation
- Supports Bearer token testing
- Reduces need for external tools for basic testing

### Viva Answer

> Swagger is helpful because it documents APIs and allows developers to test endpoints directly from the browser.

---

## 4. Important Swagger Files

### Service Program.cs Files

```text
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
```

### Gateway Files

```text
Backend/API_Gateway/Program.cs
Backend/API_Gateway/ocelot.json
```

### Optional Operation Filters

```text
Backend/IdentityService/Services/AuthorizeOperationFilter.cs
Backend/InterviewService/Services/AuthorizeOperationFilter.cs
Backend/AssessmentService/Services/AuthorizeOperationFilter.cs
Backend/SubscriptionService/Services/AuthorizeOperationFilter.cs
```

### Viva Answer

> Swagger is configured in each service's Program.cs and aggregated in API Gateway using SwaggerForOcelot.

---

## 5. Swagger Packages

### In Your Project

Package versions are centrally managed in:

```text
Backend/Directory.Packages.props
```

### Packages

```text
Swashbuckle.AspNetCore
Swashbuckle.AspNetCore.SwaggerUI
MMLib.SwaggerForOcelot
```

### Meaning

```text
Swashbuckle -> service Swagger/OpenAPI
SwaggerUI -> browser UI
SwaggerForOcelot -> gateway aggregation
```

### Viva Answer

> The project uses Swashbuckle for service Swagger documentation and MMLib.SwaggerForOcelot for gateway Swagger aggregation.

---

## 6. AddEndpointsApiExplorer

### Code

```csharp
builder.Services.AddEndpointsApiExplorer();
```

### Purpose

Provides endpoint metadata used by Swagger/OpenAPI generation.

### Viva Answer

> AddEndpointsApiExplorer provides endpoint metadata so Swagger can discover and document API routes.

---

## 7. AddSwaggerGen

### Code

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IdentityService",
        Version = "v1",
        Description = "Handles user registration, login, profile lookup and premium status updates."
    });
});
```

### Purpose

Registers Swagger/OpenAPI document generation.

### Viva Answer

> AddSwaggerGen registers Swagger generation and defines API metadata like title, version, and description.

---

## 8. OpenApiInfo

### Simple Explanation

OpenApiInfo describes the API document.

### In Your Project

Each service has its own title:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Includes

```text
Title
Version
Description
```

### Viva Answer

> OpenApiInfo provides metadata such as API title, version, and description in Swagger.

---

## 9. Service-Level Swagger

### Each Service Has Its Own Swagger

```text
IdentityService -> /swagger/v1/swagger.json
InterviewService -> /swagger/v1/swagger.json
AssessmentService -> /swagger/v1/swagger.json
SubscriptionService -> /swagger/v1/swagger.json
```

### Swagger UI

Each service exposes its own Swagger UI.

### Viva Answer

> Each backend service exposes its own Swagger documentation and Swagger UI.

---

## 10. UseSwagger

### Code

```csharp
app.UseSwagger();
```

### Purpose

Enables serving the OpenAPI JSON document.

### Example

```text
/swagger/v1/swagger.json
```

### Viva Answer

> UseSwagger enables the service to expose its OpenAPI JSON document.

---

## 11. UseSwaggerUI

### Code

```csharp
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityService v1"));
```

### Purpose

Enables browser-based Swagger UI.

### Viva Answer

> UseSwaggerUI enables the interactive browser page for viewing and testing APIs.

---

## 12. Swagger JSON vs Swagger UI

### Swagger JSON

Machine-readable OpenAPI document:

```text
/swagger/v1/swagger.json
```

### Swagger UI

Human-friendly browser interface:

```text
/swagger
```

### Viva Answer

> Swagger JSON is the OpenAPI document, while Swagger UI is the interactive page that displays and tests it.

---

## 13. XML Comments

### Code

```csharp
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
if (File.Exists(xmlPath))
{
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
}
```

### Purpose

Adds controller/action XML summary comments into Swagger documentation.

### Example

Controller comments like:

```csharp
/// <summary>
/// Logs in a user and returns a JWT token.
/// </summary>
```

can appear in Swagger.

### Viva Answer

> XML comments enrich Swagger documentation by showing controller and action summaries.

---

## 14. Bearer Token Security Scheme

### Code

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "JWT Authorization header using the Bearer scheme.",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
});
```

### Meaning

Swagger knows the API uses JWT Bearer token in:

```text
Authorization header
```

### Viva Answer

> Bearer security scheme allows Swagger UI to send JWT token in Authorization header for protected APIs.

---

## 15. AddSecurityRequirement

### Code

```csharp
c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
{
    {
        new OpenApiSecuritySchemeReference("Bearer", doc),
        new List<string>()
    }
});
```

### Purpose

Adds Bearer authentication requirement to Swagger operations.

### Viva Answer

> AddSecurityRequirement tells Swagger that API endpoints can require the Bearer JWT security scheme.

---

## 16. Testing Protected APIs in Swagger

### Steps

```text
1. Call /api/auth/login from Swagger.
2. Copy JWT token from response.
3. Click Authorize button in Swagger UI.
4. Enter: Bearer {token}
5. Test protected endpoints.
```

### Example

```text
Bearer eyJhbGciOiJIUzI1NiIs...
```

### Viva Answer

> To test protected APIs in Swagger, first login to get JWT, then use Authorize button and enter Bearer token.

---

## 17. Public vs Protected Endpoints in Swagger

### Public Endpoints

Examples:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password/request-otp
POST /api/auth/forgot-password/reset
POST /api/subscriptions/webhook/stripe
```

### Protected Endpoints

Examples:

```text
GET /api/auth/me
POST /api/interviews/start
POST /api/assessments/start
POST /api/subscriptions/subscribe
```

### Admin Endpoints

Need Admin role:

```text
GET /api/auth/admin/users
GET /api/interviews/admin/all
GET /api/assessments/admin/all
GET /api/subscriptions/all
```

### Viva Answer

> Swagger shows public and protected APIs. Protected APIs require JWT token, and admin APIs require Admin role claim.

---

## 18. AuthorizeOperationFilter

### File

```text
Backend/*Service/Services/AuthorizeOperationFilter.cs
```

### Purpose

Marks endpoints with `[Authorize]` as requiring Bearer token.

### Logic

```text
If endpoint has AllowAnonymous -> clear security
If endpoint has Authorize -> add Bearer requirement
Add 401 and 403 responses
```

### Viva Answer

> AuthorizeOperationFilter is used to mark authorized endpoints in Swagger and add 401/403 response descriptions.

---

## 19. 401 and 403 in Swagger

### 401 Unauthorized

Means:

```text
No token or invalid token
```

### 403 Forbidden

Means:

```text
Valid token but insufficient permission
```

### In Operation Filter

```csharp
operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
```

### Viva Answer

> Swagger can document 401 and 403 responses so developers understand authentication and authorization failures.

---

## 20. API Gateway Swagger Aggregation

### Simple Explanation

Each service has its own Swagger.

API Gateway can aggregate them into one combined Swagger UI.

### In Your Project

Gateway uses:

```text
MMLib.SwaggerForOcelot
```

### Viva Answer

> Gateway Swagger aggregation allows developers to view multiple service APIs from one gateway Swagger UI.

---

## 21. SwaggerForOcelot Registration

### File

```text
Backend/API_Gateway/Program.cs
```

### Code

```csharp
builder.Services.AddSwaggerForOcelot(builder.Configuration);
```

### UI

```csharp
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});
```

### Viva Answer

> API Gateway registers SwaggerForOcelot and exposes merged Swagger UI using UseSwaggerForOcelotUI.

---

## 22. SwaggerEndPoints in ocelot.json

### File

```text
Backend/API_Gateway/ocelot.json
```

### Config

```json
"SwaggerEndPoints": [
  {
    "Key": "identity",
    "Config": [
      {
        "Name": "Identity API",
        "Version": "v1",
        "Url": "http://localhost:5005/swagger/v1/swagger.json"
      }
    ]
  }
]
```

### Services Listed

```text
Identity API
Interviews API
Assessments API
Subscriptions API
```

### Viva Answer

> SwaggerEndPoints in ocelot.json tells the gateway where to fetch each service's Swagger JSON.

---

## 23. SwaggerKey in Ocelot Routes

### Example

```json
"SwaggerKey": "identity"
```

### Purpose

Links Ocelot routes to the matching Swagger endpoint group.

### In Your Project

Keys:

```text
identity
interviews
assessments
subscriptions
```

### Viva Answer

> SwaggerKey connects Ocelot route configuration with the correct service Swagger document.

---

## 24. Gateway BaseUrl

### Config

```json
"GlobalConfiguration": {
  "BaseUrl": "http://localhost:5190"
}
```

### Meaning

Gateway Swagger knows the gateway base URL.

### Viva Answer

> BaseUrl defines the API Gateway address used for aggregated Swagger routing.

---

## 25. Service Swagger Ports

### From ocelot.json

```text
IdentityService -> 5005
InterviewService -> 5002
AssessmentService -> 5003
SubscriptionService -> 5004
API Gateway -> 5190
```

### Viva Answer

> Each service exposes Swagger on its own port, and the gateway aggregates those Swagger documents.

---

## 26. Swagger and JWT Authentication

### Important Point

Swagger UI does not automatically login.

You must manually:

```text
Login
Copy token
Authorize
Call protected endpoint
```

### Token Header

```text
Authorization: Bearer {token}
```

### Viva Answer

> Swagger can test JWT-protected endpoints by manually adding the Bearer token through the Authorize button.

---

## 27. Swagger and DTOs

### Simple Explanation

Swagger shows request and response schemas from DTO classes.

### Examples

```text
RegisterRequest
LoginRequest
StartInterviewRequest
SubmitAssessmentRequest
ConfirmPaymentRequest
```

### Why Useful

Frontend developer knows what JSON body to send.

### Viva Answer

> Swagger displays DTO schemas so developers know the expected request and response JSON structure.

---

## 28. Swagger and Model Validation

### DTO Attributes

Swagger can reflect validation attributes like:

```text
Required
MinLength
MaxLength
Range
RegularExpression
EmailAddress
```

### Example

```csharp
[Required]
[EmailAddress]
public string Email { get; set; }
```

### Viva Answer

> Swagger shows validation metadata from DTO attributes, helping developers send valid requests.

---

## 29. Swagger for Frontend Developers

### Helps Frontend Know

```text
Endpoint URL
HTTP method
Request body
Query parameters
Route parameters
Response shape
Authentication requirement
```

### Viva Answer

> Swagger helps frontend developers integrate with backend APIs by showing routes, request bodies, parameters, and responses.

---

## 30. Swagger for Backend Testing

### Useful For

```text
Testing login/register
Testing protected APIs
Testing admin endpoints
Testing request DTOs
Checking response status
Debugging route issues
```

### Limitation

Complex flows like Stripe webhooks are usually better tested with Stripe CLI/Postman because they need signed raw body.

### Viva Answer

> Swagger is useful for simple API testing, but signed webhooks may need special tools like Stripe CLI.

---

## 31. Swagger and XML Summary Comments

### Example Controller Comment

```csharp
/// <summary>
/// Sends a password-reset OTP to the user's registered email address.
/// </summary>
```

### Benefit

Swagger displays readable endpoint descriptions.

### Viva Answer

> XML summary comments make Swagger documentation more descriptive and easier to understand.

---

## 32. Why API Documentation Matters

### Reasons

- Helps frontend/backend collaboration
- Makes APIs easier to test
- Reduces integration mistakes
- Helps new developers understand system
- Useful for viva/demo
- Shows API contract clearly

### Viva Answer

> API documentation matters because it defines the contract between frontend and backend and makes development/testing easier.

---

## 33. Complete Flow: Testing Login in Swagger

```text
1. Open IdentityService Swagger UI.
2. Expand POST /api/auth/login.
3. Click Try it out.
4. Enter email and password JSON.
5. Click Execute.
6. Copy token from response.
```

### Viva Explanation

> Login can be tested in Swagger by sending LoginRequest and copying returned JWT token.

---

## 34. Complete Flow: Testing Protected API in Swagger

```text
1. Login and copy JWT token.
2. Click Authorize in Swagger UI.
3. Enter Bearer token.
4. Open protected endpoint like GET /api/auth/me.
5. Click Try it out.
6. Execute request.
7. Swagger sends Authorization header.
8. Backend returns authenticated user data.
```

### Viva Explanation

> Protected APIs can be tested after adding Bearer token in Swagger UI.

---

## 35. Complete Flow: Gateway Swagger Aggregation

```text
1. IdentityService exposes /swagger/v1/swagger.json.
2. InterviewService exposes /swagger/v1/swagger.json.
3. AssessmentService exposes /swagger/v1/swagger.json.
4. SubscriptionService exposes /swagger/v1/swagger.json.
5. ocelot.json lists these in SwaggerEndPoints.
6. API Gateway loads SwaggerForOcelot.
7. Gateway Swagger UI shows multiple service APIs together.
```

### Viva Explanation

> Gateway Swagger aggregation collects Swagger JSON from all services and displays them through one gateway UI.

---

## 36. What Happens If Swagger Is Removed?

### Problems

- Harder to test APIs manually
- Frontend must guess request format
- New developers need to inspect code more
- Demo/viva API explanation becomes harder
- API contract is less visible

### Viva Answer

> Without Swagger, API testing and documentation become harder, especially for frontend integration and demos.

---

## 37. Swagger Limitations

### Limitations

- Does not replace full API documentation
- Does not explain business flows deeply
- Does not automatically handle complex auth flows
- Webhook signature testing is difficult
- Does not guarantee backend behavior is correct
- Needs services running to fetch docs

### Viva Answer

> Swagger documents and tests API contracts, but it does not replace deeper documentation, automated tests, or special webhook testing tools.

---

## 38. Possible Improvements

### Improvements

- Actually register AuthorizeOperationFilter in AddSwaggerGen if not already active
- Add more XML comments
- Add example request/response payloads
- Add response type annotations
- Generate OpenAPI docs in CI
- Publish static API documentation
- Add API versioning
- Add grouped tags by module
- Add ProblemDetails/error response documentation
- Restrict Swagger in production

### Balanced Viva Answer

> Current Swagger setup documents all main services and supports Bearer token testing. Future improvements could include examples, response annotations, API versioning, static docs, and production restrictions.

---

## 39. Important Security Note

### In Production

Swagger may expose API structure publicly.

Recommended:

```text
Enable only in development
Protect Swagger with authentication
Restrict by environment/network
```

### In Your Project

Swagger is currently enabled in services.

### Viva Answer

> Swagger is useful in development, but in production it should be restricted or protected because it exposes API details.

---

## 40. Best Full Viva Answer for Topic 20

> Swagger is used in my project to document and test APIs. Each Web API service registers Swagger using AddSwaggerGen and exposes OpenAPI JSON using UseSwagger and Swagger UI using UseSwaggerUI. Each service defines OpenApiInfo with title, version, and description, includes XML comments when available, and configures a Bearer JWT security scheme so protected APIs can be tested with Authorization header. API Gateway uses MMLib.SwaggerForOcelot. In ocelot.json, SwaggerEndPoints define where each service's swagger.json is available, and SwaggerKey connects routes to those documents. This allows developers to view service APIs individually or through gateway Swagger aggregation. Swagger helps frontend integration, testing, debugging, and viva explanation.

---

## 41. Common Viva Questions and Answers

### Q1. What is Swagger?

Swagger is a tool for documenting and testing APIs through a browser UI.

### Q2. What is OpenAPI?

OpenAPI is the standard format for describing REST APIs.

### Q3. What is Swagger UI?

Swagger UI is the interactive browser page that displays OpenAPI documentation.

### Q4. What is swagger.json?

It is the machine-readable OpenAPI document for an API.

### Q5. Why use Swagger?

For API documentation, testing, frontend integration, and debugging.

### Q6. Where is Swagger configured?

In each service's Program.cs using AddSwaggerGen, UseSwagger, and UseSwaggerUI.

### Q7. What does AddSwaggerGen do?

It registers Swagger/OpenAPI document generation.

### Q8. What does UseSwagger do?

It serves the OpenAPI JSON document.

### Q9. What does UseSwaggerUI do?

It serves the interactive Swagger UI page.

### Q10. How do you test protected APIs in Swagger?

Login, copy JWT token, click Authorize, enter Bearer token, then call protected endpoint.

### Q11. What is Bearer security scheme?

It tells Swagger that JWT token is sent in Authorization header.

### Q12. What is Authorization header format?

Authorization: Bearer {token}.

### Q13. Why include XML comments?

To show controller/action summaries in Swagger documentation.

### Q14. What is SwaggerForOcelot?

It aggregates Swagger docs from downstream services in API Gateway.

### Q15. Where are gateway Swagger endpoints configured?

In API_Gateway/ocelot.json under SwaggerEndPoints.

### Q16. What is SwaggerKey?

SwaggerKey links Ocelot routes to the corresponding service Swagger document.

### Q17. Which services expose Swagger?

IdentityService, InterviewService, AssessmentService, and SubscriptionService.

### Q18. Which service does not need Swagger?

NotificationService because it is a worker service with no HTTP controller.

### Q19. What are 401 and 403?

401 means unauthenticated, 403 means authenticated but forbidden.

### Q20. What are Swagger limitations?

It does not replace full documentation, automated tests, or special tools for signed webhooks.

### Q21. Why restrict Swagger in production?

Because it exposes API structure and should not always be public.

### Q22. How does Swagger help frontend?

It shows routes, methods, request bodies, parameters, and response structures.

### Q23. How does Swagger show DTOs?

It reads request/response model metadata and validation attributes.

### Q24. Can Stripe webhook be fully tested in Swagger?

Not easily, because it requires a valid Stripe signature and raw body.

### Q25. What improvements can be made?

Add examples, response annotations, API versioning, static docs, better operation filters, and production restrictions.

---

## 42. Quick Revision Summary

- Swagger documents and tests APIs.
- OpenAPI is the API specification.
- Swagger UI displays OpenAPI interactively.
- Each service has AddSwaggerGen.
- Each service has UseSwagger.
- Each service has UseSwaggerUI.
- Swagger JSON is /swagger/v1/swagger.json.
- Bearer security scheme supports JWT testing.
- Use Authorize button to add JWT.
- Protected endpoints require Bearer token.
- Admin endpoints require Admin role.
- XML comments improve endpoint descriptions.
- DTO validation attributes appear in docs.
- API Gateway uses SwaggerForOcelot.
- ocelot.json has SwaggerEndPoints.
- SwaggerKey maps routes to docs.
- Gateway aggregates Identity, Interview, Assessment, Subscription APIs.
- NotificationService has no Swagger because it has no controllers.
- Swagger helps frontend integration and viva demos.
- Swagger should be restricted in production.
- Future improvements include examples, response annotations, API versioning, and static docs.

