# Topic 14: InterviewService Deep Dive

Project: Mock Interview Platform  
Focus: Understanding subjective mock interview flow: interview lifecycle, free vs premium limits, question generation, question caching, avoiding repeated questions, Gemini integration, answer evaluation, scoring, feedback, premium result details, and interview events.

---

## 1. What Is InterviewService?

### Simple Explanation

InterviewService manages mock interview sessions.

It handles:

- Creating interview sessions
- Starting interviews
- Generating interview questions
- Saving user answers
- Evaluating subjective answers
- Calculating score and grade
- Returning interview results

### In Your Project

InterviewService is responsible for subjective interview practice.

Unlike MCQ assessment, answers are not simply A/B/C/D.

User writes descriptive answers, and Gemini helps evaluate them.

### Viva Answer

> InterviewService manages subjective mock interviews. It creates interviews, generates questions, stores answers, evaluates answers using Gemini, calculates score, and returns result feedback.

---

## 2. Why InterviewService Is Separate

### Simple Explanation

Interview logic is different from authentication, payments, and MCQ assessments.

It has its own:

```text
Interview lifecycle
Question generation
Subjective answer evaluation
Result calculation
Premium result rules
```

### Service Ownership

```text
IdentityService -> users and JWT claims
InterviewService -> interviews, questions, answers, interview results
AssessmentService -> MCQ tests
SubscriptionService -> payment and premium plans
NotificationService -> email sending
```

### Viva Answer

> InterviewService is separate because interview creation, question generation, subjective answer evaluation, and interview result calculation are independent responsibilities.

---

## 3. Important InterviewService Files

### Program and Configuration

```text
Backend/InterviewService/Program.cs
Backend/InterviewService/appsettings.Example.json
```

### Controller

```text
Backend/InterviewService/Controllers/InterviewController.cs
```

### Service

```text
Backend/InterviewService/Services/IInterviewSvc.cs
Backend/InterviewService/Services/InterviewService.cs
```

### Data and Models

```text
Backend/InterviewService/Data/AppDbContext.cs
Backend/InterviewService/Models/InterviewModels.cs
```

### DTOs

```text
Backend/InterviewService/DTOs/InterviewDtos.cs
```

### Viva Answer

> Important InterviewService files are InterviewController, IInterviewSvc, InterviewSvc, AppDbContext, InterviewModels, and InterviewDtos.

---

## 4. InterviewService Database Tables

### DbContext

File:

```text
Backend/InterviewService/Data/AppDbContext.cs
```

### DbSets

```csharp
public DbSet<Interview> Interviews { get; set; }
public DbSet<Question> Questions { get; set; }
public DbSet<InterviewAnswer> InterviewAnswers { get; set; }
public DbSet<InterviewResult> InterviewResults { get; set; }
public DbSet<GlobalInterviewQuestion> GlobalInterviewQuestions { get; set; }
```

### Viva Answer

> InterviewService uses tables for interviews, questions, interview answers, interview results, and global cached interview questions.

---

## 5. Interview Model

### File

```text
Backend/InterviewService/Models/InterviewModels.cs
```

### Fields

```text
Id
UserId
Title
Domain
Type
Status
CreatedAt
StartedAt
CompletedAt
```

### Status Values

```text
Pending
InProgress
Completed
```

### Type Values

```text
Normal
Premium
```

### Viva Answer

> Interview model stores the interview session with user id, title, domain, type, status, and timestamps.

---

## 6. Question Model

### Fields

```text
Id
InterviewId
Text
QuestionType
OptionA
OptionB
OptionC
OptionD
CorrectAnswer
Subtopic
Source
OrderIndex
```

### Meaning

Questions belong to one interview using:

```text
InterviewId
```

`OrderIndex` is the question number shown to frontend.

### Source Values

```text
Admin
JIT_Pool
Gemini_AI
```

### Viva Answer

> Question model stores generated or curated questions for an interview, including text, subtopic, source, ideal answer, and order index.

---

## 7. GlobalInterviewQuestion Model

### Purpose

Stores reusable cached interview questions in a global question pool.

### Fields

```text
Domain
Difficulty
Text
QuestionType
IdealAnswer
Subtopic
Source
CreatedAt
```

### Why Useful

Gemini calls can be slow or rate-limited.

Global cached questions reduce wait time and provide fallback.

### Viva Answer

> GlobalInterviewQuestion stores reusable cached questions so InterviewService can serve questions faster and reduce dependency on Gemini every time.

---

## 8. InterviewAnswer Model

### Purpose

Stores user's submitted answer for each interview question.

### Fields

```text
InterviewId
QuestionId
UserId
AnswerText
IsCorrect
Score
SubmittedAt
```

### Important Note

For subjective interviews, `IsCorrect` means whether the answer is strong according to AI evaluation.

It is not a simple MCQ true/false check.

### Viva Answer

> InterviewAnswer stores each submitted answer, score, and whether the answer was considered strong by the evaluator.

---

## 9. InterviewResult Model

### Purpose

Stores final result of completed interview.

### Fields

```text
InterviewId
UserId
TotalScore
MaxScore
Percentage
Grade
Feedback
IsPremiumResult
CreatedAt
```

### Viva Answer

> InterviewResult stores total score, max score, percentage, grade, feedback, and whether the result was generated for a premium user.

---

## 10. InterviewController

### File

```text
Backend/InterviewService/Controllers/InterviewController.cs
```

### Base Route

```text
/api/interviews
```

### Protection

```csharp
[Authorize]
```

All interview endpoints require logged-in user.

### Viva Answer

> InterviewController exposes interview APIs and is protected with Authorize so only authenticated users can access interview operations.

---

## 11. InterviewService Endpoints

### User Endpoints

```text
POST /api/interviews/start
POST /api/interviews/{id}/begin
POST /api/interviews/warm-up
POST /api/interviews/{id}/fetch-more
POST /api/interviews/submit
GET /api/interviews
GET /api/interviews/{id}
GET /api/interviews/{id}/result
```

### Admin Endpoint

```text
GET /api/interviews/admin/all
```

### Viva Answer

> InterviewService exposes endpoints to start, begin, warm up, fetch questions, submit answers, view interviews, view results, and let admins view all interviews.

---

## 12. IInterviewSvc

### File

```text
Backend/InterviewService/Services/IInterviewSvc.cs
```

### Main Methods

```text
StartInterviewAsync
BeginInterviewAsync
FetchMoreQuestionsAsync
WarmUpCacheAsync
SubmitInterviewAsync
GetMyInterviewsAsync
GetInterviewByIdAsync
GetResultAsync
GetAllInterviewsAsync
```

### Viva Answer

> IInterviewSvc defines interview lifecycle operations like start, begin, fetch more questions, submit answers, and get results.

---

## 13. InterviewSvc Dependencies

### Constructor

```csharp
public InterviewSvc(
    AppDbContext context,
    HttpClient httpClient,
    IConfiguration config,
    ILogger<InterviewSvc> logger)
```

### Meaning

```text
AppDbContext -> database operations
HttpClient -> Gemini API calls
IConfiguration -> Gemini API settings
ILogger -> logging
```

### Viva Answer

> InterviewSvc uses AppDbContext for data, HttpClient for Gemini calls, IConfiguration for API settings, and ILogger for logs.

---

## 14. Reading User Details from JWT

### In Controller

InterviewController reads:

```text
UserId
isPremium
Email
```

from JWT claims.

### Code Concept

```csharp
var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var isPremiumStr = User.FindFirst("isPremium")?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
```

### Why Needed

```text
UserId -> ownership and database records
isPremium -> free vs premium limit/result
email -> completion email
```

### Viva Answer

> InterviewController reads user id, premium status, and email from JWT claims to enforce ownership, apply limits, and publish notification/email events.

---

## 15. Interview Lifecycle

### States

```text
Pending -> InProgress -> Completed
```

### Meaning

```text
Pending: Interview record created but questions not started
InProgress: Questions loaded and user can answer
Completed: Answers submitted and result generated
```

### Viva Answer

> Interview lifecycle moves from Pending to InProgress when questions are loaded, and then to Completed after answer submission and result generation.

---

## 16. Start Interview

### Endpoint

```text
POST /api/interviews/start
```

### Request

```csharp
public class StartInterviewRequest
{
    public string Title { get; set; }
    public string Domain { get; set; }
}
```

### Service Method

```text
StartInterviewAsync
```

### Flow

```text
1. Validate domain.
2. Check free user limit.
3. Create Interview record.
4. Set Type as Premium or Normal.
5. Set Status = Pending.
6. Save interview.
7. Publish InterviewStartedEvent.
```

### Viva Answer

> StartInterviewAsync creates a pending interview record, checks free user limit, stores interview type, and saves it in database.

---

## 17. Free vs Premium Interview Limit

### In Your Project

Free users can create only one interview.

### Code Concept

```csharp
if (!isPremium)
{
    var totalInterviews = await _context.Interviews.CountAsync(i => i.UserId == userId);
    if (totalInterviews >= 1)
    {
        throw new ForbiddenAppException(...);
    }
}
```

### Why

Premium subscription gives unlimited or expanded access.

### Viva Answer

> Free users are limited to one interview, while premium users can attempt more interviews. This is checked using isPremium claim and interview count.

---

## 18. Begin Interview

### Endpoint

```text
POST /api/interviews/{id}/begin
```

### Service Method

```text
BeginInterviewAsync
```

### Flow

```text
1. Find interview by id and user id.
2. If missing, return not found.
3. If completed, block restart.
4. If already in progress, return existing questions.
5. Extract requested question count from domain.
6. Find previous user questions to avoid repeats.
7. Build question set.
8. Save questions.
9. Set Status = InProgress.
10. Set StartedAt.
11. Return questions to frontend.
```

### Viva Answer

> BeginInterviewAsync starts a pending interview by generating questions, saving them, marking interview as InProgress, and returning questions.

---

## 19. Idempotent Begin

### Simple Explanation

If user clicks begin twice or refreshes page, backend should not create duplicate questions.

### In Your Project

If interview is already:

```text
InProgress
```

the service returns existing saved questions.

### Viva Answer

> BeginInterviewAsync is idempotent. If the interview is already in progress, it returns existing questions instead of creating duplicates.

---

## 20. Question Count Extraction

### Method

```text
ExtractQuestionCount
```

### How It Works

It looks for text like:

```text
5 Questions
10 Questions
```

inside the domain string.

### Rule

```text
Minimum 3
Maximum 12
Default 5
```

### Viva Answer

> InterviewService extracts requested question count from the domain string and clamps it between 3 and 12, defaulting to 5.

---

## 21. Domain String

### Simple Explanation

The domain field contains interview profile information.

Example:

```text
Software Engineer | 1-3 years | Technical | Medium | 5 Questions | Angular, TypeScript
```

### Used For

```text
Question count
Role matching
Technology matching
Gemini prompt
JIT cache lookup
Previous question comparison
```

### Viva Answer

> Domain stores interview profile details such as role, experience, difficulty, question count, and technologies. It guides question generation and cache lookup.

---

## 22. Question Generation Strategy

### Three-Tier Fallback

InterviewService builds questions using:

```text
1. Admin curated questions
2. JIT global question pool
3. Gemini AI generation
```

### Why

This improves reliability.

If Gemini is rate-limited, service can still use admin or cached questions.

### Viva Answer

> InterviewService uses a three-tier strategy: admin curated questions first, then JIT global pool, then Gemini AI generation.

---

## 23. Admin Curated Questions

### Source

Admin questions are stored in:

```text
Questions table
InterviewId = 0
Source = Admin
```

### Matching

The service matches profile terms with:

```text
Question subtopic
Question text
```

### Why Useful

Admin questions are stable and high quality.

### Viva Answer

> Admin curated questions are manually managed questions stored with InterviewId 0 and Source Admin. They are preferred before AI-generated questions.

---

## 24. JIT Global Question Pool

### Simple Explanation

JIT pool contains previously generated reusable questions.

### Search Levels

```text
1. Exact normalized domain match
2. Role-based partial match
3. Technology-based match
```

### Why Useful

It reduces Gemini calls and helps when AI is unavailable.

### Viva Answer

> JIT global question pool stores reusable questions and is searched by normalized domain, role, and technology before calling Gemini.

---

## 25. Gemini AI Question Generation

### Method

```text
GenerateAiQuestionsAsync
```

### Uses

```text
Gemini:ApiKey
Gemini:Model
HttpClient
Polly retry
```

### Prompt Goal

Generate concise practical mock interview questions based on interview profile.

### Rules

Gemini is asked to return:

```json
{
  "questions": [
    {
      "question": "string",
      "subtopic": "string",
      "idealAnswer": "string"
    }
  ]
}
```

### Viva Answer

> Gemini is used to generate practical scenario-based interview questions when admin and cached questions are insufficient.

---

## 26. Gemini Retry with Polly

### Simple Explanation

External APIs can fail temporarily.

### In Your Project

InterviewService uses Polly retry for:

```text
429 Too Many Requests
500+ server errors
HttpRequestException
TaskCanceledException
```

### Backoff

Retries wait longer each time:

```text
3 seconds
9 seconds
27 seconds
```

For 429, it can respect Retry-After header.

### Viva Answer

> Polly retry is used for Gemini calls to handle transient failures and rate limits with backoff.

---

## 27. Why Previous Questions Are Avoided

### Simple Explanation

Users should not repeatedly get the same questions.

### Method

```text
GetPreviousUserQuestionTextsAsync
```

### Logic

```text
1. Find user's previous interviews for same domain.
2. Load previous question texts.
3. Normalize text.
4. Exclude duplicates while building new set.
```

### Viva Answer

> Previous questions are avoided to give users fresh practice and prevent repeated interview questions for the same domain.

---

## 28. Normalize Question Text

### Purpose

Helps detect duplicate questions even if spacing or casing differs.

### Logic

```text
Trim
Lowercase
Replace multiple spaces with one space
```

### Viva Answer

> Question text is normalized before duplicate comparison so similar questions with different casing or spacing are treated as duplicates.

---

## 29. Warm Up Cache

### Endpoint

```text
POST /api/interviews/warm-up
```

### Method

```text
WarmUpCacheAsync
```

### Purpose

Pre-generates questions while user is choosing interview parameters.

### Flow

```text
1. Normalize domain.
2. Check existing global pool count.
3. If not enough, generate AI questions.
4. Store/reuse global pool questions.
```

### Viva Answer

> Warm-up prepares interview questions in advance to reduce wait time when the user begins the interview.

---

## 30. Fetch More Questions

### Endpoint

```text
POST /api/interviews/{id}/fetch-more
```

### Method

```text
FetchMoreQuestionsAsync
```

### Purpose

Generates missing questions for an in-progress interview.

### Current Behavior

If all questions are already available, it returns all questions.

If not, it generates remaining questions and returns full set.

### Viva Answer

> FetchMoreQuestionsAsync fills missing questions for an in-progress interview and returns the full question set.

---

## 31. Why Fallback Exists If Gemini Fails

### Problem

Gemini can fail because of:

```text
Rate limit
Invalid API key
Network issue
Server error
Invalid JSON response
Timeout
```

### Project Solution

Use:

```text
Admin questions
JIT pool questions
Retry logic
Partial question set warning
Fallback scoring if evaluation fails
```

### Viva Answer

> Fallback exists because external AI services can fail or be rate-limited. The project uses admin questions, cached questions, retries, and fallback evaluation.

---

## 32. Submit Interview

### Endpoint

```text
POST /api/interviews/submit
```

### Request DTO

```csharp
public class SubmitInterviewRequest
{
    public int InterviewId { get; set; }
    public List<InterviewAnswerSubmission> Answers { get; set; }
}
```

### Answer DTO

```csharp
public class InterviewAnswerSubmission
{
    public int QuestionId { get; set; }
    public string AnswerText { get; set; }
}
```

### Viva Answer

> Submit interview receives interview id and user answers, validates the interview, evaluates answers, saves result, and marks interview completed.

---

## 33. Submit Interview Flow

```text
1. Find interview by id.
2. Check interview belongs to current user.
3. Ensure status is InProgress.
4. Load interview questions.
5. Validate submitted question ids.
6. Remove old answers if any.
7. Group duplicate submissions.
8. Evaluate answers using Gemini.
9. If Gemini evaluation fails, use fallback zero-score evaluation.
10. Save InterviewAnswer records.
11. Calculate total score, percentage, and grade.
12. Save InterviewResult.
13. Mark interview Completed.
14. Return result.
```

### Viva Explanation

> SubmitInterviewAsync validates ownership and state, evaluates answers, stores answer records, calculates result, and completes the interview.

---

## 34. Why QuestionId Uses OrderIndex in Submit

### Simple Explanation

Frontend receives question numbers as:

```text
1, 2, 3...
```

These correspond to:

```text
OrderIndex
```

### Validation

Backend checks submitted question ids against valid order indexes.

### Viva Answer

> Submit uses the question numbers returned by begin endpoint, which are based on OrderIndex, to validate user-submitted answers.

---

## 35. AI Answer Evaluation

### Method

```text
EvaluateInterviewAnswersAsync
```

### Why Needed

Subjective answers cannot be checked like MCQs.

Gemini evaluates:

```text
Clarity
Technical accuracy
Practical depth
Completeness
```

### Score

Each answer is scored out of:

```text
10
```

### Viva Answer

> Subjective interview answers need AI evaluation because there is no fixed option. Gemini scores answers based on clarity, technical accuracy, practical depth, and completeness.

---

## 36. Evaluation Batching

### Simple Explanation

The service evaluates questions in batches to avoid timeout.

### In Your Project

Batch size:

```text
4 questions
```

### Why

Large prompts may timeout or fail.

Smaller batches are safer.

### Viva Answer

> InterviewService evaluates answers in batches of 4 questions to reduce timeout risk and improve reliability.

---

## 37. Evaluation JSON Format

### Gemini Returns

```json
{
  "overallFeedback": "string",
  "questionEvaluations": [
    {
      "questionId": 1,
      "score": 8,
      "isStrong": true,
      "idealAnswer": "string",
      "followUpQuestion": "string"
    }
  ]
}
```

### Used For

```text
Score
IsCorrect/IsStrong
Ideal answer
Feedback
Follow-up question
```

### Viva Answer

> Gemini evaluation returns per-question score, strength flag, ideal answer, follow-up question, and overall feedback.

---

## 38. Fallback Evaluation

### When Used

If Gemini evaluation fails.

### Behavior

The service:

```text
Saves user answers
Gives score 0 for each question
Stores fallback feedback
Avoids losing the submission
```

### Why Good

User work is not lost even if AI is unavailable.

### Viva Answer

> If Gemini evaluation fails, the service saves answers and creates fallback evaluation so the user's submission is not lost.

---

## 39. Scoring Logic

### Per Question

```text
Score: 0 to 10
```

### Max Score

```text
questions.Count * 10
```

### Percentage

```text
TotalScore / MaxScore * 100
```

### Viva Answer

> Each interview answer is scored out of 10. Total score is divided by max score to calculate percentage.

---

## 40. Grade Calculation

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

> InterviewService calculates grade from percentage using thresholds: A+ for 90+, A for 80+, B for 70+, C for 60+, D for 50+, otherwise F.

---

## 41. Feedback Generation

### Feedback Sources

```text
Overall feedback from Gemini
Follow-up questions from per-question evaluation
Fallback message if AI unavailable
```

### In Result

Stored in:

```text
InterviewResult.Feedback
```

### Viva Answer

> Feedback is built from Gemini's overall feedback and follow-up questions, then stored in InterviewResult.

---

## 42. Get My Interviews

### Endpoint

```text
GET /api/interviews
```

### Method

```text
GetMyInterviewsAsync
```

### Purpose

Returns all interviews for logged-in user.

### Viva Answer

> GetMyInterviewsAsync returns interviews belonging to the authenticated user.

---

## 43. Get Interview By Id

### Endpoint

```text
GET /api/interviews/{id}
```

### Method

```text
GetInterviewByIdAsync
```

### Returns

```text
Interview
Questions
```

### Ownership

Only returns if:

```text
Interview.UserId == current user id
```

### Viva Answer

> GetInterviewByIdAsync returns one user's interview and saved questions after checking ownership.

---

## 44. Get Result

### Endpoint

```text
GET /api/interviews/{id}/result
```

### Method

```text
GetResultAsync
```

### Returns

For all users:

```text
TotalScore
MaxScore
Percentage
Grade
WrongQuestionIds
Feedback
```

For premium users, also:

```text
Breakdown
Strengths
WeakAreas
Suggestions
```

### Viva Answer

> GetResultAsync returns basic result for free users and detailed review breakdown for premium users.

---

## 45. Free vs Premium Result

### Free Result

Free users get:

```text
Score
Percentage
Grade
Wrong question ids
Feedback
```

### Premium Result

Premium users get:

```text
Question-wise breakdown
Your answer
Correct/ideal answer
Score
Strengths
Weak areas
Suggestions
```

### Viva Answer

> Free users get result summary, while premium users get detailed question-wise review, strengths, weak areas, and suggestions.

---

## 46. Interview Events

### Published Events

InterviewController publishes:

```text
InterviewStartedEvent
InterviewCompletedEvent
EmailRequestedEvent
```

### When Started

After:

```text
POST /api/interviews/start
```

### When Completed

After:

```text
POST /api/interviews/submit
```

### Viva Answer

> InterviewService publishes interview started and completed events, plus email notification events after completion.

---

## 47. Interview Completion Notification Flow

```text
1. User submits interview.
2. InterviewService evaluates answers.
3. InterviewResult is saved.
4. InterviewController publishes InterviewCompletedEvent.
5. IdentityService UserNotificationEventConsumer creates bell notification.
6. InterviewController publishes EmailRequestedEvent.
7. NotificationService sends interview completion email.
```

### Viva Explanation

> Interview completion triggers RabbitMQ events for in-app notification and email notification.

---

## 48. Why Subjective Interviews Need AI

### MCQ Evaluation

MCQ has fixed correct option:

```text
A/B/C/D
```

### Subjective Evaluation

Interview answer can be written in many valid ways.

AI can judge:

```text
Correctness
Depth
Structure
Examples
Trade-offs
Completeness
```

### Viva Answer

> Subjective interview answers need AI because there is no single fixed answer. AI can evaluate quality, depth, clarity, and correctness.

---

## 49. What Happens If Gemini Returns Invalid JSON?

### During Question Generation

Service tries to parse Gemini response.

If parsing fails:

```text
Logs warning
Retries or tries smaller batch
Falls back to available admin/pool questions
May return partial set or 503 if no questions
```

### During Evaluation

If parsing/evaluation fails:

```text
Fallback evaluation is used
Answers are still saved
```

### Viva Answer

> If Gemini returns invalid JSON, the service logs it and falls back. During evaluation, user answers are still saved with fallback feedback.

---

## 50. Security and Ownership Checks

### Ownership

Interview operations check:

```text
Interview.UserId == current user id
```

### Admin

Admin all-interviews endpoint uses:

```csharp
[Authorize(Roles = "Admin")]
```

### Why

Users should not access or submit another user's interview.

### Viva Answer

> InterviewService enforces ownership using UserId from JWT and protects admin endpoint with role-based authorization.

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
Free user limit reached -> 403
Interview not found -> 404
Gemini unavailable -> 503
```

### Viva Answer

> InterviewService uses custom exceptions for validation, forbidden access, not found records, and service unavailable cases.

---

## 52. Complete Flow: Start and Begin Interview

```text
1. User logs in and gets JWT.
2. Frontend sends POST /api/interviews/start with title and domain.
3. Controller reads userId and isPremium from JWT.
4. StartInterviewAsync validates domain and free limit.
5. Interview is saved with Status = Pending.
6. InterviewStartedEvent is published.
7. Frontend calls POST /api/interviews/{id}/begin.
8. BeginInterviewAsync finds interview and checks ownership.
9. Question count is extracted from domain.
10. Previous user questions are loaded to avoid repeats.
11. Questions are selected from admin, JIT pool, or Gemini.
12. Questions are saved.
13. Interview becomes InProgress.
14. Questions are returned to frontend.
```

### Viva Explanation

> Start creates a pending interview, and begin generates questions, marks the interview in progress, and returns the question set.

---

## 53. Complete Flow: Submit Interview

```text
1. User submits answers.
2. Controller reads userId, isPremium, and email from JWT.
3. SubmitInterviewAsync validates interview ownership and InProgress status.
4. Questions are loaded.
5. Submitted question ids are validated.
6. Gemini evaluates answers in batches.
7. Scores are saved in InterviewAnswers.
8. InterviewResult is created.
9. Interview status becomes Completed.
10. GetResultAsync returns free or premium result.
11. InterviewCompletedEvent is published.
12. EmailRequestedEvent is published.
```

### Viva Explanation

> Submit saves user answers, evaluates them, calculates result, completes the interview, and publishes completion events.

---

## 54. What Happens If InterviewService Is Down?

### Effects

Users cannot:

```text
Start interview
Begin interview
Submit interview
View interview result
View interview history
```

Other services may still work:

```text
Login
Assessments
Subscriptions
Notifications
```

### Viva Answer

> If InterviewService is down, interview-related APIs fail, but other services like IdentityService and AssessmentService can still work.

---

## 55. Limitations and Improvements

### Current Limitations

- Domain field stores many profile values as a formatted string
- Gemini dependency can cause rate-limit issues
- Fallback evaluation gives zero score
- Admin question management for interviews is not clearly exposed as endpoints
- Some controller-level try-catch overlaps with global exception middleware
- Question id mapping uses OrderIndex for frontend submit
- No separate processed event id table

### Possible Improvements

- Store interview profile in structured fields
- Add stronger admin question management endpoints
- Add retry queue for failed evaluations
- Add option to re-evaluate saved answers later
- Use outbox pattern for event publishing
- Add more detailed rubric storage per answer
- Add pagination for interview history and admin all interviews
- Add Redis/global cache for generated questions
- Add more robust AI JSON schema validation
- Add idempotency protection for submit

### Balanced Viva Answer

> InterviewService supports the main subjective interview flow well. Future improvements could include structured interview profile fields, re-evaluation of failed AI scoring, better admin question management, pagination, outbox pattern, and stronger idempotency.

---

## 56. Best Full Viva Answer for Topic 14

> InterviewService manages subjective mock interview sessions. A user starts an interview, and the service creates a pending interview record after checking free versus premium limits. When the user begins, the service generates questions using a three-tier strategy: admin curated questions, JIT global question pool, and Gemini AI generation. It avoids repeated questions by comparing previous normalized question texts. The interview moves from Pending to InProgress. When the user submits answers, InterviewService validates ownership and question ids, evaluates answers using Gemini in batches, scores each answer out of 10, calculates total score, percentage, grade, and feedback, then marks the interview Completed. Free users get a basic result summary, while premium users get detailed breakdown, strengths, weak areas, and suggestions. Interview completion also publishes RabbitMQ events for notifications and email.

---

## 57. Common Viva Questions and Answers

### Q1. What is InterviewService?

InterviewService manages subjective mock interviews, questions, answers, evaluation, and results.

### Q2. Why is InterviewService separate?

Because subjective interview generation and evaluation are independent from authentication, assessment, and payment logic.

### Q3. What tables does InterviewService use?

Interviews, Questions, InterviewAnswers, InterviewResults, and GlobalInterviewQuestions.

### Q4. What are interview statuses?

Pending, InProgress, and Completed.

### Q5. What does start interview do?

It creates a pending interview after validating domain and checking free user limit.

### Q6. What is the free user interview limit?

Free users can create only one interview.

### Q7. What does begin interview do?

It generates questions, saves them, marks interview InProgress, and returns questions.

### Q8. Why is begin idempotent?

To avoid duplicate questions if the user refreshes or clicks begin again.

### Q9. How are questions generated?

Using admin curated questions first, then JIT pool, then Gemini AI generation.

### Q10. What is JIT question pool?

It is a global cache of reusable generated interview questions.

### Q11. Why avoid previous questions?

To give users fresh practice and avoid repeated questions.

### Q12. How is Gemini used?

Gemini generates interview questions and evaluates subjective answers.

### Q13. What happens if Gemini fails?

The service uses admin/pool questions for generation and fallback evaluation for answer submission.

### Q14. How are answers evaluated?

Gemini scores answers based on clarity, technical accuracy, practical depth, and completeness.

### Q15. What is score per question?

Each answer is scored out of 10.

### Q16. How is grade calculated?

Percentage determines grade: A+ for 90+, A for 80+, B for 70+, C for 60+, D for 50+, otherwise F.

### Q17. What does free result include?

Score, max score, percentage, grade, wrong question ids, and feedback.

### Q18. What does premium result include?

Free result plus question-wise breakdown, strengths, weak areas, and suggestions.

### Q19. What events does InterviewService publish?

InterviewStartedEvent, InterviewCompletedEvent, and EmailRequestedEvent.

### Q20. Why publish InterviewCompletedEvent?

So other services can create notifications or react to interview completion asynchronously.

### Q21. How is ownership checked?

InterviewService checks Interview.UserId against the current user id from JWT.

### Q22. What is WarmUpCache?

It pre-generates or checks cached questions for a domain to reduce wait time.

### Q23. Why use Polly?

Polly retries transient Gemini failures and rate limits.

### Q24. What is GlobalInterviewQuestion?

It stores reusable cached questions for future interviews.

### Q25. What improvements can be made?

Structured profile fields, re-evaluation support, better admin question endpoints, pagination, outbox pattern, and stronger idempotency.

---

## 58. Quick Revision Summary

- InterviewService handles subjective interviews.
- All interview APIs require authentication.
- Admin all-interviews endpoint requires Admin role.
- Interview lifecycle is Pending, InProgress, Completed.
- Start creates Pending interview.
- Begin generates questions and marks InProgress.
- Submit saves answers and marks Completed.
- Free users can create only one interview.
- Premium users get more access and detailed results.
- Questions come from admin curated questions, JIT pool, or Gemini.
- JIT pool reduces Gemini dependency.
- Previous questions are avoided.
- Domain string includes profile and question count.
- Question count is clamped between 3 and 12.
- Gemini generates scenario-based questions.
- Gemini evaluates subjective answers.
- Evaluation is batched to reduce timeout risk.
- Each answer is scored out of 10.
- Grade is calculated from percentage.
- Free result gives summary.
- Premium result gives breakdown, strengths, weak areas, suggestions.
- Interview events trigger notifications and emails.
- Fallbacks exist for Gemini failures.
- Future improvements include structured profile fields and re-evaluation support.

