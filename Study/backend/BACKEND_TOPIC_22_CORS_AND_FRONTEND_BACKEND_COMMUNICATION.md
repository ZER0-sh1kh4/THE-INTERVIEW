# Topic 22: CORS and Frontend-Backend Communication

Project: Mock Interview Platform  
Focus: Understanding how Angular talks to ASP.NET Core backend APIs, CORS, origins, preflight requests, API Gateway communication, Ocelot routing, Bearer tokens, headers, JSON request/response flow, and production CORS security.

---

## 1. What Is Frontend-Backend Communication?

### Simple Explanation

Frontend-backend communication means the browser application sends HTTP requests to backend APIs and receives JSON responses.

In your project:

```text
Angular frontend -> API Gateway -> Microservice -> Database / RabbitMQ / External API
```

### In Your Project

The Angular frontend talks mainly to the API Gateway:

```text
http://localhost:5190/api
```

The gateway forwards requests to services:

```text
/api/auth          -> IdentityService
/api/interviews    -> InterviewService
/api/assessments   -> AssessmentService
/api/subscriptions -> SubscriptionService
```

### Viva Answer

> Frontend-backend communication is the process where Angular sends HTTP requests to backend APIs, usually through the API Gateway, and receives JSON responses for login, assessments, interviews, subscriptions, and admin operations.

---

## 2. What Is CORS?

### Simple Explanation

CORS means Cross-Origin Resource Sharing.

It is a browser security mechanism that controls whether a web page from one origin can call an API from another origin.

### Example

Angular may run on:

```text
http://localhost:4200
```

Backend API Gateway may run on:

```text
http://localhost:5190
```

These are different origins because the ports are different.

### Viva Answer

> CORS is a browser security rule that decides whether JavaScript running on one origin is allowed to call APIs on another origin.

---

## 3. What Is an Origin?

### Simple Explanation

An origin is made of:

```text
scheme + host + port
```

### Examples

| URL | Origin Parts |
|---|---|
| `http://localhost:4200` | scheme `http`, host `localhost`, port `4200` |
| `http://localhost:5190` | scheme `http`, host `localhost`, port `5190` |
| `https://example.com` | scheme `https`, host `example.com`, default port `443` |

### Important Point

These are different origins:

```text
http://localhost:4200
http://localhost:5190
```

Even though both use `localhost`, the ports are different.

### Viva Answer

> An origin is the combination of protocol, domain, and port. If any one of these changes, the browser treats it as a different origin.

---

## 4. Why CORS Is Needed in Your Project

### Simple Explanation

Your frontend and backend run on different ports during development.

Because of that, the browser requires backend permission before allowing Angular to read API responses.

### Without CORS

The backend may be running correctly, but the browser blocks the request.

Possible browser error:

```text
Access to XMLHttpRequest has been blocked by CORS policy
```

### In Your Project

CORS is needed because:

```text
Angular frontend: http://localhost:4200
API Gateway:      http://localhost:5190
```

### Viva Answer

> CORS is needed because the Angular frontend and ASP.NET Core backend run on different origins during development, so the backend must allow the frontend origin.

---

## 5. CORS Is Enforced by Browser, Not Server

### Simple Explanation

CORS is mainly a browser rule.

If you call the same API from Postman, Swagger, or server-side code, CORS usually does not block it.

### Important Difference

| Client | CORS Applies? |
|---|---|
| Browser JavaScript | Yes |
| Angular app in browser | Yes |
| Postman | No |
| Swagger UI on same backend origin | Usually no cross-origin issue |
| Backend-to-backend call | No browser CORS |

### Viva Answer

> CORS is enforced by browsers. Tools like Postman can call the API even when a browser-based Angular app is blocked by CORS.

---

## 6. CORS Configuration in API Gateway

### In Your Project

The API Gateway configures a CORS policy:

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

Then the middleware is added:

```csharp
app.UseCors("CorsPolicy");
```

### Meaning

| Method | Meaning |
|---|---|
| `AllowAnyOrigin()` | Allows any frontend origin |
| `AllowAnyMethod()` | Allows methods like GET, POST, PUT, DELETE, OPTIONS |
| `AllowAnyHeader()` | Allows headers like Authorization and Content-Type |

### Viva Answer

> The API Gateway enables CORS using a policy that allows any origin, method, and header. This is useful for development so Angular can call the gateway.

---

## 7. Development CORS in Individual Services

### In Your Project

Services also allow CORS in development:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
```

This pattern exists in services such as:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Why This Helps

It allows direct service testing from a browser during development.

Example:

```text
Angular -> InterviewService directly
```

But the normal frontend route should be:

```text
Angular -> API Gateway -> InterviewService
```

### Viva Answer

> Individual services allow open CORS only in development, mainly for direct testing. In normal flow, the Angular app should call the API Gateway.

---

## 8. Why AllowAnyOrigin Is Not Good for Production

### Simple Explanation

`AllowAnyOrigin()` means any website can make browser requests to your API.

This is convenient locally, but too open for production.

### Production Risk

If the backend accepts requests from any origin:

- Unknown websites can call your APIs from a browser
- Attack surface increases
- Token-based requests may be abused if tokens are exposed
- Security policy becomes too loose

### Better Production Policy

```csharp
policy.WithOrigins("https://mockinterview.example.com")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

### Viva Answer

> `AllowAnyOrigin` is acceptable for local development, but production should restrict CORS to the actual frontend domain.

---

## 9. Preflight Request

### Simple Explanation

A preflight request is an automatic `OPTIONS` request sent by the browser before some actual API calls.

The browser asks:

```text
Is this origin allowed?
Is this method allowed?
Are these headers allowed?
```

### When Preflight Happens

Preflight commonly happens when requests use:

- `Authorization` header
- `Content-Type: application/json`
- Methods like `PUT` or `DELETE`
- Custom headers

### In Your Project

Angular sends JWT in:

```text
Authorization: Bearer <token>
```

Because of this, protected API calls may trigger preflight.

### Viva Answer

> A preflight request is an `OPTIONS` request sent by the browser to check CORS permission before sending the actual request.

---

## 10. Why Ocelot Routes Include OPTIONS

### In Your Project

Ocelot routes include `OPTIONS` in `UpstreamHttpMethod`.

Example:

```json
{
  "UpstreamPathTemplate": "/api/interviews/{everything}",
  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "OPTIONS" ]
}
```

### Why It Matters

If `OPTIONS` is missing, browser preflight requests may fail before the real request reaches the service.

### Viva Answer

> Ocelot routes include `OPTIONS` so browser CORS preflight requests can pass through the API Gateway correctly.

---

## 11. Angular API Base URL

### In Your Project

The frontend environment file contains:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5190/api'
};
```

### Meaning

Angular services build API URLs from this base.

Example:

```typescript
private apiUrl = `${environment.apiUrl}/auth`;
```

This becomes:

```text
http://localhost:5190/api/auth
```

### Viva Answer

> Angular stores the backend base URL in the environment file, and services use it to call API Gateway endpoints.

---

## 12. Angular HttpClient

### Simple Explanation

`HttpClient` is Angular's service for making HTTP requests.

### In Your Project

Frontend services use `HttpClient`.

Examples:

```typescript
this.http.post(`${this.apiUrl}/login`, credentials)
this.http.get(`${this.apiUrl}/me`)
this.http.post(`${this.apiUrl}/start`, request)
```

### Common HTTP Methods

| Method | Used For |
|---|---|
| `GET` | Fetch data |
| `POST` | Create or perform action |
| `PUT` | Update data |
| `DELETE` | Delete data |

### Viva Answer

> Angular uses `HttpClient` to send HTTP requests to the API Gateway and receive typed JSON responses.

---

## 13. JSON Request and Response

### Simple Explanation

The frontend sends JSON data to the backend.

The backend returns JSON data to the frontend.

### Login Request

```json
{
  "email": "user@example.com",
  "password": "Pass@123"
}
```

### Login Response Shape

Your backend usually wraps responses using `ApiResponse<T>`.

Example concept:

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "token": "jwt-token"
  }
}
```

### Viva Answer

> The frontend and backend communicate using JSON request bodies and JSON responses, usually wrapped in a common API response structure.

---

## 14. Authentication Header

### Simple Explanation

After login, the frontend stores the JWT token.

For protected requests, it sends the token in the `Authorization` header.

### Header Format

```http
Authorization: Bearer <token>
```

### In Your Project

The Angular auth interceptor adds:

```typescript
Authorization: `Bearer ${token}`
```

### Viva Answer

> The frontend sends JWT tokens using the `Authorization: Bearer <token>` header so the API Gateway and backend services can authenticate the user.

---

## 15. Angular Auth Interceptor

### Simple Explanation

An interceptor runs before HTTP requests are sent.

It can modify requests globally.

### In Your Project

The auth interceptor:

- Reads token from `AuthService`
- Adds `Authorization` header
- Handles `401 Unauthorized`
- Logs out user on unauthorized feature requests

### Example Concept

```typescript
if (token) {
  req = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}
```

### Viva Answer

> The Angular auth interceptor automatically attaches the JWT token to outgoing API requests so individual services do not need to manually add the Authorization header.

---

## 16. Token Storage in Frontend

### In Your Project

After login/register:

```typescript
localStorage.setItem('token', token);
```

Then the interceptor reads:

```typescript
localStorage.getItem('token');
```

### Important Security Point

`localStorage` is easy to use but can be exposed by XSS attacks.

For production systems, teams may consider:

- Strong XSS protection
- Secure coding and sanitization
- Short token expiry
- Refresh token strategy
- HttpOnly secure cookies, depending on architecture

### Viva Answer

> This project stores the JWT in localStorage and uses an interceptor to attach it to requests. In production, token storage must be protected carefully against XSS.

---

## 17. API Gateway Communication Flow

### Flow

```text
Angular component
Angular service
HttpClient
Auth interceptor adds Bearer token
Browser checks CORS
API Gateway receives request
Ocelot matches route
Gateway validates JWT if route is protected
Gateway forwards request to microservice
Microservice controller handles request
Service layer performs business logic
Backend returns JSON response
Angular updates UI
```

### Viva Answer

> The Angular app sends requests to the API Gateway, the gateway handles routing and authentication, forwards the request to the correct microservice, and returns the JSON response back to Angular.

---

## 18. Ocelot Upstream and Downstream Routes

### Simple Explanation

Ocelot uses upstream and downstream route configuration.

### Upstream

The URL received by the API Gateway.

Example:

```text
/api/auth/login
```

### Downstream

The URL of the actual microservice endpoint.

Example:

```text
http://localhost:5005/api/auth/login
```

### In Your Project

Example route:

```json
{
  "DownstreamPathTemplate": "/api/auth/login",
  "DownstreamHostAndPorts": [ { "Host": "localhost", "Port": 5005 } ],
  "UpstreamPathTemplate": "/api/auth/login",
  "UpstreamHttpMethod": [ "POST", "OPTIONS" ]
}
```

### Viva Answer

> In Ocelot, upstream means the route exposed by the gateway, and downstream means the actual route and host of the microservice.

---

## 19. Public and Protected Routes

### Public Routes

Routes like login and register are public:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password/request-otp
POST /api/auth/forgot-password/reset
POST /api/subscriptions/webhook/stripe
```

These do not need a user JWT.

### Protected Routes

Routes like these require JWT:

```text
GET /api/auth/me
POST /api/interviews/start
POST /api/assessments/start
POST /api/subscriptions/subscribe
GET /api/subscriptions/my
```

### In Ocelot

Protected gateway routes include:

```json
"AuthenticationOptions": {
  "AuthenticationProviderKey": "Bearer",
  "AllowedScopes": []
}
```

### Viva Answer

> Public routes such as login and register do not require JWT, while protected routes use Ocelot authentication options and require a valid Bearer token.

---

## 20. CORS and Authentication Are Different

### Simple Explanation

CORS and authentication solve different problems.

### Difference

| Concept | Purpose |
|---|---|
| CORS | Allows browser frontend to call backend across origins |
| Authentication | Verifies who the user is |
| Authorization | Verifies what the user can access |

### Important Point

A request can pass CORS but still fail authentication.

Example:

```text
CORS allowed
JWT missing
Result: 401 Unauthorized
```

### Viva Answer

> CORS only controls browser cross-origin access. It does not authenticate users. JWT authentication is still required for protected APIs.

---

## 21. Common Frontend API Calls

### Authentication

```text
POST /api/auth/register
POST /api/auth/login
GET /api/auth/me
POST /api/auth/refresh-claims
```

### Assessment

```text
POST /api/assessments/start
POST /api/assessments/submit
GET /api/assessments
GET /api/assessments/{id}/result
```

### Interview

```text
POST /api/interviews/start
POST /api/interviews/{id}/begin
POST /api/interviews/submit
GET /api/interviews
GET /api/interviews/{id}/result
```

### Subscription

```text
POST /api/subscriptions/subscribe
POST /api/subscriptions/confirm
POST /api/subscriptions/cancel
GET /api/subscriptions/my
GET /api/subscriptions/my/payments
```

### Viva Answer

> The frontend communicates with backend features through REST endpoints exposed by the API Gateway, grouped by auth, assessment, interview, and subscription routes.

---

## 22. Login Communication Flow

### Flow

```text
User enters email and password
Angular AuthService sends POST /api/auth/login
API Gateway forwards to IdentityService
IdentityService validates credentials
IdentityService returns JWT
Angular stores JWT in localStorage
Angular calls GET /api/auth/me
Dashboard loads user details
```

### Viva Answer

> In login flow, Angular sends credentials to the gateway, IdentityService returns a JWT, and the frontend stores the token for later protected API calls.

---

## 23. Protected Request Flow

### Flow

```text
User starts interview
Angular calls POST /api/interviews/start
Auth interceptor adds Authorization Bearer token
Browser may send OPTIONS preflight
Gateway validates JWT
Ocelot forwards request to InterviewService
InterviewService reads user claims
Service creates interview record
JSON response returns to Angular
```

### Viva Answer

> For protected requests, Angular sends the JWT in the Authorization header, the gateway validates it, and the downstream service uses token claims to identify the user.

---

## 24. Stripe Redirect Communication

### Simple Explanation

Some communication does not stay entirely inside normal JSON calls.

Stripe checkout involves redirecting the user to Stripe.

### Flow

```text
Angular calls POST /api/subscriptions/subscribe
SubscriptionService creates Stripe Checkout session
Backend returns checkout URL
Angular redirects browser to Stripe
Stripe redirects user to success or cancel URL
Stripe sends webhook to backend
Frontend calls refresh-claims after premium activation
```

### CORS Point

Stripe webhook is server-to-server, so browser CORS is not the main issue there.

### Viva Answer

> Stripe checkout starts with a frontend API call, then the browser redirects to Stripe, and final payment confirmation is handled by a server-to-server webhook.

---

## 25. Request Headers

### Common Headers

```http
Content-Type: application/json
Authorization: Bearer <token>
```

### Content-Type

Tells the backend the request body is JSON.

### Authorization

Carries the JWT token for protected endpoints.

### CORS Headers

The backend may return headers like:

```http
Access-Control-Allow-Origin
Access-Control-Allow-Methods
Access-Control-Allow-Headers
```

### Viva Answer

> Frontend requests commonly send `Content-Type: application/json` and `Authorization: Bearer <token>`, while backend CORS responses tell the browser which origins, methods, and headers are allowed.

---

## 26. Same-Origin Policy

### Simple Explanation

The same-origin policy is a browser security rule that prevents one website from freely reading data from another origin.

CORS is the controlled exception to this rule.

### Example

Without CORS, a malicious site should not be able to read responses from:

```text
http://localhost:5190/api/auth/me
```

### Viva Answer

> Same-origin policy blocks cross-origin browser reads by default, and CORS allows the server to opt into controlled cross-origin access.

---

## 27. CORS Middleware Order

### Simple Explanation

Middleware order matters in ASP.NET Core.

### In Your Project

API Gateway uses:

```csharp
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseOcelot().Wait();
```

### Why This Order Helps

CORS runs before authentication/authorization so browser preflight requests can be handled properly.

### Viva Answer

> CORS middleware should run before authentication and authorization so preflight requests are handled correctly.

---

## 28. CORS Error vs API Error

### CORS Error

Browser blocks the response before Angular can read it.

Usually shown in browser console.

### API Error

Backend returns a real HTTP response.

Examples:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
500 Internal Server Error
```

### Difference

| Problem | Who Blocks / Returns It? |
|---|---|
| CORS error | Browser |
| API error | Backend |

### Viva Answer

> A CORS error is blocked by the browser before the frontend can read the response, while an API error is an actual backend response with an HTTP status code.

---

## 29. Why Postman Works but Angular Fails

### Simple Explanation

Postman is not a browser, so it does not enforce CORS.

Angular runs in a browser, so it must obey CORS.

### Example

If CORS is misconfigured:

```text
Postman request -> Works
Angular request -> Blocked by browser
```

### Viva Answer

> Postman can work while Angular fails because CORS is enforced by browsers, not by Postman.

---

## 30. Credentials and CORS

### Simple Explanation

Credentials include cookies, client certificates, or HTTP auth information.

JWT in an `Authorization` header is not the same as browser cookie credentials, but it is still a custom/sensitive header that requires CORS permission.

### Important Rule

If using cookies with CORS:

```csharp
AllowAnyOrigin()
```

cannot be safely combined with:

```csharp
AllowCredentials()
```

You must specify exact origins.

### In Your Project

Your project uses Bearer tokens in headers, not cookie-based authentication.

### Viva Answer

> When using cookies across origins, CORS must specify exact allowed origins and credentials settings. This project uses JWT Bearer headers instead of cookie authentication.

---

## 31. Production CORS Recommendation

### Recommended Production Policy

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendOnly", policy =>
    {
        policy.WithOrigins("https://mockinterview.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Environment-Based Usage

Development:

```text
AllowAnyOrigin
```

Production:

```text
WithOrigins("actual frontend domain")
```

### Viva Answer

> In production, CORS should allow only trusted frontend domains instead of allowing every origin.

---

## 32. Common Frontend-Backend Problems

### CORS Blocked

Cause:

```text
Backend did not allow frontend origin, method, or header.
```

### 401 Unauthorized

Cause:

```text
Missing, expired, or invalid JWT.
```

### 403 Forbidden

Cause:

```text
User is authenticated but does not have required role or permission.
```

### 404 Not Found

Cause:

```text
Wrong API URL or Ocelot route mismatch.
```

### 500 Internal Server Error

Cause:

```text
Backend exception, database issue, external service issue, or missing config.
```

### Viva Answer

> Common frontend-backend issues include CORS blocking, invalid JWTs, missing authorization, incorrect gateway routes, and backend exceptions.

---

## 33. Debugging CORS and Communication Issues

### Steps

1. Check browser console for CORS errors.
2. Check browser Network tab.
3. Confirm Angular `environment.apiUrl`.
4. Confirm API Gateway is running.
5. Confirm Ocelot route exists.
6. Confirm `OPTIONS` is allowed.
7. Confirm `Authorization` header is attached.
8. Test same endpoint in Swagger or Postman.
9. Check backend logs.
10. Check service port and route.

### Viva Answer

> To debug frontend-backend communication, I check the browser console, network tab, API base URL, gateway route, token header, and backend logs.

---

## 34. Important Files in Your Project

### Backend

```text
Backend/API_Gateway/Program.cs
Backend/API_Gateway/ocelot.json
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
```

### Frontend

```text
Frontend/src/environments/environment.ts
Frontend/src/app/interceptors/auth.interceptor.ts
Frontend/src/app/services/auth.service.ts
Frontend/src/app/services/interview.service.ts
Frontend/src/app/services/assessment.service.ts
Frontend/src/app/services/subscription.service.ts
Frontend/src/app/admin/services/admin.service.ts
```

---

## 35. Best Full Viva Answer

> In our Mock Interview Platform, the Angular frontend communicates with the backend mainly through the Ocelot API Gateway. The frontend base URL is configured as `http://localhost:5190/api`, and Angular services use `HttpClient` to call routes such as `/api/auth/login`, `/api/interviews/start`, `/api/assessments/start`, and `/api/subscriptions/subscribe`.
>
> Since Angular and the backend run on different ports during development, they are different origins. The API Gateway enables CORS using a policy that allows any origin, method, and header for development. Individual services also allow open CORS in development for direct testing. Ocelot routes include `OPTIONS` so browser preflight requests can succeed, especially when Angular sends the `Authorization` Bearer token header.
>
> After login, IdentityService returns a JWT. The frontend stores it in localStorage, and the Angular auth interceptor attaches it to protected API calls using `Authorization: Bearer <token>`. The API Gateway validates the token for protected routes and forwards the request to the correct microservice.
>
> CORS is different from authentication. CORS only allows browser cross-origin communication, while JWT authentication verifies the user. In production, `AllowAnyOrigin` should be replaced with a restricted policy that allows only the real frontend domain.

---

## 36. Common Viva Questions and Answers

### Q1. What is CORS?

CORS means Cross-Origin Resource Sharing. It allows a backend to decide which browser origins can call its APIs.

### Q2. Why is CORS needed in this project?

Because Angular and the API Gateway run on different ports, so they are different origins.

### Q3. What is an origin?

An origin is the combination of scheme, host, and port.

### Q4. Who enforces CORS?

The browser enforces CORS.

### Q5. Why does Postman work when Angular gets a CORS error?

Postman does not enforce browser CORS rules, but Angular runs inside a browser.

### Q6. What is a preflight request?

It is an automatic `OPTIONS` request sent by the browser to check whether the actual cross-origin request is allowed.

### Q7. Why do Ocelot routes include `OPTIONS`?

To allow browser CORS preflight requests to pass through the API Gateway.

### Q8. What does `AllowAnyOrigin` mean?

It allows requests from any frontend origin.

### Q9. Is `AllowAnyOrigin` safe for production?

No. Production should restrict CORS to trusted frontend domains.

### Q10. How does Angular know the backend URL?

It reads the API base URL from `environment.ts`.

### Q11. What is Angular `HttpClient`?

It is Angular's built-in service for making HTTP requests.

### Q12. What does the auth interceptor do?

It attaches the JWT token to outgoing requests using the `Authorization` header.

### Q13. What is the difference between CORS and authentication?

CORS allows browser cross-origin access, while authentication verifies the identity of the user.

### Q14. What happens if JWT is missing?

The browser request may pass CORS, but protected APIs return `401 Unauthorized`.

### Q15. What happens if Ocelot route is wrong?

The frontend may receive `404 Not Found` or the request may be forwarded to the wrong service.

---

## 37. Quick Revision Summary

```text
CORS = Cross-Origin Resource Sharing.

Origin = scheme + host + port.

Angular: http://localhost:4200
API Gateway: http://localhost:5190

Different ports = different origins.

Browser enforces CORS.

Postman does not enforce CORS.

Preflight = browser OPTIONS request.

Authorization header can trigger preflight.

API Gateway allows CORS using CorsPolicy.

Ocelot routes include OPTIONS.

Angular base API URL = http://localhost:5190/api.

HttpClient sends requests.

Auth interceptor adds Bearer token.

CORS is not authentication.

Production should use restricted origins, not AllowAnyOrigin.
```

---

## 38. One-Line Viva Answer

> CORS allows the Angular frontend running on a different origin to call the ASP.NET Core API Gateway, while frontend-backend communication happens through Angular HttpClient, Bearer token headers, Ocelot routing, and JSON API responses.
