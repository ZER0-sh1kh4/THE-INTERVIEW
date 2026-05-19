import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mocked } from 'vitest';
import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';

/**
 * Unit tests for LoginComponent — form validation, submission, and routing.
 */
describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: Mocked<Pick<AuthService, 'login'>> & Pick<AuthService, 'currentUser$'>;
  let navigateSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    authServiceSpy = {
      currentUser$: of({ userId: '1', email: 'test@test.com', role: 'Candidate', isPremium: false }),
      login: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigate');
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ── Form Initialization ───────────────────────────────────────────
  describe('Form Initialization', () => {
    it('should initialize with empty email and password', () => {
      expect(component.loginForm.get('email')?.value).toBe('');
      expect(component.loginForm.get('password')?.value).toBe('');
    });

    it('should mark form as invalid when empty', () => {
      expect(component.loginForm.valid).toBe(false);
    });

    it('should have isLoading set to false initially', () => {
      expect(component.isLoading).toBe(false);
    });

    it('should have empty error message initially', () => {
      expect(component.errorMsg).toBe('');
    });
  });

  // ── Form Validation ───────────────────────────────────────────────
  describe('Form Validation', () => {
    it('should be invalid with empty email', () => {
      component.loginForm.patchValue({ email: '', password: 'test123' });
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be invalid with invalid email format', () => {
      component.loginForm.patchValue({ email: 'not-an-email', password: 'test123' });
      expect(component.loginForm.get('email')?.hasError('email')).toBe(true);
    });

    it('should be invalid with empty password', () => {
      component.loginForm.patchValue({ email: 'test@test.com', password: '' });
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be valid with proper email and password', () => {
      component.loginForm.patchValue({ email: 'test@test.com', password: 'password123' });
      expect(component.loginForm.valid).toBe(true);
    });
  });

  // ── Password Visibility ───────────────────────────────────────────
  describe('togglePasswordVisibility()', () => {
    it('should toggle showPassword from false to true', () => {
      expect(component.showPassword).toBe(false);
      component.togglePasswordVisibility();
      expect(component.showPassword).toBe(true);
    });

    it('should toggle back to false on second call', () => {
      component.togglePasswordVisibility();
      component.togglePasswordVisibility();
      expect(component.showPassword).toBe(false);
    });
  });

  // ── Form Submission ───────────────────────────────────────────────
  describe('onSubmit()', () => {
    it('should not call login if form is invalid', () => {
      component.onSubmit();
      expect(authServiceSpy.login).not.toHaveBeenCalled();
    });

    it('should call AuthService.login with form values on valid submit', () => {
      authServiceSpy.login.mockReturnValue(of({ userId: 1, token: 'jwt' }));
      component.loginForm.patchValue({ email: 'test@test.com', password: '123456' });
      component.onSubmit();
      expect(authServiceSpy.login).toHaveBeenCalledWith({ email: 'test@test.com', password: '123456' });
    });

    it('should set error message on login failure', () => {
      authServiceSpy.login.mockReturnValue(throwError(() => ({ status: 401 })));
      component.loginForm.patchValue({ email: 'test@test.com', password: 'wrong' });
      component.onSubmit();
      expect(component.errorMsg).toBe('Invalid credentials. Please try again.');
      expect(component.isLoading).toBe(false);
    });
  });
});
