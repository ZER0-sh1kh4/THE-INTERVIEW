# Topic 9: Entity Framework Core and SQL Server

Project: Mock Interview Platform  
Focus: Understanding how the backend stores data, how EF Core maps C# classes to SQL Server tables, what DbContext and DbSet mean, how LINQ queries become SQL, and how migrations manage database schema.

---

## 1. Why Database Is Needed

### Simple Explanation

Your application needs to remember data even after the user closes the browser or the server restarts.

This permanent storage is handled by the database.

### Practical Scenario

User registers today.

Tomorrow they login again.

The application must remember:

```text
Name
Email
PasswordHash
Role
Premium status
Previous interviews
Assessment results
Payment history
```

This data cannot only live in memory. It must be saved permanently.

### In Your Project

SQL Server stores:

- Users
- Notifications
- Interviews
- Interview questions
- Interview answers
- Interview results
- MCQ questions
- Assessments
- User answers
- Assessment results
- Subscriptions
- Payment records
- Stripe webhook logs

### Viva Answer

> A database is needed to persist user accounts, interviews, assessments, results, payments, subscriptions, and notifications even after the application restarts.

---

## 2. What Is SQL Server?

### Simple Explanation

SQL Server is a relational database management system from Microsoft.

It stores data in tables.

### Example Table

Users table:

| Id | FullName | Email | Role | IsPremium |
|---|---|---|---|---|
| 1 | Shobhit | shobhit@example.com | Candidate | false |

### Why SQL Server Is Used

SQL Server is good for structured data.

Your project has structured records like:

```text
User
Assessment
Interview
PaymentRecord
Subscription
```

Each has fixed fields.

### Viva Answer

> SQL Server is a relational database used in my project to store structured backend data in tables.

---

## 3. What Is Entity Framework Core?

### Simple Explanation

Entity Framework Core, or EF Core, is an ORM.

ORM means Object Relational Mapper.

It allows us to work with database using C# objects instead of writing SQL manually for every operation.

### Without EF Core

You may write SQL manually:

```sql
SELECT * FROM Users WHERE Email = 'student@gmail.com';
```

### With EF Core

You write C#:

```csharp
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == request.Email);
```

EF Core converts this into SQL internally.

### In Your Project

All main services use EF Core:

```text
IdentityService
InterviewService
AssessmentService
SubscriptionService
```

### Viva Answer

> EF Core is an ORM that maps C# classes to SQL Server tables and allows database operations using C# and LINQ instead of writing raw SQL everywhere.

---

## 4. What Is ORM?

### Simple Explanation

ORM connects object-oriented code with relational database tables.

C# side:

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
}
```

Database side:

```text
Users table
Id column
Email column
```

ORM maps:

```text
User class → Users table
User.Email property → Email column
User object → One row in Users table
```

### Why ORM Helps

It reduces repetitive SQL code.

It also makes code easier to read for C# developers.

### Viva Answer

> ORM maps C# classes and objects to database tables and rows. EF Core is the ORM used in my project.

---

## 5. Model Class to Database Table Mapping

### Simple Explanation

In EF Core, model classes become database tables.

Properties become columns.

Objects become rows.

### Example: User Model

File:

```text
Backend/IdentityService/Models/User.cs
```

Model:

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

Database table:

```text
Users
```

Columns:

```text
Id
FullName
Email
PasswordHash
Role
IsPremium
IsActive
CreatedAt
```

### Viva Answer

> EF Core maps model classes to database tables. For example, the User class becomes the Users table and its properties become table columns.

---

## 6. What Is DbContext?

### Simple Explanation

DbContext is the bridge between C# code and database.

It represents a database session.

Through DbContext, backend can:

- Read data
- Insert data
- Update data
- Delete data
- Save changes

### Real-Life Analogy

Think of DbContext as a database manager sitting between your service class and SQL Server.

Your service says:

```text
Find this user.
Add this assessment.
Save this payment.
```

DbContext handles database communication.

### In Your Project

Each service has its own AppDbContext:

```text
Backend/IdentityService/Data/AppDbContext.cs
Backend/InterviewService/Data/AppDbContext.cs
Backend/AssessmentService/Data/AppDbContext.cs
Backend/SubscriptionService/Data/AppDbContext.cs
```

### Viva Answer

> DbContext is the main EF Core class that manages database access. Each service in my project has its own AppDbContext for its own tables.

---

## 7. What Is DbSet?

### Simple Explanation

DbSet represents a table in the database.

Example:

```csharp
public DbSet<User> Users { get; set; }
```

This means:

```text
Users table
```

### In Your Project

IdentityService DbSets:

```csharp
public DbSet<User> Users { get; set; }
public DbSet<Notification> Notifications { get; set; }
```

InterviewService DbSets:

```csharp
public DbSet<Interview> Interviews { get; set; }
public DbSet<Question> Questions { get; set; }
public DbSet<InterviewAnswer> InterviewAnswers { get; set; }
public DbSet<InterviewResult> InterviewResults { get; set; }
public DbSet<GlobalInterviewQuestion> GlobalInterviewQuestions { get; set; }
```

AssessmentService DbSets:

```csharp
public DbSet<MCQQuestion> MCQQuestions { get; set; }
public DbSet<Assessment> Assessments { get; set; }
public DbSet<UserAnswer> UserAnswers { get; set; }
public DbSet<AssessmentResult> AssessmentResults { get; set; }
```

SubscriptionService DbSets:

```csharp
public DbSet<Subscription> Subscriptions { get; set; }
public DbSet<PaymentRecord> PaymentRecords { get; set; }
public DbSet<WebhookEventLog> WebhookEventLogs { get; set; }
```

### Viva Answer

> DbSet represents a database table. In my project, DbSet<User> represents Users table, DbSet<Assessment> represents Assessments table, and so on.

---

## 8. Why Each Service Has Its Own DbContext

### Simple Explanation

Your backend is service-based.

Each service owns its own business data.

### In Your Project

IdentityService owns:

```text
Users
Notifications
```

InterviewService owns:

```text
Interviews
Questions
InterviewAnswers
InterviewResults
GlobalInterviewQuestions
```

AssessmentService owns:

```text
MCQQuestions
Assessments
UserAnswers
AssessmentResults
```

SubscriptionService owns:

```text
Subscriptions
PaymentRecords
WebhookEventLogs
```

### Why This Is Useful

It keeps data responsibility clear.

InterviewService should not directly manage user passwords.

SubscriptionService should not directly manage MCQ questions.

### Microservices Idea

In ideal microservices, each service can even have its own database.

Your project follows that separation at code/DbContext level.

### Viva Answer

> Each service has its own DbContext because each service owns different data. This keeps responsibilities separated and supports microservices-style architecture.

---

## 9. Registering DbContext in Program.cs

### Simple Explanation

Before using DbContext, we must register it in dependency injection.

### In Your Project

Services register DbContext like this:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Meaning

```text
Use AppDbContext
Connect it to SQL Server
Read connection string named DefaultConnection
```

### Why Connection String Is Used

Connection string tells backend:

- SQL Server location
- Database name
- Authentication details

### Viva Answer

> DbContext is registered in Program.cs using AddDbContext and configured to use SQL Server with the DefaultConnection connection string.

---

## 10. Dependency Injection of DbContext

### Simple Explanation

After registration, ASP.NET Core injects DbContext into services.

### Example

```csharp
private readonly AppDbContext _context;

public AuthService(AppDbContext context)
{
    _context = context;
}
```

### In Your Project

Services using DbContext:

```text
AuthService
InterviewSvc
AssessmentSvc
SubscriptionSvc
```

### Why This Is Useful

Service classes can perform database operations without manually creating database connections.

### Viva Answer

> DbContext is injected into service classes through constructor injection, allowing services to perform database operations cleanly.

---

## 11. CRUD Operations with EF Core

CRUD means:

```text
Create
Read
Update
Delete
```

### Create

Example: Register user.

```csharp
_context.Users.Add(user);
await _context.SaveChangesAsync();
```

### Read

Example: Find user by email.

```csharp
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == request.Email);
```

### Update

Example: Update premium status.

```csharp
user.IsPremium = true;
await _context.SaveChangesAsync();
```

### Delete

Example: Delete MCQ question.

```csharp
_context.MCQQuestions.Remove(question);
await _context.SaveChangesAsync();
```

### Viva Answer

> EF Core supports CRUD operations using DbSet methods like Add, query methods like FirstOrDefaultAsync, property updates, Remove, and SaveChangesAsync.

---

## 12. SaveChangesAsync

### Simple Explanation

Changing C# objects does not immediately update database.

`SaveChangesAsync()` commits changes to database.

### Example

```csharp
user.FullName = "New Name";
await _context.SaveChangesAsync();
```

Without `SaveChangesAsync`, change may not be saved permanently.

### In Your Project

Used after:

- Registering user
- Updating password
- Starting interview
- Submitting assessment
- Confirming payment
- Updating subscription
- Deleting question

### Viva Answer

> SaveChangesAsync commits pending EF Core changes to the database asynchronously.

---

## 13. Async Database Calls

### Simple Explanation

Database operations may take time.

Async methods prevent server thread blocking.

### Examples

```csharp
await _context.Users.FirstOrDefaultAsync(...)
await _context.Interviews.CountAsync(...)
await _context.AssessmentResults.ToListAsync()
await _context.SaveChangesAsync()
```

### Why Async Is Used

While waiting for database response, server can handle other requests.

This improves scalability.

### Viva Answer

> Async EF Core methods are used so database operations do not block server threads, improving API scalability.

---

## 14. LINQ with EF Core

### Simple Explanation

LINQ lets you write database queries in C#.

### Project Examples

Find user:

```csharp
await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email)
```

Count user interviews:

```csharp
await _context.Interviews.CountAsync(i => i.UserId == userId)
```

Get user's assessment results:

```csharp
await _context.AssessmentResults
    .Where(r => r.UserId == userId)
    .ToListAsync()
```

Fetch questions by domain:

```csharp
await _context.MCQQuestions
    .Where(q => q.Domain == domain)
    .ToListAsync()
```

### What EF Core Does

EF Core converts LINQ into SQL query.

### Viva Answer

> LINQ is used with EF Core to query SQL Server using C# syntax. EF Core translates LINQ queries into SQL.

---

## 15. FirstOrDefaultAsync, FindAsync, ToListAsync, CountAsync

### FirstOrDefaultAsync

Returns first matching record or null.

Example:

```csharp
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == request.Email);
```

Use when searching with condition.

### FindAsync

Finds by primary key.

Example:

```csharp
var user = await _context.Users.FindAsync(id);
```

Use when you have primary key.

### ToListAsync

Returns multiple records as list.

Example:

```csharp
var users = await _context.Users.ToListAsync();
```

### CountAsync

Counts records.

Example:

```csharp
var totalAttempts = await _context.Assessments
    .CountAsync(a => a.UserId == userId);
```

### Viva Answer

> FirstOrDefaultAsync finds one record by condition, FindAsync finds by primary key, ToListAsync returns multiple records, and CountAsync counts matching records.

---

## 16. Entity Tracking

### Simple Explanation

EF Core tracks objects loaded from database.

If you modify a tracked object and call SaveChangesAsync, EF Core updates the database.

### Example

```csharp
var user = await _context.Users.FindAsync(id);
user.IsPremium = true;
await _context.SaveChangesAsync();
```

EF Core knows:

```text
This user was changed.
Update database row.
```

### In Your Project

Used when updating:

- User role
- Premium flag
- PasswordHash
- Assessment status
- Subscription status
- Payment status

### Viva Answer

> EF Core tracks loaded entities. When their properties change, SaveChangesAsync updates the corresponding database rows.

---

## 17. Primary Key

### Simple Explanation

Primary key uniquely identifies a row in a table.

### In Your Project

Most models use:

```csharp
public int Id { get; set; }
```

Examples:

```text
User.Id
Interview.Id
Assessment.Id
Subscription.Id
PaymentRecord.Id
```

### Why Needed

To fetch, update, or delete a specific record.

Example:

```text
GET /api/interviews/5
```

uses interview id.

### Viva Answer

> Primary key uniquely identifies each database row. Most models in my project use Id as primary key.

---

## 18. Foreign Key Concept

### Simple Explanation

Foreign key connects one table to another.

Even if explicit navigation properties are not heavily used, your project stores IDs to connect records.

### Examples

Interview:

```text
Interview.UserId
```

means interview belongs to a user.

Question:

```text
Question.InterviewId
```

means question belongs to an interview.

InterviewAnswer:

```text
InterviewAnswer.InterviewId
InterviewAnswer.QuestionId
InterviewAnswer.UserId
```

AssessmentResult:

```text
AssessmentResult.AssessmentId
AssessmentResult.UserId
```

PaymentRecord:

```text
PaymentRecord.SubscriptionId
PaymentRecord.UserId
```

### Why Needed

It connects related data.

For example:

```text
User 5 has Assessment 10
Assessment 10 has UserAnswers
Assessment 10 has AssessmentResult
```

### Viva Answer

> Foreign keys connect related records. My project uses fields like UserId, InterviewId, AssessmentId, QuestionId, and SubscriptionId to link data.

---

## 19. Relationships in Your Project

### IdentityService

```text
User
Notification
```

Notifications can be associated with users.

### InterviewService

```text
One user → many interviews
One interview → many questions
One interview → many answers
One interview → one result
```

### AssessmentService

```text
One user → many assessments
One assessment → many user answers
One assessment → one result
MCQQuestion → referenced by UserAnswer
```

### SubscriptionService

```text
One user → many subscriptions
One subscription → many payment records
WebhookEventLog stores processed Stripe events
```

### Viva Answer

> The project data is connected using IDs. A user can have many interviews, assessments, subscriptions, answers, and results.

---

## 20. OnModelCreating

### Simple Explanation

`OnModelCreating` customizes how EF Core maps models to database.

### In IdentityService

It seeds initial users:

```csharp
modelBuilder.Entity<User>().HasData(...)
```

This creates default users like an admin/candidate during migration.

### In SubscriptionService

It configures precision:

```csharp
modelBuilder.Entity<Subscription>()
    .Property(x => x.Price)
    .HasPrecision(18, 2);
```

Payment amount precision:

```csharp
modelBuilder.Entity<PaymentRecord>()
    .Property(x => x.Amount)
    .HasPrecision(18, 2);
```

Webhook unique index:

```csharp
modelBuilder.Entity<WebhookEventLog>()
    .HasIndex(x => x.EventId)
    .IsUnique();
```

### Why Unique EventId Matters

Stripe may send duplicate webhook events.

Unique index helps prevent duplicate event processing records.

### Viva Answer

> OnModelCreating customizes EF Core model configuration. My project uses it for seed users, decimal precision, and unique webhook event IDs.

---

## 21. Data Seeding

### Simple Explanation

Seeding means inserting initial data into database automatically.

### In Your Project

IdentityService seeds:

- Admin user
- Candidate user

Using:

```csharp
HasData(...)
```

### Why Useful

For testing and demo, you already have users available.

Admin can login and manage data.

### Important Security Note

In real production, seeded default passwords must be changed or avoided.

### Viva Answer

> Data seeding inserts initial records into the database. My IdentityService seeds default users for demo/testing.

---

## 22. Decimal Precision

### Simple Explanation

Money values need proper decimal precision.

Floating-point numbers can create rounding errors.

### In Your Project

Subscription price:

```csharp
HasPrecision(18, 2)
```

Payment amount:

```csharp
HasPrecision(18, 2)
```

### Meaning

`18,2` means:

```text
18 total digits
2 digits after decimal
```

Example:

```text
499.00
```

### Viva Answer

> Decimal precision is configured for money fields like subscription price and payment amount to store currency values accurately.

---

## 23. Unique Index

### Simple Explanation

Unique index prevents duplicate values in a column.

### In Your Project

WebhookEventLog has unique EventId:

```csharp
modelBuilder.Entity<WebhookEventLog>()
    .HasIndex(x => x.EventId)
    .IsUnique();
```

### Why Needed

Stripe may send the same webhook event multiple times.

Unique EventId helps prevent duplicate processing records.

### Viva Answer

> Unique index ensures a value cannot be repeated. My project uses unique EventId for webhook logs to avoid duplicate Stripe event records.

---

## 24. EF Core Migrations

### Simple Explanation

Migrations are version history for database schema.

When model classes change, migration records how database should change.

### Example

If you add a new property:

```csharp
public bool IsActive { get; set; }
```

Migration may create SQL like:

```sql
ALTER TABLE Users ADD IsActive bit NOT NULL;
```

### In Your Project

Migration folders:

```text
Backend/IdentityService/Migrations
Backend/InterviewService/Migrations
Backend/AssessmentService/Migrations
Backend/SubscriptionService/Migrations
```

### Why Migrations Are Used

They keep database schema synchronized with C# models.

### Viva Answer

> EF Core migrations track database schema changes over time and update SQL Server to match C# model changes.

---

## 25. Migration Snapshot

### Simple Explanation

Model snapshot stores EF Core's current understanding of database model.

### In Your Project

Files like:

```text
AppDbContextModelSnapshot.cs
```

exist in migration folders.

### Why Snapshot Exists

EF Core compares current models with snapshot to generate new migrations.

### Viva Answer

> Model snapshot represents the current EF Core model state and helps EF Core detect changes for future migrations.

---

## 26. Database Update

### Simple Explanation

Creating migration only creates migration files.

Database must be updated to apply changes.

Common command:

```text
dotnet ef database update
```

### In Your Project

InterviewService also runs migration at startup:

```csharp
await db.Database.MigrateAsync();
```

### What MigrateAsync Does

It applies pending migrations automatically when service starts.

### Viva Answer

> Database update applies migration changes to SQL Server. InterviewService uses MigrateAsync at startup to apply pending migrations automatically.

---

## 27. Pending Model Changes Warning

### In Your Project

DbContext registration includes:

```csharp
.ConfigureWarnings(w => 
    w.Ignore(RelationalEventId.PendingModelChangesWarning))
```

### Simple Explanation

EF Core warns when model and migration snapshot may not match.

Your project suppresses that warning.

### Viva-Safe Explanation

> The project suppresses pending model changes warning to avoid startup noise during development, but ideally migrations should be kept in sync with model changes.

---

## 28. IdentityService Database Details

### DbContext

```text
Backend/IdentityService/Data/AppDbContext.cs
```

### Tables

```text
Users
Notifications
```

### User Data

Stores:

```text
FullName
Email
PasswordHash
Role
IsPremium
IsActive
CreatedAt
```

### Used For

- Register
- Login
- JWT claims
- Admin user management
- Premium status update
- Forgot password reset
- Notifications

### Viva Answer

> IdentityService database stores users and notifications. User data is used for authentication, roles, premium status, and account management.

---

## 29. InterviewService Database Details

### DbContext

```text
Backend/InterviewService/Data/AppDbContext.cs
```

### Tables

```text
Interviews
Questions
InterviewAnswers
InterviewResults
GlobalInterviewQuestions
```

### Used For

- Saving interview sessions
- Saving generated questions
- Saving submitted answers
- Saving AI evaluation result
- Caching global interview questions

### Practical Flow

```text
Start interview → Interviews table
Begin interview → Questions table
Submit answers → InterviewAnswers table
Result generated → InterviewResults table
Warm-up/cache → GlobalInterviewQuestions table
```

### Viva Answer

> InterviewService stores interview sessions, questions, submitted answers, results, and global cached questions for reuse.

---

## 30. AssessmentService Database Details

### DbContext

```text
Backend/AssessmentService/Data/AppDbContext.cs
```

### Tables

```text
MCQQuestions
Assessments
UserAnswers
AssessmentResults
```

### Used For

- Question bank
- Assessment attempt tracking
- User selected answers
- Score and result storage

### Practical Flow

```text
Start assessment → Assessments table
Questions loaded/generated → MCQQuestions table
Submit answers → UserAnswers table
Calculate result → AssessmentResults table
```

### Viva Answer

> AssessmentService stores MCQ questions, assessment attempts, user answers, and assessment results.

---

## 31. SubscriptionService Database Details

### DbContext

```text
Backend/SubscriptionService/Data/AppDbContext.cs
```

### Tables

```text
Subscriptions
PaymentRecords
WebhookEventLogs
```

### Used For

- Premium subscription records
- Payment history
- Stripe session/payment IDs
- Webhook duplicate detection

### Practical Flow

```text
Subscribe → PaymentRecords pending
Payment success → PaymentRecords success
Premium activated → Subscriptions active
Stripe event received → WebhookEventLogs record
```

### Viva Answer

> SubscriptionService stores subscriptions, payment records, and webhook event logs for payment and premium lifecycle management.

---

## 32. Why UserId Is Stored Across Services

### Simple Explanation

Services need to connect their records to the user.

But they do not store full user object everywhere.

They store:

```text
UserId
```

### Examples

```text
Interview.UserId
Assessment.UserId
AssessmentResult.UserId
Subscription.UserId
PaymentRecord.UserId
```

### Why This Is Useful

Each service can store user-related data without duplicating full user details.

### Important Microservices Note

In service-based systems, services often reference user by ID instead of database foreign key across service databases.

### Viva Answer

> UserId connects records to the authenticated user across services. Services store UserId instead of duplicating complete user data.

---

## 33. Database Ownership and Security

### Simple Explanation

Backend must ensure users access only their own records.

### Example

Assessment result:

```csharp
if (assessment.UserId != userId)
{
    throw new ForbiddenAppException(...);
}
```

### Why Needed

If user changes URL:

```text
/api/assessments/5/result
```

to:

```text
/api/assessments/6/result
```

Backend must check whether assessment 6 belongs to current user.

### Viva Answer

> Database records include UserId so services can enforce ownership checks and prevent users from accessing others' data.

---

## 34. Raw SQL vs EF Core

### EF Core Benefits

- Less boilerplate SQL
- LINQ queries
- Strong typing
- Migrations
- Change tracking
- Easier CRUD operations
- Works well with dependency injection

### Raw SQL Benefits

- More control
- Useful for complex optimized queries
- Can call stored procedures

### In Your Project

EF Core is suitable because most operations are normal CRUD and query flows.

### Viva Answer

> EF Core reduces manual SQL and provides LINQ, migrations, and change tracking. Raw SQL can be used for complex performance-critical queries, but EF Core is suitable for this project.

---

## 35. EF Core vs Dapper

### EF Core

Full ORM:

- Tracks entities
- Supports migrations
- Maps classes to tables
- Higher abstraction

### Dapper

Micro ORM:

- Lightweight
- Faster for raw queries
- Less abstraction
- You write more SQL

### Viva Answer

> EF Core is a full ORM with migrations and tracking, while Dapper is a lightweight micro ORM where developers write more SQL. My project uses EF Core for productivity and maintainability.

---

## 36. In-Memory Database for Tests

### Simple Explanation

Tests should not always use real SQL Server.

EF Core provides InMemory provider for testing service logic.

### In Your Project

Package exists:

```text
Microsoft.EntityFrameworkCore.InMemory
```

Test projects:

```text
IdentityService.Tests
InterviewService.Tests
AssessmentService.Tests
SubscriptionService.Tests
```

### Why Useful

Tests run faster and do not require real database setup.

### Viva Answer

> EF Core InMemory database is useful for unit tests because service logic can be tested without connecting to real SQL Server.

---

## 37. Complete Flow: Register User with EF Core

```text
1. Frontend sends register request.
2. AuthService checks existing user using LINQ.
3. BCrypt hashes password.
4. New User object is created.
5. _context.Users.Add(user) marks it for insertion.
6. SaveChangesAsync inserts row into Users table.
7. JWT token is generated.
```

### Viva Explanation

> During registration, EF Core checks for existing email, adds a new User entity to Users DbSet, and SaveChangesAsync inserts it into SQL Server.

---

## 38. Complete Flow: Submit Assessment with EF Core

```text
1. Assessment is loaded by AssessmentId.
2. Backend checks UserId ownership.
3. Submitted MCQ questions are loaded from MCQQuestions table.
4. UserAnswers are created.
5. Assessment status changes to Completed.
6. AssessmentResult is created.
7. SaveChangesAsync stores answers and result.
```

### Viva Explanation

> Submit assessment uses EF Core to load assessment and questions, save user answers, update assessment status, and insert assessment result.

---

## 39. Complete Flow: Stripe Webhook with EF Core

```text
1. Stripe webhook arrives.
2. WebhookEventLogs checks duplicate EventId.
3. PaymentRecord is found by StripeSessionId.
4. Payment status changes to Success.
5. Subscription is created or updated as Active.
6. WebhookEventLog is saved.
7. SaveChangesAsync commits payment and subscription data.
```

### Viva Explanation

> SubscriptionService uses EF Core to update payment status, activate subscription, and store webhook event log to prevent duplicate processing.

---

## 40. What Happens If EF Core Is Not Used?

### Alternatives

You would need:

- Raw SQL queries
- Manual database connection management
- Manual object mapping
- Manual schema update scripts

### Problems

- More code
- More chance of SQL mistakes
- Harder migrations
- More repetitive CRUD logic

### Viva Answer

> Without EF Core, we would need raw SQL or another ORM like Dapper. EF Core reduces boilerplate and manages mapping, tracking, and migrations.

---

## 41. Possible Improvements

### Improvements

- Add explicit relationships with navigation properties
- Add indexes on frequently searched fields like Email, UserId, Domain
- Add unique index on User.Email
- Add audit fields like UpdatedAt
- Use AsNoTracking for read-only queries
- Use database transactions for multi-step critical operations
- Use separate physical databases per service in production
- Add soft delete for admin-managed question bank
- Add pagination for admin lists

### Balanced Viva Answer

> Current EF Core implementation supports core CRUD and migrations. Future improvements could include more indexes, explicit relationships, transactions, AsNoTracking for reads, pagination, and stronger database constraints.

---

## 42. Best Full Viva Answer for Topic 9

> My project uses SQL Server as the relational database and Entity Framework Core as the ORM. EF Core maps C# model classes to database tables using DbContext and DbSet. Each backend service has its own AppDbContext because each service owns different data: IdentityService stores users and notifications, InterviewService stores interviews, questions, answers, and results, AssessmentService stores MCQ questions, assessments, answers, and results, and SubscriptionService stores subscriptions, payments, and webhook logs. Services use LINQ methods like FirstOrDefaultAsync, Where, CountAsync, and ToListAsync to query data, and SaveChangesAsync to commit changes. EF Core migrations manage schema changes over time.

---

## 43. Common Viva Questions and Answers

### Q1. What is SQL Server?

SQL Server is a relational database used to store structured data in tables.

### Q2. What is EF Core?

EF Core is an ORM that maps C# classes to database tables and allows querying using C# LINQ.

### Q3. What is ORM?

ORM means Object Relational Mapper. It maps objects to database rows and classes to tables.

### Q4. What is DbContext?

DbContext is the main EF Core class that manages database operations and represents a database session.

### Q5. What is DbSet?

DbSet represents a database table for a specific entity type.

### Q6. Why does each service have its own DbContext?

Because each service owns different data and responsibilities.

### Q7. What is SaveChangesAsync?

It commits pending EF Core changes to the database.

### Q8. What is migration?

Migration is a versioned database schema change generated from model changes.

### Q9. What is primary key?

A primary key uniquely identifies each row in a table.

### Q10. What is foreign key?

A foreign key connects one table record to another, such as AssessmentId or UserId.

### Q11. What is LINQ?

LINQ allows querying data using C# syntax. EF Core converts it into SQL.

### Q12. What is entity tracking?

EF Core tracks loaded entities and saves property changes during SaveChangesAsync.

### Q13. What is data seeding?

Data seeding inserts initial records into the database, such as default users.

### Q14. Why configure decimal precision?

To store money values accurately with fixed decimal places.

### Q15. Why use unique index on webhook EventId?

To prevent duplicate Stripe webhook event records.

### Q16. What is EF Core InMemory used for?

It is used for testing service logic without connecting to real SQL Server.

### Q17. What are alternatives to EF Core?

Dapper, ADO.NET, NHibernate, raw SQL, or stored procedures.

---

## 44. Quick Revision Summary

- SQL Server stores persistent structured data.
- EF Core is the ORM.
- ORM maps classes to tables and objects to rows.
- DbContext is bridge between code and database.
- DbSet represents a table.
- Each service has its own AppDbContext.
- IdentityService stores Users and Notifications.
- InterviewService stores Interviews, Questions, Answers, Results, GlobalInterviewQuestions.
- AssessmentService stores MCQQuestions, Assessments, UserAnswers, AssessmentResults.
- SubscriptionService stores Subscriptions, PaymentRecords, WebhookEventLogs.
- LINQ queries are converted to SQL.
- SaveChangesAsync commits changes.
- Migrations manage schema changes.
- Primary key uniquely identifies rows.
- Foreign key connects related records.
- EF Core tracks loaded entities.
- OnModelCreating customizes mapping.
- Data seeding inserts initial users.
- Decimal precision is used for money.
- Unique index prevents duplicate webhook event IDs.

