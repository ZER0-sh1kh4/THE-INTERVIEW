# Topic 5: API Gateway and Ocelot

Project: Mock Interview Platform  
Focus: Understanding why the backend uses an API Gateway, how Ocelot routes requests, what upstream/downstream means, how authentication works at gateway level, and how your frontend benefits from one backend entry point.

---

## 1. What Is an API Gateway?

### Simple Explanation

An API Gateway is the single entry point for frontend requests.

Instead of frontend directly calling every backend service, frontend calls one gateway.

The gateway then forwards the request to the correct service.

### Real-Life Analogy

Imagine a hospital.

A patient does not directly search every department.

They first go to the reception.

Reception sends them to:

- Cardiology
- Orthopedics
- Billing
- Pharmacy
- Lab

In your project:

```text
API Gateway = reception
IdentityService = user/login department
InterviewService = interview department
AssessmentService = test department
SubscriptionService = payment department
```

### In Your Project

Frontend calls the API Gateway.

Gateway forwards requests to:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Viva Answer

> API Gateway is a single entry point for frontend requests. In my project, the Ocelot API Gateway receives requests from Angular and routes them to the correct backend service like IdentityService, InterviewService, AssessmentService, or SubscriptionService.

---

## 2. Why Do We Need an API Gateway?

### Simple Explanation

Your backend has multiple services.

Without gateway, Angular would need to know all service URLs and ports.

Example:

```text
IdentityService      → localhost:5005
InterviewService     → localhost:5002
AssessmentService    → localhost:5003
SubscriptionService  → localhost:5004
```

This becomes messy.

With gateway, frontend can call one common backend entry point.

### Practical Scenario

Without API Gateway:

```text
Login API        → http://localhost:5005/api/auth/login
Interview API    → http://localhost:5002/api/interviews/start
Assessment API   → http://localhost:5003/api/assessments/start
Subscription API → http://localhost:5004/api/subscriptions/subscribe
```

With API Gateway:

```text
/api/auth/login
/api/interviews/start
/api/assessments/start
/api/subscriptions/subscribe
```

Frontend does not worry about internal ports.

### Why It Is Useful

- Single frontend entry point
- Hides internal service locations
- Central place for routing
- Central place for CORS
- Can centralize authentication
- Easier frontend configuration
- Easier future deployment

### What If We Do Not Use API Gateway?

Problems:

- Frontend must call multiple service ports
- More CORS complexity
- More environment configuration
- Internal service URLs are exposed
- Harder to add common policies
- Harder to change service locations later

### Viva Answer

> API Gateway is used because the backend has multiple services. It hides internal service addresses and gives Angular a single entry point for all backend APIs.

---

## 3. What Is Ocelot?

### Simple Explanation

Ocelot is an API Gateway library for .NET.

It is used to build gateway behavior in ASP.NET Core applications.

Ocelot reads route configuration and forwards requests to downstream services.

### In Your Project

Your API Gateway project uses Ocelot.

Important files:

```text
Backend/API_Gateway/Program.cs
Backend/API_Gateway/ocelot.json
```

### What Ocelot Does

Ocelot handles:

- Routing
- Reverse proxy behavior
- Upstream/downstream mapping
- Authentication options
- Request forwarding
- Swagger endpoint grouping
- Timeout/QoS settings

### Viva Answer

> Ocelot is a .NET API Gateway library. My project uses Ocelot to map frontend routes to internal backend service routes.

---

## 4. Reverse Proxy Concept

### Simple Explanation

A reverse proxy receives a client request and forwards it to another server.

The client thinks it is talking to one server, but internally the request is forwarded.

### Practical Example

Frontend calls:

```text
http://localhost:5190/api/auth/login
```

API Gateway forwards to:

```text
http://localhost:5005/api/auth/login
```

Frontend does not directly know about port `5005`.

### In Your Project

Ocelot works as a reverse proxy.

It takes upstream request from frontend and forwards to downstream service.

### Viva Answer

> A reverse proxy forwards client requests to internal backend servers. In my project, Ocelot API Gateway works as a reverse proxy between Angular and backend services.

---

## 5. Upstream and Downstream

### Simple Explanation

These two words are very important for API Gateway viva.

### Upstream

Upstream means the route exposed to the frontend.

This is what Angular calls.

Example:

```text
/api/auth/login
```

### Downstream

Downstream means the actual backend service route where gateway forwards the request.

Example:

```text
http://localhost:5005/api/auth/login
```

### In ocelot.json

Example:

```json
{
  "DownstreamPathTemplate": "/api/auth/login",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 5005
    }
  ],
  "UpstreamPathTemplate": "/api/auth/login",
  "UpstreamHttpMethod": [ "POST", "OPTIONS" ]
}
```

### Practical Meaning

```text
Frontend calls upstream:
/api/auth/login

Gateway forwards to downstream:
localhost:5005/api/auth/login
```

### Viva Answer

> Upstream is the route exposed by the gateway to frontend, while downstream is the actual internal service route where the request is forwarded.

---

## 6. API Gateway Project Structure

### In Your Project

API Gateway folder:

```text
Backend/API_Gateway
```

Important files:

```text
Program.cs
ocelot.json
API_Gateway.csproj
appsettings.Example.json
```

### Program.cs Role

`Program.cs` configures:

- Ocelot
- JWT authentication
- CORS
- Swagger for Ocelot
- Polly support
- Gateway middleware

### ocelot.json Role

`ocelot.json` defines:

- Which frontend routes exist
- Which service each route maps to
- HTTP methods allowed
- Authentication requirements
- Swagger endpoint keys
- Timeout settings

### Viva Answer

> In my project, Program.cs configures Ocelot, authentication, CORS, Swagger, and middleware. ocelot.json contains route mapping between frontend routes and backend service routes.

---

## 7. How Gateway Loads ocelot.json

### In Program.cs

API Gateway loads Ocelot config:

```csharp
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
```

### Simple Explanation

This tells ASP.NET Core:

```text
Read routes from ocelot.json.
If file is missing, application should fail.
Reload if file changes.
```

### Why reloadOnChange Is Useful

If route config changes, Ocelot can reload the route configuration.

### Viva Answer

> The gateway loads route configuration from ocelot.json using AddJsonFile. This file defines how upstream frontend paths map to downstream services.

---

## 8. Gateway Startup Flow

### In Program.cs

Important setup:

```csharp
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddAuthentication("Bearer").AddJwtBearer(...);
builder.Services.AddCors(...);
builder.Services.AddOcelot().AddPolly();
```

Then:

```csharp
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerForOcelotUI(...);
app.UseOcelot().Wait();
app.Run();
```

### Simple Flow

```text
1. Create builder
2. Load ocelot.json
3. Read JWT settings
4. Register Swagger
5. Register JWT authentication
6. Register CORS
7. Register Ocelot + Polly
8. Build app
9. Use CORS
10. Use authentication and authorization
11. Use Swagger UI
12. Start Ocelot routing
13. Run app
```

### Viva Answer

> API Gateway startup loads Ocelot routes, configures JWT authentication, CORS, Swagger aggregation, and then starts Ocelot middleware to forward requests.

---

## 9. IdentityService Routes

### Public Auth Routes

Register:

```text
Upstream:   /api/auth/register
Downstream: localhost:5005/api/auth/register
Method:     POST
```

Login:

```text
Upstream:   /api/auth/login
Downstream: localhost:5005/api/auth/login
Method:     POST
```

Forgot password OTP:

```text
Upstream:   /api/auth/forgot-password/request-otp
Downstream: localhost:5005/api/auth/forgot-password/request-otp
Method:     POST
```

Reset password:

```text
Upstream:   /api/auth/forgot-password/reset
Downstream: localhost:5005/api/auth/forgot-password/reset
Method:     POST
```

### Why These Are Public

User is not logged in yet during:

- Register
- Login
- Forgot password
- Reset password

So these routes do not require JWT at gateway level.

### Protected Identity Routes

```text
/api/auth/{everything}
```

This catches other auth APIs like:

```text
GET /api/auth/me
PUT /api/auth/me
POST /api/auth/refresh-claims
GET /api/auth/admin/users
```

These require Bearer authentication.

### Viva Answer

> Gateway exposes public Identity routes for register, login, and forgot password. Other auth routes are protected using Bearer JWT authentication.

---

## 10. InterviewService Routes

### Route Mapping

Base interviews route:

```text
Upstream:   /api/interviews
Downstream: localhost:5002/api/interviews
Methods:    GET, POST
```

Wildcard interviews route:

```text
Upstream:   /api/interviews/{everything}
Downstream: localhost:5002/api/interviews/{everything}
Methods:    GET, POST, PUT, DELETE
```

### Examples

```text
POST /api/interviews/start
```

forwards to:

```text
localhost:5002/api/interviews/start
```

```text
GET /api/interviews/5/result
```

forwards to:

```text
localhost:5002/api/interviews/5/result
```

### Authentication

Interview routes require Bearer JWT.

Why?

Because interviews belong to a logged-in user.

### Timeout

Interview route has:

```json
"QoSOptions": {
  "TimeoutValue": 120000
}
```

This gives longer timeout because AI generation/evaluation may take more time.

### Viva Answer

> InterviewService routes are protected and forwarded by the gateway to port 5002. Timeout is increased because interview question generation and AI evaluation may take longer than normal requests.

---

## 11. AssessmentService Routes

### Route Mapping

Base assessment route:

```text
Upstream:   /api/assessments
Downstream: localhost:5003/api/assessments
Methods:    GET, POST
```

Wildcard assessment route:

```text
Upstream:   /api/assessments/{everything}
Downstream: localhost:5003/api/assessments/{everything}
Methods:    GET, POST, PUT, DELETE
```

### Examples

```text
POST /api/assessments/start
```

forwards to:

```text
localhost:5003/api/assessments/start
```

```text
GET /api/assessments/10/result
```

forwards to:

```text
localhost:5003/api/assessments/10/result
```

```text
DELETE /api/assessments/questions/5
```

forwards to:

```text
localhost:5003/api/assessments/questions/5
```

### Authentication

Assessment routes require Bearer JWT.

Admin question-bank APIs are further protected inside AssessmentService using role authorization.

### Viva Answer

> AssessmentService routes are forwarded to port 5003 and protected by JWT. Admin routes like question management are also role-protected in the service itself.

---

## 12. SubscriptionService Routes

### Protected Subscription Routes

Most subscription routes go through:

```text
Upstream:   /api/subscriptions/{everything}
Downstream: localhost:5004/api/subscriptions/{everything}
```

Examples:

```text
POST /api/subscriptions/subscribe
POST /api/subscriptions/confirm
POST /api/subscriptions/cancel
GET /api/subscriptions/my
GET /api/subscriptions/my/payments
```

These require JWT because they belong to current logged-in user.

### Public Stripe Webhook Route

```text
Upstream:   /api/subscriptions/webhook/stripe
Downstream: localhost:5004/api/subscriptions/webhook/stripe
Method:     POST
```

This route does not require JWT.

### Why Webhook Is Public

Stripe server calls this endpoint.

Stripe cannot send your app's JWT token.

Security is handled using:

```text
Stripe-Signature header
```

inside SubscriptionService.

### Route Priority

Webhook route has:

```json
"Priority": 1
```

This helps Ocelot match the specific webhook route before the general subscription wildcard route.

### Viva Answer

> Subscription APIs are protected except the Stripe webhook. The webhook is public because it is called by Stripe, but it is secured by Stripe signature verification. Its gateway route has higher priority so it is matched before the general subscription route.

---

## 13. Wildcard Route: {everything}

### Simple Explanation

`{everything}` means match any remaining path after the base route.

### Example

Route:

```text
/api/interviews/{everything}
```

Can match:

```text
/api/interviews/start
/api/interviews/5/begin
/api/interviews/5/result
/api/interviews/admin/all
```

### Why It Is Used

Without wildcard, you would need to write a separate gateway route for every endpoint.

That would make `ocelot.json` large and repetitive.

### Risk

Wildcard routes can accidentally match routes that should have special behavior.

That is why specific routes like Stripe webhook and login/register have priority.

### Viva Answer

> The `{everything}` wildcard route allows Ocelot to forward many nested endpoints using one route rule. Specific routes are given priority when they need special behavior.

---

## 14. Route Priority

### Simple Explanation

Route priority tells Ocelot which route should match first when multiple routes could match.

### Practical Example

This route is specific:

```text
/api/subscriptions/webhook/stripe
```

This route is general:

```text
/api/subscriptions/{everything}
```

The webhook route should match first because it is public.

If general route matched first, it might require JWT and block Stripe.

### In Your Project

Public routes like:

```text
/api/auth/register
/api/auth/login
/api/subscriptions/webhook/stripe
```

have priority to avoid being swallowed by wildcard protected routes.

### Viva Answer

> Priority is used so specific routes like login, register, and Stripe webhook are matched before general wildcard routes.

---

## 15. Gateway-Level JWT Authentication

### Simple Explanation

Gateway can check JWT before forwarding request to service.

If token is missing or invalid, request can be blocked at gateway.

### In Program.cs

Gateway configures Bearer authentication:

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
```

### In ocelot.json

Protected routes include:

```json
"AuthenticationOptions": {
  "AuthenticationProviderKey": "Bearer",
  "AllowedScopes": []
}
```

### What Gateway Validates

The gateway checks:

- Issuer
- Audience
- Token expiry
- Signature key
- Token lifetime

### Why Use Gateway-Level Auth?

It can block unauthorized requests before they reach internal services.

This reduces unnecessary traffic to services.

### Why Services Still Also Use JWT?

Your services also configure JWT authentication.

This is defense-in-depth.

Even if someone directly calls a service port, the service can still validate token.

### Viva Answer

> Gateway-level JWT authentication blocks unauthorized requests before forwarding them to services. My services also validate JWT, which provides defense-in-depth.

---

## 16. CORS in API Gateway

### Simple Explanation

CORS allows Angular frontend running on a different origin to call backend APIs.

### In Program.cs

Gateway configures:

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

Then:

```csharp
app.UseCors("CorsPolicy");
```

### Why Gateway Handles CORS

Frontend mostly talks to gateway.

So gateway is the best place to allow frontend requests.

### Development vs Production

Development:

```text
AllowAnyOrigin
```

is convenient.

Production should restrict to your actual frontend domain.

### Viva Answer

> CORS is configured in the API Gateway so Angular can call backend APIs from a different origin. In production, allowed origins should be restricted.

---

## 17. Swagger For Ocelot

### Simple Explanation

Swagger documents APIs.

But in microservices, each service has its own Swagger.

SwaggerForOcelot helps gateway show combined API documentation.

### In Program.cs

```csharp
builder.Services.AddSwaggerForOcelot(builder.Configuration);
```

Then:

```csharp
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});
```

### In ocelot.json

Swagger endpoints:

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

Similar entries exist for:

```text
interviews
assessments
subscriptions
```

### Why It Is Useful

Instead of opening Swagger separately for every service, gateway can show merged documentation.

### Viva Answer

> SwaggerForOcelot aggregates Swagger documentation from multiple backend services and shows them through the API Gateway.

---

## 18. Ocelot + Polly

### Simple Explanation

Polly is a .NET resilience library.

It helps with:

- Retry
- Timeout
- Circuit breaker
- Fallback

In your gateway:

```csharp
builder.Services.AddOcelot().AddPolly();
```

### In This Project

Your `ocelot.json` uses timeout options for interview and assessment routes:

```json
"QoSOptions": {
  "TimeoutValue": 120000
}
```

This means gateway can wait longer for these routes.

### Why Longer Timeout Is Needed

Interview and assessment services may call Gemini API.

AI generation/evaluation can take longer than normal database operations.

### Viva Answer

> Ocelot is integrated with Polly for resilience support. In my project, longer timeout settings are used for AI-related interview and assessment routes.

---

## 19. QoS Timeout

### Simple Explanation

QoS means Quality of Service.

Timeout means maximum time gateway waits for downstream response.

### In Your Project

Interview and assessment routes have:

```json
"TimeoutValue": 120000
```

This is 120000 milliseconds:

```text
120 seconds
```

### Why Needed

Some operations take longer:

- Gemini question generation
- Gemini interview answer evaluation
- Fetching multiple AI questions

Without longer timeout, gateway might stop waiting too early.

### What If Timeout Is Too Short?

Frontend may receive timeout error even though service is still working.

### What If Timeout Is Too Long?

Bad requests may hold resources for too long.

### Viva Answer

> Timeout controls how long the gateway waits for a service response. Interview and assessment routes have longer timeout because AI operations may take more time.

---

## 20. GlobalConfiguration BaseUrl

### In ocelot.json

```json
"GlobalConfiguration": {
  "BaseUrl": "http://localhost:5190"
}
```

### Simple Explanation

BaseUrl represents the gateway's own public address.

It is used by Ocelot and Swagger-related tools to understand the gateway URL.

### Viva Answer

> GlobalConfiguration BaseUrl defines the public base address of the API Gateway.

---

## 21. Complete Flow: Login Through Gateway

### Request

```text
POST /api/auth/login
```

### Flow

```text
1. Angular sends login request to API Gateway.
2. Ocelot checks ocelot.json routes.
3. It finds /api/auth/login route.
4. This route does not require JWT.
5. Gateway forwards request to localhost:5005/api/auth/login.
6. IdentityService AuthController handles login.
7. AuthService verifies password.
8. JWT token is returned.
9. Gateway sends response back to Angular.
```

### Viva Explanation

> Login request goes through the gateway, but it is public because the user does not have a token yet. Ocelot forwards it to IdentityService, which verifies credentials and returns JWT.

---

## 22. Complete Flow: Start Interview Through Gateway

### Request

```text
POST /api/interviews/start
Authorization: Bearer jwt_token
```

### Flow

```text
1. Angular sends request to gateway with JWT.
2. Ocelot matches /api/interviews/{everything}.
3. Gateway validates Bearer token.
4. If token is invalid, request is rejected.
5. If token is valid, gateway forwards to InterviewService on port 5002.
6. InterviewService also validates JWT.
7. InterviewController reads userId and isPremium claims.
8. InterviewSvc creates pending interview.
9. Response returns through gateway to frontend.
```

### Viva Explanation

> Protected interview APIs pass through gateway-level JWT validation before reaching InterviewService. The service also validates JWT and uses claims to identify user and premium status.

---

## 23. Complete Flow: Stripe Webhook Through Gateway

### Request

```text
POST /api/subscriptions/webhook/stripe
Stripe-Signature: signature_here
```

### Flow

```text
1. Stripe sends webhook request to gateway.
2. Ocelot matches the specific webhook route.
3. Route does not require JWT.
4. Gateway forwards request to SubscriptionService on port 5004.
5. SubscriptionService reads raw body and Stripe-Signature header.
6. SubscriptionService verifies signature using Stripe webhook secret.
7. If valid, payment state is updated.
8. Premium activation events are published.
```

### Why It Must Not Require JWT

Stripe is not a logged-in user of your application.

### Viva Explanation

> Stripe webhook route is public at gateway level because Stripe cannot send our JWT. It is still secure because SubscriptionService validates the Stripe-Signature header.

---

## 24. Common Gateway Route Groups in Your Project

| Frontend Route | Internal Service | Port | Auth |
|---|---|---|---|
| /api/auth/register | IdentityService | 5005 | Public |
| /api/auth/login | IdentityService | 5005 | Public |
| /api/auth/{everything} | IdentityService | 5005 | JWT |
| /api/interviews | InterviewService | 5002 | JWT |
| /api/interviews/{everything} | InterviewService | 5002 | JWT |
| /api/assessments | AssessmentService | 5003 | JWT |
| /api/assessments/{everything} | AssessmentService | 5003 | JWT |
| /api/subscriptions/webhook/stripe | SubscriptionService | 5004 | Public + Stripe signature |
| /api/subscriptions/{everything} | SubscriptionService | 5004 | JWT |

---

## 25. What Happens If API Gateway Is Down?

### Simple Explanation

If frontend depends on gateway and gateway is down, frontend cannot reach backend through normal route.

Internal services may still be running, but frontend entry point is unavailable.

### Practical Impact

Users may not be able to:

- Login
- Start interview
- Start assessment
- Subscribe
- Fetch result

### Production Solution

In production, gateway should be made reliable using:

- Multiple gateway instances
- Load balancer
- Health checks
- Monitoring
- Container orchestration

### Viva Answer

> If API Gateway is down, frontend cannot access backend services through the normal entry point. In production, gateway should be deployed with high availability.

---

## 26. What Happens If a Downstream Service Is Down?

### Example

Suppose AssessmentService is down.

Then:

```text
/api/assessments/start
```

will fail, but other services may still work:

```text
/api/auth/login
/api/interviews/start
/api/subscriptions/subscribe
```

### Why This Is Useful

Failure is isolated.

Assessment issue does not necessarily stop login or subscriptions.

### Viva Answer

> If one downstream service is down, only that service's routes fail. Other services can continue working, which is one benefit of service separation.

---

## 27. Security Benefits of API Gateway

### Benefits

- Central entry point
- Can validate JWT before services
- Hides internal service ports
- Controls CORS
- Can add rate limiting in future
- Can add request logging in future
- Can add IP filtering in future

### Important Note

Gateway should not be the only security layer.

Services should still validate JWT and authorization.

Your project does this.

### Viva Answer

> API Gateway improves security by centralizing entry, validating tokens, controlling CORS, and hiding internal services. My services also validate tokens for defense-in-depth.

---

## 28. Alternatives to Ocelot

### Alternatives

| Tool | Description |
|---|---|
| YARP | Microsoft's reverse proxy library for .NET |
| NGINX | Popular web server/reverse proxy |
| Kong | API gateway platform |
| Azure API Management | Managed cloud API gateway |
| AWS API Gateway | AWS managed gateway service |
| Traefik | Cloud-native reverse proxy |

### Why Ocelot Is Suitable Here

Ocelot is suitable because:

- It works with ASP.NET Core
- Configuration is simple JSON
- Good for .NET microservices learning
- Supports routing and authentication
- Easy to integrate with SwaggerForOcelot

### Viva Answer

> Alternatives include YARP, NGINX, Kong, Azure API Management, and AWS API Gateway. Ocelot is used because it integrates easily with ASP.NET Core and supports JSON-based routing.

---

## 29. Possible Improvements in Gateway

### Improvements

- Restrict CORS to actual frontend domain
- Add rate limiting
- Add centralized request logging
- Add health checks
- Add load balancing if multiple service instances exist
- Add circuit breaker policies
- Add correlation ID for tracing requests
- Use HTTPS in downstream URLs for production
- Move secrets to secure secret manager

### Balanced Viva Answer

> Current gateway handles routing, JWT authentication, CORS, Swagger, and timeouts. Future improvements could include rate limiting, load balancing, centralized logging, health checks, and stricter production CORS.

---

## 30. What If We Remove Ocelot?

### Result

Frontend would directly call services:

```text
localhost:5005
localhost:5002
localhost:5003
localhost:5004
```

### Problems

- Multiple backend URLs in frontend
- More CORS setup
- Harder deployment
- No single API entry point
- Harder to apply common policies
- Service ports exposed directly

### Possible Without Ocelot

For a very small project, direct calls can work.

But for microservices-style backend, gateway is cleaner.

### Viva Answer

> If Ocelot is removed, the frontend must call every service directly. This increases configuration, CORS complexity, and reduces centralized control.

---

## 31. Best Full Viva Answer for Topic 5

> My project uses Ocelot API Gateway as the single entry point for Angular frontend requests. The gateway reads routes from ocelot.json and maps upstream frontend paths to downstream backend services. For example, auth routes go to IdentityService on port 5005, interview routes go to InterviewService on port 5002, assessment routes go to AssessmentService on port 5003, and subscription routes go to SubscriptionService on port 5004. Public routes like login and register do not require JWT, while protected routes use Bearer authentication at the gateway. Services also validate JWT for defense-in-depth. Ocelot also handles CORS, Swagger aggregation, and timeout settings for AI-heavy routes.

---

## 32. Common Viva Questions and Answers

### Q1. What is API Gateway?

API Gateway is a single entry point that receives frontend requests and forwards them to the correct backend service.

### Q2. Why did you use API Gateway?

Because my backend has multiple services. Gateway hides internal service URLs and gives frontend one common entry point.

### Q3. What is Ocelot?

Ocelot is a .NET API Gateway library used for routing requests to downstream services.

### Q4. What is upstream path?

Upstream path is the route exposed by the gateway and called by frontend.

### Q5. What is downstream path?

Downstream path is the internal route of the backend service where gateway forwards the request.

### Q6. What is reverse proxy?

A reverse proxy receives client requests and forwards them to internal servers. Ocelot works as a reverse proxy.

### Q7. Why are login and register public in gateway?

Because users do not have JWT token before registration or login.

### Q8. Why is Stripe webhook public?

Because Stripe server calls it and cannot send our JWT. It is secured using Stripe signature verification.

### Q9. Why use route priority?

Priority ensures specific routes like webhook, login, and register are matched before general wildcard routes.

### Q10. What does `{everything}` mean?

It is a wildcard route parameter that matches any remaining path after the base route.

### Q11. Why configure JWT in gateway?

To block unauthorized requests before they reach backend services.

### Q12. Why do services also validate JWT?

For defense-in-depth. If someone directly calls service port, the service still validates token.

### Q13. What is SwaggerForOcelot?

It aggregates Swagger documentation from multiple services and displays it through the gateway.

### Q14. Why are timeout values increased for interviews and assessments?

Because those services may call Gemini AI, which can take longer than normal database operations.

### Q15. What are alternatives to Ocelot?

YARP, NGINX, Kong, Azure API Management, AWS API Gateway, and Traefik.

---

## 33. Quick Revision Summary

- API Gateway is the single entry point.
- Ocelot is the .NET gateway library used.
- Gateway works as a reverse proxy.
- Upstream means frontend-facing route.
- Downstream means internal service route.
- ocelot.json stores route mappings.
- Program.cs configures Ocelot, JWT, CORS, Swagger, and Polly.
- IdentityService runs on port 5005.
- InterviewService runs on port 5002.
- AssessmentService runs on port 5003.
- SubscriptionService runs on port 5004.
- Login/register are public routes.
- Protected routes require Bearer JWT.
- Stripe webhook is public but signature-protected.
- `{everything}` handles wildcard nested routes.
- Priority ensures specific routes match first.
- Gateway validates JWT, but services also validate JWT.
- SwaggerForOcelot aggregates API documentation.
- Timeout is increased for AI-heavy routes.

