import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mocked } from 'vitest';
import { AuthService } from '../services/auth.service';
import { User } from '../models/user.model';
import { adminGuard } from './admin.guard';

/**
 * Unit tests for adminGuard — restricts access to Admin-role users only.
 */
describe('adminGuard', () => {
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
      const result$ = adminGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(false);
        });
      }
    });

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should allow access for Admin users', () => {
    authServiceSpy.getToken.mockReturnValue('admin-token');
    currentUserValue = { userId: '1', email: 'admin@test.com', role: 'Admin', isPremium: false };

    TestBed.runInInjectionContext(() => {
      const result$ = adminGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(true);
        });
      }
    });
  });

  it('should redirect Candidate users to /user-dashboard', () => {
    authServiceSpy.getToken.mockReturnValue('user-token');
    currentUserValue = { userId: '2', email: 'user@test.com', role: 'Candidate', isPremium: false };

    TestBed.runInInjectionContext(() => {
      const result$ = adminGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(false);
        });
      }
    });

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/user-dashboard']);
  });

  it('should call getMe() when token exists but user is undefined', () => {
    authServiceSpy.getToken.mockReturnValue('token');
    authServiceSpy.getMe.mockReturnValue(of({
      userId: '1', email: 'admin@test.com', role: 'Admin', isPremium: false
    } as any));

    TestBed.runInInjectionContext(() => {
      const result$ = adminGuard({} as any, {} as any);
      if (result$ && typeof (result$ as any).subscribe === 'function') {
        (result$ as any).subscribe((val: boolean) => {
          expect(val).toBe(true);
        });
      }
    });

    expect(authServiceSpy.getMe).toHaveBeenCalled();
  });
});
