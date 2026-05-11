# Topic 18: Gemini API and AI Integration

Project: Mock Interview Platform  
Focus: Understanding external API integration with Gemini, HttpClient, API keys, prompts, structured JSON output, retry handling, rate limiting, JSON parsing, AI question generation, subjective answer evaluation, caching, and fallback behavior.

---

## 1. What Is Gemini API?

### Simple Explanation

Gemini API is an external AI service from Google.

Your backend sends a prompt to Gemini.

Gemini returns generated text or structured JSON.

### In Your Project

Gemini is used in:

```text
InterviewService
AssessmentService
```

It is used for:

- Interview question generation
- Interview answer evaluation
- MCQ assessment question generation

### Viva Answer

> Gemini API is an external AI API used in my project to generate interview questions, evaluate subjective interview answers, and generate MCQ assessment questions.

---

## 2. Why Gemini Is Used

### Simple Explanation

The platform needs dynamic practice content.

Manually creating enough questions for every domain is difficult.

Gemini helps generate questions based on:

```text
Domain
Role
Difficulty
Technologies
Question count
Sub-scenario
```

### For Interviews

Gemini can generate practical subjective questions and evaluate written answers.

### For Assessments

Gemini can generate MCQ questions with options and correct answer.

### Viva Answer

> Gemini is used to dynamically generate practice questions and evaluate subjective interview answers, reducing dependency on only manually created question banks.

---

## 3. External API Integration

### Simple Explanation

External API integration means backend calls another service over HTTP.

### In Your Project

Backend calls:

```text
https://generativelanguage.googleapis.com/v1beta/...:generateContent
```

### Required Pieces

```text
HttpClient
API key
Request body
Prompt
JSON response parsing
Retry/fallback handling
```

### Viva Answer

> External API integration means calling third-party services over HTTP. My project integrates Gemini using HttpClient, API key, prompts, JSON request body, and JSON response parsing.

---

## 4. Where Gemini Is Configured

### InterviewService Config

```text
Backend/InterviewService/appsettings.Example.json
```

### AssessmentService Config

```text
Backend/AssessmentService/appsettings.Example.json
```

### Config Section

```json
"Gemini": {
  "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
  "Model": "gemini-2.5-flash"
}
```

### Viva Answer

> Gemini API key and model are stored in the Gemini configuration section of InterviewService and AssessmentService.

---

## 5. Why API Key Should Not Be Hardcoded

### Simple Explanation

API key is a secret.

If hardcoded and committed, anyone can use it.

### Correct Approach

Store it in:

```text
appsettings for local example only
environment variables
user secrets
secret manager
cloud key vault
```

### Viva Answer

> Gemini API key should not be hardcoded because it is a secret. It should be stored in configuration or secret manager.

---

## 6. HttpClient

### Simple Explanation

HttpClient is used to send HTTP requests from backend to Gemini API.

### In InterviewService

Registered:

```csharp
builder.Services.AddHttpClient();
builder.Services.AddScoped<IInterviewSvc, InterviewSvc>();
```

Injected:

```csharp
private readonly HttpClient _httpClient;
```

### In AssessmentService

Registered:

```csharp
builder.Services.AddHttpClient<IAssessmentService, AssessmentSvc>();
```

### Viva Answer

> HttpClient is used to call Gemini API. InterviewService registers HttpClient support, and AssessmentService uses typed HttpClient registration.

---

## 7. Gemini Request URL

### Interview Question Generation

```text
https://generativelanguage.googleapis.com/v1beta/{model}:generateContent?key={apiKey}
```

### Interview Evaluation

```text
https://generativelanguage.googleapis.com/v1beta/{model}:generateContent
```

with:

```text
x-goog-api-key header
```

### Assessment Question Generation

```text
https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}
```

### Viva Answer

> Gemini calls use the generateContent endpoint, passing API key either in URL or x-goog-api-key header depending on the method.

---

## 8. Gemini Model

### Config

```text
Gemini:Model
```

### Examples in Project

```text
gemini-2.5-flash
gemini-2.0-flash-lite fallback in code
```

### NormalizeGeminiModel

InterviewService normalizes model name:

```text
gemini-2.5-flash -> models/gemini-2.5-flash
```

if needed.

### Viva Answer

> Gemini model is configurable. InterviewService normalizes model name so it works with the generateContent endpoint.

---

## 9. Gemini in InterviewService

### Used For

```text
1. Generate subjective interview questions
2. Evaluate subjective interview answers
```

### Main Methods

```text
GenerateAiQuestionsAsync
EvaluateInterviewAnswersAsync
EvaluateBatchAsync
```

### Viva Answer

> InterviewService uses Gemini for both question generation and subjective answer evaluation.

---

## 10. Gemini in AssessmentService

### Used For

```text
Generate MCQ questions
```

### Main Method

```text
GenerateAiAssessmentQuestionsAsync
```

### Important Difference

AssessmentService does not need Gemini to score answers.

MCQs are scored by comparing selected option with correct option.

### Viva Answer

> AssessmentService uses Gemini only to generate MCQ questions. It does not use Gemini for scoring because MCQs have stored correct options.

---

## 11. Prompting

### Simple Explanation

Prompting means giving clear instructions to AI.

The prompt tells Gemini:

```text
What to generate
How many items
What format to return
What rules to follow
```

### Why Important

Bad prompt can produce:

```text
Wrong format
Too many questions
Theory questions instead of practical questions
Markdown instead of JSON
```

### Viva Answer

> Prompting is the instruction given to Gemini. Good prompts are important to get correct, structured, useful output.

---

## 12. Interview Question Generation Prompt

### Goal

Generate concise practical mock interview questions.

### Prompt Rules

```text
Generate exactly needed questions
Each question under 50 words
Use scenario-based questions
Return only valid JSON
Do not use markdown
Do not ask multi-part questions
```

### Expected JSON

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

> Interview question prompt asks Gemini for concise scenario-based questions with subtopic and ideal answer in strict JSON format.

---

## 13. Assessment MCQ Generation Prompt

### Goal

Generate practical MCQs for a domain and difficulty.

### Prompt Includes

```text
Question count
Difficulty
Domain
Random sub-scenario
Four plausible options
Exactly one correct answer
```

### Expected Fields

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

> Assessment prompt asks Gemini to generate practical MCQs with four options, one correct option, and subtopic.

---

## 14. Interview Answer Evaluation Prompt

### Goal

Evaluate subjective answers out of 10.

### Scoring Criteria

```text
Clarity: 0-2
Technical Accuracy: 0-3
Practical Depth: 0-3
Completeness: 0-2
```

### Expected JSON

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

### Viva Answer

> Interview evaluation prompt asks Gemini to score answers using clarity, technical accuracy, practical depth, and completeness, and return structured JSON feedback.

---

## 15. Structured JSON Output

### Simple Explanation

Backend needs predictable data.

So Gemini is asked to return JSON, not normal paragraph text.

### In Project

Uses:

```text
responseMimeType = application/json
responseSchema
```

in question generation.

### Why Useful

Structured JSON is easier to parse into C# DTOs/models.

### Viva Answer

> Structured JSON output is used so backend can reliably parse Gemini response into questions or evaluations.

---

## 16. Response Schema

### In InterviewService

Gemini is given schema:

```text
object with questions array
question
subtopic
idealAnswer
```

### In AssessmentService

Gemini is given schema:

```text
array of MCQ objects
text
optionA
optionB
optionC
optionD
correctOption
subtopic
```

### Viva Answer

> Response schema tells Gemini the exact JSON shape expected by backend, reducing invalid output.

---

## 17. JSON Parsing

### Tools Used

```text
JsonDocument
JsonSerializer
JsonSerializerOptions PropertyNameCaseInsensitive
```

### Gemini Response Structure

Backend reads:

```text
candidates[0].content.parts[0].text
```

Then parses the text as JSON.

### Viva Answer

> The backend parses Gemini response using JsonDocument to extract text and JsonSerializer to convert JSON into C# objects.

---

## 18. Cleaning JSON Response

### Problem

AI may return markdown code fences:

```text
```json
...
```
```

### Project Solution

Code removes:

```text
```json
```
```

and trims response before deserialization.

### Viva Answer

> The project cleans Gemini response by removing markdown code fences before JSON deserialization.

---

## 19. Interview Question Parsing

### Method

```text
ParseAiQuestionList
```

### Logic

```text
Remove code fences
Find first { and last }
Deserialize to AiQuestionList
Return null if parsing fails
```

### Viva Answer

> InterviewService parses Gemini question response into AiQuestionList and safely returns null if parsing fails.

---

## 20. Assessment Question Parsing

### DTO

```text
GeminiAssessmentQuestionDto
```

### Fields

```text
text
optionA
optionB
optionC
optionD
correctOption
subtopic
```

### Safety

If correct option is invalid:

```text
Default to A
```

### Viva Answer

> AssessmentService parses Gemini MCQs into GeminiAssessmentQuestionDto and validates fields before saving as MCQQuestion.

---

## 21. Retry Handling

### Why Needed

External APIs may fail because of:

```text
Rate limits
Network errors
Timeouts
Server errors
Temporary unavailability
```

### Project Uses

```text
Polly retry
Manual retry loop for evaluation
```

### Viva Answer

> Retry handling is used because Gemini is an external API and may fail temporarily or return rate limit errors.

---

## 22. Polly in InterviewService

### Handles

```text
HttpRequestException
TaskCanceledException
429 Too Many Requests
500+ server errors
```

### Backoff

```text
3 seconds
9 seconds
27 seconds
```

If 429 has Retry-After header, it uses that delay plus buffer.

### Viva Answer

> InterviewService uses Polly with exponential backoff and Retry-After support for Gemini question generation.

---

## 23. Polly in AssessmentService

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
```

### Viva Answer

> AssessmentService uses Polly fast retries for Gemini MCQ generation to recover from temporary failures.

---

## 24. Rate Limiting

### Simple Explanation

Rate limit means API refuses requests because too many calls were made.

### Status Code

```text
429 Too Many Requests
```

### In InterviewService

If Gemini returns 429 after retries:

```text
Stop generation loop
Do not keep hammering API
Use available questions/fallback
```

### Viva Answer

> Rate limiting is handled by retrying with delay and stopping further calls when 429 persists.

---

## 25. Batching

### Why Batching Is Used

Large requests can timeout.

Small batches reduce risk.

### Interview Question Generation

Batch size:

```text
3 questions
```

### Interview Evaluation

Batch size:

```text
4 questions
```

### Viva Answer

> Batching reduces timeout and rate-limit risk. Interview generation uses small question batches, and evaluation batches answers in groups of 4.

---

## 26. Dynamic Sub-Scenarios

### Purpose

Generate variety without sending a long exclusion list.

### Interview Examples

```text
System Architecture
Debugging & Root-Cause Analysis
Performance Optimization
Security & Threat Modeling
API Design & Integration
```

### Assessment Examples

```text
memory leaks and garbage collection
API design and REST best practices
LINQ query optimization
event-driven architecture
retry patterns
```

### Viva Answer

> Dynamic sub-scenarios guide Gemini to generate diverse, practical questions instead of repetitive generic questions.

---

## 27. Question Quality Filtering

### InterviewService

Rejects low-quality interview questions if:

```text
Text is empty
Word count too low
Length too short
```

### AssessmentService

Rejects MCQs if:

```text
Question text is empty
Any option is empty
Question is too short
Starts with true or false
```

### Viva Answer

> Generated questions are filtered for basic quality before saving or returning to users.

---

## 28. Duplicate Avoidance

### InterviewService

Normalizes question text and avoids duplicates using:

```text
NormalizeQuestionText
previous user questions
group by normalized text
```

### AssessmentService

Generated MCQs are deduplicated within generated batch by normalized text.

### Viva Answer

> The project normalizes question text to reduce duplicate generated questions.

---

## 29. JIT Cache / Global Pool

### InterviewService

Gemini-generated questions are saved to:

```text
GlobalInterviewQuestions
```

with source:

```text
Gemini_JIT
```

### AssessmentService

Gemini-generated MCQs are saved to:

```text
MCQQuestions
```

for future use.

### Viva Answer

> Gemini-generated questions are cached in the database so future sessions can reuse them and reduce API calls.

---

## 30. Warm-Up Cache

### InterviewService

```text
POST /api/interviews/warm-up
```

Pre-generates interview questions for a domain.

### AssessmentService

```text
POST /api/assessments/warm-up
```

Pre-generates MCQ questions for a domain.

### Viva Answer

> Warm-up cache calls Gemini before the user starts answering, reducing wait time during actual session.

---

## 31. Fallback in Interview Question Generation

### Three-Tier Strategy

```text
1. Admin curated questions
2. JIT global pool
3. Gemini generation
```

### If Gemini Fails

Use available admin/JIT questions.

If no questions are available:

```text
503 Service Unavailable
```

### Viva Answer

> InterviewService falls back to admin curated questions and JIT pool if Gemini generation fails.

---

## 32. Fallback in Interview Evaluation

### If Gemini Evaluation Fails

The service:

```text
Saves user answers
Creates zero-score fallback evaluations
Stores fallback feedback
Does not lose user submission
```

### Viva Answer

> If Gemini answer evaluation fails, InterviewService still saves answers and returns fallback feedback so user submission is not lost.

---

## 33. Fallback in AssessmentService

### If Gemini Generation Fails

AssessmentService uses cached database questions if available.

If no cached or generated questions exist:

```text
503 Service Unavailable
```

### Viva Answer

> If Gemini MCQ generation fails, AssessmentService uses cached questions when available; otherwise it returns service unavailable.

---

## 34. What Happens If Gemini API Key Is Missing?

### InterviewService

Throws:

```text
Gemini API key is not configured.
```

or:

```text
Gemini is not configured on the interview service.
```

### AssessmentService

Throws:

```text
Gemini is not configured on the assessment service.
```

### Status

```text
503 Service Unavailable
```

### Viva Answer

> If Gemini API key is missing, services throw 503 because AI functionality is unavailable.

---

## 35. What Happens If Gemini Returns Invalid JSON?

### InterviewService

```text
Log warning
Try smaller batch / continue
Return available questions
Fallback evaluation if evaluation parsing fails
```

### AssessmentService

```text
Throw AppException for unreadable payload
Polly retries
Use cached questions if possible
```

### Viva Answer

> If Gemini returns invalid JSON, backend logs the issue, retries or falls back, and avoids crashing the whole request where possible.

---

## 36. What Happens If Gemini Returns Empty Candidates?

### Meaning

Gemini response does not contain generated content.

### Project Behavior

InterviewService logs warning and continues/falls back.

AssessmentService throws service unavailable and retry policy may retry.

### Viva Answer

> Empty Gemini candidates are treated as external API failure and handled through retry or fallback.

---

## 37. AI Answer Evaluation vs MCQ Checking

### Interview Answer

Subjective:

```text
Needs AI scoring
Score based on rubric
Feedback generated by AI
```

### Assessment Answer

Objective:

```text
SelectedOption == CorrectOption
No AI needed
```

### Viva Answer

> Interview answers need AI because they are subjective, while MCQ assessments can be scored directly using stored correct options.

---

## 38. Logging Gemini Failures

### Logged Cases

```text
Retry attempts
Rate limit
No candidates
No text
Parse failure
Batch evaluation failure
Warm-up failure
JIT pool save failure
```

### Why Useful

Logs help debug:

```text
Bad API key
Rate limits
Invalid AI output
Network problems
Timeouts
```

### Viva Answer

> Gemini failures are logged so developers can debug rate limits, invalid JSON, empty responses, and external API errors.

---

## 39. Security of API Keys

### Important Rules

- Do not hardcode API key
- Do not expose API key to frontend
- Do not commit real key
- Use environment variables or secret manager
- Rotate key if leaked
- Avoid logging key

### Viva Answer

> Gemini API key must stay on backend and should be stored securely in configuration or secret manager, never exposed to frontend.

---

## 40. Complete Flow: Interview Question Generation

```text
1. User begins interview.
2. InterviewService checks admin curated questions.
3. If not enough, checks JIT global pool.
4. If still not enough, calls Gemini.
5. Backend builds prompt with domain and scenario.
6. Backend sends JSON request using HttpClient.
7. Gemini returns candidates text.
8. Backend extracts candidates[0].content.parts[0].text.
9. Backend parses JSON into question list.
10. Backend filters quality and duplicates.
11. Backend saves generated questions to JIT pool.
12. Backend saves selected questions for interview.
```

### Viva Explanation

> Interview question generation uses admin/JIT fallback first and Gemini only when more questions are needed.

---

## 41. Complete Flow: Interview Answer Evaluation

```text
1. User submits interview answers.
2. InterviewService loads saved questions.
3. Questions are split into batches of 4.
4. Backend creates evaluation payload with question, ideal answer, and user's answer.
5. Backend prompts Gemini to score out of 10 using rubric.
6. Gemini returns JSON evaluations.
7. Backend parses score, ideal answer, feedback, and follow-up question.
8. Scores are saved in InterviewAnswers.
9. InterviewResult is created.
10. If evaluation fails, fallback zero-score evaluation is used.
```

### Viva Explanation

> Interview answer evaluation uses Gemini rubric scoring and fallback handling so answers are saved even if AI fails.

---

## 42. Complete Flow: Assessment MCQ Generation

```text
1. User starts assessment or requests next batch.
2. AssessmentService checks cached MCQQuestions.
3. If not enough, calls Gemini.
4. Backend chooses random sub-scenario.
5. Backend sends prompt and response schema.
6. Gemini returns MCQ JSON.
7. Backend parses question, options, correct option, and subtopic.
8. Backend validates question quality.
9. Backend saves generated MCQs to MCQQuestions table.
10. Backend returns questions to frontend.
```

### Viva Explanation

> AssessmentService uses Gemini to generate MCQs only when the database question cache is insufficient.

---

## 43. Why Caching Is Important

### Benefits

- Reduces Gemini API calls
- Reduces cost
- Reduces rate-limit risk
- Improves speed
- Provides fallback when Gemini fails

### In Project

```text
InterviewService -> GlobalInterviewQuestions
AssessmentService -> MCQQuestions
```

### Viva Answer

> Caching generated questions reduces API dependency, improves performance, and helps when Gemini is unavailable.

---

## 44. Why Retry Should Not Be Infinite

### Problem

Infinite retry can:

```text
Block API request
Increase cost
Hit rate limits harder
Exhaust server resources
Delay user response
```

### Project Approach

Uses limited retries and fallback.

### Viva Answer

> Retry should be limited because infinite retry can overload the API, increase latency, and waste resources.

---

## 45. Limitations and Improvements

### Current Limitations

- Gemini responses can still be invalid despite schema
- Some model defaults differ between config and code fallback
- Assessment previous-question avoidance method exists but is not fully used in start flow
- No centralized AI client abstraction
- No persistent AI request/response audit table
- No token/cost tracking
- No circuit breaker around Gemini
- Fallback evaluation gives zero score
- API key validation is simple string checks

### Possible Improvements

- Create shared Gemini client service
- Add circuit breaker with Polly
- Add AI request timeout settings
- Store AI generation metadata
- Track token usage and cost
- Add re-evaluation job for failed interview evaluations
- Add stronger JSON schema validation
- Add Redis/domain cache layer
- Add content safety filtering
- Add admin review workflow for AI-generated questions

### Balanced Viva Answer

> Current Gemini integration supports dynamic generation and evaluation with retries, structured JSON, caching, and fallback. Future improvements could include shared AI client, circuit breaker, audit logging, cost tracking, stronger schema validation, and re-evaluation support.

---

## 46. Best Full Viva Answer for Topic 18

> My project integrates Gemini API as an external AI service using HttpClient. Gemini configuration is stored in appsettings under Gemini:ApiKey and Gemini:Model. InterviewService uses Gemini to generate subjective interview questions and evaluate user answers using a rubric for clarity, technical accuracy, practical depth, and completeness. AssessmentService uses Gemini to generate MCQ questions with four options and one correct option, but MCQ scoring itself does not use AI because answers are checked against CorrectOption. The backend asks Gemini for structured JSON using prompts and response schemas, parses responses with JsonDocument and JsonSerializer, and handles invalid JSON, empty responses, rate limits, and failures using Polly retries and fallback logic. Generated questions are cached in database to reduce API calls and improve reliability.

---

## 47. Common Viva Questions and Answers

### Q1. What is Gemini API?

Gemini API is an external AI API used for generating content and evaluating subjective answers.

### Q2. Where is Gemini used in your project?

InterviewService and AssessmentService.

### Q3. What does InterviewService use Gemini for?

Interview question generation and subjective answer evaluation.

### Q4. What does AssessmentService use Gemini for?

Generating MCQ questions.

### Q5. Does AssessmentService use Gemini for scoring?

No. MCQ scoring compares selected option with stored correct option.

### Q6. Where is Gemini API key stored?

In configuration under Gemini:ApiKey.

### Q7. Why should API key not be hardcoded?

Because it is a secret and could be leaked.

### Q8. What is HttpClient used for?

To send HTTP requests from backend to Gemini API.

### Q9. What is prompting?

Prompting is giving instructions to Gemini about what to generate and in what format.

### Q10. Why use structured JSON output?

So backend can parse AI response reliably.

### Q11. What is responseSchema?

It tells Gemini the expected JSON structure.

### Q12. How does backend parse Gemini response?

It extracts candidates content text using JsonDocument and deserializes JSON using JsonSerializer.

### Q13. What happens if Gemini returns invalid JSON?

The backend logs it and retries or falls back depending on the flow.

### Q14. What is Polly used for?

Polly is used for retry handling around Gemini calls.

### Q15. What is rate limiting?

Rate limiting means Gemini rejects requests when too many calls are made, usually with 429 status.

### Q16. Why use batching?

Batching reduces timeout and rate-limit risk.

### Q17. How are interview answers scored?

Gemini scores each answer out of 10 using clarity, accuracy, practical depth, and completeness.

### Q18. What happens if interview evaluation fails?

Answers are saved and fallback zero-score evaluation with feedback is used.

### Q19. Why cache generated questions?

To reduce Gemini calls, improve speed, and provide fallback.

### Q20. What is JIT pool?

A database cache of generated interview questions for reuse.

### Q21. What happens if Gemini API key is missing?

Service returns 503 because AI functionality is unavailable.

### Q22. Why not retry forever?

Infinite retry can increase latency, cost, rate limits, and resource usage.

### Q23. What is dynamic sub-scenario?

A randomly chosen focus area that helps Gemini generate varied questions.

### Q24. What security precautions are needed?

Keep API key on backend, do not log it, and store it in secret manager or environment variables.

### Q25. What improvements can be made?

Shared Gemini client, circuit breaker, audit logs, cost tracking, re-evaluation jobs, and stronger schema validation.

---

## 48. Quick Revision Summary

- Gemini is an external AI API.
- Used in InterviewService and AssessmentService.
- InterviewService generates questions and evaluates answers.
- AssessmentService generates MCQs only.
- MCQ scoring does not need AI.
- Gemini settings are ApiKey and Model.
- HttpClient sends requests to generateContent.
- Prompts instruct Gemini what to return.
- Structured JSON output is required.
- responseSchema improves JSON reliability.
- JsonDocument extracts candidates text.
- JsonSerializer parses JSON into C# objects.
- Code removes markdown code fences.
- Polly handles retry.
- 429 means rate limit.
- Batching reduces timeout risk.
- Interview evaluation uses rubric scoring.
- Fallback saves answers if evaluation fails.
- Generated questions are cached.
- Interview cache uses GlobalInterviewQuestions.
- Assessment cache uses MCQQuestions.
- API keys must stay secret.
- Future improvements include shared AI client, circuit breaker, audit logs, and cost tracking.

