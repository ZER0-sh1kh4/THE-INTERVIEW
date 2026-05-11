# Topic 1: Backend Architecture Overview

Project: Mock Interview Platform  
Focus: Understanding what the backend is, how requests flow, why services are separated, and how your actual project backend is arranged.

---

## 1. What Is Backend?

### Simple Explanation

When a user opens your application, they see buttons, pages, forms, dashboards, interviews, assessments, and payment screens. That visible part is the frontend.

But behind those screens, many important decisions need to happen:

- Is this user registered?
- Is the password correct?
- Is this user premium or free?
- Is this user allowed to access this result?
- Which questions should be shown?
- What score should be calculated?
- Has the payment really succeeded?
- Should an email be sent?

All this work is handled by the backend.

### Practical Scenario

Suppose a student enters email and password on the login page.

The frontend only collects the input. It cannot safely decide whether the password is correct.

So it sends the data to the backend:

```text
User enters email/password
        ↓
Frontend sends login request
        ↓
Backend checks database
        ↓
Backend verifies password
        ↓
Backend creates JWT token
        ↓
Frontend receives login success
```

### Technical Definition

Backend is the server-side part of an application that handles business logic, authentication, authorization, database operations, external integrations, and API responses.

### In Your Project

Your backend is built using:

- ASP.NET Core Web API
- C#
- SQL Server
- Entity Framework Core
- Ocelot API Gateway
- JWT authentication
- RabbitMQ
- Stripe
- Gemini API
- MailKit

### Viva Answer

> The backend is the server-side system of my project. It handles business logic, authentication, authorization, database operations, interview and assessment workflows, payment processing, email notifications, and returns secure API responses to the Angular frontend.

---

## 2. Client-Server Architecture

### Simple Explanation

Client-server architecture means the application is divided into two main parts:

- Client: the part used by the user
- Server: the part that processes requests and controls data

In your project:

```text
Client = Angular frontend
Server = ASP.NET Core backend
Database = SQL Server
```

### Practical Scenario

A user wants to see assessment result.

```text
Angular frontend asks:
"Give me result for assessment 10"

Backend checks:
"Is the user logged in?"
"Does this assessment belong to this user?"
"Is the result available?"

Database gives:
Score, percentage, grade

Backend returns:
Result response to frontend
```

### Why We Use This

The frontend should not directly access the database. If the browser had database access, users could inspect code and misuse database credentials.

Backend acts as a secure middle layer.

### What If We Do Not Use Backend?

If frontend directly talks to the database:

- Database credentials may be exposed
- Users may bypass validation
- Users may modify marks or premium status
- Payment verification becomes unsafe
- Admin-only data may leak

### Viva Answer

> My project follows client-server architecture. Angular is the client, ASP.NET Core services are the server, and SQL Server stores persistent data. The backend protects business logic and database access.

---

## 3. Request-Response Cycle

### Simple Explanation

Most backend communication happens through request and response.

Request means the frontend asks something.

Response means the backend replies.

### Practical Login Example

Request:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "student@gmail.com",
  "password": "123456"
}
```

Response:

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

### Project Flow

```text
Frontend sends request
        ↓
API Gateway receives request
        ↓
Correct backend service handles request
        ↓
Service applies business logic
        ↓
Database may be used
        ↓
Response goes back to frontend
```

### In Your Project

Your project uses common response models from BuildingBlocks:

- `ApiResponse`
- `ApiErrorResponse`

These help services return consistent response structures.

Important file:

`Backend/BuildingBlocks/Contracts/ApiResponse.cs`

### Why Consistent Response Matters

If every API returns data differently, frontend becomes harder to manage.

Bad situation:

```json
{ "msg": "done" }
```

```json
{ "status": "ok", "result": {} }
```

```json
{ "data": {}, "message": "success" }
```

Better situation:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": {}
}
```

### Viva Answer

> In my project, frontend sends HTTP requests to backend APIs. The backend processes those requests, performs validation and database operations, then returns JSON responses using a common response format.

---

## 4. REST API Basics

### Simple Explanation

REST API is a way for frontend and backend to communicate using URLs and HTTP methods.

Example:

```text
/api/auth/login
/api/interviews/start
/api/assessments/start
/api/subscriptions/subscribe
```

Each endpoint represents an operation.

### HTTP Methods

| Method | Meaning | Project Example |
|---|---|---|
| GET | Fetch data | Get my interviews |
| POST | Create or perform action | Login, start assessment |
| PUT | Update existing data | Update profile |
| DELETE | Delete data | Delete question |

### Project Examples

```text
POST /api/auth/login
```

Used for login because user sends email and password in request body.

```text
GET /api/interviews
```

Used to fetch current user's interviews.

```text
PUT /api/auth/me
```

Used to update current user's profile.

```text
DELETE /api/assessments/questions/{id}
```

Used by admin to delete an MCQ question.

### Why Login Is POST, Not GET

GET is meant for reading data. Login sends sensitive information like password, so POST is more appropriate.

Also, GET data may appear in browser history or logs if sent as query string.

### Viva Answer

> REST API allows the Angular frontend to communicate with backend services using HTTP methods. My project uses GET for fetching data, POST for creating or performing actions, PUT for updates, and DELETE for removing records.

---

## 5. HTTP Status Codes

### Simple Explanation

Status codes tell the frontend whether the request succeeded or failed.

### Important Status Codes

| Status Code | Meaning | Project Scenario |
|---|---|---|
| 200 | Success | Login successful |
| 400 | Bad request | Missing domain |
| 401 | Unauthorized | Invalid token or wrong credentials |
| 403 | Forbidden | Free user exceeded limit |
| 404 | Not found | Interview not found |
| 500 | Server error | Unexpected backend error |

### Practical Examples

Wrong password:

```text
401 Unauthorized
Invalid credentials.
```

Free user tries second interview:

```text
403 Forbidden
Free users can create only 1 interview.
```

Assessment ID does not exist:

```text
404 Not Found
Assessment not found.
```

### Why Status Codes Are Useful

Frontend can react properly.

Example:

- `401`: redirect user to login
- `403`: show upgrade-to-premium message
- `404`: show not found message
- `500`: show general error message

### Viva Answer

> HTTP status codes help the frontend understand the result of an API request. My project uses codes like 200 for success, 400 for validation errors, 401 for authentication failure, 403 for permission issues, 404 for missing resources, and 500 for unexpected server errors.

---

## 6. Overall Backend Architecture of This Project

### Simple Explanation

Your backend is not one single application. It is divided into multiple services.

Each service has a specific job.

```text
Angular Frontend
        ↓
API Gateway
        ↓
------------------------------------------------
IdentityService       → users, login, JWT
InterviewService      → subjective interviews
AssessmentService     → MCQ assessments
SubscriptionService   → payment and premium
NotificationService   → emails
BuildingBlocks        → shared backend code
------------------------------------------------
        ↓
SQL Server, RabbitMQ, Stripe, Gemini, Email SMTP
```

### Why This Architecture Is Useful

Each service focuses on one responsibility.

IdentityService does not need to know how interview answers are evaluated.

InterviewService does not need to know how Stripe works.

NotificationService does not need to know how marks are calculated.

This makes the backend easier to understand, maintain, and extend.

### Viva Answer

> My backend uses a microservices-style architecture. Different business capabilities are separated into services like IdentityService, InterviewService, AssessmentService, SubscriptionService, and NotificationService. API Gateway acts as the single entry point, and BuildingBlocks contains shared code.

---

## 7. API Gateway

### Simple Explanation

API Gateway is like the main reception desk of a hospital.

The patient does not directly search for every department. The reception guides them to the correct department.

Similarly, frontend does not directly call each backend service. It calls API Gateway.

### In Your Project

Your API Gateway uses Ocelot.

Important files:

```text
Backend/API_Gateway/Program.cs
Backend/API_Gateway/ocelot.json
```

### Practical Routing Example

```text
Frontend calls:
/api/auth/login

Gateway forwards to:
IdentityService
```

```text
Frontend calls:
/api/interviews/start

Gateway forwards to:
InterviewService
```

```text
Frontend calls:
/api/assessments/start

Gateway forwards to:
AssessmentService
```

```text
Frontend calls:
/api/subscriptions/subscribe

Gateway forwards to:
SubscriptionService
```

### Why We Use Gateway

Without gateway, frontend must know all service addresses:

```text
IdentityService: localhost:5005
InterviewService: localhost:5002
AssessmentService: localhost:5003
SubscriptionService: localhost:5004
```

That becomes hard to manage.

With gateway, frontend gets one entry point.

### What If We Do Not Use Gateway?

Problems:

- Frontend configuration becomes complex
- More CORS issues
- Service URLs are exposed directly
- Authentication and routing become scattered
- Deployment becomes harder

### Alternatives

- NGINX
- YARP
- Kong Gateway
- Azure API Management
- Direct frontend-to-service calls for small projects

### Viva Answer

> API Gateway is the single entry point for frontend requests. In my project, Ocelot routes requests from the frontend to the correct backend service, such as IdentityService, InterviewService, AssessmentService, or SubscriptionService.

---

## 8. IdentityService

### Simple Explanation

IdentityService answers the question:

```text
Who is this user?
```

It handles user identity and access.

### Responsibilities

- Register user
- Login user
- Generate JWT token
- Forgot password OTP
- Reset password
- Get user profile
- Update profile
- Manage roles
- Manage premium flag
- Deactivate/reactivate users
- Admin user management

### Practical Scenario

User logs in:

```text
Frontend sends email/password
        ↓
IdentityService checks user
        ↓
BCrypt verifies password
        ↓
JWT token is generated
        ↓
Frontend stores token
```

### Important Files

```text
Backend/IdentityService/Controllers/AuthController.cs
Backend/IdentityService/Services/AuthService.cs
Backend/IdentityService/Models/User.cs
```

### Why It Is Separate

Authentication is a core responsibility. Other services should not directly manage passwords or login logic.

InterviewService should only care about interviews. AssessmentService should only care about assessments.

### Viva Answer

> IdentityService manages users, authentication, JWT token generation, roles, premium status, and admin user operations. It is separated so user identity logic remains centralized and secure.

---

## 9. InterviewService

### Simple Explanation

InterviewService handles subjective mock interviews.

Subjective means answers are written in text and cannot be checked using only A/B/C/D.

### Responsibilities

- Start interview
- Begin interview
- Generate questions
- Cache questions
- Submit answers
- Evaluate answers using Gemini
- Calculate score
- Generate feedback
- Store interview result
- Return premium detailed review

### Practical Scenario

User starts a backend developer interview:

```text
User chooses domain
        ↓
InterviewService creates interview
        ↓
Questions are generated or loaded
        ↓
User answers in text
        ↓
Gemini evaluates answers
        ↓
Score and feedback are saved
```

### Important Files

```text
Backend/InterviewService/Controllers/InterviewController.cs
Backend/InterviewService/Services/InterviewService.cs
Backend/InterviewService/Models/InterviewModels.cs
```

### Why It Is Separate

Interview flow is complex and different from MCQ assessment.

It involves:

- Subjective answers
- AI evaluation
- Feedback generation
- Follow-up questions
- Premium result breakdown

### Viva Answer

> InterviewService manages subjective mock interview sessions. It creates interviews, generates questions, accepts text answers, evaluates them using Gemini, calculates score and grade, and stores detailed feedback.

---

## 10. AssessmentService

### Simple Explanation

AssessmentService handles MCQ-based tests.

MCQ answers are objective because each question has one correct option.

### Responsibilities

- Start assessment
- Load MCQ questions
- Generate questions using Gemini if needed
- Lazy load question batches
- Submit answers
- Compare selected option with correct option
- Calculate score and grade
- Provide weak area analysis for premium users
- Allow admin question management

### Practical Scenario

User starts a C# assessment:

```text
AssessmentService creates assessment
        ↓
Questions are shown
        ↓
User selects options
        ↓
Backend compares selected answers with correct options
        ↓
Score and grade are calculated
```

### Important Files

```text
Backend/AssessmentService/Controllers/AssessmentController.cs
Backend/AssessmentService/Services/AssessmentService.cs
Backend/AssessmentService/Models/AssessmentModels.cs
```

### Difference From InterviewService

AssessmentService:

```text
Checks selected option against correct option.
```

InterviewService:

```text
Uses AI to evaluate written answers.
```

### Viva Answer

> AssessmentService handles objective MCQ tests. It loads or generates questions, checks selected options against correct answers, calculates score and grade, and provides detailed review for premium users.

---

## 11. SubscriptionService

### Simple Explanation

SubscriptionService handles premium access.

It answers:

```text
Has the user paid?
Should the user become premium?
When does premium expire?
```

### Responsibilities

- Create subscription payment session
- Support Stripe checkout
- Support simulated payment for local demo
- Confirm local payment
- Handle Stripe webhook
- Store payment records
- Create active subscription
- Cancel subscription
- Publish premium activation events

### Practical Scenario

User buys premium:

```text
User clicks subscribe
        ↓
SubscriptionService creates payment session
        ↓
Payment succeeds
        ↓
Subscription record becomes active
        ↓
RabbitMQ event is published
        ↓
IdentityService updates IsPremium
        ↓
User refreshes JWT
```

### Important Files

```text
Backend/SubscriptionService/Controllers/SubscriptionController.cs
Backend/SubscriptionService/Services/SubscriptionService.cs
Backend/SubscriptionService/Models/SubscriptionModels.cs
```

### Why Payment Must Be Verified in Backend

Frontend cannot be trusted for payment success.

A user could modify frontend code and pretend payment succeeded.

So backend waits for trusted confirmation from Stripe webhook or controlled simulated confirmation.

### Viva Answer

> SubscriptionService manages payment and premium lifecycle. It creates Stripe or simulated checkout sessions, records payments, handles webhook verification, activates subscriptions, and publishes events to update user premium status.

---

## 12. NotificationService

### Simple Explanation

NotificationService sends emails in the background.

It is not a normal API service where frontend directly sends requests.

It listens to RabbitMQ events.

### Responsibilities

- Send welcome email
- Send password reset OTP email
- Send payment success email
- Send subscription upgrade email
- Send interview completion email
- Send assessment completion email

### Practical Scenario

User registers:

```text
IdentityService registers user
        ↓
IdentityService publishes EmailRequestedEvent
        ↓
NotificationService consumes event
        ↓
Email is sent in background
```

### Important Files

```text
Backend/NotificationService/Program.cs
Backend/NotificationService/Messaging/EmailNotificationConsumer.cs
Backend/NotificationService/Services/MailKitEmailSender.cs
```

### Why Email Is Separate

Email sending can be slow or fail temporarily.

Registration should not fail just because email service is slow.

So email is handled asynchronously.

### Viva Answer

> NotificationService is a background worker that consumes email events from RabbitMQ and sends emails using MailKit. It keeps email processing separate from main business APIs.

---

## 13. BuildingBlocks

### Simple Explanation

BuildingBlocks contains common backend code reused by many services.

Instead of copying the same code everywhere, your project keeps shared logic in one place.

### Responsibilities

- Standard API responses
- Standard error responses
- Custom exceptions
- Global exception middleware
- RabbitMQ publisher
- Queue names
- Shared service registration extensions

### Important Files

```text
Backend/BuildingBlocks/Contracts/ApiResponse.cs
Backend/BuildingBlocks/Contracts/ApiErrorResponse.cs
Backend/BuildingBlocks/Middleware/GlobalExceptionMiddleware.cs
Backend/BuildingBlocks/Messaging/RabbitMqPublisher.cs
Backend/BuildingBlocks/Messaging/QueueNames.cs
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
```

### Practical Example

All services need consistent error responses.

Instead of every service writing separate exception handling code, they use:

```csharp
app.UseGlobalExceptionHandling();
```

### Why It Is Useful

- Reduces duplicate code
- Keeps responses consistent
- Makes shared messaging code reusable
- Makes maintenance easier

### Viva Answer

> BuildingBlocks is a shared library that contains common contracts, exception handling, RabbitMQ messaging, and extension methods used by multiple backend services.

---

## 14. Monolith vs Microservices

### Monolith

A monolith means the whole backend is one application.

Example:

```text
One backend handles login, interviews, assessments, payments, emails, and admin.
```

### Microservices

Microservices split backend into smaller services.

Example:

```text
IdentityService handles users
InterviewService handles interviews
AssessmentService handles tests
SubscriptionService handles payment
NotificationService handles emails
```

### Your Project Style

Your project follows a microservices-style structure.

Each service is separate and has its own responsibility.

### Why This Is Useful

- Better separation of concerns
- Easier to understand service-wise
- Easier to test individual services
- Easier to scale features separately
- Easier to modify one feature without touching all others

### What If Project Was Monolithic?

Advantages:

- Easier initial development
- Easier deployment
- Less inter-service complexity

Disadvantages:

- Code becomes large over time
- Features become tightly coupled
- Payment changes may affect unrelated modules
- Email delay may slow user registration
- Harder to scale only one part

### Balanced Viva Answer

> A monolith is simpler for small applications, but my project uses microservices-style separation because identity, interviews, assessments, subscriptions, and notifications are different business capabilities. This improves maintainability and separation of concerns.

---

## 15. Important End-to-End Project Flows

## Flow 1: Login Flow

```text
Angular frontend
        ↓
POST /api/auth/login
        ↓
API Gateway
        ↓
IdentityService AuthController
        ↓
AuthService
        ↓
Database user lookup
        ↓
BCrypt password verification
        ↓
JWT token generation
        ↓
Response returned to frontend
```

### Viva Explanation

> During login, the frontend sends email and password to the API Gateway. The gateway routes the request to IdentityService. AuthService verifies the password using BCrypt, generates a JWT token with user claims, and returns it to the frontend.

---

## Flow 2: Start Assessment Flow

```text
Angular frontend
        ↓
POST /api/assessments/start with JWT
        ↓
API Gateway
        ↓
AssessmentService
        ↓
Reads userId and isPremium from JWT
        ↓
Checks free/premium limit
        ↓
Fetches or generates MCQ questions
        ↓
Saves assessment in database
        ↓
Returns questions to frontend
```

### Viva Explanation

> AssessmentService starts an MCQ test by reading the authenticated user from JWT claims, checking access limits, loading or generating questions, saving the assessment state, and returning questions to the frontend.

---

## Flow 3: Submit Interview Flow

```text
Angular frontend
        ↓
POST /api/interviews/submit
        ↓
API Gateway
        ↓
InterviewService
        ↓
Fetches interview and questions
        ↓
Stores submitted answers
        ↓
Gemini evaluates answers
        ↓
Score, grade, and feedback are saved
        ↓
Completion email event is published
        ↓
Result returned to frontend
```

### Viva Explanation

> InterviewService handles subjective answer submission. It saves user answers, sends them to Gemini for evaluation, calculates score and grade, saves feedback, publishes completion events, and returns the result.

---

## Flow 4: Premium Subscription Flow

```text
Angular frontend
        ↓
POST /api/subscriptions/subscribe
        ↓
API Gateway
        ↓
SubscriptionService
        ↓
Creates Stripe or simulated payment session
        ↓
Payment succeeds
        ↓
Subscription becomes active
        ↓
RabbitMQ event is published
        ↓
IdentityService updates IsPremium
        ↓
Frontend calls refresh-claims
        ↓
New JWT contains isPremium = true
```

### Viva Explanation

> Premium subscription is handled by SubscriptionService. After successful payment, it activates the subscription and publishes an event. IdentityService updates the user's premium flag, and the frontend refreshes JWT claims to get the updated premium status.

---

## 16. Technologies Used in This Backend

| Technology | Role in Project |
|---|---|
| ASP.NET Core | Builds backend Web APIs |
| C# | Backend programming language |
| SQL Server | Stores persistent data |
| Entity Framework Core | ORM for database operations |
| Ocelot | API Gateway and routing |
| JWT | Stateless authentication |
| BCrypt | Password hashing |
| RabbitMQ | Asynchronous event communication |
| Stripe | Payment processing |
| Gemini API | AI question generation and answer evaluation |
| MailKit | Email sending |
| Swagger | API documentation and testing |
| xUnit | Backend unit testing |
| Polly | Retry/resilience handling |

---

## 17. Why Backend Is Trusted More Than Frontend

Frontend runs in the user's browser.

That means a user can inspect frontend code, modify requests, and try to bypass rules.

Backend runs on the server and controls the real business decisions.

### Examples

Frontend should not decide:

```text
User is premium
Payment is successful
Answer is correct
User is admin
Assessment belongs to this user
```

Backend must decide these things.

### Viva Answer

> Frontend cannot be trusted for business-critical decisions because it runs in the user's browser. Backend validates authentication, authorization, payment status, scoring, and database operations securely on the server.

---

## 18. Best Full Viva Answer for Topic 1

> My project backend is a microservices-style ASP.NET Core Web API system. Angular frontend sends requests to an Ocelot API Gateway, which routes them to separate backend services. IdentityService manages users, authentication, JWT tokens, roles, and premium status. InterviewService handles subjective mock interviews and uses Gemini for answer evaluation. AssessmentService manages MCQ assessments and direct scoring. SubscriptionService handles Stripe or simulated payments and premium activation. NotificationService sends emails asynchronously using RabbitMQ events. BuildingBlocks contains shared contracts, exceptions, middleware, and messaging code. This architecture improves separation of concerns, maintainability, scalability, and security.

---

## 19. Common Viva Questions and Answers

### Q1. What is backend?

Backend is the server-side part of an application that handles business logic, database operations, authentication, authorization, payments, and API responses.

### Q2. Why do we need backend if frontend exists?

Frontend is only the user interface and runs in the browser. Backend is needed to securely validate users, protect data, apply business rules, and communicate with the database.

### Q3. What is client-server architecture?

Client-server architecture means the client sends requests and the server processes them. In this project, Angular is the client and ASP.NET Core services are the server.

### Q4. What is API Gateway?

API Gateway is the single entry point for frontend requests. It routes requests to the correct backend service.

### Q5. Why did you use multiple services?

Because different business features have different responsibilities. Identity, interviews, assessments, payments, and notifications are separated to improve maintainability and clarity.

### Q6. What is the difference between InterviewService and AssessmentService?

InterviewService handles subjective text-based answers and AI evaluation. AssessmentService handles MCQ answers and checks selected options against correct options.

### Q7. Why is NotificationService separate?

Email sending can be slow or fail temporarily, so it is handled asynchronously in a separate background service using RabbitMQ.

### Q8. What is BuildingBlocks?

BuildingBlocks is a shared library containing common response models, exceptions, global middleware, RabbitMQ publisher, queue names, and service extensions.

### Q9. What happens if API Gateway is removed?

Frontend would need to call every service directly using different ports and URLs. This would increase complexity, CORS issues, and deployment difficulty.

### Q10. Is your project monolithic or microservices?

It follows a microservices-style architecture because major features are separated into independent backend services.

---

## 20. Quick Revision Summary

- Backend handles server-side logic.
- Angular frontend sends requests to backend.
- API Gateway is the single entry point.
- IdentityService handles users and JWT.
- InterviewService handles subjective interviews.
- AssessmentService handles MCQ tests.
- SubscriptionService handles payments and premium.
- NotificationService sends emails in background.
- BuildingBlocks stores shared code.
- SQL Server stores data.
- RabbitMQ handles async events.
- Stripe handles payments.
- Gemini handles AI generation/evaluation.

