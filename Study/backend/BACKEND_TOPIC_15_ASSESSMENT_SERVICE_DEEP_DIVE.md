# Topic 15: AssessmentService Deep Dive

Project: Mock Interview Platform  
Focus: Understanding MCQ assessment flow: assessment lifecycle, question bank, lazy question loading, Gemini-generated MCQs, warm-up cache, timer/expiry, answer submission, objective scoring, premium review, admin question management, and assessment events.

---

## 1. What Is AssessmentService?

### Simple Explanation

AssessmentService manages MCQ-based tests.

It handles:

- Starting assessments
- Loading MCQ questions
- Generating missing questions with Gemini
- Accepting selected answers
- Checking answers against correct options
- Calculating score, percentage, and grade
- Returning assessment result
- Managing question bank for admins

### In Your Project

AssessmentService is used for objective MCQ tests.

Unlike InterviewService, it does not need AI to evaluate answers because every MCQ already has a correct option.

### Viva Answer

> AssessmentService manages MCQ assessments. It starts tests, loads questions, accepts answers, checks them against correct options, calculates score and grade, and returns results.

---

## 2. Why AssessmentService Is Separate

### Simple Explanation

MCQ assessment logic is different from interview, login, payment, and email logic.

### Service Responsibility

```text
IdentityService -> users, JWT, roles, premium state
InterviewService -> subjective mock interviews
AssessmentService -> MCQ tests and objective scoring
SubscriptionService -> payments and premium subscriptions
NotificationService -> email sending
```

### Why This Is Good

- MCQ logic stays separate
- Question bank ownership is clear
- Scoring rules are simple and isolated
- Assessment data has its own database context

### Viva Answer

> AssessmentService is separate because MCQ test flow, question bank management, objective answer checking, and assessment results are independent responsibilities.

---

## 3. Important AssessmentService Files

### Program and Configuration

```text
Backend/AssessmentService/Program.cs
Backend/AssessmentService/appsettings.Example.json
```

### Controller

```text
Backend/AssessmentService/Controllers/AssessmentController.cs
```

### Service

```text
Backend/AssessmentService/Services/IAssessmentService.cs
Backend/AssessmentService/Services/AssessmentService.cs
```

### Data and Models

```text
Backend/AssessmentService/Data/AppDbContext.cs
Backend/AssessmentService/Models/AssessmentModels.cs
```

### DTOs

```text
Backend/AssessmentService/DTOs/AssessmentDtos.cs
```

### Viva Answer

> Important AssessmentService files include AssessmentController, IAssessmentService, AssessmentSvc, AppDbContext, AssessmentModels, and AssessmentDtos.

---

## 4. AssessmentService Database Tables

### DbContext

File:

```text
Backend/AssessmentService/Data/AppDbContext.cs
```

### DbSets

```csharp
public DbSet<MCQQuestion> MCQQuestions { get; set; }
public DbSet<Assessment> Assessments { get; set; }
public DbSet<UserAnswer> UserAnswers { get; set; }
public DbSet<AssessmentResult> AssessmentResults { get; set; }
```

### Viva Answer

> AssessmentService uses MCQQuestions, Assessments, UserAnswers, and AssessmentResults tables.

---

## 5. MCQQuestion Model

### Fields

```text
Id
Domain
Text
OptionA
OptionB
OptionC
OptionD
CorrectOption
Subtopic
Marks
OrderIndex
```

### Meaning

Each MCQ has:

```text
Question text
Four options
One correct option
Subtopic
Marks
Order inside domain
```

### Viva Answer

> MCQQuestion stores assessment questions with four options, correct option, domain, subtopic, marks, and order index.

---

## 6. Assessment Model

### Fields

```text
Id
UserId
Domain
Status
AttemptNumber
TimeLimitMinutes
StartedAt
ExpiresAt
CreatedAt
```

### Status Values

```text
NotStarted
InProgress
Completed
Expired
```

### In Current Flow

Assessment starts directly as:

```text
InProgress
```

### Viva Answer

> Assessment model stores one test attempt with user id, domain, status, attempt number, time limit, start time, and expiry time.

---

## 7. UserAnswer Model

### Fields

```text
AssessmentId
QuestionId
UserId
SelectedOption
IsCorrect
```

### Purpose

Stores what option the user selected for each question.

### Viva Answer

> UserAnswer stores the selected option and whether it was correct for each submitted MCQ answer.

---

## 8. AssessmentResult Model

### Fields

```text
AssessmentId
UserId
Domain
Score
MaxScore
Percentage
Grade
IsPremiumResult
CreatedAt
```

### Purpose

Stores final result after assessment submission.

### Viva Answer

> AssessmentResult stores the final score, max score, percentage, grade, premium flag, and creation time for a completed assessment.

---

## 9. AssessmentController

### File

```text
Backend/AssessmentService/Controllers/AssessmentController.cs
```

### Base Route

```text
/api/assessments
```

### Protection

```csharp
[Authorize]
```

All normal assessment APIs require login.

Admin question bank APIs require:

```csharp
[Authorize(Roles = "Admin")]
```

### Viva Answer

> AssessmentController exposes assessment APIs and protects them using JWT authorization, with admin-only endpoints protected by role authorization.

---

## 10. AssessmentService Endpoints

### User Endpoints

```text
POST /api/assessments/start
GET /api/assessments/{id}/next-batch
POST /api/assessments/warm-up
POST /api/assessments/submit
GET /api/assessments
GET /api/assessments/{id}/result
```

### Admin Endpoints

```text
GET /api/assessments/admin/all
GET /api/assessments/questions
POST /api/assessments/questions
PUT /api/assessments/questions/{id}
DELETE /api/assessments/questions/{id}
```

### Viva Answer

> AssessmentService has endpoints for starting assessments, loading question batches, warming cache, submitting answers, viewing results, and admin question bank management.

---

## 11. IAssessmentService

### File

```text
Backend/AssessmentService/Services/IAssessmentService.cs
```

### Main Methods

```text
StartAssessmentAsync
GetNextBatchAsync
WarmUpCacheAsync
SubmitAssessmentAsync
GetUserAssessmentsAsync
GetAssessmentResultAsync
GetAllAssessmentsAsync
GetAllQuestionsAsync
AddQuestionAsync
UpdateQuestionAsync
DeleteQuestionAsync
```

### Viva Answer

> IAssessmentService defines operations for assessment lifecycle, result retrieval, question generation/cache, and admin question management.

---

## 12. AssessmentSvc Dependencies

### Constructor

```csharp
public AssessmentSvc(
    AppDbContext context,
    HttpClient httpClient,
    IConfiguration config,
    ILogger<AssessmentSvc> logger)
```

### Meaning

```text
AppDbContext -> database access
HttpClient -> Gemini API calls
IConfiguration -> Gemini settings
ILogger -> logs
```

### Registration

```csharp
builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>();
```

### Viva Answer

> AssessmentSvc uses AppDbContext for data, HttpClient for Gemini calls, IConfiguration for settings, and ILogger for logging. It is registered as a typed HttpClient service.

---

## 13. Reading User Details from JWT

### In Controller

AssessmentController reads:

```text
UserId
isPremium
Email
```

from JWT claims.

### Why

```text
UserId -> ownership and results
isPremium -> free limit and premium result details
Email -> completion email event
```

### Viva Answer

> AssessmentController reads user id, premium status, and email from JWT claims to enforce ownership, limits, and event publishing.

---

## 14. Start Assessment

### Endpoint

```text
POST /api/assessments/start
```

### Request DTO

```csharp
public class StartAssessmentRequest
{
    public string Domain { get; set; }
    public int QuestionCount { get; set; }
    public string Difficulty { get; set; }
}
```

### Service Method

```text
StartAssessmentAsync
```

### Viva Answer

> StartAssessmentAsync creates a new assessment attempt, loads the first questions, sets time limit and expiry, and returns start response.

---

## 15. Start Assessment Flow

```text
1. Trim domain.
2. Clamp requested question count between 1 and 60.
3. Default difficulty to Medium.
4. Validate domain.
5. Check free user attempt limit.
6. Load cached MCQ questions from database.
7. Filter high-quality questions.
8. Shuffle questions for variety.
9. If not enough for first batch, generate using Gemini.
10. Save generated questions to DB as cache.
11. Create Assessment record with Status = InProgress.
12. Set AttemptNumber.
13. Set TimeLimitMinutes and ExpiresAt.
14. Return first question batch.
15. Publish AssessmentStartedEvent.
```

### Viva Explanation

> Starting an assessment creates an InProgress assessment, loads cached or AI-generated questions, sets timer and expiry, and returns initial questions.

---

## 16. Free vs Premium Assessment Limit

### In Your Project

Free users can create only:

```text
2 assessment tests
```

### Code Concept

```csharp
if (!isPremium)
{
    var totalAttempts = await _context.Assessments.CountAsync(a => a.UserId == userId);
    if (totalAttempts >= 2)
    {
        throw new ForbiddenAppException(...);
    }
}
```

### Viva Answer

> Free users can create only two assessments, while premium users have unlimited access. This is checked using isPremium claim and assessment count.

---

## 17. Question Count

### In Request

```csharp
[Range(1, 60)]
public int QuestionCount { get; set; } = 10;
```

### Service Rule

```text
Minimum 1
Maximum 60
Default 10
```

### Viva Answer

> Assessment question count is requested by frontend and clamped between 1 and 60, with default 10.

---

## 18. Time Limit and Expiry

### Time Limit Rule

```text
TimeLimitMinutes = max(10, ceil(questionCount * 1.5))
```

### Example

```text
10 questions -> max(10, 15) = 15 minutes
5 questions -> max(10, 8) = 10 minutes
```

### ExpiresAt

```text
StartedAt + TimeLimitMinutes
```

### Viva Answer

> AssessmentService sets a time limit based on question count, with minimum 10 minutes, and stores ExpiresAt to block late submissions.

---

## 19. Attempt Number

### Simple Explanation

AttemptNumber tracks how many times a user attempted assessment for the same domain.

### Code Concept

```csharp
var previousAttempts = await _context.Assessments.CountAsync(
    a => a.UserId == userId && a.Domain == domain);

AttemptNumber = previousAttempts + 1;
```

### Viva Answer

> AttemptNumber tracks the user's attempt count for a specific domain.

---

## 20. Question Bank

### Simple Explanation

Question bank is the database collection of MCQ questions.

### Stored In

```text
MCQQuestions table
```

### Sources

```text
Seeded C# questions
Admin-created questions
Gemini-generated cached questions
```

### Viva Answer

> The assessment question bank is stored in MCQQuestions table and contains seeded, admin-created, and Gemini-generated questions.

---

## 21. Seeded C# Questions

### File

```text
Backend/AssessmentService/Program.cs
```

### Purpose

If the database has no C# questions, Program.cs seeds initial C# MCQs.

### Why Useful

AssessmentService can work even before Gemini generates questions.

### Viva Answer

> AssessmentService seeds C# questions at startup if none exist, giving the system an initial question bank.

---

## 22. High Quality Question Filter

### Method

```text
IsHighQualityAssessmentQuestion
```

### Checks

Rejects questions if:

```text
Question text is empty
Any option is empty
Text has fewer than 5 words
Text length is under 20 characters
Question starts with "true or false"
```

### Viva Answer

> AssessmentService filters out low-quality MCQs by checking question text, options, length, word count, and trivial patterns.

---

## 23. Lazy Loading Questions

### Simple Explanation

The service does not need to return all questions at once.

It can return initial questions first and load more later.

### Start Response

```text
Questions -> first batch
TotalExpected -> total requested count
HasMore -> whether frontend should request more
```

### Viva Answer

> AssessmentService supports lazy loading by returning initial questions first and letting frontend call next-batch for more questions.

---

## 24. First Batch

### In StartAssessmentAsync

First batch size:

```text
min(3, requestedCount)
```

### Why

This makes assessment start faster.

More questions can be fetched while the user answers first ones.

### Viva Answer

> StartAssessmentAsync returns a small first batch, up to 3 questions, to reduce initial wait time.

---

## 25. Get Next Batch

### Endpoint

```text
GET /api/assessments/{id}/next-batch
```

### Query Parameters

```text
currentCount
batchSize
```

### Method

```text
GetNextBatchAsync
```

### Flow

```text
1. Find assessment.
2. Check user ownership.
3. Ensure status is InProgress.
4. Load cached questions for domain.
5. Skip currentCount questions.
6. Take batchSize questions.
7. If not enough, generate more with Gemini.
8. Save generated questions.
9. Return next batch with order indexes.
```

### Viva Answer

> GetNextBatchAsync returns the next lazy-loaded MCQ batch for an in-progress assessment and generates more questions if the cache is insufficient.

---

## 26. Warm Up Cache

### Endpoint

```text
POST /api/assessments/warm-up
```

### Method

```text
WarmUpCacheAsync
```

### Purpose

Pre-generates questions while user reads instructions.

### Flow

```text
1. Count existing domain questions.
2. If enough, skip generation.
3. If not enough, generate up to 3 Gemini questions.
4. Save them to MCQQuestions table.
5. Return cached count.
```

### Viva Answer

> WarmUpCacheAsync pre-generates and caches questions for a domain to reduce wait time when assessment starts.

---

## 27. Gemini MCQ Generation

### Method

```text
GenerateAiAssessmentQuestionsAsync
```

### Uses

```text
Gemini:ApiKey
Gemini:Model
HttpClient
Polly retry
```

### Prompt Goal

Generate practical, scenario-based, interview-level MCQs.

### Response Shape

Gemini is asked for JSON array containing:

```text
text
optionA
optionB
optionC
optionD
correctOption
subtopic
```

### Viva Answer

> Gemini is used to generate practical MCQ questions when the database cache does not have enough questions.

---

## 28. Gemini Response Schema

### In Your Project

The request uses:

```text
responseMimeType = application/json
responseSchema
```

### Why Useful

It encourages Gemini to return clean JSON in expected structure.

### Viva Answer

> AssessmentService uses Gemini response schema to request structured JSON output for MCQ generation.

---

## 29. Dynamic Sub-Scenarios

### Simple Explanation

To avoid repetitive questions, the service randomly chooses sub-scenarios.

### Examples

```text
memory leaks and garbage collection
API design and REST best practices
LINQ query optimization
authentication and authorization
event-driven architecture
retry patterns
```

### Why

It creates more variety without sending a long exclusion list.

### Viva Answer

> Dynamic sub-scenarios help Gemini generate diverse MCQs by focusing each generation call on a random practical topic.

---

## 30. Polly Retry for Gemini

### Retry Delays

```text
500 ms
1 second
2 seconds
```

### Handles

```text
HttpRequestException
AppException
429 rate limit
500+ server errors
```

### Viva Answer

> AssessmentService uses Polly retry for Gemini generation to recover from transient failures and rate limits.

---

## 31. Submit Assessment

### Endpoint

```text
POST /api/assessments/submit
```

### Request DTO

```csharp
public class SubmitAssessmentRequest
{
    public int AssessmentId { get; set; }
    public List<AnswerSubmission> Answers { get; set; }
    public int? TotalExpected { get; set; }
}
```

### Answer DTO

```csharp
public class AnswerSubmission
{
    public int QuestionId { get; set; }
    public string SelectedOption { get; set; }
}
```

### Viva Answer

> SubmitAssessmentAsync receives assessment id and selected options, validates them, calculates score, saves answers, and creates assessment result.

---

## 32. Submit Assessment Flow

```text
1. Validate assessment id.
2. Validate answers exist.
3. Find assessment.
4. Check ownership.
5. Ensure assessment is InProgress.
6. Check expiry.
7. Load submitted MCQ questions.
8. Validate question ids.
9. Remove existing answers if resubmitting.
10. Deduplicate submitted answers.
11. Compare selected option with correct option.
12. Add marks for correct answers.
13. Save UserAnswer records.
14. Mark assessment Completed.
15. Calculate percentage and grade.
16. Save AssessmentResult.
17. Return result.
18. Controller publishes completion/email events.
```

### Viva Explanation

> Assessment submission validates ownership and timing, checks selected options against correct options, saves answers, calculates result, and completes the assessment.

---

## 33. Objective Answer Checking

### Simple Explanation

MCQ has a fixed correct option.

### Code Concept

```csharp
var isCorrect = string.Equals(
    q.CorrectOption,
    ans.SelectedOption,
    StringComparison.OrdinalIgnoreCase);
```

### Why AI Is Not Needed

Because correct answer is already stored.

### Viva Answer

> MCQ answers are evaluated by directly comparing selected option with stored correct option, so AI is not needed for scoring.

---

## 34. Score Calculation

### Per Question

Each MCQ has:

```text
Marks = 1
```

### Score

```text
Add marks for every correct answer
```

### Max Score

```text
TotalExpected if provided
Otherwise number of distinct submitted answers
```

### Viva Answer

> Score is calculated by adding marks for correct answers. Max score is based on expected total questions or submitted answer count.

---

## 35. Grade Calculation

### Grade Rules

```text
90+ -> A+
80+ -> A
70+ -> B
60+ -> C
50+ -> D
Below 50 -> F
```

### Viva Answer

> Assessment grade is calculated from percentage using thresholds: A+ for 90+, A for 80+, B for 70+, C for 60+, D for 50+, otherwise F.

---

## 36. Assessment Expiry

### In Submit

If current UTC time is greater than `ExpiresAt`:

```text
Assessment status becomes Expired
Error: Time expired.
```

### Why

Prevents users from submitting after time limit.

### Viva Answer

> AssessmentService checks ExpiresAt during submit and marks the assessment Expired if the time limit has passed.

---

## 37. Get My Assessments

### Endpoint

```text
GET /api/assessments
```

### Method

```text
GetUserAssessmentsAsync
```

### Returns

All assessment results for logged-in user.

### Viva Answer

> GetUserAssessmentsAsync returns the current user's assessment result history.

---

## 38. Get Assessment Result

### Endpoint

```text
GET /api/assessments/{id}/result
```

### Method

```text
GetAssessmentResultAsync
```

### Flow

```text
1. Validate assessment id.
2. Find assessment.
3. Check ownership.
4. Find result.
5. Load user answers.
6. Build free or premium response.
```

### Viva Answer

> GetAssessmentResultAsync validates ownership and returns the saved assessment result, with extra review for premium users.

---

## 39. Free vs Premium Result

### Free Result

Free users get:

```text
Score
MaxScore
Percentage
Grade
WrongQuestionIds
```

### Premium Result

Premium users also get:

```text
AnswerReview
WeakAreas
WeakAreasSummary
```

### Viva Answer

> Free users get result summary, while premium users get detailed answer review and weak area analysis.

---

## 40. Premium Answer Review

### Includes

For each answer:

```text
Question text
Subtopic
Selected option
Correct option
IsCorrect
```

### Weak Areas

Weak areas come from subtopics of wrong answers.

### Viva Answer

> Premium result includes question-wise answer review and weak areas based on incorrect answer subtopics.

---

## 41. Admin Assessment Result View

### Endpoint

```text
GET /api/assessments/admin/all
```

### Protection

```csharp
[Authorize(Roles = "Admin")]
```

### Purpose

Admins can view all assessment results.

### Viva Answer

> Admin all-assessments endpoint lets admins view all assessment results and is protected by Admin role.

---

## 42. Admin Question Bank Management

### Endpoints

```text
GET /api/assessments/questions
POST /api/assessments/questions
PUT /api/assessments/questions/{id}
DELETE /api/assessments/questions/{id}
```

### Protection

```csharp
[Authorize(Roles = "Admin")]
```

### Viva Answer

> Admin question bank APIs allow admins to list, add, update, and delete MCQ questions.

---

## 43. Add Question

### Method

```text
AddQuestionAsync
```

### Flow

```text
1. Validate domain, text, options, correct option, subtopic.
2. Find next order index for domain.
3. Create MCQQuestion.
4. Save to database.
```

### Viva Answer

> AddQuestionAsync validates MCQ data, assigns next order index for the domain, and saves the question.

---

## 44. Update Question

### Method

```text
UpdateQuestionAsync
```

### Flow

```text
1. Validate question id.
2. Validate request fields.
3. Find existing question.
4. Update fields.
5. If domain changed, assign new order index.
6. Save changes.
7. Re-index old and new domain questions.
```

### Viva Answer

> UpdateQuestionAsync edits an MCQ question and re-indexes domain questions if needed.

---

## 45. Delete Question

### Method

```text
DeleteQuestionAsync
```

### Flow

```text
1. Validate question id.
2. Find question.
3. Remove question.
4. Save changes.
5. Re-index remaining questions in that domain.
```

### Viva Answer

> DeleteQuestionAsync removes an MCQ question and reorders remaining questions in the same domain.

---

## 46. ReIndexDomainQuestionsAsync

### Purpose

Keeps order indexes sequential inside each domain.

### Example

If question 3 is deleted:

```text
1, 2, 4, 5
```

becomes:

```text
1, 2, 3, 4
```

### Viva Answer

> ReIndexDomainQuestionsAsync rebuilds order indexes so domain questions remain sequential after update or delete.

---

## 47. ValidateQuestionRequest

### Checks

```text
Domain is required
Question text is required
All four options are required
Correct option must be A, B, C, or D
Subtopic is required
```

### Viva Answer

> ValidateQuestionRequest ensures admin-created or updated MCQ questions are complete and valid before saving.

---

## 48. Assessment Events

### Published Events

AssessmentController publishes:

```text
AssessmentStartedEvent
AssessmentCompletedEvent
EmailRequestedEvent
```

### When Started

After:

```text
POST /api/assessments/start
```

### When Completed

After:

```text
POST /api/assessments/submit
```

### Viva Answer

> AssessmentService publishes assessment started and completed events, plus email notification events after completion.

---

## 49. Assessment Completion Notification Flow

```text
1. User submits assessment.
2. AssessmentService calculates result.
3. Controller fetches result.
4. AssessmentCompletedEvent is published.
5. IdentityService creates in-app notification.
6. EmailRequestedEvent is published.
7. NotificationService sends assessment completion email.
```

### Viva Explanation

> Assessment completion triggers RabbitMQ events for in-app notification and email notification.

---

## 50. Difference Between Assessment and Interview

### Assessment

```text
MCQ based
Objective evaluation
Correct option already stored
Fast scoring
Premium review shows correct/wrong answers
```

### Interview

```text
Subjective answer based
AI evaluates answer quality
No single fixed answer
Feedback and follow-up questions
```

### Viva Answer

> Assessment is objective MCQ-based and can be scored without AI, while InterviewService handles subjective answers that need AI evaluation.

---

## 51. Error Handling

### Custom Exceptions

```text
ValidationAppException
ForbiddenAppException
NotFoundAppException
AppException
```

### Examples

```text
Domain is required -> 400
Free limit reached -> 403
Assessment not found -> 404
Gemini unavailable -> 503
Time expired -> 400
```

### Viva Answer

> AssessmentService uses custom exceptions for validation errors, forbidden actions, not found records, expired submissions, and external service failures.

---

## 52. Security and Ownership

### Ownership Checks

AssessmentService checks:

```text
Assessment.UserId == current user id
```

### Protected APIs

Normal APIs:

```text
[Authorize]
```

Admin APIs:

```text
[Authorize(Roles = "Admin")]
```

### Viva Answer

> AssessmentService protects endpoints with JWT authorization and checks ownership before allowing submission or result viewing.

---

## 53. Complete Flow: Start Assessment

```text
1. User logs in and receives JWT.
2. Frontend sends POST /api/assessments/start.
3. Controller reads userId and isPremium from JWT.
4. StartAssessmentAsync validates domain and question count.
5. Free user limit is checked.
6. Cached MCQs are loaded from database.
7. If not enough, Gemini generates questions.
8. Generated questions are saved to MCQQuestions table.
9. Assessment record is created with InProgress status.
10. TimeLimitMinutes and ExpiresAt are set.
11. First batch questions are returned.
12. AssessmentStartedEvent is published.
```

### Viva Explanation

> Start assessment creates an in-progress assessment, prepares first batch questions, sets timer, and publishes a started event.

---

## 54. Complete Flow: Submit Assessment

```text
1. User submits selected answers.
2. Controller reads userId, isPremium, and email from JWT.
3. SubmitAssessmentAsync validates assessment id and answers.
4. Assessment is loaded.
5. Ownership is checked.
6. Status and expiry are checked.
7. Submitted question ids are validated.
8. Selected options are compared with correct options.
9. UserAnswer records are saved.
10. Score, max score, percentage, and grade are calculated.
11. AssessmentResult is saved.
12. Assessment status becomes Completed.
13. Result is returned.
14. AssessmentCompletedEvent and EmailRequestedEvent are published.
```

### Viva Explanation

> Submit assessment objectively checks selected options, saves answers, calculates result, completes assessment, and publishes completion events.

---

## 55. What Happens If AssessmentService Is Down?

### Effects

Users cannot:

```text
Start assessment
Fetch next batch
Submit assessment
View assessment result
View assessment history
```

Other services may still work:

```text
Identity
Interview
Subscription
Notification
```

### Viva Answer

> If AssessmentService is down, assessment-related APIs fail, but other independent services can still operate.

---

## 56. Limitations and Improvements

### Current Limitations

- Lazy loading skips by current count after shuffling, which may cause repeated or missed questions
- Submitted assessment does not store a fixed question set per assessment
- Previous question avoidance method exists but is not actively used in start flow
- Minor repetition is allowed in generated questions
- Domain is a simple string
- No pagination for question bank or results
- No detailed explanation field for MCQs
- No negative marking

### Possible Improvements

- Store AssessmentQuestion mapping table for exact question set
- Avoid repeated questions per user using previous question history
- Add explanation field for each MCQ
- Add pagination and filtering for admin question bank
- Add negative marking or difficulty-based marks
- Add Redis cache for generated question pools
- Add outbox pattern for event publishing
- Add stronger duplicate detection for generated questions
- Add per-assessment question snapshot
- Add review mode only after completion

### Balanced Viva Answer

> AssessmentService supports the main MCQ flow well. Future improvements could include fixed per-assessment question mapping, stronger duplicate prevention, explanations, pagination, difficulty-based marks, and better caching.

---

## 57. Best Full Viva Answer for Topic 15

> AssessmentService manages MCQ-based assessments. It starts an assessment by validating domain and question count, checking free user limits, loading cached MCQ questions, generating missing questions through Gemini, creating an InProgress assessment, and returning the first batch. It supports lazy loading using the next-batch endpoint and warm-up caching to reduce user wait time. On submission, it validates ownership, checks expiry, compares selected options with stored correct options, saves UserAnswer records, calculates score, percentage, and grade, stores AssessmentResult, and marks the assessment Completed. Free users get result summary and wrong question ids, while premium users get answer review and weak areas. Admins can manage the MCQ question bank through protected endpoints.

---

## 58. Common Viva Questions and Answers

### Q1. What is AssessmentService?

AssessmentService manages MCQ assessments, questions, answers, scoring, results, and admin question bank.

### Q2. How is assessment different from interview?

Assessment is objective MCQ-based, while interview has subjective answers evaluated by AI.

### Q3. What tables does AssessmentService use?

MCQQuestions, Assessments, UserAnswers, and AssessmentResults.

### Q4. What does StartAssessmentAsync do?

It creates an in-progress assessment, loads or generates questions, sets timer, and returns first batch.

### Q5. What is the free user limit?

Free users can create only two assessment tests.

### Q6. What is QuestionCount?

It is the requested number of questions, clamped between 1 and 60.

### Q7. How is time limit calculated?

It is max of 10 minutes and 1.5 minutes per question.

### Q8. What is ExpiresAt?

It is the deadline after which assessment submission is rejected.

### Q9. What is lazy loading?

Lazy loading returns initial questions first and loads more questions through next-batch endpoint.

### Q10. What is WarmUpCache?

It pre-generates and caches questions while user reads instructions.

### Q11. Why is Gemini used in AssessmentService?

Gemini generates MCQ questions when database cache does not have enough questions.

### Q12. Does AssessmentService use AI to check answers?

No. MCQ answers are checked directly against CorrectOption.

### Q13. How is score calculated?

Score increases by marks for every correct answer.

### Q14. How is grade calculated?

Grade is based on percentage: A+ 90+, A 80+, B 70+, C 60+, D 50+, F below 50.

### Q15. What happens if time expires?

Assessment status becomes Expired and submission is rejected.

### Q16. What does free result include?

Score, max score, percentage, grade, and wrong question ids.

### Q17. What does premium result include?

Free result plus answer review, weak areas, and weak area summary.

### Q18. What are admin question APIs?

List, add, update, and delete MCQ questions.

### Q19. Why re-index questions?

To keep order indexes sequential after update or delete.

### Q20. What events does AssessmentService publish?

AssessmentStartedEvent, AssessmentCompletedEvent, and EmailRequestedEvent.

### Q21. Why publish AssessmentCompletedEvent?

So other services can create notifications or send emails asynchronously.

### Q22. How is ownership checked?

Assessment.UserId is compared with current user id from JWT.

### Q23. What is IsPremiumResult?

It records whether result was generated for a premium user.

### Q24. What happens if Gemini fails?

The service uses cached questions if available; otherwise start may return service unavailable.

### Q25. What improvements can be made?

Fixed assessment-question mapping, stronger duplicate prevention, explanations, pagination, Redis cache, and difficulty-based marks.

---

## 59. Quick Revision Summary

- AssessmentService handles MCQ tests.
- MCQ answers are checked without AI.
- Main tables are MCQQuestions, Assessments, UserAnswers, AssessmentResults.
- All user endpoints require JWT.
- Admin question endpoints require Admin role.
- Free users can create two assessments.
- QuestionCount is clamped between 1 and 60.
- Time limit is max 10 minutes or 1.5 minutes per question.
- Assessment has ExpiresAt.
- Expired assessments cannot be submitted.
- StartAssessment returns first batch.
- Next-batch endpoint lazy-loads more questions.
- Warm-up pre-generates cached questions.
- Gemini generates MCQs when cache is insufficient.
- Gemini response schema asks for clean JSON.
- Submit compares selected option with correct option.
- Score is based on correct answers.
- Grade is based on percentage.
- Free result gives summary.
- Premium result gives answer review and weak areas.
- Admin can add, update, delete questions.
- ReIndex keeps domain order indexes sequential.
- Assessment events trigger notifications and emails.
- Future improvements include fixed question mapping and explanations.

