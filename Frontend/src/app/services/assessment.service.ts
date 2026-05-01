import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, tap, BehaviorSubject, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { AssessmentResult, QuestionDto, StartAssessmentRequest, StartAssessmentResponse, SubmitAssessmentRequest } from '../models/assessment.model';
import { ApiResponse } from '../models/user.model';

/**
 * Purpose: Centralizes dashboard reads for assessment activity.
 * Keeping this in a service prevents the dashboard component from knowing API casing details.
 */
@Injectable({
  providedIn: 'root'
})
export class AssessmentService {
  /** Base URL for AssessmentService endpoints through the API Gateway. */
  private apiUrl = `${environment.apiUrl}/assessments`;
  private readonly assessmentCacheKey = 'the_interview_my_assessments';
  private assessmentsSubject = new BehaviorSubject<AssessmentResult[] | null>(null);

  constructor(private http: HttpClient) {}

  /** Returns assessments from session storage if available. */
  getCachedAssessments(): AssessmentResult[] {
    try {
      return JSON.parse(sessionStorage.getItem(this.assessmentCacheKey) || '[]') as AssessmentResult[];
    } catch {
      return [];
    }
  }

  /** Stores assessments in session storage for continuity. */
  private cacheAssessments(results: AssessmentResult[]): void {
    sessionStorage.setItem(this.assessmentCacheKey, JSON.stringify(results));
  }

  /** Reads both camelCase and PascalCase API wrappers. */
  private unwrapData<T>(response: any): T {
    return (response?.data ?? response?.Data ?? response) as T;
  }

  /** Converts .NET collection wrappers and plain arrays into a normal array. */
  private normalizeArray<T>(value: any): T[] {
    if (Array.isArray(value)) {
      return value;
    }

    if (Array.isArray(value?.$values)) {
      return value.$values;
    }

    return [];
  }

  /** Normalizes one assessment result row for the dashboard table. */
  private normalizeAssessmentResult(rawResult: any): AssessmentResult {
    const result = rawResult || {};

    return {
      assessmentId: result.assessmentId ?? result.AssessmentId ?? result.id ?? result.Id ?? 0,
      domain: result.domain ?? result.Domain ?? 'Assessment',
      score: result.score ?? result.Score ?? 0,
      maxScore: result.maxScore ?? result.MaxScore ?? 0,
      percentage: result.percentage ?? result.Percentage ?? 0,
      grade: result.grade ?? result.Grade ?? '-'
    };
  }

  /** Returns completed assessment summaries for the authenticated user. */
  getMyAssessments(forceRefresh = false): Observable<AssessmentResult[]> {
    if (!forceRefresh && this.assessmentsSubject.value !== null) {
      return of(this.assessmentsSubject.value);
    }

    return this.http.get<ApiResponse<AssessmentResult[]>>(`${this.apiUrl}`).pipe(
      map(response => this.normalizeArray<any>(this.unwrapData<any>(response)).map(result => this.normalizeAssessmentResult(result))),
      tap(results => {
        this.cacheAssessments(results);
        this.assessmentsSubject.next(results);
      })
    );
  }

  /** Starts a new assessment. */
  startAssessment(request: StartAssessmentRequest): Observable<StartAssessmentResponse> {
    return this.http.post<ApiResponse<StartAssessmentResponse>>(`${this.apiUrl}/start`, request).pipe(
      map(response => this.unwrapData<StartAssessmentResponse>(response))
    );
  }

  /** Submits answers. */
  submitAssessment(request: SubmitAssessmentRequest): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/submit`, request).pipe(
      map(response => this.unwrapData<any>(response)),
      tap(() => this.assessmentsSubject.next(null)) // Invalidate cache so dashboard re-fetches
    );
  }

  /** Fetches a single assessment result. */
  getAssessmentResult(id: number): Observable<any> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/result`).pipe(
      map(response => this.unwrapData<any>(response))
    );
  }

  /** Fetches next batch of questions for lazy loading (batch of 3). */
  getNextBatch(assessmentId: number, currentCount: number, batchSize: number = 3): Observable<QuestionDto[]> {
    return this.http.get<ApiResponse<QuestionDto[]>>(
      `${this.apiUrl}/${assessmentId}/next-batch?currentCount=${currentCount}&batchSize=${batchSize}`
    ).pipe(
      map(response => this.normalizeArray<QuestionDto>(this.unwrapData<any>(response)))
    );
  }

  /** Predictive pre-generation — warms up the question cache while user reads instructions. */
  warmUpCache(request: { domain: string; difficulty: string; targetCount: number }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/warm-up`, request).pipe(
      map(response => this.unwrapData<any>(response))
    );
  }
}
