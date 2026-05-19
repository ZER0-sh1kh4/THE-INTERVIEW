import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { vi, type Mocked } from 'vitest';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

/**
 * Unit tests for authInterceptor — JWT header injection and 401 handling.
 */
describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;
  let authServiceSpy: Mocked<Pick<AuthService, 'getToken' | 'logout'>>;

  beforeEach(() => {
    authServiceSpy = {
      getToken: vi.fn(),
      logout: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should add Authorization header when token exists', () => {
    authServiceSpy.getToken.mockReturnValue('my-jwt-token');

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-jwt-token');
    req.flush({});
  });

  it('should NOT add Authorization header when no token exists', () => {
    authServiceSpy.getToken.mockReturnValue(null);

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should call logout on 401 for non-me endpoints', () => {
    authServiceSpy.getToken.mockReturnValue('expired-token');

    httpClient.get('/api/interviews').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/api/interviews');
    req.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authServiceSpy.logout).toHaveBeenCalled();
  });

  it('should NOT call logout on 401 for /auth/me endpoint', () => {
    authServiceSpy.getToken.mockReturnValue('token');

    httpClient.get('/api/auth/me').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/api/auth/me');
    req.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authServiceSpy.logout).not.toHaveBeenCalled();
  });

  it('should pass through non-401 errors without logout', () => {
    authServiceSpy.getToken.mockReturnValue('token');

    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.status).toBe(500);
      }
    });

    const req = httpMock.expectOne('/api/test');
    req.flush({}, { status: 500, statusText: 'Internal Server Error' });

    expect(authServiceSpy.logout).not.toHaveBeenCalled();
  });
});
