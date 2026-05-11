# Topic 8: Password Hashing and OTP

Project: Mock Interview Platform  
Focus: Understanding why passwords are hashed, how BCrypt works in your project, how forgot-password OTP flow works, why OTP is hashed and expires, and what security points matter in viva.

---

## 1. Why Password Security Matters

### Simple Explanation

Passwords are one of the most sensitive parts of any application.

If someone gets user passwords, they can:

- Login as users
- Access private data
- Use same passwords on other websites
- Misuse accounts

So backend must never store plain passwords.

### Practical Scenario

Bad database record:

```text
Email: student@gmail.com
Password: 123456
```

If database leaks, password is directly visible.

Good database record:

```text
Email: student@gmail.com
PasswordHash: $2a$11$F0x...
```

Even if database leaks, original password is not directly visible.

### In Your Project

Your project stores:

```text
PasswordHash
```

not:

```text
Password
```

Important file:

```text
Backend/IdentityService/Services/AuthService.cs
```

### Viva Answer

> Password security is important because plain passwords can expose user accounts if the database is leaked. My project stores BCrypt password hashes instead of plain passwords.

---

## 2. Hashing vs Encryption

### Simple Explanation

Hashing and encryption are different.

### Encryption

Encryption is reversible if you have the key.

Example:

```text
Plain text → Encrypt → Cipher text
Cipher text → Decrypt with key → Plain text
```

### Hashing

Hashing is one-way.

Example:

```text
Password → Hash → PasswordHash
```

You cannot directly convert hash back to password.

### Why Passwords Use Hashing

Backend does not need to know original password.

It only needs to verify:

```text
Does entered password match stored hash?
```

### Viva Answer

> Encryption is reversible with a key, while hashing is one-way. Passwords should be hashed, not encrypted, because the backend only needs to verify them, not recover them.

---

## 3. What Is Password Hashing?

### Simple Explanation

Password hashing converts a password into a fixed-looking unreadable value.

Example:

```text
123456
```

becomes something like:

```text
$2a$11$V6xZq...
```

### Important Property

Same password should verify successfully, but the original password should not be recoverable.

### In Your Project

During registration:

```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
```

During login:

```csharp
BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
```

### Viva Answer

> Password hashing converts a plain password into a one-way hash. My project uses BCrypt to hash passwords during registration and verify them during login.

---

## 4. What Is BCrypt?

### Simple Explanation

BCrypt is a password hashing algorithm designed for securely storing passwords.

It is stronger than simple hashing algorithms like MD5 or SHA256 for passwords.

### Why BCrypt Is Good

BCrypt is:

- Slow by design
- Salted automatically
- Resistant to brute-force attacks
- Commonly used for password storage

### What "Slow By Design" Means

For normal users, login still feels fast.

But for attackers trying millions of password guesses, BCrypt slows them down.

### In Your Project

Package:

```text
BCrypt.Net-Next
```

Used in:

```text
Backend/IdentityService/Services/AuthService.cs
```

### Viva Answer

> BCrypt is a password hashing algorithm designed for secure password storage. It is slow by design and includes salting, which makes brute-force attacks harder.

---

## 5. What Is Salt?

### Simple Explanation

Salt is random extra data added before hashing the password.

It ensures that the same password does not always produce the same hash.

### Practical Example

Two users both use password:

```text
123456
```

Without salt, both hashes may be same.

With salt, BCrypt produces different hashes.

### Why Salt Matters

It protects against:

- Rainbow table attacks
- Comparing users with same password
- Precomputed hash attacks

### In BCrypt

BCrypt handles salt automatically.

You do not manually create salt in your code.

### Viva Answer

> Salt is random data added during hashing so identical passwords produce different hashes. BCrypt automatically handles salting in my project.

---

## 6. Registration Password Flow

### Endpoint

```text
POST /api/auth/register
```

### Request

```json
{
  "fullName": "Shobhit",
  "email": "shobhit@example.com",
  "password": "MyPassword123"
}
```

### Backend Steps

```text
1. AuthController receives RegisterRequest.
2. AuthService checks whether email already exists.
3. Password is passed to BCrypt.HashPassword.
4. BCrypt creates salted password hash.
5. User object stores PasswordHash.
6. Plain password is not stored.
7. User is saved in database.
8. JWT token is generated.
9. Welcome email event is published.
```

### Code Idea

```csharp
var user = new User
{
    FullName = request.FullName,
    Email = request.Email,
    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
    Role = "Candidate",
    IsPremium = false,
    IsActive = true
};
```

### Viva Answer

> During registration, the backend hashes the password using BCrypt and stores only PasswordHash in the database. The plain password is never saved.

---

## 7. Login Password Verification Flow

### Endpoint

```text
POST /api/auth/login
```

### Request

```json
{
  "email": "shobhit@example.com",
  "password": "MyPassword123"
}
```

### Backend Steps

```text
1. AuthService finds user by email.
2. If user does not exist, login fails.
3. Entered password is verified against stored PasswordHash.
4. BCrypt.Verify returns true or false.
5. If false, login fails.
6. If account inactive, login fails.
7. If valid, JWT token is generated.
```

### Code Idea

```csharp
if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
{
    throw new AppException("Invalid credentials.", StatusCodes.Status401Unauthorized);
}
```

### Important Point

Login does not hash the entered password and compare strings manually.

It uses BCrypt.Verify because BCrypt hash contains salt and metadata.

### Viva Answer

> During login, the backend uses BCrypt.Verify to compare the entered password with the stored password hash. If it matches and the account is active, JWT is generated.

---

## 8. Why Not Store Plain Passwords?

### Problems

If passwords are stored directly:

- Database leak exposes all passwords
- Admin/developer can see passwords
- Users using same password elsewhere are at risk
- Application violates basic security practice

### Practical Attack

Database leaked:

```text
student@gmail.com | 123456
```

Attacker can directly login.

With hash:

```text
student@gmail.com | $2a$11$...
```

Attacker must brute-force, which is much harder with BCrypt.

### Viva Answer

> Plain passwords are never stored because database leaks would expose user accounts. Hashing protects passwords by storing only one-way hashes.

---

## 9. Why Not Use MD5 or SHA256 Directly?

### Simple Explanation

MD5 and SHA256 are fast general-purpose hash algorithms.

Fast is bad for password storage because attackers can test millions of guesses quickly.

### Why BCrypt Is Better

BCrypt is slow and salted by design.

It makes brute-force attacks much more expensive.

### Viva Answer

> MD5 and SHA256 are too fast for password storage. BCrypt is better because it is slow by design and includes salting.

---

## 10. Forgot Password Flow

### Simple Explanation

Forgot password flow allows a user to reset password using OTP sent to email.

### Endpoints

Request OTP:

```text
POST /api/auth/forgot-password/request-otp
```

Reset password:

```text
POST /api/auth/forgot-password/reset
```

### Flow Overview

```text
1. User enters email.
2. Backend checks if active user exists.
3. Backend generates 6-digit OTP.
4. OTP is hashed using BCrypt.
5. Hashed OTP is stored in memory cache with expiry.
6. Email event is published to NotificationService.
7. User enters OTP and new password.
8. Backend verifies OTP hash.
9. New password is hashed.
10. PasswordHash is updated in database.
11. OTP cache entry is removed.
```

### Viva Answer

> Forgot password flow generates a short-lived OTP, sends it by email, verifies it, and then updates the user's password hash.

---

## 11. OTP Generation

### Simple Explanation

OTP means One-Time Password.

It is a temporary code used for verification.

### In Your Project

OTP is generated using:

```csharp
var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
```

### Meaning

This generates a secure random number from:

```text
000000 to 999999
```

`D6` ensures it is always 6 digits.

Example:

```text
004921
```

### Why RandomNumberGenerator Is Used

It is cryptographically stronger than normal `Random`.

For security OTPs, stronger randomness is important.

### Viva Answer

> OTP is generated as a 6-digit code using RandomNumberGenerator, which is more secure than normal Random for security-sensitive values.

---

## 12. Why OTP Is Hashed

### Simple Explanation

OTP is also sensitive.

If someone gets server memory/cache data, they should not directly see OTP.

### In Your Project

OTP is stored as:

```csharp
OtpHash = BCrypt.Net.BCrypt.HashPassword(otp)
```

Then verified using:

```csharp
BCrypt.Net.BCrypt.Verify(request.Otp, otpEntry.OtpHash)
```

### Why This Is Good

Even if cache entry is exposed, original OTP is not directly visible.

### Viva Answer

> OTP is hashed before storing because OTP is sensitive like a temporary password. My project uses BCrypt to hash OTP and verify it during reset.

---

## 13. Memory Cache for OTP

### Simple Explanation

Memory cache stores temporary data in application memory.

OTP is temporary, so memory cache is used.

### In Your Project

IdentityService registers memory cache:

```csharp
builder.Services.AddMemoryCache();
```

AuthService uses:

```csharp
private readonly IMemoryCache _memoryCache;
```

OTP is stored using:

```csharp
_memoryCache.Set(cacheKey, otpEntry, options);
```

### Why Memory Cache Is Used

OTP is short-lived.

It does not need permanent database storage.

### Limitation

Memory cache is local to one service instance.

If app restarts, OTP is lost.

If multiple IdentityService instances run, OTP generated on one instance may not exist on another.

### Production Alternatives

- Redis cache
- Database OTP table
- Distributed cache

### Viva Answer

> Memory cache stores OTP temporarily with expiry. It is simple for local development, but Redis or distributed cache is better for production multi-instance deployments.

---

## 14. OTP Expiry

### Simple Explanation

OTP should be valid only for a short time.

If OTP never expires, it becomes a long-term password reset risk.

### In Your Project

Expiry minutes are read from configuration:

```csharp
_config["PasswordResetOtp:ExpiryMinutes"]
```

Default fallback:

```text
10 minutes
```

OTP cache entry stores:

```csharp
ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes)
```

Memory cache also expires:

```csharp
AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes)
```

### Why Both Are Used

Memory cache expiration removes the entry.

`ExpiresAtUtc` gives an extra explicit validation check.

### Viva Answer

> OTP expiry limits password reset risk. My project stores OTP with absolute cache expiration and also checks ExpiresAtUtc before resetting password.

---

## 15. Generic Forgot Password Response

### Simple Explanation

Forgot password should not reveal whether an email exists.

### Bad Response

```text
Email not found.
```

Problem:

Attackers can test emails and discover registered users.

### Good Response

```text
If the account exists, an OTP has been sent to the registered email address.
```

This is what your project uses.

### In Your Project

AuthService returns generic message even when user is not found or inactive.

### Why This Is Secure

It prevents email enumeration.

Email enumeration means attackers discovering which emails are registered.

### Viva Answer

> Forgot-password response is generic to prevent email enumeration. The backend does not reveal whether the email exists.

---

## 16. Request OTP Flow in Detail

### Endpoint

```text
POST /api/auth/forgot-password/request-otp
```

### Request

```json
{
  "email": "student@gmail.com"
}
```

### Backend Steps

```text
1. Controller receives ForgotPasswordOtpRequest.
2. AuthService searches active user by email.
3. If user not found, returns generic success message without email.
4. If user exists, generates 6-digit OTP.
5. OTP is hashed using BCrypt.
6. Cache key is created using normalized email.
7. OTP hash and expiry are stored in memory cache.
8. Controller publishes EmailRequestedEvent.
9. NotificationService sends OTP email.
10. Generic response returns to frontend.
```

### Cache Key

```csharp
$"password-reset-otp:{email.Trim().ToLowerInvariant()}"
```

### Why Normalize Email

To avoid separate cache entries for:

```text
Student@Gmail.com
student@gmail.com
 student@gmail.com
```

### Viva Answer

> Request OTP flow generates a secure 6-digit OTP, stores its BCrypt hash in memory cache with expiry, and sends the OTP by email through RabbitMQ notification event.

---

## 17. Reset Password Flow in Detail

### Endpoint

```text
POST /api/auth/forgot-password/reset
```

### Request

```json
{
  "email": "student@gmail.com",
  "otp": "123456",
  "newPassword": "NewPassword123"
}
```

### Backend Steps

```text
1. Controller receives ResetPasswordWithOtpRequest.
2. AuthService finds active user by email.
3. AuthService gets OTP cache entry.
4. If user or OTP missing, error is returned.
5. Expiry is checked.
6. Entered OTP is verified against OtpHash using BCrypt.
7. If OTP valid, new password is hashed.
8. User.PasswordHash is updated.
9. Database changes are saved.
10. OTP cache entry is removed.
11. Success response is returned.
```

### Why OTP Cache Is Removed

OTP should be one-time use.

After successful reset, the same OTP should not work again.

### Viva Answer

> Reset password verifies the OTP hash and expiry. If valid, the new password is hashed with BCrypt, saved in the database, and the OTP cache entry is removed.

---

## 18. Why OTP Is Sent Through NotificationService

### Simple Explanation

IdentityService should handle identity logic.

NotificationService should handle email sending.

### Flow

```text
IdentityService generates OTP
        ↓
Publishes EmailRequestedEvent
        ↓
RabbitMQ carries event
        ↓
NotificationService sends email
```

### Why This Is Useful

Email sending is external and can be slow.

IdentityService does not wait for SMTP directly.

### Viva Answer

> OTP email is sent through NotificationService using RabbitMQ so IdentityService remains focused on authentication logic and email sending is handled asynchronously.

---

## 19. Password Reset Security Checks

### Checks in Your Project

- User must exist and be active
- OTP must exist in memory cache
- OTP must not be expired
- OTP must match BCrypt hash
- New password is hashed before saving
- OTP is removed after successful reset
- Generic response prevents email enumeration

### Why These Checks Matter

They prevent:

- Expired OTP reuse
- Wrong OTP reset
- Plain password storage
- User enumeration
- Reusing same OTP after reset

### Viva Answer

> Password reset flow includes OTP expiry, hash verification, active user check, password hashing, and OTP removal to make the reset process secure.

---

## 20. What If We Do Not Hash Passwords?

### Problems

- Database leak exposes all passwords
- Developers/admins may see user passwords
- Users may be compromised on other platforms
- Application becomes insecure

### Viva Answer

> Without password hashing, user passwords would be directly exposed if the database is leaked. Hashing is a basic security requirement.

---

## 21. What If We Do Not Hash OTP?

### Problems

- OTP visible in cache
- Anyone with memory/cache access can reset password
- Temporary code acts like plain password

### Viva Answer

> Without OTP hashing, anyone who accesses the cache could see the OTP and reset the user's password. Hashing OTP reduces this risk.

---

## 22. What If OTP Does Not Expire?

### Problems

- Old OTP can be used later
- Password reset risk remains open
- If email is compromised later, old OTP may still work

### Viva Answer

> OTP must expire because it is a temporary security code. Without expiry, password reset remains vulnerable for too long.

---

## 23. What If Forgot Password Reveals Email Does Not Exist?

### Problem

Attackers can test emails:

```text
abc@gmail.com → email not found
student@gmail.com → OTP sent
```

Now attacker knows:

```text
student@gmail.com is registered
```

This is email enumeration.

### Viva Answer

> Revealing whether an email exists allows email enumeration. My project returns a generic forgot-password response to avoid this.

---

## 24. Limitations of Current OTP Implementation

### Limitation 1: Memory Cache Is Not Distributed

If multiple IdentityService instances exist, OTP may exist only on one instance.

### Limitation 2: OTP Lost on Restart

If IdentityService restarts, memory cache is cleared.

### Limitation 3: No Attempt Limit Shown

Repeated wrong OTP attempts should ideally be limited.

### Limitation 4: No Rate Limiting

Attackers could request many OTPs unless rate limiting is added.

### Possible Improvements

- Use Redis distributed cache
- Add OTP attempt limit
- Add resend cooldown
- Add rate limiting per email/IP
- Add audit logs
- Use stronger password policy
- Add email verification

### Balanced Viva Answer

> Current OTP flow is secure for local/demo usage with hashing and expiry, but production can improve it using Redis, rate limiting, attempt limits, and resend cooldown.

---

## 25. Password Strength

### Simple Explanation

Password strength means ensuring users choose hard-to-guess passwords.

### Good Policy

Require:

- Minimum length
- Uppercase/lowercase
- Number
- Special character
- Block common passwords

### Project Note

Your backend hashes passwords, but password policy depends on DTO validation rules and frontend/backend validation.

### Viva Answer

> Password hashing protects stored passwords, but password strength policy is also important to prevent weak passwords like 123456.

---

## 26. Complete Flow: Register and Login Security

### Register

```text
1. User sends password.
2. BCrypt hashes password.
3. PasswordHash is saved.
4. Plain password is discarded.
5. JWT token is returned.
```

### Login

```text
1. User sends password.
2. User is found by email.
3. BCrypt.Verify compares password with hash.
4. If valid and active, JWT is created.
5. User logs in.
```

### Viva Explanation

> Registration hashes password and stores only the hash. Login verifies entered password against the stored hash using BCrypt.

---

## 27. Complete Flow: Forgot Password OTP

```text
1. User requests password reset OTP.
2. Backend returns generic response.
3. If active user exists, OTP is generated.
4. OTP is hashed with BCrypt.
5. Hash is stored in memory cache with expiry.
6. OTP is sent through email event.
7. User submits OTP and new password.
8. Backend verifies OTP hash and expiry.
9. New password is hashed.
10. PasswordHash is updated.
11. OTP is removed from cache.
```

### Viva Explanation

> Forgot-password OTP flow securely resets password using a short-lived hashed OTP sent by email and then updates PasswordHash after verification.

---

## 28. Alternatives

### Password Hashing Alternatives

| Alternative | Notes |
|---|---|
| Argon2 | Modern strong password hashing |
| PBKDF2 | Common and widely supported |
| scrypt | Memory-hard hashing algorithm |
| ASP.NET Core Identity PasswordHasher | Built-in .NET password hashing |

### OTP Storage Alternatives

| Alternative | Notes |
|---|---|
| Redis | Best for distributed cache |
| SQL table | Persistent OTP records |
| Distributed cache | Works across multiple instances |
| External identity provider | Handles reset flow externally |

### Viva Answer

> Alternatives to BCrypt include Argon2, PBKDF2, scrypt, and ASP.NET Core Identity PasswordHasher. OTP can be stored in Redis, database, or distributed cache for production.

---

## 29. Best Full Viva Answer for Topic 8

> My project secures passwords using BCrypt hashing. During registration, the plain password is hashed and only PasswordHash is stored in the database. During login, BCrypt.Verify checks the entered password against the stored hash. BCrypt is used because it is salted and slow by design, which makes brute-force attacks harder. For forgot password, IdentityService generates a secure 6-digit OTP using RandomNumberGenerator, hashes the OTP with BCrypt, stores it in memory cache with expiry, and sends it by email through NotificationService using RabbitMQ. During reset, the OTP hash and expiry are verified, the new password is hashed, saved, and the OTP is removed. The forgot-password response is generic to prevent email enumeration.

---

## 30. Common Viva Questions and Answers

### Q1. Why should passwords not be stored as plain text?

Because if the database leaks, all user passwords become visible. Passwords should be stored as hashes.

### Q2. What is hashing?

Hashing is a one-way process that converts data into a fixed unreadable value.

### Q3. What is the difference between hashing and encryption?

Encryption is reversible with a key. Hashing is one-way and cannot be reversed directly.

### Q4. What is BCrypt?

BCrypt is a password hashing algorithm designed for secure password storage.

### Q5. Why is BCrypt better than SHA256 for passwords?

BCrypt is slow by design and salted, while SHA256 is too fast and easier to brute-force.

### Q6. What is salt?

Salt is random data added to password before hashing so identical passwords produce different hashes.

### Q7. How is password stored in your project?

Only PasswordHash is stored. The plain password is never saved.

### Q8. How is login password verified?

Using BCrypt.Verify with entered password and stored PasswordHash.

### Q9. What is OTP?

OTP is a one-time password used temporarily for verification, usually during password reset.

### Q10. How is OTP generated?

Using RandomNumberGenerator to create a secure 6-digit code.

### Q11. Why is OTP hashed?

Because OTP is sensitive like a temporary password. Hashing prevents direct exposure from cache.

### Q12. Where is OTP stored?

In IdentityService memory cache with expiry.

### Q13. Why should OTP expire?

Because it is temporary. Expiry reduces password reset risk.

### Q14. Why is forgot-password response generic?

To prevent attackers from discovering registered emails.

### Q15. What improvement can be made to OTP storage?

Use Redis or distributed cache in production.

---

## 31. Quick Revision Summary

- Passwords must never be stored as plain text.
- Hashing is one-way.
- Encryption is reversible; hashing is not.
- BCrypt is used for password hashing.
- BCrypt automatically uses salt.
- Register flow stores PasswordHash.
- Login flow uses BCrypt.Verify.
- OTP is used for forgot password.
- OTP is generated using RandomNumberGenerator.
- OTP is 6 digits.
- OTP is hashed before storing.
- OTP is stored in memory cache.
- OTP has expiry.
- OTP is removed after successful reset.
- Forgot-password response is generic.
- Generic response prevents email enumeration.
- NotificationService sends OTP email asynchronously.
- Redis/distributed cache is better for production OTP storage.

