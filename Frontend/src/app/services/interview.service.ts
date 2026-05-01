import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap, timeout, BehaviorSubject, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/user.model';
import {
  BeginInterviewResponse,
  Interview,
  InterviewResult,
  InterviewStartForm,
  SubmitInterviewRequest
} from '../models/interview.model';

/**
 * Purpose: Centralizes interview API communication.
 * Components call this service for start, begin, and submit instead of building HTTP calls themselves.
 */
@Injectable({
  providedIn: 'root'
})
export class InterviewService {
  /** Base URL for InterviewService endpoints through the API Gateway. */
  private apiUrl = `${environment.apiUrl}/interviews`;
  private readonly interviewCacheKey = 'the_interview_my_interviews';
  private interviewsSubject = new BehaviorSubject<Interview[] | null>(null);

  constructor(private http: HttpClient) {}

  /** Reads both camelCase and PascalCase API wrappers so the UI is not blocked by serializer casing. */
  private unwrapData<T>(response: any): T {
    return (response?.data ?? response?.Data ?? response) as T;
  }

  /** Converts .NET $values wrappers and normal arrays into a plain frontend array. */
  private normalizeArray<T>(value: any): T[] {
    if (Array.isArray(value)) {
      return value;
    }

    if (Array.isArray(value?.$values)) {
      return value.$values;
    }

    return [];
  }

  /** Normalizes interview entities so routing never depends on JSON casing. */
  private normalizeInterview(rawInterview: any): Interview {
    const interview = rawInterview || {};

    return {
      id: interview.id ?? interview.Id ?? 0,
      userId: interview.userId ?? interview.UserId ?? 0,
      title: interview.title ?? interview.Title ?? '',
      domain: interview.domain ?? interview.Domain ?? '',
      type: interview.type ?? interview.Type ?? '',
      status: interview.status ?? interview.Status ?? '',
      createdAt: interview.createdAt ?? interview.CreatedAt ?? '',
      startedAt: interview.startedAt ?? interview.StartedAt,
      completedAt: interview.completedAt ?? interview.CompletedAt
    };
  }

  /** Normalizes question arrays from begin/get responses into one frontend shape. */
  private normalizeSession(interviewId: number, rawSession: any): BeginInterviewResponse {
    const session = rawSession || {};
    const rawQuestions =
      session.questions ??
      session.Questions ??
      session.interview?.questions ??
      session.interview?.Questions ??
      session.Interview?.questions ??
      session.Interview?.Questions ??
      [];

    const questions = this.normalizeArray<any>(rawQuestions);

    return {
      interviewId:
        session.interviewId ||
        session.InterviewId ||
        session.interview?.id ||
        session.interview?.Id ||
        session.Interview?.id ||
        session.Interview?.Id ||
        interviewId,
      message: session.message || session.Message || 'Interview questions loaded.',
      questions: questions.map((question: any) => ({
        id: question.id ?? question.Id,
        text: question.text ?? question.Text,
        questionType: question.questionType ?? question.QuestionType,
        optionA: question.optionA ?? question.OptionA,
        optionB: question.optionB ?? question.OptionB,
        optionC: question.optionC ?? question.OptionC,
        optionD: question.optionD ?? question.OptionD,
        orderIndex: question.orderIndex ?? question.OrderIndex
      }))
    };
  }

  private normalizeInterviewResult(rawResult: any): InterviewResult {
    const result = rawResult || {};
    const breakdown = this.normalizeArray<any>(result.breakdown ?? result.Breakdown);
    const wrongQuestionIds = this.normalizeArray<number>(result.wrongQuestionIds ?? result.WrongQuestionIds);
    const strengths = this.normalizeArray<string>(result.strengths ?? result.Strengths);
    const weakAreas = this.normalizeArray<string>(result.weakAreas ?? result.WeakAreas);
    const suggestions = this.normalizeArray<string>(result.suggestions ?? result.Suggestions);

    return {
      totalScore: result.totalScore ?? result.TotalScore ?? 0,
      maxScore: result.maxScore ?? result.MaxScore ?? 0,
      percentage: result.percentage ?? result.Percentage ?? 0,
      grade: result.grade ?? result.Grade ?? '-',
      wrongQuestionIds,
      feedback: result.feedback ?? result.Feedback ?? '',
      isPremiumResult: breakdown.length > 0 || strengths.length > 0 || weakAreas.length > 0 || suggestions.length > 0,
      breakdown: breakdown.map((item: any) => ({
        text: item.text ?? item.Text ?? '',
        subtopic: item.subtopic ?? item.Subtopic ?? '',
        yourAnswer: item.yourAnswer ?? item.YourAnswer ?? '',
        correctAnswer: item.correctAnswer ?? item.CorrectAnswer ?? '',
        isCorrect: item.isCorrect ?? item.IsCorrect ?? null,
        score: item.score ?? item.Score ?? 0
      })),
      strengths,
      weakAreas,
      suggestions
    };
  }

  /**
   * Creates a pending interview record.
   * Current backend requires title/domain, so the richer UI form is safely mapped into those fields.
   */
  startInterview(form: InterviewStartForm): Observable<Interview> {
    const title = `${form.role} ${form.interviewType} Interview`;
    const domainParts = [
      form.role,
      form.experience,
      form.interviewType,
      form.difficulty,
      `${form.numberOfQuestions} Questions`,
      form.techStack.join(', ')
    ].filter(Boolean);

    const payload = {
      title,
      domain: domainParts.join(' | '),
      role: form.role,
      experience: form.experience,
      interviewType: form.interviewType,
      techStack: form.techStack,
      difficulty: form.difficulty,
      numberOfQuestions: form.numberOfQuestions
    };

    return this.http
      .post<ApiResponse<Interview>>(`${this.apiUrl}/start`, payload)
      .pipe(map(response => this.normalizeInterview(this.unwrapData<any>(response))));
  }

  /**
   * Convenience method for the requested flow: create interview, then expose its id for routing.
   * Backend 403 responses are allowed to reach the component so upgrade messaging is visible.
   */
  createInterviewSession(form: InterviewStartForm): Observable<Interview> {
    return this.startInterview(form);
  }

  /** Returns interviews already stored for the authenticated user, fetching if not cached. */
  getMyInterviews(forceRefresh = false): Observable<Interview[]> {
    if (!forceRefresh && this.interviewsSubject.value !== null) {
      return of(this.interviewsSubject.value);
    }

    return this.http
      .get<ApiResponse<Interview[]>>(`${this.apiUrl}`)
      .pipe(
        timeout(10000),
        map(response => this.normalizeArray<any>(this.unwrapData<any>(response)).map(interview => this.normalizeInterview(interview))),
        tap(interviews => {
          this.cacheInterviews(interviews);
          this.interviewsSubject.next(interviews);
        })
      );
  }

  /** Returns the last successful interview list so navigation failures do not reset the dashboard to zero. */
  getCachedInterviews(): Interview[] {
    try {
      return JSON.parse(sessionStorage.getItem(this.interviewCacheKey) || '[]') as Interview[];
    } catch {
      return [];
    }
  }

  /** Stores successful interview reads for dashboard/history continuity during route changes. */
  private cacheInterviews(interviews: Interview[]): void {
    sessionStorage.setItem(this.interviewCacheKey, JSON.stringify(interviews));
  }

  /** Starts the session and returns the questions to display. */
  beginInterview(interviewId: number): Observable<BeginInterviewResponse> {
    return this.http
      .post<ApiResponse<BeginInterviewResponse>>(`${this.apiUrl}/${interviewId}/begin`, {})
      .pipe(
        timeout(60000),
        map(response => this.normalizeSession(interviewId, this.unwrapData<any>(response)))
      );
  }

  /**
   * Loads an already-started interview if begin is not allowed anymore.
   * This helps if the user refreshes the session page after questions were generated.
   */
  getInterview(interviewId: number): Observable<BeginInterviewResponse> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${interviewId}`).pipe(
      timeout(8000),
      map(response => {
        const data = this.unwrapData<any>(response);
        return this.normalizeSession(interviewId, data);
      })
    );
  }

  /**
   * Begins a session through the Gemini-backed backend flow only.
   * The UI intentionally does not fall back to GET /interviews/{id}, because that endpoint returns saved DB questions.
   */
  loadSessionQuestions(interviewId: number): Observable<BeginInterviewResponse> {
    return this.beginInterview(interviewId);
  }

  /** Submits all saved transcriptions for scoring and persistence. */
  submitInterview(request: SubmitInterviewRequest): Observable<InterviewResult> {
    return this.http
      .post<ApiResponse<InterviewResult>>(`${this.apiUrl}/submit`, request)
      .pipe(
        timeout(120000), // 2 minutes — backend evaluates in batches of 4
        map(response => this.normalizeInterviewResult(this.unwrapData<any>(response))),
        tap(() => {
          this.interviewsSubject.next(null); // Invalidate cache so dashboard re-fetches
        })
      );
  }

  /**
   * Predictive pre-generation — warms up the question pool while user reads instructions.
   */
  warmUpCache(request: { domain: string; targetCount: number }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/warm-up`, request).pipe(
      map(response => this.unwrapData<any>(response))
    );
  }

  getInterviewResult(interviewId: number): Observable<InterviewResult> {
    return this.http
      .get<ApiResponse<InterviewResult>>(`${this.apiUrl}/${interviewId}/result`)
      .pipe(
        timeout(10000),
        map(response => this.normalizeInterviewResult(this.unwrapData<any>(response)))
      );
  }
}
