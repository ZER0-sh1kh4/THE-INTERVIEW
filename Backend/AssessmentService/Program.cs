using BuildingBlocks.Extensions;
using AssessmentService.Data;
using AssessmentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AssessmentService",
        Version = "v1",
        Description = "Manages MCQ assessments, submissions and results."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
    c.OperationFilter<AuthorizeOperationFilter>();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>(); // FRESHLY ADDED: enables backend Gemini calls for assessment questions.
builder.Services.AddRabbitMqMessaging(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]!;

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seed C# questions ONLY if the DB has none for this domain (JIT cache bootstrap)
    var csharpCount = await db.MCQQuestions.CountAsync(q => q.Domain == "C#");
    if (csharpCount == 0)
    {
        var seedQuestions = new List<AssessmentService.Models.MCQQuestion>
        {
            new() { Domain = "C#", Text = "In a production ASP.NET Core application, which middleware ordering issue would cause authentication to silently fail for all API endpoints?", OptionA = "Placing UseAuthentication() after UseAuthorization()", OptionB = "Placing UseRouting() before UseAuthentication()", OptionC = "Placing UseCors() after UseAuthentication()", OptionD = "Placing UseEndpoints() before UseRouting()", CorrectOption = "A", Subtopic = "ASP.NET Core", Marks = 1, OrderIndex = 1 },
            new() { Domain = "C#", Text = "You have a high-throughput service processing 10,000 requests per second. Which collection should you use for a thread-safe producer-consumer pattern?", OptionA = "List<T> with lock statements", OptionB = "ConcurrentQueue<T>", OptionC = "BlockingCollection<T>", OptionD = "Dictionary<int, T> with Mutex", CorrectOption = "C", Subtopic = "Concurrency", Marks = 1, OrderIndex = 2 },
            new() { Domain = "C#", Text = "A developer notices that their async method is blocking the UI thread. Which line is most likely causing the issue?", OptionA = "await Task.Run(() => HeavyWork());", OptionB = "var result = GetDataAsync().Result;", OptionC = "await Task.Delay(1000);", OptionD = "await File.ReadAllTextAsync(path);", CorrectOption = "B", Subtopic = "Async/Await", Marks = 1, OrderIndex = 3 },
            new() { Domain = "C#", Text = "In Entity Framework Core, which approach prevents the N+1 query problem when loading related entities?", OptionA = "Using .Find() in a loop", OptionB = "Using .Include() with eager loading", OptionC = "Using lazy loading proxies", OptionD = "Using .AsNoTracking() on every query", CorrectOption = "B", Subtopic = "Entity Framework", Marks = 1, OrderIndex = 4 },
            new() { Domain = "C#", Text = "Which SOLID principle is violated when a single class handles both database access and email notifications?", OptionA = "Open/Closed Principle", OptionB = "Liskov Substitution Principle", OptionC = "Single Responsibility Principle", OptionD = "Interface Segregation Principle", CorrectOption = "C", Subtopic = "Design Principles", Marks = 1, OrderIndex = 5 },
            new() { Domain = "C#", Text = "A memory profiler shows your application has a growing number of Gen2 objects. Which scenario most likely causes this?", OptionA = "Frequent string concatenation in a loop using StringBuilder", OptionB = "Storing large byte arrays that survive multiple garbage collections", OptionC = "Using value types (structs) for small data objects", OptionD = "Disposing IDisposable objects promptly with using statements", CorrectOption = "B", Subtopic = "Memory Management", Marks = 1, OrderIndex = 6 },
            new() { Domain = "C#", Text = "Which dependency injection lifetime should you use for a DbContext in an ASP.NET Core web API to avoid concurrency issues?", OptionA = "Singleton", OptionB = "Transient", OptionC = "Scoped", OptionD = "Static instance", CorrectOption = "C", Subtopic = "Dependency Injection", Marks = 1, OrderIndex = 7 },
            new() { Domain = "C#", Text = "You need to implement a retry pattern for an HTTP client that calls an external payment API. Which library and strategy is most appropriate?", OptionA = "System.Timers.Timer with manual retry loops", OptionB = "Polly with exponential backoff and jitter", OptionC = "Thread.Sleep in a catch block", OptionD = "TaskCompletionSource with cancellation tokens", CorrectOption = "B", Subtopic = "Resilience Patterns", Marks = 1, OrderIndex = 8 },
            new() { Domain = "C#", Text = "What is the key difference between IEnumerable<T> and IQueryable<T> when querying a database?", OptionA = "IEnumerable executes queries on the server, IQueryable on the client", OptionB = "IQueryable builds expression trees and defers execution to the database provider", OptionC = "IEnumerable supports async operations, IQueryable does not", OptionD = "There is no difference; they are interchangeable", CorrectOption = "B", Subtopic = "LINQ", Marks = 1, OrderIndex = 9 },
            new() { Domain = "C#", Text = "In a microservices architecture, which pattern should you implement to handle partial failures when one downstream service is unavailable?", OptionA = "Synchronous retry with no delay", OptionB = "Circuit Breaker pattern", OptionC = "Increasing the HTTP timeout to 5 minutes", OptionD = "Catching all exceptions and returning HTTP 200", CorrectOption = "B", Subtopic = "Design Patterns", Marks = 1, OrderIndex = 10 },
            new() { Domain = "C#", Text = "Which C# feature allows you to add methods to an existing type without modifying its source code or creating a derived type?", OptionA = "Partial classes", OptionB = "Extension methods", OptionC = "Abstract classes", OptionD = "Operator overloading", CorrectOption = "B", Subtopic = "Language Features", Marks = 1, OrderIndex = 11 },
            new() { Domain = "C#", Text = "A developer wraps a database call in a using statement but the connection is still leaking. Which scenario would cause this?", OptionA = "The using block catches and rethrows exceptions", OptionB = "An unhandled exception occurs before the using block is entered", OptionC = "The connection string specifies Pooling=true", OptionD = "The DbContext is registered as Scoped in DI", CorrectOption = "B", Subtopic = "Resource Management", Marks = 1, OrderIndex = 12 },
            new() { Domain = "C#", Text = "Which pattern is most appropriate for decoupling event producers from consumers in a large C# application?", OptionA = "Singleton pattern", OptionB = "Observer pattern or event aggregator", OptionC = "Factory method pattern", OptionD = "Adapter pattern", CorrectOption = "B", Subtopic = "Design Patterns", Marks = 1, OrderIndex = 13 },
            new() { Domain = "C#", Text = "When using record types in C#, what is the default behavior for equality comparison?", OptionA = "Reference equality, same as classes", OptionB = "Value-based equality using all properties", OptionC = "Only the first property is compared", OptionD = "Records cannot be compared for equality", CorrectOption = "B", Subtopic = "Language Features", Marks = 1, OrderIndex = 14 },
            new() { Domain = "C#", Text = "You need to process a large CSV file (2GB) line by line without loading it entirely into memory. Which approach is most efficient?", OptionA = "File.ReadAllLines() and iterate the array", OptionB = "File.ReadAllText() and split by newline", OptionC = "StreamReader with ReadLineAsync() in a while loop", OptionD = "MemoryMappedFile with full file mapping", CorrectOption = "C", Subtopic = "File I/O", Marks = 1, OrderIndex = 15 },
        };

        db.MCQQuestions.AddRange(seedQuestions);
        await db.SaveChangesAsync();
    }
}

app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AssessmentService v1"));

app.MapControllers();

app.Run();

