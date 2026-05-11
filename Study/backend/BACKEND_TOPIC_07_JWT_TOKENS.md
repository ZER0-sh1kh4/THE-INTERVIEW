# Topic 7: JWT Tokens

Project: Mock Interview Platform  
Focus: Understanding what JWT is, why it is used, how your backend generates and validates it, what claims are stored, why refresh-claims is needed, and what security points matter in viva.

---

## 1. What Is JWT?

### Simple Explanation

JWT means JSON Web Token.

It is a digitally signed token used to prove that a user is logged in.

After login, backend gives the frontend a JWT token.

Then frontend sends that token with future API requests.

### Real-Life Analogy

JWT is like an entry pass.

When you enter a college event, security checks your identity once and gives you a pass.

After that, when you go to different rooms, you show the pass instead of proving identity again and again.

In your project:

```text
Login successfully
        ↓
Backend gives JWT token
        ↓
Frontend stores token
        ↓
Frontend sends token with protected API requests
        ↓
Backend validates token
        ↓
Access is allowed
```

### Technical Definition

JWT is a compact, URL-safe token format that contains JSON claims and is digitally signed so backend services can verify that the token was issued by a trusted server and has not been modified.

### Viva Answer

> JWT is a signed token used for stateless authentication. In my project, IdentityService generates JWT after login or registration, and the frontend sends it with protected API requests.

---

## 2. Why JWT Is Used in This Project

### Simple Explanation

Your project has multiple backend services:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
API Gateway
```

All these services need to know who the current user is.

JWT helps each service identify the user without calling IdentityService every time.

### Practical Scenario

User starts interview:

```text
POST /api/interviews/start
Authorization: Bearer jwt_token
```

InterviewService reads token and gets:

```text
userId = 5
email = student@gmail.com
role = Candidate
isPremium = false
```

Now InterviewService can apply rules:

```text
Is this user free or premium?
How many interviews has this user already created?
Should access be allowed?
```

### Why Not Ask IdentityService Every Time?

Without JWT, every service may need to call IdentityService:

```text
InterviewService → IdentityService → Who is this user?
AssessmentService → IdentityService → Is this user premium?
SubscriptionService → IdentityService → What is user's email?
```

That creates extra network calls and tight coupling.

### Viva Answer

> JWT is used because my backend has multiple services. Each service can validate the token and read user claims without calling IdentityService for every request.

---

## 3. JWT Structure

### Simple Explanation

A JWT has three parts:

```text
Header.Payload.Signature
```

Example format:

```text
xxxxx.yyyyy.zzzzz
```

### Part 1: Header

Header contains token type and signing algorithm.

Example:

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

Meaning:

```text
Token type = JWT
Algorithm = HMAC SHA-256
```

### Part 2: Payload

Payload contains claims.

Example:

```json
{
  "nameid": "5",
  "email": "student@gmail.com",
  "role": "Candidate",
  "isPremium": "false",
  "FullName": "Student Name"
}
```

### Part 3: Signature

Signature proves the token was created by trusted backend and was not changed.

### Important Point

JWT payload is encoded, not encrypted.

Anyone with the token can decode and read payload.

So do not store sensitive secrets like password in JWT.

### Viva Answer

> JWT has three parts: header, payload, and signature. Header stores algorithm, payload stores claims, and signature ensures the token was not modified.

---

## 4. JWT Header

### Simple Explanation

The header tells how the token is signed.

### In Your Project

Your project signs JWT using:

```csharp
SecurityAlgorithms.HmacSha256
```

This corresponds to:

```text
HS256
```

### Why Header Is Needed

Token validator needs to know which algorithm was used.

### Viva Answer

> JWT header contains metadata like token type and signing algorithm. My project uses HMAC SHA-256 for signing JWT.

---

## 5. JWT Payload and Claims

### Simple Explanation

Payload contains claims.

Claims are information about the user.

### Claims in Your Project

Generated in:

```text
Backend/IdentityService/Services/AuthService.cs
```

Claims:

```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
new Claim(ClaimTypes.Email, user.Email)
new Claim(ClaimTypes.Role, user.Role)
new Claim("isPremium", user.IsPremium.ToString().ToLower())
new Claim("FullName", user.FullName ?? "")
```

### Meaning

| Claim | Meaning | Used For |
|---|---|---|
| NameIdentifier | User id | Ownership, user-specific data |
| Email | User email | Email notifications |
| Role | Candidate/Admin | Admin authorization |
| isPremium | true/false | Premium feature checks |
| FullName | User name | Profile/display |

### Practical Example

AssessmentService reads:

```text
userId
isPremium
email
```

Then it can decide:

```text
Can this user start another assessment?
Should premium result details be shown?
Which user owns this assessment?
```

### Viva Answer

> JWT payload contains claims. My project stores user id, email, role, premium status, and full name as claims so services can identify and authorize the current user.

---

## 6. JWT Signature

### Simple Explanation

Signature protects the token from tampering.

If someone changes payload, signature validation fails.

### Practical Scenario

Suppose user changes token payload from:

```json
{
  "role": "Candidate"
}
```

to:

```json
{
  "role": "Admin"
}
```

The token signature will no longer match.

Backend rejects the token.

### In Your Project

JWT is signed using secret key:

```csharp
var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

var creds = new SigningCredentials(
    key,
    SecurityAlgorithms.HmacSha256);
```

### Why Secret Key Is Important

Only backend should know the secret key.

If attacker gets the secret key, they can create fake valid tokens.

### Viva Answer

> JWT signature ensures the token has not been modified. My project signs tokens using a secret key and HMAC SHA-256 algorithm.

---

## 7. JWT Generation in This Project

### Where It Happens

JWT is generated in:

```text
Backend/IdentityService/Services/AuthService.cs
```

Method:

```text
GenerateJwtToken(User user)
```

### Flow

```text
1. Read JWT settings from configuration.
2. Create symmetric signing key from SecretKey.
3. Create signing credentials using HmacSha256.
4. Add user claims.
5. Set issuer and audience.
6. Set expiry time.
7. Create JwtSecurityToken.
8. Convert token to string.
9. Return token to frontend.
```

### Code Idea

```csharp
var token = new JwtSecurityToken(
    issuer: jwtSettings["Issuer"],
    audience: jwtSettings["Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(
        double.Parse(jwtSettings["ExpiryMinutes"]!)),
    signingCredentials: creds);
```

### When Token Is Generated

Token is generated after:

- Registration
- Login
- Refresh claims
- Profile update

### Viva Answer

> IdentityService generates JWT in AuthService after successful registration, login, or claim refresh. The token includes user claims, issuer, audience, expiry, and signing credentials.

---

## 8. JWT Validation in This Project

### Simple Explanation

Validation means checking whether token is trustworthy.

### Where Validation Happens

JWT validation is configured in:

```text
Backend/API_Gateway/Program.cs
Backend/IdentityService/Program.cs
Backend/InterviewService/Program.cs
Backend/AssessmentService/Program.cs
Backend/SubscriptionService/Program.cs
```

### Validation Parameters

Your project checks:

```csharp
ValidateIssuer = true
ValidateAudience = true
ValidateLifetime = true
ValidateIssuerSigningKey = true
ValidIssuer = jwtSettings["Issuer"]
ValidAudience = jwtSettings["Audience"]
IssuerSigningKey = new SymmetricSecurityKey(...)
ClockSkew = TimeSpan.Zero
```

### Meaning

| Validation | Meaning |
|---|---|
| ValidateIssuer | Token came from expected issuer |
| ValidateAudience | Token is meant for this app |
| ValidateLifetime | Token is not expired |
| ValidateIssuerSigningKey | Signature key is valid |
| ClockSkew zero | Expiry is strict |

### Viva Answer

> JWT validation checks issuer, audience, expiry, and signing key. My gateway and services validate JWT before allowing protected APIs.

---

## 9. Issuer and Audience

### Simple Explanation

Issuer means who created the token.

Audience means who the token is intended for.

### Real-Life Analogy

A college ID card:

```text
Issuer = College administration
Audience = College security/library/labs
```

If a random coaching center ID is shown, college security should reject it.

### In Your Project

JWT has:

```csharp
issuer: jwtSettings["Issuer"]
audience: jwtSettings["Audience"]
```

Services validate:

```csharp
ValidIssuer = jwtSettings["Issuer"]
ValidAudience = jwtSettings["Audience"]
```

### Why It Matters

It prevents accepting tokens from unknown systems.

### Viva Answer

> Issuer identifies who created the token, and audience identifies who the token is for. My services validate both to ensure the token belongs to this application.

---

## 10. Token Expiry

### Simple Explanation

JWT should not be valid forever.

It has expiry time.

### In Your Project

Token expiry is set using:

```csharp
expires: DateTime.UtcNow.AddMinutes(
    double.Parse(jwtSettings["ExpiryMinutes"]!))
```

### Why Expiry Is Needed

If token is stolen, expiry limits how long attacker can use it.

### ClockSkew

Your project uses:

```csharp
ClockSkew = TimeSpan.Zero
```

This means token expires exactly at expiry time.

### What If Token Never Expires?

Security risk:

- Stolen token works forever
- Old premium/role claims remain forever
- Deactivated users may still use old token longer

### Viva Answer

> JWT expiry limits token lifetime. My project validates token lifetime and uses zero clock skew for strict expiry.

---

## 11. Bearer Token

### Simple Explanation

Bearer token means whoever carries the token can use it.

Frontend sends JWT like this:

```text
Authorization: Bearer jwt_token_here
```

### Why "Bearer" Is Used

It tells backend authentication middleware:

```text
This request contains a bearer token for authentication.
```

### In Your Project

Angular frontend sends token with protected requests.

Backend services use JWT Bearer authentication.

### Security Point

Because bearer token gives access, it must be protected.

Do not expose it in:

- URLs
- Logs
- Screenshots
- Public storage

### Viva Answer

> Bearer token is sent in the Authorization header. In my project, frontend sends JWT as Bearer token for protected API calls.

---

## 12. Stateless Authentication

### Simple Explanation

Stateless means backend does not store login session for each user.

Each request carries JWT.

Backend validates token and gets user identity from it.

### Traditional Session Login

```text
Login
        ↓
Server stores session
        ↓
Browser stores session id
        ↓
Server checks session storage on every request
```

### JWT Stateless Login

```text
Login
        ↓
Server creates signed token
        ↓
Browser sends token every request
        ↓
Server validates token without session lookup
```

### Why Good for Microservices

Each service can validate JWT independently.

InterviewService does not need server session from IdentityService.

### Viva Answer

> JWT supports stateless authentication because each request carries the signed token. Backend services validate the token without storing server-side sessions.

---

## 13. How Controllers Read JWT Claims

### Simple Explanation

After JWT validation, ASP.NET Core puts claims into:

```csharp
User
```

### In InterviewService

```csharp
var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var isPremiumStr = User.FindFirst("isPremium")?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
```

### In AssessmentService

Same pattern:

```text
Read userId
Read isPremium
Read email
```

### In SubscriptionService

Reads:

```text
userId
email
```

### Why Controllers Read Claims

They need current user context for:

- Ownership
- Limits
- Premium access
- Email events
- Payment records

### Viva Answer

> After JWT validation, controllers read claims from the User object. My services read userId, email, and isPremium claims to process user-specific requests.

---

## 14. JWT and Role-Based Authorization

### Simple Explanation

Role claim decides whether user is Admin or Candidate.

### In Token

```csharp
new Claim(ClaimTypes.Role, user.Role)
```

### In Controller

```csharp
[Authorize(Roles = "Admin")]
```

### Practical Scenario

Candidate token:

```text
role = Candidate
```

Calls:

```text
GET /api/auth/admin/users
```

Result:

```text
403 Forbidden
```

Admin token:

```text
role = Admin
```

Allowed.

### Viva Answer

> JWT stores the user's role as a claim. ASP.NET Core uses this role claim to enforce admin-only endpoints with [Authorize(Roles = "Admin")].

---

## 15. JWT and Premium Access

### Simple Explanation

JWT also stores premium status.

Claim:

```text
isPremium = true/false
```

### In Project

InterviewService uses it:

```text
Free users can create only 1 interview.
Premium users have more access.
```

AssessmentService uses it:

```text
Free users can create only 2 assessments.
Premium users get more detailed result review.
```

### Why Premium Is in JWT

Services can quickly check premium state without calling IdentityService.

### Important Limitation

If premium status changes in database, old JWT still has old claim.

That is why refresh-claims is needed.

### Viva Answer

> The isPremium claim in JWT controls premium/free access. Services use it to apply attempt limits and show premium-only result details.

---

## 16. Refresh Claims

### Simple Explanation

Refresh claims means create a new JWT using latest user data from database.

### Why Needed

JWT is stateless.

Once generated, its claims do not change automatically.

### Practical Scenario

Before payment:

```text
JWT says isPremium = false
```

After payment:

```text
Database says IsPremium = true
```

But old token still says:

```text
isPremium = false
```

So frontend calls:

```text
POST /api/auth/refresh-claims
```

IdentityService returns new token:

```text
isPremium = true
```

### In Your Project

Refresh method:

```text
RefreshClaimsAsync(int id)
```

Endpoint:

```text
POST /api/auth/refresh-claims
```

### Viva Answer

> Refresh-claims is needed because JWT claims do not automatically update after database changes. The endpoint generates a new token with latest user role, premium status, and profile data.

---

## 17. JWT After Profile Update

### Simple Explanation

JWT stores FullName claim.

If user updates profile name, old token still contains old name.

### In Your Project

Profile update endpoint:

```text
PUT /api/auth/me
```

After updating name, it returns refreshed JWT.

### Why

Frontend can immediately show updated name from new token.

### Viva Answer

> Profile update returns a refreshed token because FullName is stored as a claim, and old JWT would still contain the old name.

---

## 18. Gateway-Level JWT Validation

### Simple Explanation

API Gateway can validate JWT before forwarding request to service.

### In Your Project

API Gateway validates protected routes using:

```json
"AuthenticationOptions": {
  "AuthenticationProviderKey": "Bearer",
  "AllowedScopes": []
}
```

### Benefit

Invalid token requests are blocked before reaching internal services.

### Example

Request:

```text
POST /api/interviews/start
Authorization: Bearer invalid_token
```

Gateway rejects it.

### Viva Answer

> API Gateway validates JWT for protected routes so invalid requests can be blocked before reaching backend services.

---

## 19. Service-Level JWT Validation

### Simple Explanation

Even though gateway validates JWT, services also validate JWT.

### Why

Defense-in-depth.

If someone directly calls:

```text
localhost:5002/api/interviews/start
```

InterviewService still requires and validates JWT.

### In Your Project

IdentityService, InterviewService, AssessmentService, and SubscriptionService all configure JWT authentication in `Program.cs`.

### Viva Answer

> Services also validate JWT so security does not depend only on the gateway. This provides defense-in-depth.

---

## 20. JWT in Public vs Protected APIs

### Public APIs

Do not need JWT:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password/request-otp
POST /api/auth/forgot-password/reset
POST /api/subscriptions/webhook/stripe
```

### Protected APIs

Need JWT:

```text
GET /api/auth/me
POST /api/interviews/start
POST /api/assessments/start
POST /api/subscriptions/subscribe
```

### Admin APIs

Need JWT with Admin role:

```text
GET /api/auth/admin/users
POST /api/assessments/questions
DELETE /api/assessments/questions/{id}
```

### Viva Answer

> Public APIs do not require JWT, protected APIs require valid JWT, and admin APIs require JWT with Admin role claim.

---

## 21. JWT Security Risks

### Risk 1: Token Theft

If attacker steals JWT, they can use it until expiry.

Prevention:

- Use HTTPS
- Avoid logging tokens
- Store securely
- Keep expiry reasonable

### Risk 2: Storing Sensitive Data in Token

JWT payload can be decoded.

Do not store:

```text
Password
PasswordHash
OTP
Secret keys
Payment card details
```

Your project stores safe claims:

```text
userId
email
role
isPremium
FullName
```

### Risk 3: Long Expiry

Long-lived token increases damage if stolen.

### Risk 4: Weak Secret Key

Weak JWT secret can be guessed.

Use strong secret key.

### Risk 5: Not Validating Issuer/Audience

Could accept token from wrong system.

Your project validates both.

### Viva Answer

> JWT security risks include token theft, weak signing keys, long expiry, and storing sensitive data in payload. My project signs tokens, validates issuer/audience/lifetime, and stores only necessary claims.

---

## 22. Where Should JWT Be Stored in Frontend?

### Common Options

| Storage | Pros | Cons |
|---|---|---|
| localStorage | Easy to use | Vulnerable to XSS token theft |
| sessionStorage | Clears on tab close | Still vulnerable to XSS |
| HttpOnly cookie | Safer from JavaScript access | Needs CSRF handling |
| In-memory | Safer from persistence | Lost on refresh |

### Viva-Safe Answer

For production, HttpOnly secure cookies are generally safer because JavaScript cannot read them.

But many Angular projects store JWT in localStorage for simplicity during development.

### Project Note

Your backend supports Bearer tokens. The exact frontend storage decision belongs to frontend implementation.

### Viva Answer

> JWT can be stored in localStorage, sessionStorage, memory, or HttpOnly cookies. For production, HttpOnly secure cookies are safer, while localStorage is simpler but more exposed to XSS.

---

## 23. JWT vs Session Authentication

### Session Authentication

Server stores session.

Browser stores session ID.

Good for traditional web apps.

### JWT Authentication

Server issues signed token.

Client sends token with each request.

Good for APIs and microservices.

### Comparison

| Feature | Session | JWT |
|---|---|---|
| Server storage | Needed | Not needed |
| Stateless | No | Yes |
| Microservice friendly | Less | More |
| Easy revocation | Easier | Harder |
| Payload claims | Limited | Built-in |

### Viva Answer

> Session authentication stores login state on the server, while JWT stores signed claims on the client. JWT is more suitable for stateless APIs and microservices.

---

## 24. Token Revocation Challenge

### Simple Explanation

JWT is stateless, so once issued, it remains valid until expiry unless we add extra revocation logic.

### Practical Scenario

Admin deactivates user.

Problem:

```text
Old token may still be valid until expiry.
```

Login is blocked after deactivation, but already issued token may work until it expires unless service checks database or token blacklist.

### Possible Solutions

- Short token expiry
- Token blacklist
- Refresh token rotation
- Check user active status on sensitive APIs
- Store token version in database

### Viva Answer

> JWT revocation is harder because tokens are stateless. Possible solutions include short expiry, blacklist, refresh token rotation, or checking user status for sensitive operations.

---

## 25. JWT and 401/403

### 401 Unauthorized

Means token is missing, invalid, expired, or not trusted.

Example:

```text
GET /api/interviews
without token
```

Response:

```text
401 Unauthorized
```

### 403 Forbidden

Means token is valid but user lacks permission.

Example:

```text
Candidate accesses admin API
```

Response:

```text
403 Forbidden
```

### Viva Answer

> 401 means JWT is missing or invalid. 403 means JWT is valid but user does not have required permission or role.

---

## 26. Complete Flow: Login and Use JWT

### Step-by-Step

```text
1. User enters email/password.
2. Angular sends POST /api/auth/login.
3. IdentityService verifies credentials.
4. IdentityService creates JWT with claims.
5. JWT is returned to frontend.
6. Frontend stores token.
7. User starts assessment.
8. Frontend sends Authorization: Bearer token.
9. Gateway validates token.
10. AssessmentService validates token.
11. Controller reads userId and isPremium claims.
12. Assessment starts for that user.
```

### Viva Explanation

> JWT connects login with future protected API access. After login, token is sent with requests, validated by gateway and services, and claims are used to identify user and apply access rules.

---

## 27. Complete Flow: Premium Claim Update

### Step-by-Step

```text
1. User logs in.
2. JWT contains isPremium = false.
3. User buys premium.
4. SubscriptionService activates subscription.
5. IdentityService updates database IsPremium = true.
6. Old JWT still says isPremium = false.
7. Frontend calls POST /api/auth/refresh-claims.
8. IdentityService reads latest user from database.
9. New JWT is generated.
10. New JWT contains isPremium = true.
```

### Viva Explanation

> Since JWT is stateless, claims do not change after token creation. Refresh-claims creates a new token after premium status changes.

---

## 28. What Happens If JWT Is Not Used?

### Alternatives Needed

Without JWT, project would need:

- Server-side sessions
- Frequent IdentityService calls
- API keys
- OAuth/OpenID Connect provider

### Problems for This Project

Because this project has multiple services, no JWT would mean:

```text
Each service must ask IdentityService who the user is.
```

This increases coupling and network calls.

### Viva Answer

> Without JWT, services would need server sessions or repeated calls to IdentityService. JWT is better for this microservices-style backend because services can validate user identity independently.

---

## 29. Alternatives to JWT

### Alternatives

| Alternative | Use Case |
|---|---|
| Server-side sessions | Traditional web apps |
| OAuth2 access tokens | Third-party authorization |
| OpenID Connect | Centralized identity/login |
| Reference tokens | Token stored server-side |
| API keys | Service-to-service/simple access |
| SAML | Enterprise SSO |

### Best for This Project

JWT is suitable because:

- Angular frontend consumes APIs
- Backend is split into services
- Services need stateless user identity
- Role and premium claims are useful

### Viva Answer

> Alternatives include sessions, OAuth2, OpenID Connect, reference tokens, API keys, and SAML. JWT is suitable here because it supports stateless API authentication across services.

---

## 30. Possible Improvements

### Improvements

- Add refresh token support
- Add token revocation or blacklist
- Add token version claim
- Use HttpOnly secure cookies
- Add shorter access token lifetime
- Add MFA for admin users
- Add audit logging for sensitive actions
- Check IsActive for sensitive protected APIs
- Use centralized identity provider like OpenIddict or IdentityServer

### Balanced Viva Answer

> Current JWT implementation supports stateless authentication with signed claims. Future improvements could include refresh tokens, token revocation, HttpOnly cookies, MFA, and centralized identity management.

---

## 31. Best Full Viva Answer for Topic 7

> JWT is a signed JSON Web Token used for stateless authentication. In my project, IdentityService generates JWT after registration, login, profile update, or refresh-claims. The token contains claims like userId, email, role, isPremium, and FullName. It is signed using a secret key and HMAC SHA-256 so services can verify it was not modified. API Gateway and backend services validate issuer, audience, lifetime, and signing key before allowing protected APIs. Controllers read claims from the User object to identify the current user, apply premium limits, check ownership, and enforce admin roles. Since JWT claims do not automatically update after database changes, refresh-claims is used after premium or profile updates.

---

## 32. Common Viva Questions and Answers

### Q1. What is JWT?

JWT is a signed token that contains user claims and is used for stateless authentication.

### Q2. What are the three parts of JWT?

Header, payload, and signature.

### Q3. What is stored in JWT payload?

Claims like user id, email, role, premium status, and full name.

### Q4. Is JWT encrypted?

No. JWT payload is encoded, not encrypted. It should not contain sensitive secrets.

### Q5. Why is signature needed?

Signature ensures the token has not been modified and was issued by trusted backend.

### Q6. What algorithm does your project use?

HMAC SHA-256 through `SecurityAlgorithms.HmacSha256`.

### Q7. What is issuer?

Issuer identifies who created the token.

### Q8. What is audience?

Audience identifies who the token is intended for.

### Q9. Why validate token lifetime?

To reject expired tokens and limit risk if a token is stolen.

### Q10. What is Bearer token?

Bearer token is sent in Authorization header as `Authorization: Bearer token`.

### Q11. Why use JWT in microservices?

Because each service can validate the token independently without server-side sessions.

### Q12. Why refresh claims after payment?

Because old JWT still contains old premium status. Refresh-claims creates a new token with latest data.

### Q13. What happens if user changes token role manually?

Signature validation fails, and backend rejects the token.

### Q14. What is 401 in JWT context?

Token is missing, invalid, expired, or not trusted.

### Q15. What is 403 in JWT context?

Token is valid, but user does not have required permission or role.

### Q16. What is a JWT security risk?

If token is stolen, attacker can use it until expiry. HTTPS and secure storage reduce this risk.

### Q17. Why should password not be stored in JWT?

Because JWT payload can be decoded by anyone who has the token.

### Q18. Why do gateway and services both validate JWT?

Gateway blocks invalid requests early, and service-level validation gives defense-in-depth.

---

## 33. Quick Revision Summary

- JWT means JSON Web Token.
- JWT has header, payload, and signature.
- Header stores algorithm and token type.
- Payload stores claims.
- Signature prevents tampering.
- JWT is encoded, not encrypted.
- Do not store passwords/secrets in JWT.
- IdentityService generates JWT.
- Claims include userId, email, role, isPremium, and FullName.
- Frontend sends JWT as Bearer token.
- API Gateway validates JWT.
- Backend services also validate JWT.
- Token validation checks issuer, audience, lifetime, and signing key.
- JWT supports stateless authentication.
- Role claim protects admin APIs.
- isPremium claim controls premium features.
- Refresh-claims creates new JWT with latest database values.
- 401 means token problem.
- 403 means permission problem.

