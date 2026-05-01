import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/user.model';
import {
  AdminUser, AdminInterview, AdminAssessment,
  MCQQuestion, AdminSubscription, AdminPayment
} from '../models/admin.models';

/**
 * Centralizes all admin-only HTTP calls.
 * Every endpoint requires the JWT to contain role=Admin.
 */
@Injectable({ providedIn: 'root' })
export class AdminService {
  private authUrl = `${environment.apiUrl}/auth`;
  private interviewUrl = `${environment.apiUrl}/interviews`;
  private assessmentUrl = `${environment.apiUrl}/assessments`;
  private subscriptionUrl = `${environment.apiUrl}/subscriptions`;

  constructor(private http: HttpClient) {}

  // ─── Users ───
  getUsers(): Observable<AdminUser[]> {
    return this.http.get<ApiResponse<AdminUser[]>>(`${this.authUrl}/admin/users`)
      .pipe(map(r => r.data));
  }

  getUserById(id: number): Observable<AdminUser> {
    return this.http.get<ApiResponse<AdminUser>>(`${this.authUrl}/admin/users/${id}`)
      .pipe(map(r => r.data));
  }

  updateUserRole(id: number, role: string): Observable<any> {
    return this.http.put(`${this.authUrl}/admin/users/${id}/role`, JSON.stringify(role), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  updateUserPremium(id: number, isPremium: boolean): Observable<any> {
    return this.http.put(`${this.authUrl}/admin/users/${id}/premium`, JSON.stringify(isPremium), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deactivateUser(id: number): Observable<any> {
    return this.http.put(`${this.authUrl}/admin/users/${id}/deactivate`, {});
  }

  reactivateUser(id: number): Observable<any> {
    return this.http.put(`${this.authUrl}/admin/users/${id}/reactivate`, {});
  }

  // ─── Interviews ───
  getAllInterviews(): Observable<AdminInterview[]> {
    return this.http.get<ApiResponse<AdminInterview[]>>(`${this.interviewUrl}/admin/all`)
      .pipe(map(r => r.data));
  }

  // ─── Assessments ───
  getAllAssessments(): Observable<AdminAssessment[]> {
    return this.http.get<ApiResponse<AdminAssessment[]>>(`${this.assessmentUrl}/admin/all`)
      .pipe(map(r => r.data));
  }

  // ─── Questions ───
  getAllQuestions(): Observable<MCQQuestion[]> {
    return this.http.get<ApiResponse<MCQQuestion[]>>(`${this.assessmentUrl}/questions`)
      .pipe(map(r => r.data));
  }

  createQuestion(question: Partial<MCQQuestion>): Observable<MCQQuestion> {
    return this.http.post<ApiResponse<MCQQuestion>>(`${this.assessmentUrl}/questions`, question)
      .pipe(map(r => r.data));
  }

  updateQuestion(id: number, question: Partial<MCQQuestion>): Observable<MCQQuestion> {
    return this.http.put<ApiResponse<MCQQuestion>>(`${this.assessmentUrl}/questions/${id}`, question)
      .pipe(map(r => r.data));
  }

  deleteQuestion(id: number): Observable<any> {
    return this.http.delete(`${this.assessmentUrl}/questions/${id}`);
  }

  // ─── Subscriptions ───
  getAllSubscriptions(): Observable<AdminSubscription[]> {
    return this.http.get<ApiResponse<AdminSubscription[]>>(`${this.subscriptionUrl}/all`)
      .pipe(map(r => r.data));
  }

  getAllPayments(): Observable<AdminPayment[]> {
    return this.http.get<ApiResponse<AdminPayment[]>>(`${this.subscriptionUrl}/payments`)
      .pipe(map(r => r.data));
  }
}
