/**
 * Purpose: Contains the small TypeScript models used by the authentication flow.
 * These interfaces mirror the backend contracts so services and components can stay type-safe.
 */

/**
 * Shape returned by the backend's standard ApiResponse<T> wrapper.
 * Every success response places the useful payload inside the data property.
 */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

/**
 * Auth payload returned after login/register.
 * The JWT token is stored by AuthService and reused by the HTTP interceptor.
 */
export interface AuthResponse {
  userId: number;
  token: string;
  message?: string;
}

/**
 * Current user profile returned by /api/auth/me.
 * The backend currently returns claim values, so isPremium can arrive as text.
 */
export interface User {
  userId: string;
  email: string;
  role: string;
  isPremium: boolean | string;
  fullName?: string;
}
