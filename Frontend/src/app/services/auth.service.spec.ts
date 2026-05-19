import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

function createStorageMock(): Storage {
  let store: Record<string, string> = {};

  return {
    get length() {
      return Object.keys(store).length;
    },
    clear: () => {
      store = {};
    },
    getItem: (key: string) => store[key] ?? null,
    key: (index: number) => Object.keys(store)[index] ?? null,
    removeItem: (key: string) => {
      delete store[key];
    },
    setItem: (key: string, value: string) => {
      store[key] = String(value);
    }
  };
}

/**
 * Unit tests for AuthService — the central authentication hub.
 * Covers token management, login/register flows, logout, and profile update.
 */
describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/auth`;

  beforeAll(() => {
    Object.defineProperty(window, 'localStorage', {
      value: createStorageMock(),
      configurable: true
    });
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ── Token Management ─────────────────────────────────────────────
  describe('Token Management', () => {
    it('should store a token in localStorage', () => {
      service.setToken('test-jwt-token');
      expect(localStorage.getItem('token')).toBe('test-jwt-token');
    });

    it('should retrieve a stored token', () => {
      localStorage.setItem('token', 'stored-token');
      expect(service.getToken()).toBe('stored-token');
    });

    it('should return null when no token exists', () => {
      expect(service.getToken()).toBeNull();
    });
  });

  // ── isLoggedIn ────────────────────────────────────────────────────
  describe('isLoggedIn()', () => {
    it('should return true when a token is present', () => {
      localStorage.setItem('token', 'any-token');
      expect(service.isLoggedIn()).toBe(true);
    });

    it('should return false when no token is present', () => {
      expect(service.isLoggedIn()).toBe(false);
    });
  });

  // ── hasRole ───────────────────────────────────────────────────────
  describe('hasRole()', () => {
    it('should return false when no user is loaded', () => {
      expect(service.hasRole('Admin')).toBe(false);
    });
  });

  // ── Login ─────────────────────────────────────────────────────────
  describe('login()', () => {
    it('should POST credentials and store the returned JWT', () => {
      const mockResponse = {
        success: true,
        message: 'Login successful',
        data: { userId: 1, token: 'jwt-from-server' }
      };

      service.login({ email: 'test@test.com', password: '123456' }).subscribe(response => {
        expect(response.token).toBe('jwt-from-server');
        expect(localStorage.getItem('token')).toBe('jwt-from-server');
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'test@test.com', password: '123456' });
      req.flush(mockResponse);

      const meReq = httpMock.expectOne(`${apiUrl}/me`);
      meReq.flush({ success: true, message: '', data: { userId: '1', email: 'test@test.com', role: 'Candidate', isPremium: false } });
    });

    it('should handle login failure gracefully', () => {
      service.login({ email: 'wrong@test.com', password: 'bad' }).subscribe({
        error: (err) => {
          expect(err.status).toBe(401);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      req.flush({ message: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  // ── Register ──────────────────────────────────────────────────────
  describe('register()', () => {
    it('should POST user data and store the returned JWT', () => {
      const mockResponse = {
        success: true,
        message: 'Registered',
        data: { userId: 2, token: 'new-user-jwt' }
      };

      service.register({ fullName: 'Test User', email: 'new@test.com', password: '123456' }).subscribe(response => {
        expect(response.token).toBe('new-user-jwt');
        expect(localStorage.getItem('token')).toBe('new-user-jwt');
      });

      const req = httpMock.expectOne(`${apiUrl}/register`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);

      const meReq = httpMock.expectOne(`${apiUrl}/me`);
      meReq.flush({ success: true, message: '', data: { userId: '2', email: 'new@test.com', role: 'Candidate', isPremium: false } });
    });
  });

  // ── getMe ─────────────────────────────────────────────────────────
  describe('getMe()', () => {
    it('should fetch user profile and normalize isPremium to boolean', () => {
      service.getMe().subscribe(user => {
        expect(user.email).toBe('user@test.com');
        expect(user.isPremium).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/me`);
      expect(req.request.method).toBe('GET');
      req.flush({
        success: true, message: '',
        data: { userId: '1', email: 'user@test.com', role: 'Candidate', isPremium: 'true' }
      });
    });

    it('should update the currentUser$ observable', () => {
      service.getMe().subscribe();

      const req = httpMock.expectOne(`${apiUrl}/me`);
      req.flush({
        success: true, message: '',
        data: { userId: '1', email: 'loaded@test.com', role: 'Candidate', isPremium: false }
      });

      expect(service.currentUserValue).toBeTruthy();
      expect(service.currentUserValue?.email).toBe('loaded@test.com');
    });
  });

  // ── Logout ────────────────────────────────────────────────────────
  describe('logout()', () => {
    it('should clear the token from localStorage', () => {
      localStorage.setItem('token', 'will-be-removed');
      service.logout();
      expect(localStorage.getItem('token')).toBeNull();
    });

    it('should clear sessionStorage', () => {
      sessionStorage.setItem('someKey', 'someValue');
      service.logout();
      expect(sessionStorage.getItem('someKey')).toBeNull();
    });

    it('should set currentUser to null', () => {
      service.logout();
      expect(service.currentUserValue).toBeNull();
    });
  });

  // ── Password Reset ────────────────────────────────────────────────
  describe('Password Reset', () => {
    it('should send OTP request for the given email', () => {
      service.sendPasswordResetOtp('reset@test.com').subscribe();

      const req = httpMock.expectOne(`${apiUrl}/forgot-password/request-otp`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'reset@test.com' });
      req.flush({ success: true });
    });

    it('should send password reset with email, OTP, and new password', () => {
      const payload = { email: 'reset@test.com', otp: '123456', newPassword: 'newPass123' };
      service.resetPassword(payload).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/forgot-password/reset`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(payload);
      req.flush({ success: true });
    });
  });

  // ── Update Profile ────────────────────────────────────────────────
  describe('updateProfile()', () => {
    it('should PUT the new name and store the refreshed JWT', () => {
      service.updateProfile({ fullName: 'Updated Name' }).subscribe(response => {
        expect(response.token).toBe('refreshed-jwt');
      });

      const req = httpMock.expectOne(`${apiUrl}/me`);
      expect(req.request.method).toBe('PUT');
      req.flush({ success: true, message: '', data: { userId: 1, token: 'refreshed-jwt' } });

      const meReq = httpMock.expectOne(`${apiUrl}/me`);
      meReq.flush({ success: true, message: '', data: { userId: '1', email: 'user@test.com', role: 'Candidate', isPremium: false, fullName: 'Updated Name' } });
    });
  });
});
