import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, AuthResponse, User } from '../models/user.model';

/**
 * Purpose: Centralizes all authentication API calls and browser token storage.
 * Components use this service instead of calling backend endpoints directly.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  /** Base URL for the Identity endpoints routed through the API Gateway. */
  private apiUrl = `${environment.apiUrl}/auth`;
  
  /**
   * Keeps the currently logged-in user's claims available across the app.
   * undefined = still loading / not yet checked, null = definitively not logged in, User = logged in.
   * Guards are responsible for triggering getMe() when a token exists but user is undefined.
   */
  private currentUserSubject = new BehaviorSubject<User | null | undefined>(undefined);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  get currentUserValue(): User | null | undefined {
    return this.currentUserSubject.value;
  }

  /**
   * Sends login credentials to the backend and stores the returned JWT.
   * The backend wraps responses, so the token is read from response.data.
   */
  login(credentials: any): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, credentials)
      .pipe(
        map(response => response.data),
        tap(response => {
          if (response && response.token) {
            this.setToken(response.token);
            this.getMe().subscribe(); // Fetch user details after login
          }
        })
      );
  }

  /**
   * Registers a new user and stores the returned JWT for immediate dashboard access.
   */
  register(userData: any): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, userData)
      .pipe(
        map(response => response.data),
        tap(response => {
          if (response && response.token) {
            this.setToken(response.token);
            this.getMe().subscribe();
          }
        })
      );
  }

  /**
   * Fetches the authenticated user's claims from the backend.
   * The result is normalized so components can treat isPremium as a boolean.
   */
  getMe(): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/me`)
      .pipe(
        map(response => ({
          ...response.data,
          isPremium: response.data.isPremium === true || response.data.isPremium === 'true'
        })),
        tap(user => {
          this.currentUserSubject.next(user);
        })
      );
  }

  /** Removes the saved JWT and clears user state. */
  logout(): void {
    localStorage.removeItem('token');
    sessionStorage.clear();
    this.currentUserSubject.next(null);
  }

  /** Reads the JWT used by the auth interceptor. */
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  /** Persists a fresh JWT after login/register. */
  setToken(token: string): void {
    localStorage.setItem('token', token);
  }

  /** A lightweight route-guard helper based on token presence. */
  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  /** Checks the loaded user's role for future admin/candidate routing. */
  hasRole(role: string): boolean {
    return this.currentUserValue?.role === role;
  }

  /** Requests a six-digit reset OTP for the email address. */
  sendPasswordResetOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password/request-otp`, { email });
  }

  /** Submits email, OTP, and the new password so the backend can update PasswordHash. */
  resetPassword(data: { email: string; otp: string; newPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password/reset`, data);
  }

  /** Updates the user's display name. Returns a refreshed JWT and reloads claims. */
  updateProfile(payload: { fullName: string }): Observable<AuthResponse> {
    return this.http.put<ApiResponse<AuthResponse>>(`${this.apiUrl}/me`, payload).pipe(
      map(response => response.data),
      tap(response => {
        if (response?.token) {
          this.setToken(response.token);
          this.getMe().subscribe(); // Reload claims with updated name
        }
      })
    );
  }
}
