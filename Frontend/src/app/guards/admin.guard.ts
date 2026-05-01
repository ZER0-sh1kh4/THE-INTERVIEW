import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Guard that restricts access to Admin-role users only.
 * Uses "token-first" logic: if a token exists but user data hasn't loaded yet,
 * the guard triggers getMe() itself instead of relying on the service constructor.
 */
export const adminGuard: CanActivateFn = () => {
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
      if (!u || u.role?.toLowerCase() !== 'admin') {
        // Redirect to dashboard if they are a normal user, or login if invalid
        const target = u ? '/user-dashboard' : '/login';
        router.navigate([target]);
        return false;
      }
      return true;
    })
  );
};
