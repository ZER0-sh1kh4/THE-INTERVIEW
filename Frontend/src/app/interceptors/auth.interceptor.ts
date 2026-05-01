import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Purpose: Adds JWT auth headers to API calls and centralizes basic HTTP error handling.
 * Protected feature services can stay clean because this interceptor handles shared request behavior.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError(error => {
      // Don't force-logout on the /me self-check — AuthService handles that gracefully.
      // Only force-logout on 401s from actual feature API calls.
      if (error.status === 401 && !req.url.includes('/auth/me')) {
        authService.logout();
      }

      return throwError(() => error);
    })
  );
};
