import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Guard to prevent unauthenticated users from accessing protected routes.
 * Uses "token-first" logic: triggers getMe() if token exists but user is undefined.
 * Does NOT redirect admins — that is adminGuard's responsibility.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  if (!token) {
    router.navigate(['/login']);
    return of(false);
  }

  // If we have a token but NO user yet, trigger a fetch first
  const user = authService.currentUserValue;
  const userRequest$ = (user === undefined)
    ? authService.getMe().pipe(catchError(() => of(null)))
    : of(user);

  return userRequest$.pipe(
    map(u => {
      if (!u) {
        router.navigate(['/login']);
        return false;
      }
      return true;
    })
  );
};
