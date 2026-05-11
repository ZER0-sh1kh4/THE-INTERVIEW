# Topic 13: IdentityService Deep Dive

Project: Mock Interview Platform  
Focus: Understanding the IdentityService in depth: registration, login, password hashing, JWT creation, claims, forgot-password OTP, profile update, admin user management, premium flag update, internal API key, notifications, and RabbitMQ event handling.

---

## 1. What Is IdentityService?

### Simple Explanation

IdentityService is the service responsible for user identity.

It answers questions like:

```text
Who is this user?
Can this user login?
What is this user's role?
Is this user premium?
Is this user active?
```

### In Your Project

IdentityService manages:

- User registration
- User login
- JWT token generation
- Current user claims
- Password reset OTP
- Profile update
- Role update
- Premium flag update
- Account activation/deactivation
- User notifications
- Premium update events from SubscriptionService

### Viva Answer

> IdentityService is responsible for authentication, user profile, roles, premium status, account activation, JWT claims, password reset, and user notifications.

---

## 2. Why IdentityService Is Separate

### Simple Explanation

User identity is a core responsibility.

Other services should not directly manage passwords, roles, or login.

### Service Responsibility

```text
IdentityService -> Users, login, JWT, roles, premium flag
InterviewService -> Interviews and subjective results
AssessmentService -> MCQ assessments and results
SubscriptionService -> Payment and subscription lifecycle
NotificationService -> Email sending
```

### Why This Is Good

- Clear ownership
- Better security
- Less duplicated user logic
- Other services can trust JWT claims
- Password handling stays in one place

### Viva Answer

> IdentityService is separate because authentication and user management are security-sensitive responsibilities. Other services use JWT claims instead of directly handling passwords or login.

---

## 3. Important IdentityService Files

### Program and Configuration

```text
Backend/IdentityService/Program.cs
Backend/IdentityService/appsettings.Example.json
```

### Controllers

```text
Backend/IdentityService/Controllers/AuthController.cs
Backend/IdentityService/Controllers/NotificationController.cs
```

### Services

```text
Backend/IdentityService/Services/IAuthService.cs
Backend/IdentityService/Services/AuthService.cs
```

### Data and Models

```text
Backend/IdentityService/Data/AppDbContext.cs
Backend/IdentityService/Models/User.cs
Backend/IdentityService/Models/Notification.cs
```

### DTOs

```text
Backend/IdentityService/DTOs/AuthDTOs.cs
```

### Messaging Consumers

```text
Backend/IdentityService/Messaging/SubscriptionEventConsumer.cs
Backend/IdentityService/Messaging/UserNotificationEventConsumer.cs
```

### Viva Answer

> Important IdentityService files include AuthController, NotificationController, AuthService, IAuthService, User model, Notification model, AppDbContext, AuthDTOs, and messaging consumers.

---

## 4. User Model

### File

```text
Backend/IdentityService/Models/User.cs
```

### Code

```csharp
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Candidate";
    public bool IsPremium { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Meaning

The User table stores:

```text
Identity data
Login email
Hashed password
Role
Premium state
Active/deactivated state
Creation time
```

### Viva Answer

> The User model stores identity-related user data such as email, password hash, role, premium flag, active status, and creation date.

---

## 5. AppDbContext in IdentityService

### File

```text
Backend/IdentityService/Data/AppDbContext.cs
```

### DbSets

```csharp
public DbSet<User> Users { get; set; }
public DbSet<Notification> Notifications { get; set; }
```

### Meaning

IdentityService owns two tables:

```text
Users
Notifications
```

### Viva Answer

> IdentityService AppDbContext manages Users and Notifications tables.

---

## 6. Seed Users

### Simple Explanation

Seeding means inserting initial data into database.

### In Your Project

IdentityService seeds:

```text
Admin user
Candidate user
```

### Why Useful

Demo and testing can start with ready users.

Admin can immediately login and manage users.

### Security Note

Default seeded passwords should not be used in real production.

### Viva Answer

> IdentityService seeds default admin and candidate users for development and demo. In production, default credentials should be changed or avoided.

---

## 7. AuthController

### File

```text
Backend/IdentityService/Controllers/AuthController.cs
```

### Responsibility

AuthController exposes authentication and user-management endpoints.

### Main Endpoints

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password/request-otp
POST /api/auth/forgot-password/reset
GET /api/auth/me
PUT /api/auth/me
POST /api/auth/refresh-claims
GET /api/auth/admin/users
GET /api/auth/admin/users/{id}
PUT /api/auth/admin/users/{id}/role
PUT /api/auth/admin/users/{id}/premium
PUT /api/auth/admin/users/{id}/deactivate
PUT /api/auth/admin/users/{id}/reactivate
PUT /api/auth/internal/users/{id}/premium
```

### Viva Answer

> AuthController exposes IdentityService APIs for registration, login, password reset, current user claims, profile update, admin management, and internal premium update.

---

## 8. IAuthService

### File

```text
Backend/IdentityService/Services/IAuthService.cs
```

### Purpose

Defines authentication and user-management operations.

### Main Methods

```text
RegisterAsync
LoginAsync
SendForgotPasswordOtpAsync
ResetPasswordWithOtpAsync
RefreshClaimsAsync
UpdateProfileAsync
GetAllUsersAsync
GetUserByIdAsync
UpdateUserRoleAsync
UpdateUserPremiumAsync
DeactivateUserAsync
ReactivateUserAsync
```

### Viva Answer

> IAuthService defines the contract for registration, login, password reset, claims refresh, profile update, and admin user operations.

---

## 9. AuthService

### File

```text
Backend/IdentityService/Services/AuthService.cs
```

### Dependencies

```csharp
public AuthService(
    AppDbContext context,
    IConfiguration config,
    ILogger<AuthService> logger,
    IMemoryCache memoryCache)
```

### Meaning

AuthService uses:

```text
AppDbContext -> database access
IConfiguration -> JWT and OTP settings
ILogger -> logs
IMemoryCache -> temporary OTP storage
```

### Viva Answer

> AuthService contains IdentityService business logic and uses AppDbContext, IConfiguration, ILogger, and IMemoryCache through dependency injection.

---

## 10. Register Flow

### Endpoint

```text
POST /api/auth/register
```

### Request DTO

```csharp
public class RegisterRequest
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### Service Flow

```text
1. Check if user with email already exists.
2. If exists, update full name, password hash, and IsActive.
3. If not exists, create new User.
4. Hash password using BCrypt.
5. Save user in database.
6. Generate JWT token.
7. Return AuthResponse.
```

### Controller Event Flow

After registration, controller publishes:

```text
UserRegisteredEvent
EmailRequestedEvent for welcome email
```

### Viva Answer

> During registration, IdentityService checks existing email, hashes the password with BCrypt, saves the user, generates JWT, and publishes registration and welcome email events.

---

## 11. Why Password Is Hashed During Registration

### Simple Explanation

Passwords must never be stored directly.

Instead of storing:

```text
MyPassword123
```

Backend stores:

```text
BCrypt hash
```

### In Your Project

```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
```

### Viva Answer

> Passwords are hashed using BCrypt before saving so even if the database is leaked, original passwords are not directly exposed.

---

## 12. Login Flow

### Endpoint

```text
POST /api/auth/login
```

### Request DTO

```csharp
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### Service Flow

```text
1. Find user by email.
2. Verify password using BCrypt.Verify.
3. If user missing or password wrong, return 401.
4. If user is deactivated, return 403.
5. Generate JWT token.
6. Return AuthResponse.
```

### Code

```csharp
if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
{
    throw new AppException("Invalid credentials.", StatusCodes.Status401Unauthorized);
}
```

### Viva Answer

> During login, IdentityService finds the user by email, verifies password using BCrypt, checks IsActive, and returns a signed JWT token.

---

## 13. Invalid Credentials

### Simple Explanation

If email or password is wrong, backend should not reveal which one is wrong.

### In Your Project

Same message is used:

```text
Invalid credentials.
```

### Why

This avoids helping attackers discover valid emails.

### Viva Answer

> Login returns a generic invalid credentials message so attackers cannot easily know whether the email exists or only the password is wrong.

---

## 14. IsActive Flag

### Simple Explanation

`IsActive` controls whether a user account can login.

### In Your Project

If account is inactive:

```csharp
throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
```

### Used For

```text
Admin deactivation
Admin reactivation
Blocking login without deleting user data
```

### Viva Answer

> IsActive allows admins to deactivate an account without deleting user data. Deactivated users cannot login.

---

## 15. AuthResponse

### DTO

```csharp
public class AuthResponse
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```

### Returned By

```text
RegisterAsync
LoginAsync
RefreshClaimsAsync
```

### Viva Answer

> AuthResponse returns the user ID, JWT token, and message after registration, login, or claims refresh.

---

## 16. JWT Generation

### Method

```text
GenerateJwtToken(User user)
```

### Purpose

Creates a signed JWT token from the latest user data.

### Uses Configuration

```text
Jwt:SecretKey
Jwt:Issuer
Jwt:Audience
Jwt:ExpiryMinutes
```

### Viva Answer

> GenerateJwtToken creates a signed JWT using user details and JWT configuration like secret key, issuer, audience, and expiry time.

---

## 17. JWT Claims in IdentityService

### Claims

```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, user.Role),
new Claim("isPremium", user.IsPremium.ToString().ToLower()),
new Claim("FullName", user.FullName ?? "")
```

### Meaning

Token contains:

```text
User id
Email
Role
Premium status
Full name
```

### Viva Answer

> The JWT contains claims for user id, email, role, isPremium, and FullName. Other services use these claims for authorization and premium checks.

---

## 18. Why Role Is in JWT

### Simple Explanation

Role tells backend what the user is allowed to access.

### In Your Project

Admin endpoints use:

```csharp
[Authorize(Roles = "Admin")]
```

### Example

Only admin can call:

```text
GET /api/auth/admin/users
PUT /api/auth/admin/users/{id}/role
```

### Viva Answer

> Role is stored in JWT so backend can protect admin endpoints using role-based authorization.

---

## 19. Why IsPremium Is in JWT

### Simple Explanation

Other services need to know whether user has premium access.

### Used By

```text
InterviewService
AssessmentService
Subscription-related UI flows
```

### Example

Free users have limits:

```text
Free interview limit
Free assessment limit
Premium result details
```

### Viva Answer

> isPremium is stored in JWT so services can quickly check premium access without calling IdentityService for every request.

---

## 20. Why RefreshClaims Is Needed

### Simple Explanation

JWT is stateless.

Once created, old token does not automatically change.

### Scenario

```text
1. User logs in with isPremium = false.
2. User buys premium.
3. Database IsPremium becomes true.
4. Old JWT still says isPremium = false.
5. User calls refresh-claims.
6. IdentityService issues new JWT with isPremium = true.
```

### Endpoint

```text
POST /api/auth/refresh-claims
```

### Viva Answer

> RefreshClaims is needed because JWT claims do not update automatically. After premium or profile changes, user needs a new token with updated claims.

---

## 21. Get Current User

### Endpoint

```text
GET /api/auth/me
```

### Protection

```csharp
[Authorize]
```

### What It Returns

Reads from JWT claims:

```text
userId
email
role
isPremium
fullName
```

### Viva Answer

> /api/auth/me returns current authenticated user's claims from the JWT token.

---

## 22. Update Profile

### Endpoint

```text
PUT /api/auth/me
```

### Request

```csharp
public class UpdateProfileRequest
{
    public string FullName { get; set; }
}
```

### Flow

```text
1. Read user id from JWT.
2. Validate FullName length.
3. Update FullName in database.
4. Refresh JWT claims.
5. Return new token.
```

### Why New Token Is Returned

JWT contains `FullName`.

If name changes, frontend needs new token to show updated name.

### Viva Answer

> Profile update changes the user's full name and returns a refreshed JWT because FullName is stored as a claim.

---

## 23. Forgot Password OTP Request

### Endpoint

```text
POST /api/auth/forgot-password/request-otp
```

### Flow

```text
1. User submits email.
2. AuthService checks active user.
3. Always returns generic message.
4. If user exists, generates 6-digit OTP.
5. Hashes OTP using BCrypt.
6. Stores hashed OTP in memory cache.
7. Controller publishes EmailRequestedEvent.
8. NotificationService sends OTP email.
```

### Viva Answer

> Forgot-password request generates a 6-digit OTP for active users, stores its hash in memory cache, sends it through email event, and returns a generic response.

---

## 24. Generic Forgot-Password Response

### Message

```text
If the account exists, an OTP has been sent to the registered email address.
```

### Why

If backend says:

```text
Email not found
```

attackers can test emails and discover registered users.

### Viva Answer

> Forgot-password response is generic to prevent email enumeration attacks.

---

## 25. OTP Generation

### Code

```csharp
var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
```

### Meaning

Generates a random six-digit OTP:

```text
000000 to 999999
```

### Why RandomNumberGenerator

It is cryptographically stronger than normal random.

### Viva Answer

> OTP is generated using RandomNumberGenerator as a secure six-digit code.

---

## 26. OTP Hashing

### In Your Project

OTP is hashed before storing:

```csharp
OtpHash = BCrypt.Net.BCrypt.HashPassword(otp)
```

### Why

OTP is sensitive.

If memory/cache is exposed, plain OTP should not be visible.

### Viva Answer

> OTP is hashed before storage for security, similar to passwords. During reset, provided OTP is verified against the hash.

---

## 27. IMemoryCache for OTP

### Registration

```csharp
builder.Services.AddMemoryCache();
```

### Storage

```csharp
_memoryCache.Set(cacheKey, otpEntry, expirationOptions);
```

### Cache Key

```text
password-reset-otp:{email}
```

### Limitation

In-memory cache is local to one service instance.

If app restarts, OTP is lost.

In multi-instance deployment, another instance may not have the OTP.

### Viva Answer

> IMemoryCache stores OTP temporarily with expiry. It is simple for development, but distributed cache like Redis is better for production multi-instance systems.

---

## 28. Reset Password with OTP

### Endpoint

```text
POST /api/auth/forgot-password/reset
```

### Flow

```text
1. Find active user by email.
2. Get OTP hash from memory cache.
3. Check expiry.
4. Verify submitted OTP with BCrypt.
5. Hash new password.
6. Save new PasswordHash.
7. Remove OTP from cache.
```

### Viva Answer

> Password reset verifies the OTP from cache, checks expiry, hashes the new password, saves it, and removes the OTP so it cannot be reused.

---

## 29. Admin User Management

### Admin Endpoints

```text
GET /api/auth/admin/users
GET /api/auth/admin/users/{id}
PUT /api/auth/admin/users/{id}/role
PUT /api/auth/admin/users/{id}/premium
PUT /api/auth/admin/users/{id}/deactivate
PUT /api/auth/admin/users/{id}/reactivate
```

### Protection

```csharp
[Authorize(Roles = "Admin")]
```

### Viva Answer

> Admin APIs are role-protected and allow admins to view users, change roles, update premium status, deactivate accounts, and reactivate accounts.

---

## 30. Update User Role

### Method

```text
UpdateUserRoleAsync(int id, string role)
```

### Flow

```text
1. Find user by id.
2. If not found, throw NotFoundAppException.
3. Update Role.
4. Save changes.
```

### Viva Answer

> UpdateUserRoleAsync allows admin to change a user's role, such as Candidate or Admin.

---

## 31. Update User Premium

### Method

```text
UpdateUserPremiumAsync(int id, bool isPremium)
```

### Flow

```text
1. Find user by id.
2. If missing, throw NotFoundAppException.
3. Update IsPremium.
4. Save changes.
```

### Used By

```text
Admin premium endpoint
Internal premium endpoint
SubscriptionEventConsumer
```

### Viva Answer

> UpdateUserPremiumAsync updates the user's premium flag in IdentityService, which later appears in refreshed JWT claims.

---

## 32. Internal Premium Update Endpoint

### Endpoint

```text
PUT /api/auth/internal/users/{id}/premium
```

### Purpose

Allows internal service-to-service premium update.

### Security

Requires header:

```text
X-Internal-Key
```

It must match:

```text
InternalApiKey
```

### Viva Answer

> Internal premium update endpoint is protected using an internal API key so only trusted backend services can update premium status.

---

## 33. RabbitMQ Premium Update Consumer

### File

```text
Backend/IdentityService/Messaging/SubscriptionEventConsumer.cs
```

### Consumes

```text
SubscriptionLifecycleEvent
```

### Flow

```text
1. SubscriptionService publishes Activate or Cancel event.
2. IdentityService consumes event.
3. Finds user.
4. Updates IsPremium.
5. Creates notification.
6. Publishes result event to SubscriptionService.
```

### Viva Answer

> SubscriptionEventConsumer updates user premium state based on subscription lifecycle events from SubscriptionService.

---

## 34. Why Premium Flag Is in IdentityService

### Simple Explanation

Premium status affects user identity and authorization decisions.

Other services read it through JWT claims.

### Why Not Only SubscriptionService?

SubscriptionService owns payments and subscription records.

IdentityService owns user claims.

So after payment, SubscriptionService must notify IdentityService.

### Viva Answer

> Premium flag is stored in IdentityService because it becomes part of user claims. SubscriptionService owns payment records, but IdentityService owns the user state used for authorization.

---

## 35. Notification Model

### File

```text
Backend/IdentityService/Models/Notification.cs
```

### Fields

```text
Id
UserId
Title
Message
ActionUrl
IsRead
CreatedAt
```

### Used For

In-app bell notifications.

Examples:

```text
Interview completed
Assessment completed
Premium activated
Subscription cancelled
```

### Viva Answer

> Notification model stores in-app user notifications with title, message, action URL, read status, and creation time.

---

## 36. NotificationController

### File

```text
Backend/IdentityService/Controllers/NotificationController.cs
```

### Endpoints

```text
GET /api/auth/notifications
PUT /api/auth/notifications/read
```

### Flow

```text
GET -> Returns latest 50 notifications for logged-in user
PUT read -> Marks user's unread notifications as read
```

### Viva Answer

> NotificationController lets authenticated users view their notifications and mark them as read.

---

## 37. UserNotificationEventConsumer

### File

```text
Backend/IdentityService/Messaging/UserNotificationEventConsumer.cs
```

### Consumes

```text
InterviewCompletedEvent
AssessmentCompletedEvent
```

### What It Does

Creates notifications like:

```text
Your Java interview result is ready.
Your C# assessment result is ready.
```

### Viva Answer

> UserNotificationEventConsumer listens to interview and assessment completion events and creates in-app notifications for users.

---

## 38. RabbitMQ Events Published by AuthController

### Register

Publishes:

```text
UserRegisteredEvent
EmailRequestedEvent
```

### Forgot Password OTP

Publishes:

```text
EmailRequestedEvent
```

### Why

Email sending is handled asynchronously by NotificationService.

### Viva Answer

> AuthController publishes RabbitMQ events for user registration and password reset email so email work happens asynchronously.

---

## 39. AllowAnonymous vs Authorize

### AllowAnonymous

Used for public endpoints:

```text
Register
Login
Forgot password request
Forgot password reset
Internal premium update with internal key
```

### Authorize

Used for logged-in users:

```text
Me
Update profile
Refresh claims
Notifications
```

### Authorize Roles Admin

Used for admin-only APIs:

```text
Admin user management
```

### Viva Answer

> Public endpoints use AllowAnonymous, authenticated user endpoints use Authorize, and admin endpoints use Authorize with Roles = Admin.

---

## 40. Complete Register Flow

```text
1. Frontend sends POST /api/auth/register.
2. AuthController receives RegisterRequest.
3. AuthController calls AuthService.RegisterAsync.
4. AuthService checks if email already exists.
5. AuthService hashes password using BCrypt.
6. AuthService saves user in Users table.
7. AuthService generates JWT with user id, email, role, isPremium, FullName.
8. AuthController publishes UserRegisteredEvent.
9. AuthController publishes EmailRequestedEvent for welcome email.
10. NotificationService sends welcome email in background.
11. Frontend receives AuthResponse with token.
```

### Viva Explanation

> Registration creates or updates a user, hashes password, saves user, generates JWT, and publishes events for registration and welcome email.

---

## 41. Complete Login Flow

```text
1. Frontend sends POST /api/auth/login.
2. AuthController calls AuthService.LoginAsync.
3. AuthService finds user by email.
4. AuthService verifies password with BCrypt.
5. AuthService checks IsActive.
6. AuthService generates JWT.
7. Frontend receives token.
8. Frontend sends token in Authorization header for protected APIs.
```

### Viva Explanation

> Login verifies email/password, checks active status, and returns JWT for authenticated backend access.

---

## 42. Complete Forgot Password Flow

```text
1. User requests OTP using email.
2. AuthService checks active user.
3. AuthService returns generic message in all cases.
4. If user exists, AuthService generates secure 6-digit OTP.
5. OTP is hashed using BCrypt.
6. Hashed OTP is stored in IMemoryCache with expiry.
7. AuthController publishes EmailRequestedEvent.
8. NotificationService sends OTP email.
9. User submits email, OTP, and new password.
10. AuthService verifies OTP hash and expiry.
11. New password is hashed and saved.
12. OTP cache entry is removed.
```

### Viva Explanation

> Forgot password uses secure OTP generation, hashed OTP storage, expiry, generic response, email event, and BCrypt hashing for the new password.

---

## 43. Complete Premium Activation Flow

```text
1. User pays through SubscriptionService.
2. SubscriptionService publishes SubscriptionLifecycleEvent with Action = Activate.
3. IdentityService SubscriptionEventConsumer receives event.
4. Consumer finds user.
5. User.IsPremium becomes true.
6. IdentityService saves notification.
7. IdentityService publishes result event.
8. SubscriptionService updates saga state.
9. User calls refresh-claims.
10. New JWT contains isPremium = true.
```

### Viva Explanation

> Premium activation is event-based. IdentityService updates IsPremium when it receives subscription lifecycle event, and user refreshes JWT to get updated premium claim.

---

## 44. What Happens If IdentityService Is Down?

### Effects

If IdentityService is down:

```text
Register/login will not work
Refresh claims will not work
Admin user management will not work
Notification APIs will not work
Premium update event may wait in RabbitMQ queue
```

### Viva Answer

> If IdentityService is down, authentication and user APIs are unavailable. RabbitMQ subscription events can wait in queue until IdentityService comes back.

---

## 45. Security Features in IdentityService

### Security Features

- BCrypt password hashing
- BCrypt OTP hashing
- Generic forgot-password response
- JWT signing
- Token expiry
- Role-based admin authorization
- IsActive account blocking
- Internal API key for internal premium endpoint
- Claims refresh after premium/profile changes
- Protected notification endpoints

### Viva Answer

> IdentityService security includes password hashing, OTP hashing, JWT signing, role authorization, account activation control, generic forgot-password response, and internal API key protection.

---

## 46. What Happens If Passwords Are Stored Plainly?

### Risk

If database is leaked, all passwords are exposed.

Attackers may reuse passwords on other sites.

### BCrypt Benefit

BCrypt is slow and salted, making brute force harder.

### Viva Answer

> Plain password storage is dangerous because database leaks expose user passwords. BCrypt hashes passwords securely before storage.

---

## 47. What Happens If JWT Is Not Signed?

### Risk

User could modify token claims.

Example:

```text
role = Candidate -> role = Admin
isPremium = false -> isPremium = true
```

### Signing Prevents

Backend validates token signature using secret key.

Tampered tokens are rejected.

### Viva Answer

> JWT must be signed so users cannot tamper with claims like role or isPremium.

---

## 48. What Happens If Internal Premium Endpoint Has No Key?

### Risk

Anyone could call:

```text
PUT /api/auth/internal/users/{id}/premium
```

and make themselves premium.

### In Your Project

Request must include:

```text
X-Internal-Key
```

### Viva Answer

> Internal premium update must be protected because otherwise users could update premium status directly. The project uses X-Internal-Key.

---

## 49. Limitations and Improvements

### Current Limitations

- OTP stored in in-memory cache, not distributed cache
- Admin role update accepts raw string
- User email uniqueness should ideally have database unique index
- Internal API key is simpler than mTLS or service identity
- Refresh token flow is not implemented
- Seeded credentials are risky for production
- Some controller validation is manual

### Possible Improvements

- Use Redis for OTP cache
- Add refresh tokens
- Add email verification
- Add account lockout after failed logins
- Add unique index on Email
- Use role enum or validation for role update
- Use stronger internal service authentication
- Add audit logs for admin actions
- Add rate limiting for login and OTP endpoints
- Use multi-factor authentication for admin

### Balanced Viva Answer

> IdentityService supports core authentication and user management well. Future improvements could include Redis OTP storage, refresh tokens, email verification, account lockout, email unique index, rate limiting, and stronger service-to-service authentication.

---

## 50. Best Full Viva Answer for Topic 13

> IdentityService is responsible for user authentication and user identity in my project. It handles registration, login, password hashing with BCrypt, JWT generation, role and premium claims, forgot-password OTP, profile update, admin user management, account activation/deactivation, and notifications. User data is stored in the Users table using AppDbContext, and notifications are stored in the Notifications table. During registration, password is hashed, user is saved, JWT is generated, and RabbitMQ events are published for registration and welcome email. During login, password is verified using BCrypt and inactive users are blocked. JWT contains user id, email, role, isPremium, and FullName. Premium status is stored in IdentityService because it becomes part of JWT claims. After payment, SubscriptionService sends an event and IdentityService updates IsPremium. User must refresh claims to get a new token with updated premium status.

---

## 51. Common Viva Questions and Answers

### Q1. What is IdentityService?

IdentityService manages authentication, users, roles, JWT claims, premium status, password reset, and notifications.

### Q2. Why is IdentityService separate?

Because user identity and authentication are security-sensitive and should have clear ownership.

### Q3. Which table stores users?

The Users table managed by IdentityService AppDbContext.

### Q4. What fields are in User model?

Id, FullName, Email, PasswordHash, Role, IsPremium, IsActive, and CreatedAt.

### Q5. How does registration work?

It checks existing email, hashes password, saves user, generates JWT, and publishes registration/email events.

### Q6. How does login work?

It finds user by email, verifies password using BCrypt, checks IsActive, and returns JWT.

### Q7. Why use BCrypt?

BCrypt securely hashes passwords with salt and is slow enough to resist brute force attacks.

### Q8. What happens if user is deactivated?

Login is blocked with 403 Forbidden.

### Q9. What claims are stored in JWT?

User id, email, role, isPremium, and FullName.

### Q10. Why is role stored in JWT?

For role-based authorization, especially admin endpoints.

### Q11. Why is isPremium stored in JWT?

So services can check premium access without calling IdentityService every time.

### Q12. Why refresh claims after payment?

Old JWT still has old premium value, so a new JWT is needed.

### Q13. How does forgot password work?

It generates a 6-digit OTP, hashes it, stores it in memory cache with expiry, and sends it by email event.

### Q14. Why is forgot-password response generic?

To prevent email enumeration.

### Q15. Why hash OTP?

OTP is sensitive, so storing its hash is safer than storing plain OTP.

### Q16. What is IMemoryCache used for?

Temporary storage of password reset OTP hashes.

### Q17. What is the limitation of IMemoryCache?

It is local to one server and lost on restart; Redis is better for distributed production.

### Q18. Which endpoints are admin-only?

Admin user list, get user by id, role update, premium update, deactivate, and reactivate.

### Q19. How is admin authorization done?

Using `[Authorize(Roles = "Admin")]`.

### Q20. Why does profile update return new token?

Because FullName is stored in JWT claims.

### Q21. Why is premium flag in IdentityService?

Because premium status is part of user identity and JWT claims.

### Q22. How does SubscriptionService update premium?

It publishes SubscriptionLifecycleEvent, which IdentityService consumes and uses to update IsPremium.

### Q23. What is internal premium update endpoint?

It is an internal API protected by X-Internal-Key for service-to-service premium updates.

### Q24. What does NotificationController do?

It returns user notifications and marks them as read.

### Q25. What improvements can be made?

Redis OTP cache, refresh tokens, email verification, account lockout, rate limiting, unique email index, and stronger internal authentication.

---

## 52. Quick Revision Summary

- IdentityService owns user identity.
- It manages Users and Notifications tables.
- AuthController exposes auth and admin endpoints.
- AuthService contains authentication business logic.
- Registration hashes password using BCrypt.
- Login verifies password using BCrypt.
- Deactivated users cannot login.
- JWT is generated from current user data.
- JWT claims include user id, email, role, isPremium, FullName.
- Role claim protects admin endpoints.
- isPremium claim controls premium access.
- RefreshClaims issues new token after profile or premium changes.
- Forgot password uses secure 6-digit OTP.
- OTP is hashed using BCrypt before storage.
- OTP is stored temporarily in IMemoryCache.
- Forgot-password response is generic to prevent email enumeration.
- Admin APIs are protected with `[Authorize(Roles = "Admin")]`.
- Internal premium endpoint uses X-Internal-Key.
- SubscriptionEventConsumer updates premium from RabbitMQ events.
- UserNotificationEventConsumer creates in-app notifications.
- NotificationController returns and marks notifications as read.
- Future improvements include Redis, refresh tokens, email verification, account lockout, and rate limiting.

