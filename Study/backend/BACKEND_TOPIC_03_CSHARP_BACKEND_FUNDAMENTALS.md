# Topic 3: C# Backend Fundamentals

Project: Mock Interview Platform  
Focus: Understanding the C# language features used in your backend code and how they help build clean, maintainable APIs.

---

## 1. Why C# Fundamentals Matter in Backend

### Simple Explanation

Your backend is written in C#. ASP.NET Core gives the framework, but C# is the language used to write the actual logic.

Whenever your backend does something like:

- Register a user
- Verify a password
- Generate JWT
- Start an interview
- Calculate assessment score
- Save records in database
- Publish RabbitMQ event
- Handle errors

it is using C# concepts.

### Practical Scenario

When a user logs in:

```text
AuthController receives request
        ↓
C# object LoginRequest stores email/password
        ↓
AuthService class handles login logic
        ↓
EF Core LINQ query finds user
        ↓
BCrypt verifies password
        ↓
JWT token string is returned
```

So for viva, you must understand C# classes, objects, interfaces, async/await, collections, LINQ, exception handling, and DTOs.

### Viva Answer

> C# fundamentals are important because all backend business logic in my ASP.NET Core services is written using C# classes, interfaces, async methods, DTOs, LINQ queries, and exception handling.

---

## 2. Classes and Objects

### Simple Explanation

A class is a blueprint.

An object is a real instance created from that blueprint.

### Real-Life Analogy

Class:

```text
Student form format
```

Object:

```text
One filled form for Shobhit
```

### C# Example

```csharp
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

Creating object:

```csharp
var user = new User
{
    FullName = "Shobhit",
    Email = "shobhit@example.com"
};
```

### In Your Project

Your backend has many classes:

```text
User
Interview
Question
Assessment
MCQQuestion
Subscription
PaymentRecord
AuthService
InterviewSvc
AssessmentSvc
SubscriptionSvc
```

Example file:

```text
Backend/IdentityService/Models/User.cs
```

### Why Classes Are Used

Classes organize related data and behavior.

For example, `User` groups user-related data:

```text
Id
FullName
Email
PasswordHash
Role
IsPremium
IsActive
```

### What If We Do Not Use Classes?

Code becomes unstructured.

You would pass separate variables everywhere:

```text
userId, fullName, email, passwordHash, role, isPremium, isActive
```

This becomes hard to maintain.

### Viva Answer

> A class is a blueprint that defines data and behavior. In my project, database entities like User, Interview, Assessment, and Subscription are represented as C# classes.

---

## 3. Properties

### Simple Explanation

Properties are fields of a class with get/set access.

They store data about an object.

### Example

```csharp
public string Email { get; set; } = string.Empty;
```

This means:

- `get`: read the value
- `set`: update the value
- default value is empty string

### In Your Project

User model:

```csharp
public int Id { get; set; }
public string FullName { get; set; } = string.Empty;
public string Email { get; set; } = string.Empty;
public bool IsPremium { get; set; } = false;
```

Assessment model:

```csharp
public int Score { get; set; }
public int MaxScore { get; set; }
public double Percentage { get; set; }
public string Grade { get; set; } = string.Empty;
```

### Why Default Values Are Used

Default values help avoid null errors.

Example:

```csharp
public string Email { get; set; } = string.Empty;
```

means Email will not be null by default.

### Viva Answer

> Properties store data inside C# classes. In my project, model properties represent database columns and DTO properties represent request or response fields.

---

## 4. Constructors

### Simple Explanation

A constructor runs when an object is created.

It is commonly used to provide required dependencies.

### Example from Controller

```csharp
private readonly IAuthService _authService;

public AuthController(IAuthService authService)
{
    _authService = authService;
}
```

Here, ASP.NET Core injects `IAuthService` into the controller.

### In Your Project

Example: AuthService constructor:

```csharp
public AuthService(
    AppDbContext context,
    IConfiguration config,
    ILogger<AuthService> logger,
    IMemoryCache memoryCache)
{
    _context = context;
    _config = config;
    _logger = logger;
    _memoryCache = memoryCache;
}
```

### Why Constructors Are Used

They ensure required dependencies are available before methods run.

AuthService cannot work without:

- Database context
- Configuration
- Logger
- Memory cache

### Viva Answer

> Constructors initialize objects and receive required dependencies. In my project, controllers and services use constructor injection to receive DbContext, configuration, logger, and service interfaces.

---

## 5. Access Modifiers

### Simple Explanation

Access modifiers decide where a class, method, or property can be accessed.

### Common Access Modifiers

| Modifier | Meaning |
|---|---|
| public | Accessible from anywhere |
| private | Accessible only inside same class |
| protected | Accessible in same class and child classes |
| internal | Accessible inside same project/assembly |

### In Your Project

Public controller action:

```csharp
public async Task<IActionResult> Login(...)
```

Private helper method:

```csharp
private string GenerateJwtToken(User user)
```

Private helper methods are used when logic is needed only inside one class.

### Practical Example

`GenerateJwtToken` is private because only `AuthService` should generate token internally.

Frontend or controller should not directly call that helper.

### Viva Answer

> Access modifiers control visibility. Public members are accessible outside the class, while private members are used only internally. My project uses private helper methods for internal service logic like JWT generation.

---

## 6. Interfaces

### Simple Explanation

An interface defines what methods a class must provide, but not how they work.

It is like a contract.

### Real-Life Analogy

Suppose an interface says:

```text
Payment service must have Pay()
```

Different classes can implement it:

```text
StripePaymentService
RazorpayPaymentService
MockPaymentService
```

All follow the same contract, but internal logic is different.

### C# Example

```csharp
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
```

Implementation:

```csharp
public class AuthService : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // actual login logic
    }
}
```

### In Your Project

Interfaces:

```text
IAuthService
IInterviewSvc
IAssessmentService
ISubscriptionSvc
IEmailSender
IRabbitMqPublisher
```

### Why Interfaces Are Used

They help with:

- Loose coupling
- Testability
- Replacing implementation
- Clean architecture

Controller depends on interface:

```csharp
IAuthService
```

not directly on:

```csharp
AuthService
```

### What If We Do Not Use Interfaces?

Controllers become tightly coupled to concrete classes.

Testing becomes harder because fake/mock implementations cannot be easily substituted.

### Viva Answer

> An interface is a contract that defines required methods. My project uses interfaces like IAuthService and IInterviewSvc so controllers depend on abstractions instead of concrete implementations, improving testability and loose coupling.

---

## 7. Fields and readonly

### Simple Explanation

Fields store values inside a class.

`readonly` means the field can be assigned only once, usually in the constructor.

### Example

```csharp
private readonly AppDbContext _context;
private readonly ILogger<AuthService> _logger;
```

### Why readonly Is Used

It prevents accidental reassignment.

Once `_context` is injected in constructor, it should not be replaced later.

### In Your Project

Most services use readonly fields:

```csharp
private readonly AppDbContext _context;
private readonly IConfiguration _config;
private readonly ILogger<AssessmentSvc> _logger;
```

### Viva Answer

> readonly fields are used for dependencies injected through constructors. They prevent accidental reassignment and make service classes safer.

---

## 8. async and await

### Simple Explanation

`async` and `await` allow backend code to perform long-running operations without blocking the server thread.

Database calls, HTTP calls, email sending, and RabbitMQ publishing can take time.

Instead of freezing the thread, C# waits asynchronously.

### Practical Scenario

When user logs in, backend queries database.

Database may take some milliseconds.

With async:

```csharp
var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
```

The server can handle other requests while waiting.

### In Your Project

Async methods:

```csharp
RegisterAsync
LoginAsync
StartInterviewAsync
SubmitAssessmentAsync
ConfirmPaymentAsync
PublishAsync
```

### Task and Task<T>

`Task` means async method returns no direct value.

```csharp
public async Task ResetPasswordWithOtpAsync(...)
```

`Task<T>` means async method returns a value of type T.

```csharp
public async Task<AuthResponse> LoginAsync(...)
```

### What If We Do Not Use async/await?

Blocking calls reduce scalability.

Example:

If 100 users are waiting for database calls, blocked threads can reduce performance.

### Viva Answer

> async and await are used for non-blocking operations such as database queries, HTTP API calls, and RabbitMQ publishing. This improves scalability because server threads are not blocked while waiting.

---

## 9. LINQ

### Simple Explanation

LINQ means Language Integrated Query.

It allows us to query collections and databases using C# syntax.

### Example

```csharp
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == request.Email);
```

This means:

```text
Find the first user whose email matches request.Email
```

EF Core converts this LINQ query into SQL.

### In Your Project

Examples:

Find user:

```csharp
_context.Users.FirstOrDefaultAsync(u => u.Email == request.Email)
```

Count free user interviews:

```csharp
_context.Interviews.CountAsync(i => i.UserId == userId)
```

Fetch assessment results:

```csharp
_context.AssessmentResults.Where(r => r.UserId == userId).ToListAsync()
```

Group submitted answers:

```csharp
request.Answers.GroupBy(a => a.QuestionId).Select(g => g.First()).ToList()
```

### Why LINQ Is Used

LINQ makes database and collection queries readable in C#.

### What If We Do Not Use LINQ?

You would write raw SQL or manual loops.

Raw SQL gives control but can be more verbose and error-prone.

### Viva Answer

> LINQ is used to query data using C# syntax. In my project, LINQ is used with EF Core to fetch users, count attempts, load questions, and calculate results.

---

## 10. Lambda Expressions

### Simple Explanation

Lambda expression is a short way to write a function.

Example:

```csharp
u => u.Email == request.Email
```

Meaning:

```text
For each user u, check if u.Email equals request.Email
```

### In Your Project

```csharp
FirstOrDefaultAsync(u => u.Email == request.Email)
```

```csharp
Where(i => i.UserId == userId)
```

```csharp
Select(q => q.Text)
```

```csharp
Any(a => !validQuestionIds.Contains(a.QuestionId))
```

### Why Lambdas Are Used

They make LINQ queries concise and readable.

### Viva Answer

> Lambda expressions are short inline functions used mostly with LINQ. My project uses them to filter, select, count, and group database records.

---

## 11. Collections

### Simple Explanation

Collections store multiple values.

### Common Collections

| Collection | Use |
|---|---|
| List<T> | Ordered list of items |
| Dictionary<TKey,TValue> | Key-value lookup |
| HashSet<T> | Unique values |
| IEnumerable<T> | Read-only sequence style |

### List<T>

Used when storing multiple items.

Example:

```csharp
var questions = new List<MCQQuestion>();
```

### Dictionary

Used for key-value data.

Example from email model:

```csharp
Model = new Dictionary<string, string>
{
    ["FullName"] = request.FullName
}
```

### HashSet

Used to avoid duplicates and check existence fast.

Example:

```csharp
var validQuestionIds = mcqQuestions.Select(q => q.Id).ToHashSet();
```

### IEnumerable

Used when returning a sequence without exposing exact list implementation.

Example:

```csharp
public async Task<IEnumerable<User>> GetAllUsersAsync()
```

### Viva Answer

> Collections store multiple values. My project uses List for questions and answers, Dictionary for email template data, HashSet for unique checks, and IEnumerable for returning sequences.

---

## 12. Nullable Types

### Simple Explanation

Nullable means a variable can have no value.

In C#, reference types like string can be nullable if marked with `?`.

Example:

```csharp
public string? CorrectAnswer { get; set; }
```

This means CorrectAnswer may be null.

### In Your Project

Interview question model:

```csharp
public string? OptionA { get; set; }
public string? OptionB { get; set; }
public string? CorrectAnswer { get; set; }
```

Because subjective questions may not have MCQ options.

### Nullable Return

```csharp
public async Task<object?> GetResultAsync(...)
```

This means method may return an object or null.

### Why Nullable Types Are Useful

They force developers to think about missing data.

### Viva Answer

> Nullable types indicate that a value may be absent. My project uses nullable properties for optional fields like MCQ options or result objects that may not exist yet.

---

## 13. DTOs vs Models

### Simple Explanation

Model represents database structure.

DTO represents data sent or received by API.

### Model Example

```csharp
public class User
{
    public int Id { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Candidate";
}
```

### DTO Example

```csharp
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### Why Not Use Model Directly?

If frontend directly used `User`, it might expose:

```text
PasswordHash
Role
IsActive
CreatedAt
```

This is unsafe and unnecessary.

### In Your Project

Models:

```text
User
Interview
Assessment
Subscription
PaymentRecord
```

DTOs:

```text
LoginRequest
RegisterRequest
StartAssessmentRequest
SubmitInterviewRequest
ConfirmPaymentRequest
```

### Viva Answer

> Models represent database entities, while DTOs represent API request and response data. DTOs prevent exposing internal database structure directly to frontend.

---

## 14. Exception Handling

### Simple Explanation

Exception handling manages errors in code.

If something goes wrong, backend should not crash or return confusing errors.

### Basic C# Try-Catch

```csharp
try
{
    // risky code
}
catch (Exception ex)
{
    // handle error
}
```

### In Your Project

Your project uses custom exceptions:

```text
AppException
ValidationAppException
NotFoundAppException
ForbiddenAppException
```

Example:

```csharp
throw new ValidationAppException("Domain is required.");
```

```csharp
throw new ForbiddenAppException("Free users can create only 1 interview.");
```

Global middleware catches these exceptions and returns proper API responses.

### Why Custom Exceptions Are Used

They make errors meaningful.

Instead of random generic error:

```text
Something went wrong
```

Frontend gets:

```text
Free users can create only 1 interview. Upgrade to premium.
```

### Viva Answer

> Exception handling is used to manage runtime errors. My project uses custom exceptions and global exception middleware to return consistent API error responses.

---

## 15. Namespaces

### Simple Explanation

Namespace organizes classes and avoids name conflicts.

### Example

```csharp
namespace IdentityService.Services
{
    public class AuthService
    {
    }
}
```

### In Your Project

Examples:

```text
IdentityService.Controllers
IdentityService.Services
InterviewService.Models
AssessmentService.DTOs
BuildingBlocks.Messaging
```

### Why Namespaces Are Used

They group related classes logically.

For example, `AuthService` belongs to:

```text
IdentityService.Services
```

not InterviewService.

### Viva Answer

> Namespaces organize C# classes into logical groups and avoid naming conflicts across services.

---

## 16. using Statements

### Simple Explanation

`using` imports namespaces so we can use classes without writing full names.

### Example

```csharp
using Microsoft.EntityFrameworkCore;
```

This allows:

```csharp
await _context.Users.ToListAsync();
```

Without using, we may need full namespace references.

### In Your Project

Common using statements:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;
```

### Viva Answer

> using statements import namespaces so classes, methods, and extension methods can be used easily in a file.

---

## 17. Extension Methods

### Simple Explanation

Extension methods allow us to add methods to existing types without modifying their source code.

### In Your Project

BuildingBlocks defines extension methods:

```text
Backend/BuildingBlocks/Extensions/ServiceCollectionExtensions.cs
Backend/BuildingBlocks/Extensions/ApplicationBuilderExtensions.cs
```

Examples:

```csharp
builder.Services.AddApiDefaults();
builder.Services.AddRabbitMqMessaging(builder.Configuration);
app.UseGlobalExceptionHandling();
```

### Why Extension Methods Are Useful

They make startup code cleaner.

Instead of writing full RabbitMQ registration in every service, you call:

```csharp
AddRabbitMqMessaging()
```

### Viva Answer

> Extension methods add reusable methods to existing types. My project uses them in BuildingBlocks to register shared API defaults, RabbitMQ messaging, and global exception handling.

---

## 18. Generics

### Simple Explanation

Generics allow code to work with different data types.

`List<T>` means list of any type T.

Examples:

```csharp
List<User>
List<MCQQuestion>
List<InterviewAnswer>
```

### In Your Project

Generic API response:

```csharp
ApiResponse<AuthResponse>
ApiResponse<object>
ApiResponse<IEnumerable<User>>
```

This means the same response wrapper can hold different data types.

### Why Generics Are Useful

They avoid duplicate code.

Instead of creating separate response classes for every type:

```text
UserResponse
InterviewResponse
AssessmentResponse
```

Use:

```text
ApiResponse<T>
```

### Viva Answer

> Generics allow classes and methods to work with different data types. My project uses generic collections like List<T> and generic API responses like ApiResponse<T>.

---

## 19. Anonymous Objects

### Simple Explanation

Anonymous objects are objects created without defining a separate class.

### Example

```csharp
return new
{
    transactionId = paymentRecord.StripePaymentIntentId,
    amount = paymentRecord.Amount,
    status = "Success"
};
```

### In Your Project

Anonymous objects are used when response shape is simple and local to one method.

Example:

```csharp
return new { count, message = "Warm-up triggered." };
```

### When To Use

Use anonymous objects for small, temporary response shapes.

Use DTOs for important or reusable API contracts.

### Viva Answer

> Anonymous objects are used to create simple objects without defining a class. My project uses them for small response payloads, while DTOs are used for formal request and response contracts.

---

## 20. String Interpolation

### Simple Explanation

String interpolation creates strings with variables inside.

### Example

```csharp
$"password-reset-otp:{email.Trim().ToLowerInvariant()}"
```

### In Your Project

Used in:

- Cache keys
- Log messages
- Event messages
- Feedback strings

Example:

```csharp
$"Stripe checkout session {session.Id} created."
```

### Viva Answer

> String interpolation is a readable way to create strings using variables. My project uses it for messages, cache keys, and logs.

---

## 21. DateTime

### Simple Explanation

`DateTime` stores date and time.

### In Your Project

Used for:

- User creation time
- Interview start and completion
- Assessment expiry
- Subscription start and end date
- Payment creation time
- OTP expiry

Examples:

```csharp
DateTime.UtcNow
DateTime.UtcNow.AddMinutes(expiryMinutes)
DateTime.UtcNow.AddDays(30)
```

### Why UtcNow Is Used

UTC is timezone-independent.

If users or servers are in different timezones, UTC avoids confusion.

### Viva Answer

> DateTime is used to store timestamps. My project uses DateTime.UtcNow for consistent timezone-independent records like assessment expiry, subscription dates, and created times.

---

## 22. Logging with ILogger

### Simple Explanation

Logging records what is happening inside backend.

It helps debugging and monitoring.

### Example

```csharp
_logger.LogInformation("User {Email} logged in successfully", user.Email);
```

### In Your Project

Logs are used for:

- Register request
- Login request
- Assessment start
- Interview submission
- Gemini failure
- Stripe webhook processing
- Exception middleware

### Why Logging Is Useful

If something fails in production, logs help understand:

- Which endpoint was called
- Which user was affected
- What error occurred
- Which external API failed

### Important Security Note

Do not log sensitive data like:

- Passwords
- OTP values
- JWT tokens
- Secret keys

### Viva Answer

> ILogger is used for application logging. My project logs important backend events and warnings, which helps debugging and monitoring without exposing sensitive data.

---

## 23. Configuration with IConfiguration

### Simple Explanation

Configuration stores values that may change between environments.

Examples:

- Database connection string
- JWT secret key
- Gemini API key
- Stripe keys
- RabbitMQ settings
- Email settings

### In Your Project

Services inject:

```csharp
private readonly IConfiguration _config;
```

Then read values:

```csharp
var jwtSettings = _config.GetSection("Jwt");
var apiKey = _config["Gemini:ApiKey"];
```

### Why Configuration Is Used

Hardcoding secrets is unsafe.

Bad:

```csharp
var apiKey = "real-secret-key";
```

Good:

```csharp
var apiKey = _config["Gemini:ApiKey"];
```

### Viva Answer

> IConfiguration is used to read settings like JWT keys, connection strings, Gemini API keys, Stripe keys, RabbitMQ settings, and email configuration without hardcoding them in code.

---

## 24. Important C# Concepts Mapped to Your Project

| C# Concept | Project Example |
|---|---|
| Class | User, Interview, Assessment, AuthService |
| Object | new User during registration |
| Property | User.Email, Assessment.Score |
| Constructor | AuthService receives DbContext and logger |
| Interface | IAuthService, IInterviewSvc |
| async/await | LoginAsync, SubmitAssessmentAsync |
| Task<T> | Task<AuthResponse> |
| LINQ | Where, Select, FirstOrDefaultAsync |
| Lambda | u => u.Email == request.Email |
| List<T> | List<MCQQuestion> |
| Dictionary | Email template model |
| HashSet | valid question id lookup |
| Nullable | string? CorrectAnswer |
| Exception | ValidationAppException |
| Extension method | AddRabbitMqMessaging |
| Generic | ApiResponse<T> |
| Logger | ILogger<AuthService> |
| Configuration | IConfiguration |

---

## 25. Complete Practical Flow Using C# Concepts

### Example: User Registration

```text
1. RegisterRequest DTO receives frontend data
2. AuthController action accepts RegisterRequest
3. IAuthService interface is used by controller
4. AuthService class implements register logic
5. Constructor-injected AppDbContext accesses database
6. LINQ checks if email already exists
7. BCrypt hashes password
8. User class object is created
9. SaveChangesAsync saves user
10. GenerateJwtToken private method creates token
11. AuthResponse DTO is returned
12. RabbitMQ publisher publishes events
```

### Concepts Used

```text
DTO
Controller action
Interface
Class
Constructor injection
LINQ
async/await
Object creation
Private helper method
Generic API response
RabbitMQ publisher interface
```

---

## 26. What Happens If These Concepts Are Missing?

| Missing Concept | Problem |
|---|---|
| Classes | Code becomes unstructured |
| Interfaces | Tight coupling and hard testing |
| DTOs | Internal models may be exposed |
| async/await | Server threads may block |
| LINQ | More manual SQL/loops |
| Exceptions | Poor error handling |
| Collections | Hard to manage groups of data |
| Configuration | Secrets may be hardcoded |
| Logging | Difficult to debug issues |
| Extension methods | Duplicate startup code |

---

## 27. Best Full Viva Answer for Topic 3

> My backend uses C# fundamentals throughout the ASP.NET Core services. Classes represent models, DTOs, controllers, and service classes. Interfaces like IAuthService and IInterviewSvc define contracts and support loose coupling. Constructors are used for dependency injection. async and await are used for non-blocking database, HTTP, and messaging operations. LINQ and lambda expressions are used with EF Core to query data. Collections like List, Dictionary, and HashSet manage groups of records. Custom exceptions and global middleware provide clean error handling. IConfiguration manages external settings, and ILogger is used for logging backend events.

---

## 28. Common Viva Questions and Answers

### Q1. What is a class in C#?

A class is a blueprint that defines properties and behavior. In my project, User, Interview, Assessment, and Subscription are classes.

### Q2. What is an object?

An object is an instance of a class. For example, when a new user registers, a User object is created and saved.

### Q3. What is an interface?

An interface is a contract that defines methods without implementation. My project uses interfaces like IAuthService to keep controllers loosely coupled.

### Q4. Why use async and await?

They allow non-blocking operations for database calls, external API calls, and RabbitMQ publishing, improving scalability.

### Q5. What is Task<T>?

Task<T> represents an asynchronous operation that returns a value of type T.

### Q6. What is LINQ?

LINQ is used to query data using C# syntax. EF Core converts LINQ queries into SQL.

### Q7. What is a lambda expression?

A lambda is a short inline function, commonly used in LINQ, such as `u => u.Email == request.Email`.

### Q8. What is the difference between DTO and model?

A model represents database structure, while a DTO represents API request or response data.

### Q9. Why use custom exceptions?

Custom exceptions represent meaningful business errors and help return proper API responses.

### Q10. What is readonly?

readonly means a field can be assigned only once, usually through the constructor. It prevents accidental reassignment of dependencies.

### Q11. Why use IConfiguration?

IConfiguration reads settings like connection strings, JWT secrets, Stripe keys, Gemini keys, and RabbitMQ settings without hardcoding them.

### Q12. Why use ILogger?

ILogger records important backend events and errors, helping with debugging and monitoring.

### Q13. What are extension methods?

Extension methods add reusable methods to existing types. My project uses them for AddApiDefaults, AddRabbitMqMessaging, and UseGlobalExceptionHandling.

### Q14. What are generics?

Generics allow classes and methods to work with different types. My project uses ApiResponse<T> and List<T>.

### Q15. Why use nullable types?

Nullable types show that a value may be absent. My project uses them for optional fields like CorrectAnswer and optional result returns.

---

## 29. Quick Revision Summary

- C# is the language used for backend logic.
- Classes define models, services, controllers, and DTOs.
- Objects are instances of classes.
- Properties store class data.
- Constructors initialize classes and receive dependencies.
- Interfaces define contracts and improve loose coupling.
- readonly protects injected dependencies from reassignment.
- async/await improves scalability.
- Task<T> represents async result.
- LINQ queries data using C# syntax.
- Lambdas are short inline functions used with LINQ.
- Lists, dictionaries, and hash sets manage multiple values.
- DTOs protect internal database models.
- Exceptions handle errors cleanly.
- Namespaces organize code.
- Extension methods reduce duplicate setup code.
- Generics make reusable typed classes.
- ILogger records backend activity.
- IConfiguration reads external settings.

