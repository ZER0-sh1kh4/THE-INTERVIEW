# Topic 6: Authentication and Authorization

Project: Mock Interview Platform  
Focus: Understanding login identity, user permissions, JWT-based access, role checks, premium checks, protected APIs, public APIs, and admin-only APIs.

---

## 1. Why This Topic Is Important

### Simple Explanation

Authentication and authorization decide:

```text
Who is the user?
What is the user allowed to do?
```

In your project, this is extremely important because different users have different access.

Example:

- A guest can register and login.
- A logged-in candidate can start assessments and interviews.
- A free user has limited attempts.
- A premium user has more access.
- An admin can manage users and questions.
- A deactivated user should not login.

### Practical Scenario

Suppose a normal candidate tries to access:

```text
GET /api/auth/admin/users
```

The backend should block it.

Why?

Because the user is authenticated, but not authorized as Admin.

### Viva Answer

> Authentication and authorization are important because they protect backend APIs. Authentication verifies the user's identity, while authorization checks what actions that user is allowed to perform.

---

## 2. Authentication

### Simple Explanation

Authentication means verifying who the user is.

Question:

```text
Are you really this user?
```

### Real-Life Analogy

When you enter an exam hall, the invigilator checks your ID card.

That is authentication.

They confirm:

```text
This student is really Shobhit.
```

### In Your Project

Authentication happens during login.

User sends:

```text
email
password
```

Backend checks:

```text
Does this email exist?
Does password match stored hash?
Is account active?
```

If all correct, backend returns JWT token.

### Code Area

Important files:

```text
Backend/IdentityService/Controllers/AuthController.cs
Backend/IdentityService/Services/AuthService.cs
```

### Login Method Flow

```text
1. User sends email and password.
2. AuthController receives LoginRequest.
3. AuthService finds user by email.
4. BCrypt verifies password.
5. Service checks IsActive.
6. JWT token is generated.
7. Token is returned to frontend.
```

### Viva Answer

> Authentication verifies user identity. In my project, login authenticates the user by checking email and password using BCrypt, then returns a JWT token.

---

## 3. Authorization

### Simple Explanation

Authorization means checking what the authenticated user is allowed to access.

Question:

```text
Are you allowed to do this action?
```

### Real-Life Analogy

In a college:

- Student can enter classroom.
- Teacher can enter staff room.
- Admin can access office records.

All are authenticated people, but their permissions are different.

### In Your Project

Authorization controls:

- Candidate access
- Admin access
- Premium/free access
- Ownership access

### Examples

Logged-in user can:

```text
GET /api/auth/me
POST /api/interviews/start
POST /api/assessments/start
```

Admin can:

```text
GET /api/auth/admin/users
POST /api/assessments/questions
DELETE /api/assessments/questions/{id}
```

Free user cannot:

```text
Create unlimited interviews
Create unlimited assessments
View premium detailed result
```

### Viva Answer

> Authorization checks permissions after authentication. In my project, role-based authorization protects admin APIs, and premium/free authorization controls access to interview and assessment limits.

---

## 4. Authentication vs Authorization

### Simple Difference

| Concept | Question | Example |
|---|---|---|
| Authentication | Who are you? | Login with email/password |
| Authorization | What can you access? | Admin can manage users |

### Practical Example

A candidate logs in successfully.

Authentication:

```text
Yes, this user is valid.
```

Authorization:

```text
No, this user cannot access admin user list.
```

### In Your Project

Authentication:

```text
JWT token verifies user identity.
```

Authorization:

```text
[Authorize]
[Authorize(Roles = "Admin")]
Premium checks using isPremium claim
Ownership checks using userId
```

### Viva Answer

> Authentication confirms identity, while authorization checks access rights. A user may be authenticated but still not authorized to access admin or premium features.

---

## 5. User Identity in This Project

### User Model

Important file:

```text
Backend/IdentityService/Models/User.cs
```

User has:

```csharp
public int Id { get; set; }
public string FullName { get; set; }
public string Email { get; set; }
public string PasswordHash { get; set; }
public string Role { get; set; }
public bool IsPremium { get; set; }
public bool IsActive { get; set; }
public DateTime CreatedAt { get; set; }
```

### What Each Field Means

| Field | Meaning |
|---|---|
| Id | Unique user id |
| FullName | User display name |
| Email | Login identifier |
| PasswordHash | Hashed password |
| Role | Candidate or Admin |
| IsPremium | Whether user has premium access |
| IsActive | Whether account can login |
| CreatedAt | User creation timestamp |

### Why User Model Is Important

IdentityService uses this model to decide:

- Can user login?
- What role does user have?
- Is user premium?
- Is account active?
- What claims should JWT contain?

### Viva Answer

> The User model stores identity information such as email, hashed password, role, premium status, and active status. These fields are used during authentication and authorization.

---

## 6. Register Flow

### Simple Explanation

Register means creating a new user account.

### Endpoint

```text
POST /api/auth/register
```

### Request

```json
{
  "fullName": "Shobhit",
  "email": "shobhit@example.com",
  "password": "123456"
}
```

### Backend Steps

```text
1. AuthController receives RegisterRequest.
2. AuthService checks if email already exists.
3. Password is hashed using BCrypt.
4. New User object is created.
5. Default role is Candidate.
6. IsPremium is false.
7. IsActive is true.
8. User is saved in database.
9. JWT token is generated.
10. UserRegisteredEvent is published.
11. Welcome email event is published.
12. Response is returned.
```

### Why Default Role Is Candidate

Most users are normal candidates.

Admin access should not be given automatically.

### Why IsPremium Is False

User has not paid yet.

Premium becomes true only after subscription/payment flow or admin update.

### Viva Answer

> During registration, the backend creates a user with hashed password, Candidate role, non-premium status, and active account. It returns JWT and publishes registration/email events.

---

## 7. Login Flow

### Endpoint

```text
POST /api/auth/login
```

### Request

```json
{
  "email": "shobhit@example.com",
  "password": "123456"
}
```

### Backend Steps

```text
1. AuthController receives LoginRequest.
2. AuthService searches user by email.
3. If user not found, return invalid credentials.
4. BCrypt verifies entered password with stored PasswordHash.
5. If password is wrong, return invalid credentials.
6. If account is inactive, return forbidden error.
7. JWT token is generated.
8. Response returns token and user id.
```

### Why Error Says "Invalid Credentials"

If backend says:

```text
Email not found
```

attackers can guess registered emails.

Generic message is safer.

### Viva Answer

> During login, IdentityService verifies email and password using BCrypt. If valid and active, it creates a JWT token containing user claims and returns it to frontend.

---

## 8. Password Hashing in Authentication

### Simple Explanation

Password should never be stored as plain text.

Your project stores:

```text
PasswordHash
```

not:

```text
Password
```

### In Your Project

Registration:

```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
```

Login:

```csharp
BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
```

### Why This Matters

If database leaks and passwords are plain text, all users are compromised.

With hashing, actual password is not directly visible.

### Authentication Role

Password hashing helps authentication securely verify:

```text
Does entered password match original password?
```

without storing original password.

### Viva Answer

> Password hashing protects user passwords. My project uses BCrypt to hash passwords during registration and verify passwords during login.

---

## 9. JWT Token After Login

### Simple Explanation

After successful login, backend gives JWT token.

JWT works like a digital ID card.

Frontend sends this token with future requests.

### Header

```text
Authorization: Bearer jwt_token_here
```

### Token Contains Claims

Your project includes:

```text
NameIdentifier = user id
Email = user email
Role = user role
isPremium = user premium status
FullName = user full name
```

### Why JWT Is Needed

Without token, backend cannot know who is calling protected APIs.

### Practical Scenario

User starts assessment:

```text
POST /api/assessments/start
Authorization: Bearer token
```

AssessmentService reads token and knows:

```text
userId = 5
isPremium = false
role = Candidate
```

### Viva Answer

> JWT is returned after login and sent with protected requests. It contains claims like user id, email, role, premium status, and full name.

---

## 10. Claims

### Simple Explanation

Claims are pieces of information about the user stored inside JWT.

### In Your Project

Claims created in AuthService:

```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
new Claim(ClaimTypes.Email, user.Email)
new Claim(ClaimTypes.Role, user.Role)
new Claim("isPremium", user.IsPremium.ToString().ToLower())
new Claim("FullName", user.FullName ?? "")
```

### How Services Use Claims

InterviewService reads:

```text
userId
isPremium
email
```

AssessmentService reads:

```text
userId
isPremium
email
```

SubscriptionService reads:

```text
userId
email
```

Admin APIs read:

```text
Role = Admin
```

### Why Claims Are Useful

Services do not need to call IdentityService for every request.

They can read user information from token.

### Viva Answer

> Claims are user details stored in JWT. My project uses claims for user id, email, role, premium status, and full name so services can identify the current user.

---

## 11. Public APIs

### Simple Explanation

Public APIs do not require login.

### In Your Project

Public APIs:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password/request-otp
POST /api/auth/forgot-password/reset
POST /api/subscriptions/webhook/stripe
```

### In Code

Public endpoint uses:

```csharp
[AllowAnonymous]
```

### Why Register/Login Are Public

User does not have token before registration or login.

### Why Webhook Is Public

Stripe calls webhook, not a logged-in user.

It is protected by Stripe signature instead of JWT.

### Viva Answer

> Public APIs are accessible without JWT. Register and login are public because users do not have tokens yet, and Stripe webhook is public because Stripe calls it directly.

---

## 12. Protected APIs

### Simple Explanation

Protected APIs require valid JWT token.

### In Code

Protected endpoint uses:

```csharp
[Authorize]
```

### In Your Project

Examples:

```text
GET /api/auth/me
PUT /api/auth/me
POST /api/auth/refresh-claims
POST /api/interviews/start
GET /api/interviews
POST /api/assessments/start
POST /api/subscriptions/subscribe
GET /api/subscriptions/my
```

### Practical Scenario

If user calls:

```text
GET /api/interviews
```

without token, backend returns:

```text
401 Unauthorized
```

### Why Protected APIs Are Needed

Interviews, assessments, subscriptions, and profile data belong to a specific user.

Backend must know who is calling.

### Viva Answer

> Protected APIs require JWT token. In my project, user-specific APIs like interviews, assessments, profile, and subscriptions are protected using [Authorize].

---

## 13. Admin APIs

### Simple Explanation

Admin APIs require logged-in user with Admin role.

### In Code

```csharp
[Authorize(Roles = "Admin")]
```

### In Your Project

Admin endpoints:

```text
GET /api/auth/admin/users
PUT /api/auth/admin/users/{id}/role
PUT /api/auth/admin/users/{id}/premium
PUT /api/auth/admin/users/{id}/deactivate
PUT /api/auth/admin/users/{id}/reactivate
GET /api/interviews/admin/all
GET /api/assessments/admin/all
GET /api/assessments/questions
POST /api/assessments/questions
PUT /api/assessments/questions/{id}
DELETE /api/assessments/questions/{id}
GET /api/subscriptions/all
GET /api/subscriptions/payments
```

### Practical Scenario

Candidate tries to delete MCQ question:

```text
DELETE /api/assessments/questions/10
```

Backend checks role.

If role is not Admin:

```text
403 Forbidden
```

### Why Admin APIs Are Protected

Admin operations are powerful.

They can:

- View all users
- Change roles
- Change premium status
- Manage question bank
- View all results
- View all payments

Normal users should not access them.

### Viva Answer

> Admin APIs use role-based authorization with [Authorize(Roles = "Admin")]. This ensures only users with Admin role can manage users, questions, results, and payments.

---

## 14. Role-Based Authorization

### Simple Explanation

Role-based authorization checks the user's role.

### Roles in Your Project

Main roles:

```text
Candidate
Admin
```

Default registration role:

```text
Candidate
```

Admin role can be assigned by admin endpoint.

### JWT Role Claim

JWT contains:

```csharp
new Claim(ClaimTypes.Role, user.Role)
```

ASP.NET Core uses this claim for:

```csharp
[Authorize(Roles = "Admin")]
```

### Viva Answer

> Role-based authorization allows access based on user role. My JWT contains a role claim, and admin endpoints require the Admin role.

---

## 15. Premium Authorization

### Simple Explanation

Premium authorization checks whether user has premium access.

This is not exactly role-based authorization. It is business-rule authorization.

### In Your Project

JWT contains:

```text
isPremium = true/false
```

Services read it:

```csharp
var isPremiumStr = User.FindFirst("isPremium")?.Value;
var isPremium = string.Equals(isPremiumStr, "true", StringComparison.OrdinalIgnoreCase);
```

### Where Premium Is Used

InterviewService:

```text
Free user can create only 1 interview.
Premium user can create more.
```

AssessmentService:

```text
Free user can create only 2 assessments.
Premium user can create more.
```

Result detail:

```text
Free user gets summary.
Premium user gets detailed breakdown.
```

### Practical Scenario

Free user starts second assessment:

```text
Backend checks total attempts.
If attempts >= 2, throw ForbiddenAppException.
```

### Why Premium Uses Claim

Services can quickly know user's premium status from JWT.

### Viva Answer

> Premium access is controlled using the isPremium claim in JWT. Services use this claim to apply free-user limits and premium result features.

---

## 16. Ownership Authorization

### Simple Explanation

Ownership authorization checks whether the resource belongs to the current user.

Even if a user is logged in, they should not access another user's interview or assessment.

### Practical Scenario

User 5 tries:

```text
GET /api/assessments/20/result
```

But assessment 20 belongs to user 8.

Backend should block it.

### In Your Project

AssessmentService checks:

```csharp
if (assessment.UserId != userId)
{
    throw new ForbiddenAppException("You are not allowed to view this assessment result.");
}
```

InterviewService queries by both:

```text
InterviewId
UserId
```

Example:

```csharp
FirstOrDefaultAsync(i => i.Id == interviewId && i.UserId == userId)
```

### Why Ownership Check Is Needed

JWT proves who the user is, but service must still check if requested data belongs to that user.

### Viva Answer

> Ownership authorization ensures users can access only their own data. My project checks userId from JWT against resource owner userId for interviews and assessments.

---

## 17. IsActive User Flag

### Simple Explanation

`IsActive` decides whether user account is active or deactivated.

### In Login

If user is inactive:

```text
Login is blocked.
```

### Code Logic

```csharp
if (!user.IsActive)
{
    throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
}
```

### Why This Is Useful

Admin can deactivate account without deleting user data.

This preserves:

- History
- Results
- Payments
- Audit information

### Viva Answer

> IsActive allows admin to deactivate a user without deleting their data. Inactive users cannot login.

---

## 18. 401 vs 403

### Simple Explanation

These are commonly asked in viva.

| Status | Meaning |
|---|---|
| 401 Unauthorized | User is not authenticated |
| 403 Forbidden | User is authenticated but not allowed |

### Practical Examples

No token:

```text
GET /api/interviews
```

Response:

```text
401 Unauthorized
```

Candidate token accessing admin API:

```text
GET /api/auth/admin/users
```

Response:

```text
403 Forbidden
```

Free user exceeding limit:

```text
POST /api/interviews/start
```

Response:

```text
403 Forbidden
```

### Viva Answer

> 401 means the user is not authenticated or token is invalid. 403 means the user is authenticated but does not have permission for that action.

---

## 19. [Authorize] Attribute

### Simple Explanation

`[Authorize]` protects controller or action endpoints.

### Controller-Level Authorization

In InterviewController:

```csharp
[ApiController]
[Route("api/interviews")]
[Authorize]
public class InterviewController : ControllerBase
{
}
```

This means all interview endpoints require login unless overridden.

### Action-Level Authorization

Admin action:

```csharp
[HttpGet("admin/all")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetAllInterviews()
```

### Viva Answer

> [Authorize] ensures only authenticated users can access an endpoint. It can be applied to a whole controller or specific actions.

---

## 20. [AllowAnonymous] Attribute

### Simple Explanation

`[AllowAnonymous]` allows endpoint access without login.

### In Your Project

```csharp
[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login(...)
```

```csharp
[AllowAnonymous]
[HttpPost("webhook/stripe")]
public async Task<IActionResult> HandleStripeWebhook()
```

### Why It Is Needed

Some endpoints must be accessible before authentication.

Examples:

- Register
- Login
- Forgot password
- Stripe webhook

### Viva Answer

> [AllowAnonymous] marks endpoints that do not require authentication, such as login, register, forgot-password, and Stripe webhook.

---

## 21. Reading Current User from JWT

### Simple Explanation

After JWT validation, ASP.NET Core makes user claims available through:

```csharp
User
```

### In Your Project

InterviewController:

```csharp
private (int userId, bool isPremium, string email) GetUserDetails()
{
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var isPremiumStr = User.FindFirst("isPremium")?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    int.TryParse(userIdStr, out var userId);
    var isPremium = string.Equals(isPremiumStr, "true", StringComparison.OrdinalIgnoreCase);
    return (userId, isPremium, email);
}
```

### Why This Is Used

Controller needs current user information for:

- User-specific data
- Premium/free rules
- Email events
- Ownership checks

### Viva Answer

> After JWT validation, controllers can read user claims from the User object. My project reads userId, email, role, and isPremium from JWT claims.

---

## 22. Refresh Claims

### Simple Explanation

JWT contains old user data until a new token is issued.

If user becomes premium after payment, old JWT may still say:

```text
isPremium = false
```

So frontend must refresh claims.

### Endpoint

```text
POST /api/auth/refresh-claims
```

### Flow

```text
1. User pays for premium.
2. SubscriptionService publishes event.
3. IdentityService updates IsPremium = true.
4. Frontend calls refresh-claims.
5. IdentityService creates new JWT.
6. New JWT has isPremium = true.
```

### Why Needed

JWT is stateless and already issued. It does not automatically update when database changes.

### Viva Answer

> Refresh-claims generates a new JWT after profile or premium status changes. It is needed because old JWT claims do not automatically update.

---

## 23. Internal API Key Authorization

### Simple Explanation

Some endpoints are not for frontend users. They are for internal service communication.

Your project has an internal premium update endpoint:

```text
PUT /api/auth/internal/users/{id}/premium
```

It uses:

```text
X-Internal-Key
```

### In Code

```csharp
if (!Request.Headers.TryGetValue("X-Internal-Key", out var apiKey) 
    || apiKey != _configuration["InternalApiKey"])
{
    return Unauthorized(...);
}
```

### Why This Is Used

SubscriptionService may need to update user premium status in IdentityService.

This should not be open publicly.

### Important Gateway Note

Your gateway intentionally blocks:

```text
/api/auth/internal/{everything}
```

by routing it to a not-found downstream route.

This protects internal endpoints from normal gateway access.

### Viva Answer

> Internal API key authorization protects service-to-service endpoints. My project uses X-Internal-Key for internal premium updates, and the gateway blocks public access to internal auth routes.

---

## 24. Authentication Configuration in Program.cs

### Simple Explanation

Each protected service must know how to validate JWT.

### In Program.cs

Services configure JWT Bearer authentication:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

### What This Checks

- Token came from correct issuer
- Token is for correct audience
- Token has not expired
- Token signature is valid
- Signing key matches

### Why ClockSkew Is Zero

Default token validation may allow a few extra minutes.

`ClockSkew = TimeSpan.Zero` makes expiry strict.

### Viva Answer

> JWT authentication is configured in Program.cs. It validates issuer, audience, lifetime, signing key, and token signature before allowing protected API access.

---

## 25. Authentication Middleware Order

### Correct Order

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### Why Order Matters

Authentication creates the user identity.

Authorization checks permissions based on that identity.

If order is reversed:

```text
Authorization runs before knowing who the user is.
```

### Viva Answer

> Authentication middleware must run before authorization because the system must identify the user before checking permissions.

---

## 26. Complete Flow: Protected API Access

### Example

User calls:

```text
POST /api/interviews/start
Authorization: Bearer jwt_token
```

### Flow

```text
1. Angular sends request with JWT.
2. API Gateway validates JWT.
3. Gateway forwards request to InterviewService.
4. InterviewService validates JWT again.
5. [Authorize] allows access.
6. Controller reads userId and isPremium claims.
7. Service checks free/premium interview limit.
8. Interview is created.
9. Response returns to frontend.
```

### Viva Answer

> Protected APIs require JWT. The gateway validates token first, then the service validates it again, and the controller uses claims to perform user-specific logic.

---

## 27. Complete Flow: Admin API Access

### Example

Admin calls:

```text
GET /api/auth/admin/users
Authorization: Bearer admin_jwt
```

### Flow

```text
1. Gateway validates JWT.
2. IdentityService validates JWT.
3. ASP.NET Core checks [Authorize(Roles = "Admin")].
4. Role claim is read from token.
5. If role is Admin, action runs.
6. AuthService returns all users.
```

### Candidate Tries Same API

```text
Role claim = Candidate
```

Result:

```text
403 Forbidden
```

### Viva Answer

> Admin APIs check the role claim in JWT. If the role is Admin, access is allowed. If the user is only Candidate, the request is rejected with 403 Forbidden.

---

## 28. Complete Flow: Premium Access

### Example

Free user starts interview:

```text
POST /api/interviews/start
```

### Flow

```text
1. Controller reads isPremium claim.
2. If isPremium is false, service counts previous interviews.
3. If free limit is reached, ForbiddenAppException is thrown.
4. If allowed, interview is created.
```

### Code Idea

```text
Free user limit:
InterviewService = 1 interview
AssessmentService = 2 assessments
```

### Viva Answer

> Premium access is enforced using the isPremium claim. Free users have attempt limits, while premium users can access more attempts and detailed result breakdowns.

---

## 29. What Happens If Authentication Is Missing?

### Problems

- Anyone can access user profile
- Anyone can view interviews
- Anyone can submit assessment as another user
- Anyone can subscribe or cancel subscription
- Admin data can leak

### Example

Without authentication:

```text
GET /api/interviews
```

could expose interview history without knowing the user.

### Viva Answer

> Without authentication, backend cannot know who is calling, so user-specific data and actions become insecure.

---

## 30. What Happens If Authorization Is Missing?

### Problems

Authenticated users may access things they should not.

Examples:

- Candidate views all users
- Candidate deletes questions
- Free user gets unlimited attempts
- User views another user's result

### Viva Answer

> Without authorization, even logged-in users could access restricted actions like admin APIs, premium features, or other users' data.

---

## 31. Security Strengths in This Project

### Implemented Strengths

- BCrypt password hashing
- JWT authentication
- Role-based authorization
- Premium claim checks
- Ownership checks
- IsActive account blocking
- Internal API key for internal endpoint
- Stripe signature for webhook
- Generic invalid credentials message
- Token expiry validation
- Gateway-level and service-level JWT validation

### Viva Answer

> My project applies multiple security layers: BCrypt for passwords, JWT for authentication, roles for admin authorization, premium claims for feature limits, ownership checks for user data, and signature/API key checks for special endpoints.

---

## 32. Possible Improvements

### Improvements

- Use refresh tokens instead of only refresh-claims
- Store JWT securely using HttpOnly cookies
- Add account lockout after repeated failed login
- Add email verification
- Add password strength policy
- Add MFA for admin users
- Add centralized identity provider
- Add OAuth login with Google/Microsoft
- Add permission-based authorization beyond roles
- Add audit logs for admin actions

### Balanced Viva Answer

> Current authentication and authorization use JWT, BCrypt, roles, and premium claims. Future improvements could include refresh tokens, MFA, account lockout, email verification, and audit logging.

---

## 33. Best Full Viva Answer for Topic 6

> Authentication verifies who the user is, while authorization checks what the user can access. In my project, IdentityService handles registration, login, password hashing with BCrypt, and JWT generation. After login, the frontend sends JWT in the Authorization header. The API Gateway and backend services validate the token. JWT contains claims like userId, email, role, isPremium, and FullName. Protected APIs use [Authorize], admin APIs use [Authorize(Roles = "Admin")], and public APIs use [AllowAnonymous]. Premium access is controlled using the isPremium claim, and ownership checks ensure users can access only their own interviews and assessments. This protects user data, admin operations, and premium features.

---

## 34. Common Viva Questions and Answers

### Q1. What is authentication?

Authentication verifies the user's identity. In my project, login authenticates user by checking email and password.

### Q2. What is authorization?

Authorization checks whether an authenticated user is allowed to perform an action.

### Q3. Difference between authentication and authorization?

Authentication asks who the user is. Authorization asks what the user is allowed to access.

### Q4. What is JWT used for?

JWT is used to identify logged-in users in protected API requests.

### Q5. What claims are stored in JWT?

User id, email, role, premium status, and full name.

### Q6. Why use [Authorize]?

To protect APIs so only authenticated users can access them.

### Q7. Why use [AllowAnonymous]?

To allow public access to endpoints like register, login, forgot password, and Stripe webhook.

### Q8. How are admin APIs protected?

Using role-based authorization with `[Authorize(Roles = "Admin")]`.

### Q9. What is 401?

401 means user is not authenticated or token is invalid/missing.

### Q10. What is 403?

403 means user is authenticated but not allowed to perform the action.

### Q11. Why is password hashed?

To avoid storing plain passwords and protect users if database is leaked.

### Q12. What is IsActive?

IsActive controls whether a user account can login. Deactivated users are blocked.

### Q13. How is premium access controlled?

Using isPremium claim in JWT and business checks in services.

### Q14. Why refresh claims after payment?

Because old JWT still has old premium status. Refresh claims creates a new token with updated data.

### Q15. How do you prevent users from viewing others' results?

By checking the userId from JWT against the userId stored on the assessment or interview.

---

## 35. Quick Revision Summary

- Authentication means verifying identity.
- Authorization means checking permissions.
- Login authenticates user with email and password.
- BCrypt verifies password hash.
- JWT is issued after successful login.
- JWT is sent using Authorization Bearer header.
- Claims store userId, email, role, isPremium, and FullName.
- `[Authorize]` protects logged-in APIs.
- `[AllowAnonymous]` allows public APIs.
- `[Authorize(Roles = "Admin")]` protects admin APIs.
- Candidate is default role.
- Admin can manage users and questions.
- Premium access uses isPremium claim.
- Free users have attempt limits.
- Ownership checks protect user-specific data.
- 401 means unauthenticated.
- 403 means authenticated but forbidden.
- Refresh claims updates JWT after premium/profile changes.

