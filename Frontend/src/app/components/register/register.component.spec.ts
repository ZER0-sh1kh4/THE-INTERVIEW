import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi, type Mocked } from 'vitest';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../services/auth.service';

/**
 * Unit tests for RegisterComponent — form validation, password matching, and registration flow.
 */
describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authServiceSpy: Mocked<Pick<AuthService, 'register'>>;
  let navigateSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    authServiceSpy = {
      register: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigate');
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Initialization', () => {
    it('should have all form controls initialized', () => {
      expect(component.registerForm.contains('fullName')).toBe(true);
      expect(component.registerForm.contains('email')).toBe(true);
      expect(component.registerForm.contains('password')).toBe(true);
      expect(component.registerForm.contains('confirmPassword')).toBe(true);
      expect(component.registerForm.contains('terms')).toBe(true);
    });

    it('should mark form as invalid when empty', () => {
      expect(component.registerForm.valid).toBe(false);
    });
  });

  describe('Form Validation', () => {
    it('should require fullName', () => {
      const ctrl = component.registerForm.get('fullName');
      ctrl?.setValue('');
      expect(ctrl?.hasError('required')).toBe(true);
    });

    it('should require a valid email', () => {
      const ctrl = component.registerForm.get('email');
      ctrl?.setValue('bad');
      expect(ctrl?.hasError('email')).toBe(true);
    });

    it('should require password with min length of 6', () => {
      const ctrl = component.registerForm.get('password');
      ctrl?.setValue('123');
      expect(ctrl?.hasError('minlength')).toBe(true);
    });

    it('should require terms to be accepted', () => {
      const ctrl = component.registerForm.get('terms');
      ctrl?.setValue(false);
      expect(ctrl?.hasError('required')).toBe(true);
    });

    it('should detect password mismatch', () => {
      component.registerForm.patchValue({
        fullName: 'Test', email: 'a@b.com',
        password: 'password1', confirmPassword: 'password2', terms: true
      });
      expect(component.registerForm.hasError('mismatch')).toBe(true);
    });

    it('should be valid when all fields are correct', () => {
      component.registerForm.patchValue({
        fullName: 'Test User', email: 'test@test.com',
        password: 'password123', confirmPassword: 'password123', terms: true
      });
      expect(component.registerForm.valid).toBe(true);
    });
  });

  describe('Password Visibility', () => {
    it('should toggle showPassword', () => {
      expect(component.showPassword).toBe(false);
      component.togglePasswordVisibility();
      expect(component.showPassword).toBe(true);
    });

    it('should toggle showConfirmPassword', () => {
      expect(component.showConfirmPassword).toBe(false);
      component.toggleConfirmPasswordVisibility();
      expect(component.showConfirmPassword).toBe(true);
    });
  });

  describe('onSubmit()', () => {
    it('should not call register if form is invalid', () => {
      component.onSubmit();
      expect(authServiceSpy.register).not.toHaveBeenCalled();
    });

    it('should call AuthService.register and navigate on success', () => {
      authServiceSpy.register.mockReturnValue(of({ userId: 1, token: 'jwt' }));
      component.registerForm.patchValue({
        fullName: 'Test User', email: 'new@test.com',
        password: 'pass123', confirmPassword: 'pass123', terms: true
      });
      component.onSubmit();
      expect(authServiceSpy.register).toHaveBeenCalledWith({
        fullName: 'Test User', email: 'new@test.com', password: 'pass123'
      });
      expect(navigateSpy).toHaveBeenCalledWith(['/user-dashboard']);
    });

    it('should set error message on failure', () => {
      authServiceSpy.register.mockReturnValue(throwError(() => ({ status: 400 })));
      component.registerForm.patchValue({
        fullName: 'Test', email: 'dup@test.com',
        password: 'pass123', confirmPassword: 'pass123', terms: true
      });
      component.onSubmit();
      expect(component.errorMsg).toBe('Registration failed. Please try again.');
      expect(component.isLoading).toBe(false);
    });
  });
});
