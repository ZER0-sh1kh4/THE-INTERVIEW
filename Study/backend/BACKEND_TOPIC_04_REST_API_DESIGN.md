# Topic 4: REST API Design

Project: Mock Interview Platform  
Focus: Understanding how backend APIs are designed, how frontend communicates with them, why HTTP methods matter, and how your project endpoints follow REST-style design.

---

## 1. What Is an API?

### Simple Explanation

API means Application Programming Interface.

In simple words, API is a way for two applications to talk to each other.

In your project:

```text
Angular frontend talks to ASP.NET Core backend using APIs.
```

The frontend does not directly access database or backend classes. It calls API endpoints.

### Practical Scenario

User clicks Login.

Frontend calls:

```text
POST /api/auth/login
```

Backend receives:

```json
{
  "email": "student@gmail.com",
  "password": "123456"
}
```

Backend responds:

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "token": "jwt_token_here"
  }
}
```

### Why API Is Needed

Because frontend and backend are separate applications.

Frontend needs a controlled way to request backend actions like:

- Login
- Register
- Start assessment
- Submit interview
- Buy premium
- Fetch result

### Viva Answer

> API is a communication interface between frontend and backend. In my project, Angular calls ASP.NET Core Web APIs to perform actions like login, start interviews, attempt assessments, and manage subscriptions.

---

## 2. What Is REST?

### Simple Explanation

REST means Representational State Transfer.

It is an architectural style for designing APIs using:

- URLs
- HTTP methods
- JSON data
- Stateless requests
- Standard status codes

### Practical Meaning

Instead of calling backend methods directly like:

```text
LoginUser()
StartAssessment()
SubmitInterview()
```

Frontend calls HTTP endpoints:

```text
POST /api/auth/login
POST /api/assessments/start
POST /api/interviews/submit
```

### In Your Project

Your backend exposes REST-style endpoints:

```text
/api/auth
/api/interviews
/api/assessments
/api/subscriptions
```

Each endpoint represents one resource area.

### Viva Answer

> REST is an API design style where frontend and backend communicate using HTTP methods, URLs, JSON data, and standard status codes. My project follows REST-style APIs for authentication, interviews, assessments, and subscriptions.

---

## 3. REST Resource Concept

### Simple Explanation

In REST, endpoints are usually designed around resources.

A resource is a business object or area.

Examples:

```text
auth
users
interviews
assessments
subscriptions
questions
payments
notifications
```

### In Your Project

Resource-style API groups:

| Resource | Endpoint Base | Service |
|---|---|---|
| Authentication | /api/auth | IdentityService |
| Interviews | /api/interviews | InterviewService |
| Assessments | /api/assessments | AssessmentService |
| Subscriptions | /api/subscriptions | SubscriptionService |
| Questions | /api/assessments/questions | AssessmentService |

### Practical Example

All interview-related endpoints start with:

```text
/api/interviews
```

Examples:

```text
POST /api/interviews/start
GET /api/interviews
GET /api/interviews/{id}
GET /api/interviews/{id}/result
POST /api/interviews/submit
```

This makes the API easier to understand.

### Viva Answer

> REST APIs are organized around resources. In my project, resources like auth, interviews, assessments, subscriptions, and questions have separate endpoint groups.

---

## 4. HTTP Methods

### Simple Explanation

HTTP method tells backend what kind of action frontend wants to perform.

### Common Methods

| Method | Meaning | Project Example |
|---|---|---|
| GET | Read/fetch data | GET /api/interviews |
| POST | Create or perform action | POST /api/auth/login |
| PUT | Update existing data | PUT /api/auth/me |
| DELETE | Delete data | DELETE /api/assessments/questions/{id} |

---

## 5. GET Method

### Simple Explanation

GET is used to fetch data.

GET should not normally change database state.

### Project Examples

```text
GET /api/auth/me
```

Fetch current logged-in user claims.

```text
GET /api/interviews
```

Fetch current user's interviews.

```text
GET /api/interviews/{id}
```

Fetch one interview with questions.

```text
GET /api/assessments/{id}/result
```

Fetch assessment result.

```text
GET /api/subscriptions/my/payments
```

Fetch current user's payment history.

### Practical Scenario

User opens interview history page.

Frontend sends:

```text
GET /api/interviews
```

Backend returns all interviews belonging to that user.

### Why GET Is Used

Because frontend is only asking for existing data.

### What If We Use POST Instead?

It may still work technically, but it would be poor API design because reading data should use GET.

### Viva Answer

> GET is used to retrieve data without modifying server state. My project uses GET for fetching user profile, interviews, assessment results, subscriptions, and payment history.

---

## 6. POST Method

### Simple Explanation

POST is used to create something or perform an action.

### Project Examples

```text
POST /api/auth/register
```

Creates a new user.

```text
POST /api/auth/login
```

Performs login and creates JWT token.

```text
POST /api/interviews/start
```

Creates a new interview record.

```text
POST /api/assessments/start
```

Creates a new assessment attempt.

```text
POST /api/assessments/submit
```

Submits answers and creates result.

```text
POST /api/subscriptions/subscribe
```

Creates a payment session.

### Why Login Is POST

Login sends sensitive data:

```text
email
password
```

So it should be in request body, not URL.

GET should not be used for passwords because URLs can be stored in browser history, logs, and proxies.

### Why Submit Is POST

Submit changes server state:

- Saves answers
- Marks assessment/interview as completed
- Creates result

So POST is appropriate.

### Viva Answer

> POST is used when the request creates data or performs an action. My project uses POST for register, login, starting interviews, starting assessments, submitting answers, and creating subscription sessions.

---

## 7. PUT Method

### Simple Explanation

PUT is used to update existing data.

### Project Examples

```text
PUT /api/auth/me
```

Updates current user's profile.

```text
PUT /api/auth/admin/users/{id}/role
```

Admin updates user's role.

```text
PUT /api/auth/admin/users/{id}/premium
```

Admin updates user's premium flag.

```text
PUT /api/assessments/questions/{id}
```

Admin updates an MCQ question.

### Practical Scenario

Admin changes a user's role from Candidate to Admin.

Frontend sends:

```text
PUT /api/auth/admin/users/5/role
```

Backend updates existing user record.

### Why PUT Is Used

Because the record already exists and we are modifying it.

### Viva Answer

> PUT is used to update existing resources. My project uses PUT for updating profile, user role, premium status, and assessment questions.

---

## 8. DELETE Method

### Simple Explanation

DELETE is used to remove data.

### Project Example

```text
DELETE /api/assessments/questions/{id}
```

Admin deletes an MCQ question.

### Practical Scenario

Admin finds one wrong question in question bank.

Frontend sends:

```text
DELETE /api/assessments/questions/12
```

Backend removes that question and re-indexes remaining domain questions.

### Why DELETE Is Used

Because we are removing an existing resource.

### Viva Answer

> DELETE is used to remove resources. In my project, admin can delete MCQ questions using DELETE endpoint.

---

## 9. Endpoint Naming

### Simple Explanation

Endpoint names should be clear and predictable.

Good API design makes it easy to guess what an endpoint does.

### In Your Project

Authentication endpoints:

```text
/api/auth/register
/api/auth/login
/api/auth/me
/api/auth/refresh-claims
```

Interview endpoints:

```text
/api/interviews/start
/api/interviews/{id}/begin
/api/interviews/submit
/api/interviews/{id}/result
```

Assessment endpoints:

```text
/api/assessments/start
/api/assessments/submit
/api/assessments/{id}/result
/api/assessments/questions
```

Subscription endpoints:

```text
/api/subscriptions/subscribe
/api/subscriptions/confirm
/api/subscriptions/cancel
/api/subscriptions/webhook/stripe
```

### Why Some Endpoints Use Verbs

Pure REST usually prefers nouns.

Example:

```text
POST /api/interviews
```

instead of:

```text
POST /api/interviews/start
```

But in practical applications, action names like `start`, `submit`, `confirm`, and `cancel` are commonly used when the endpoint represents a business operation.

### Viva Answer

> My project uses REST-style endpoint grouping with practical action endpoints like start, submit, confirm, and cancel for business operations. This keeps the API understandable for frontend integration.

---

## 10. Request Body

### Simple Explanation

Request body carries data from frontend to backend, usually as JSON.

### Project Example: Login

```json
{
  "email": "student@gmail.com",
  "password": "123456"
}
```

Mapped to:

```csharp
LoginRequest
```

### Project Example: Start Assessment

```json
{
  "domain": "C#",
  "difficulty": "Medium",
  "questionCount": 10
}
```

Mapped to:

```csharp
StartAssessmentRequest
```

### Project Example: Submit Assessment

```json
{
  "assessmentId": 4,
  "totalExpected": 10,
  "answers": [
    {
      "questionId": 1,
      "selectedOption": "A"
    }
  ]
}
```

Mapped to:

```csharp
SubmitAssessmentRequest
```

### Why Request Body Is Used

Request body is used for structured or sensitive data.

Use body for:

- Login credentials
- Registration form
- Assessment answers
- Interview answers
- Payment confirmation data

### Viva Answer

> Request body carries JSON data from frontend to backend. ASP.NET Core model binding converts this JSON into DTO objects like LoginRequest, StartAssessmentRequest, and SubmitAssessmentRequest.

---

## 11. Route Parameters

### Simple Explanation

Route parameters are values inside the URL path.

### Example

```text
GET /api/interviews/7
```

Here:

```text
id = 7
```

### Project Examples

```text
GET /api/interviews/{id}
GET /api/interviews/{id}/result
GET /api/assessments/{id}/result
PUT /api/assessments/questions/{id}
DELETE /api/assessments/questions/{id}
```

### Why Route Parameters Are Used

They identify a specific resource.

Example:

```text
GET /api/assessments/10/result
```

means:

```text
Get result of assessment with id 10
```

### Viva Answer

> Route parameters are values placed inside the URL to identify a specific resource, such as interview id, assessment id, or question id.

---

## 12. Query Parameters

### Simple Explanation

Query parameters are optional values added after `?` in the URL.

### Example

```text
GET /api/assessments/10/next-batch?currentCount=5&batchSize=5
```

Here:

```text
currentCount = 5
batchSize = 5
```

### In Your Project

Assessment next batch endpoint uses query parameters:

```csharp
[HttpGet("{id}/next-batch")]
public async Task<IActionResult> NextBatch(
    int id,
    [FromQuery] int currentCount = 0,
    [FromQuery] int batchSize = 5)
```

### Why Query Parameters Are Used

They are good for filtering, paging, searching, and optional controls.

In your project:

```text
currentCount
batchSize
```

control lazy loading of assessment questions.

### Viva Answer

> Query parameters pass optional values in the URL. My project uses them in assessment next-batch API to specify current question count and batch size.

---

## 13. Headers

### Simple Explanation

Headers carry metadata about the request.

Examples:

```text
Content-Type: application/json
Authorization: Bearer jwt_token
Stripe-Signature: stripe_signature
X-Internal-Key: internal_key
```

### In Your Project

Authorization header:

```text
Authorization: Bearer token
```

Used for protected APIs.

Stripe webhook header:

```text
Stripe-Signature
```

Used to verify that webhook really came from Stripe.

Internal service header:

```text
X-Internal-Key
```

Used by internal premium update endpoint.

### Why Headers Are Used

Headers carry security and metadata separately from request body.

### Viva Answer

> Headers carry metadata such as JWT token, content type, Stripe signature, or internal API key. My project uses Authorization header for JWT-protected APIs.

---

## 14. Stateless API

### Simple Explanation

REST APIs are usually stateless.

Stateless means each request must carry enough information for backend to process it.

Backend does not rely on server-side session memory for logged-in user.

### In Your Project

Your project uses JWT.

Each protected request sends:

```text
Authorization: Bearer token
```

The token contains user identity claims:

```text
userId
email
role
isPremium
FullName
```

So backend can understand the user from each request.

### Practical Scenario

User calls:

```text
POST /api/interviews/start
```

Backend does not ask:

```text
Who logged in earlier in this server memory?
```

Instead it reads JWT from current request.

### Why Stateless Is Useful

- Easier scaling
- No server session storage
- Services can validate token independently
- Works well with microservices

### Viva Answer

> My APIs are stateless because each protected request includes a JWT token. Backend services read user claims from the token instead of relying on server-side sessions.

---

## 15. Standard API Response

### Simple Explanation

A standard response format means APIs return data in a consistent shape.

### In Your Project

Successful responses use:

```text
ApiResponse<T>
```

Error responses use:

```text
ApiErrorResponse
```

### Practical Success Example

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "token": "jwt_token"
  }
}
```

### Practical Error Example

```json
{
  "message": "Validation failed.",
  "details": "Domain is required."
}
```

### Why Standard Response Is Useful

Frontend can handle responses consistently.

It knows where to look for:

```text
message
data
details
```

### Viva Answer

> My project uses standard response wrappers so frontend receives predictable success and error formats across services.

---

## 16. Public, Protected, and Admin APIs

### Simple Explanation

Not every API should be open to everyone.

Your project has three categories:

```text
Public APIs
Protected APIs
Admin APIs
```

### Public APIs

Can be called without login.

Examples:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password/request-otp
POST /api/auth/forgot-password/reset
POST /api/subscriptions/webhook/stripe
```

### Protected APIs

Need valid JWT token.

Examples:

```text
GET /api/auth/me
POST /api/interviews/start
POST /api/assessments/start
POST /api/subscriptions/subscribe
```

### Admin APIs

Need JWT token with Admin role.

Examples:

```text
GET /api/auth/admin/users
GET /api/interviews/admin/all
GET /api/assessments/admin/all
POST /api/assessments/questions
DELETE /api/assessments/questions/{id}
```

### Why This Separation Matters

Without protection:

- Anyone could see all users
- Anyone could delete questions
- Anyone could update premium status
- Anyone could view all interviews

### Viva Answer

> My project separates APIs into public, protected, and admin-only endpoints. Public APIs use AllowAnonymous, protected APIs require JWT, and admin APIs require role-based authorization.

---

## 17. API Gateway Route Design

### Simple Explanation

Frontend calls gateway routes. Gateway forwards those routes to downstream services.

### In Your Project

Frontend calls:

```text
/api/auth/login
```

Gateway forwards to:

```text
IdentityService on port 5005
```

Frontend calls:

```text
/api/interviews/start
```

Gateway forwards to:

```text
InterviewService on port 5002
```

### Why Gateway Routes Match Service Routes

It keeps frontend simple.

Frontend does not need to know internal service ports.

### Viva Answer

> API Gateway maps upstream frontend routes to downstream service routes. This keeps frontend API calls consistent while hiding internal service addresses.

---

## 18. Endpoint Study: IdentityService

### Public Authentication APIs

```text
POST /api/auth/register
```

Creates or updates user registration and returns JWT.

```text
POST /api/auth/login
```

Verifies credentials and returns JWT.

```text
POST /api/auth/forgot-password/request-otp
```

Generates OTP and publishes email event.

```text
POST /api/auth/forgot-password/reset
```

Validates OTP and resets password.

### Protected User APIs

```text
GET /api/auth/me
```

Returns current user's claims.

```text
PUT /api/auth/me
```

Updates profile name and returns refreshed token.

```text
POST /api/auth/refresh-claims
```

Returns new JWT after premium/profile changes.

### Admin APIs

```text
GET /api/auth/admin/users
GET /api/auth/admin/users/{id}
PUT /api/auth/admin/users/{id}/role
PUT /api/auth/admin/users/{id}/premium
PUT /api/auth/admin/users/{id}/deactivate
PUT /api/auth/admin/users/{id}/reactivate
```

### Viva Explanation

> IdentityService API design separates public authentication endpoints, protected profile endpoints, and admin-only user management endpoints.

---

## 19. Endpoint Study: InterviewService

### Protected Candidate APIs

```text
POST /api/interviews/start
```

Creates a pending interview.

```text
POST /api/interviews/{id}/begin
```

Starts interview and loads questions.

```text
POST /api/interviews/warm-up
```

Pre-generates or warms question cache.

```text
POST /api/interviews/{id}/fetch-more
```

Fetches more generated questions.

```text
POST /api/interviews/submit
```

Submits answers and calculates result.

```text
GET /api/interviews
```

Gets current user's interview history.

```text
GET /api/interviews/{id}
```

Gets one interview with questions.

```text
GET /api/interviews/{id}/result
```

Gets interview result.

### Admin API

```text
GET /api/interviews/admin/all
```

Gets all interviews for admin.

### Viva Explanation

> InterviewService APIs are designed around interview lifecycle: start, begin, fetch questions, submit answers, view history, and view result.

---

## 20. Endpoint Study: AssessmentService

### Candidate APIs

```text
POST /api/assessments/start
```

Creates an assessment and returns questions.

```text
GET /api/assessments/{id}/next-batch
```

Fetches next lazy-loaded question batch.

```text
POST /api/assessments/warm-up
```

Warms question cache.

```text
POST /api/assessments/submit
```

Submits answers and creates result.

```text
GET /api/assessments
```

Gets user's assessment history.

```text
GET /api/assessments/{id}/result
```

Gets one assessment result.

### Admin APIs

```text
GET /api/assessments/admin/all
GET /api/assessments/questions
POST /api/assessments/questions
PUT /api/assessments/questions/{id}
DELETE /api/assessments/questions/{id}
```

### Viva Explanation

> AssessmentService APIs are designed around MCQ test lifecycle and question-bank management. Candidates start and submit tests, while admins manage MCQ questions.

---

## 21. Endpoint Study: SubscriptionService

### Candidate APIs

```text
POST /api/subscriptions/subscribe
```

Creates payment session.

```text
POST /api/subscriptions/confirm
```

Confirms simulated payment when Stripe mode is disabled.

```text
POST /api/subscriptions/cancel
```

Cancels active subscription.

```text
GET /api/subscriptions/my
```

Gets user's subscriptions.

```text
GET /api/subscriptions/my/payments
```

Gets user's payment history.

### Public Webhook API

```text
POST /api/subscriptions/webhook/stripe
```

Receives Stripe payment events.

### Admin APIs

```text
GET /api/subscriptions/all
GET /api/subscriptions/payments
```

### Viva Explanation

> SubscriptionService APIs handle payment session creation, payment confirmation, Stripe webhook processing, subscription cancellation, and payment/subscription history.

---

## 22. Why Webhook API Is Anonymous

### Simple Explanation

Stripe webhook is not called by logged-in frontend user.

It is called by Stripe server.

So it cannot send your app's JWT token.

### In Your Project

Webhook endpoint:

```text
POST /api/subscriptions/webhook/stripe
```

has:

```csharp
[AllowAnonymous]
```

But it is still protected using:

```text
Stripe-Signature header
```

### Why This Is Secure

The endpoint is public but signature-verified.

Backend checks whether the request really came from Stripe.

### Viva Answer

> Stripe webhook is anonymous because Stripe server cannot use our JWT token. Security is handled by verifying the Stripe-Signature header.

---

## 23. API Validation

### Simple Explanation

Validation checks whether request data is correct before processing.

### Project Examples

Interview domain required:

```text
Domain is required.
```

Assessment answers required:

```text
At least one answer must be submitted.
```

Question correct option must be:

```text
A, B, C, or D
```

### Why Validation Is Important

Without validation:

- Empty domain may create invalid interview
- Empty answers may create wrong result
- Invalid question IDs may corrupt score
- Bad data may reach database

### Viva Answer

> API validation ensures incoming request data is correct before business logic runs. My project validates required fields, user permissions, assessment ownership, and question data.

---

## 24. Idempotency and Duplicate Requests

### Simple Explanation

Idempotency means repeating the same request should not create unwanted duplicate effects.

### Practical Scenario

Stripe may send the same webhook event more than once.

If backend processes it twice:

- Duplicate subscription may be created
- Payment may be counted twice
- Email may be sent twice

### In Your Project

SubscriptionService stores webhook event IDs in:

```text
WebhookEventLog
```

If event already exists, duplicate is ignored.

### Viva Answer

> Idempotency prevents duplicate effects from repeated requests. My project handles duplicate Stripe webhooks by storing processed event IDs and ignoring repeated events.

---

## 25. REST Design Strengths in This Project

### Strengths

- Endpoints are grouped by service/resource
- HTTP methods are mostly used properly
- Public, protected, and admin endpoints are separated
- JWT is used for stateless protected APIs
- Route parameters identify specific resources
- Query parameters are used for optional batch controls
- Standard response wrappers improve frontend consistency
- Webhook endpoint is public but signature protected
- API Gateway gives one frontend entry point

### Practical Explanation

The frontend can understand backend routes easily:

```text
auth means user identity
interviews means mock interview module
assessments means MCQ test module
subscriptions means premium/payment module
```

### Viva Answer

> The API design is practical and modular. Endpoints are grouped by business resource, protected using JWT or roles, and routed through API Gateway.

---

## 26. Possible REST Improvements

### Current Practical Design

Your project uses endpoints like:

```text
POST /api/interviews/start
POST /api/assessments/submit
POST /api/subscriptions/cancel
```

These are understandable and practical.

### More REST-Pure Alternatives

Instead of:

```text
POST /api/interviews/start
```

Could use:

```text
POST /api/interviews
```

Instead of:

```text
POST /api/interviews/submit
```

Could use:

```text
POST /api/interviews/{id}/answers
```

Instead of:

```text
POST /api/subscriptions/cancel
```

Could use:

```text
DELETE /api/subscriptions/{id}
```

### Balanced Viva Answer

> My project uses REST-style APIs with some action-based endpoint names for business operations like start, submit, confirm, and cancel. These names are practical and easy for frontend integration, though more REST-pure alternatives could model them as resource creation or updates.

---

## 27. Complete Practical Flow: Assessment Submit API

### Request

```text
POST /api/assessments/submit
Authorization: Bearer token
Content-Type: application/json
```

```json
{
  "assessmentId": 4,
  "totalExpected": 10,
  "answers": [
    {
      "questionId": 1,
      "selectedOption": "A"
    },
    {
      "questionId": 2,
      "selectedOption": "C"
    }
  ]
}
```

### Backend Steps

```text
1. JWT token identifies current user.
2. Controller receives SubmitAssessmentRequest.
3. Service checks assessment exists.
4. Service checks assessment belongs to user.
5. Service checks assessment is still in progress.
6. Service checks time is not expired.
7. Service validates question IDs.
8. Service compares selected options with correct options.
9. Score and percentage are calculated.
10. Result is saved.
11. Completion events/emails are published.
12. Response is returned.
```

### Response

```json
{
  "success": true,
  "data": {
    "score": 8,
    "maxScore": 10,
    "percentage": 80,
    "grade": "A"
  }
}
```

### Viva Explanation

> Assessment submit API is a protected POST endpoint because it changes server state. It receives answers in request body, validates ownership and question IDs, calculates score, saves result, and returns the assessment result.

---

## 28. Complete Practical Flow: Login API

### Request

```text
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "student@gmail.com",
  "password": "123456"
}
```

### Backend Steps

```text
1. Gateway routes request to IdentityService.
2. AuthController receives LoginRequest.
3. AuthService searches user by email.
4. BCrypt verifies password.
5. Backend checks user is active.
6. JWT token is generated.
7. Response is returned to frontend.
```

### Response

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "token": "jwt_token"
  }
}
```

### Viva Explanation

> Login API is public and uses POST because credentials are sent in request body. It verifies the user and returns a JWT token for future protected API calls.

---

## 29. What Happens If REST Design Is Poor?

### Problems

- Frontend developers get confused
- APIs become inconsistent
- Security rules may be unclear
- Wrong HTTP methods may be used
- Sensitive data may appear in URLs
- Error handling becomes difficult
- API documentation becomes messy

### Bad Design Example

```text
GET /login?email=a@gmail.com&password=123456
```

Problems:

- Password visible in URL
- GET is wrong for login
- Browser history may store credentials
- Server logs may store password

### Better Design

```text
POST /api/auth/login
```

Credentials are sent in JSON body.

### Viva Answer

> Poor REST design can create confusion, security issues, and inconsistent frontend integration. Good API design uses proper methods, clear URLs, request bodies for sensitive data, and standard responses.

---

## 30. Best Full Viva Answer for Topic 4

> REST API design defines how frontend communicates with backend using HTTP methods, URLs, request bodies, route parameters, query parameters, headers, and status codes. My project groups APIs by resources such as auth, interviews, assessments, and subscriptions. GET is used for fetching data, POST for creating or performing actions, PUT for updates, and DELETE for deletion. Protected APIs use JWT in the Authorization header, while admin APIs use role-based authorization. The API Gateway routes frontend calls to the correct service. Standard response wrappers make frontend handling consistent. Some endpoints use practical action names like start, submit, confirm, and cancel because they represent business workflows.

---

## 31. Common Viva Questions and Answers

### Q1. What is REST API?

REST API is an architectural style where frontend and backend communicate using HTTP methods, URLs, JSON, and status codes.

### Q2. Why is login POST, not GET?

Because login sends sensitive data like password. POST sends data in request body, while GET may expose data in URL logs or browser history.

### Q3. What is the difference between GET and POST?

GET fetches data and should not change server state. POST creates data or performs an action that may change server state.

### Q4. What is PUT used for?

PUT is used to update existing data, such as user profile, user role, premium flag, or assessment question.

### Q5. What is DELETE used for?

DELETE is used to remove existing resources, such as deleting an MCQ question.

### Q6. What are route parameters?

Route parameters are values in the URL path used to identify resources, such as `{id}` in `/api/interviews/{id}`.

### Q7. What are query parameters?

Query parameters are optional values after `?` in URL, used for filtering, paging, or control values.

### Q8. What are headers?

Headers contain metadata such as Authorization token, content type, Stripe signature, or internal API key.

### Q9. What is stateless API?

Stateless API means each request contains enough information to process it, usually through JWT token, without relying on server-side session memory.

### Q10. Why is webhook endpoint anonymous?

Because Stripe server calls it, not a logged-in user. It is secured using Stripe signature verification instead of JWT.

### Q11. What is standard API response?

A standard API response gives a consistent JSON structure for success and error responses, making frontend handling easier.

### Q12. Why are admin APIs protected?

Because only admin users should access sensitive operations like viewing all users, changing roles, or managing question banks.

### Q13. What is API Gateway's role in REST design?

API Gateway gives frontend one entry point and routes requests to correct backend services.

### Q14. Are your APIs pure REST?

They are REST-style APIs with some practical action-based routes like start, submit, confirm, and cancel for business workflows.

### Q15. What improvement can be made in API design?

Some action endpoints can be made more resource-oriented, such as using `POST /api/interviews` instead of `POST /api/interviews/start`.

---

## 32. Quick Revision Summary

- API lets frontend and backend communicate.
- REST uses URLs, HTTP methods, JSON, and status codes.
- Resources in project: auth, interviews, assessments, subscriptions, questions.
- GET fetches data.
- POST creates data or performs actions.
- PUT updates existing data.
- DELETE removes data.
- Request body carries JSON data.
- Route parameters identify resources.
- Query parameters pass optional controls.
- Headers carry metadata like JWT.
- JWT makes protected APIs stateless.
- Public APIs do not require login.
- Protected APIs require JWT.
- Admin APIs require Admin role.
- Stripe webhook is anonymous but signature-protected.
- Standard API responses help frontend.
- API Gateway routes frontend calls to backend services.

