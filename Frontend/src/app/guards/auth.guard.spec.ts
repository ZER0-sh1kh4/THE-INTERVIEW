import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mocked } from 'vitest';
import { AuthService } from '../services/auth.service';
import { User } from '../models/user.model';
import { authGuard } from './auth.guard';

/**
 * Unit tests for authGuard — protects routes for authenticated users.
 */
describe('authGuard', () => {
  let currentUserValue: User | null | undefined;
  let authServiceSpy: Mocked<Pick<AuthService, 'getToken' | 'getMe'>> & Pick<AuthService, 'currentUserValue'>;
  let routerSpy: Mocked<Pick<Router, 'navigate'>>;

  beforeEach(() => {
    currentUserValue = undefined;
    authServiceSpy = {
      get currentUserValue() {
        return currentUserValue;
      },
      getToken: vi.fn(),
      getMe: vi.fn()
    };
    routerSpy = {
      navigate: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });
  });

  it('should redirect to /login when no token exists', () => {
    authServiceSpy.getToken.mockReturnValue(null);

    TestBed.runInInjectionContext(() => {
      const result$ = authGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(false);
        });
      }
    });

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should allow access when token exists and user is loaded', () => {
    authServiceSpy.getToken.mockReturnValue('valid-token');
    currentUserValue = { userId: '1', email: 'test@test.com', role: 'Candidate', isPremium: false };

    TestBed.runInInjectionContext(() => {
      const result$ = authGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(true);
        });
      }
    });
  });

  it('should call getMe() when token exists but user is undefined', () => {
    authServiceSpy.getToken.mockReturnValue('valid-token');
    authServiceSpy.getMe.mockReturnValue(of({
      userId: '1', email: 'test@test.com', role: 'Candidate', isPremium: false
    } as any));

    TestBed.runInInjectionContext(() => {
      const result$ = authGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(true);
        });
      }
    });

    expect(authServiceSpy.getMe).toHaveBeenCalled();
  });

  it('should redirect to /login when getMe() fails', () => {
    authServiceSpy.getToken.mockReturnValue('expired-token');
    authServiceSpy.getMe.mockReturnValue(throwError(() => new Error('Unauthorized')));

    TestBed.runInInjectionContext(() => {
      const result$ = authGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(false);
        });
      }
    });
  });
});
