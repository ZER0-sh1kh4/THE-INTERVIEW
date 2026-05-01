import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

/**
 * Purpose: Runs the forgot/reset password flow.
 * The backend verifies the OTP only when the final reset request is submitted, then updates the stored password hash.
 */
@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {
  /** Step 1 collects email + OTP, step 2 collects the new password. */
  step: 1 | 2 = 1;
  emailForm: FormGroup;
  otpForm: FormGroup;
  resetForm: FormGroup;
  isLoadingEmail = false;
  isLoadingReset = false;
  errorMsg = '';
  successMsg = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.otpForm = this.fb.group({
      digit1: ['', Validators.required],
      digit2: ['', Validators.required],
      digit3: ['', Validators.required],
      digit4: ['', Validators.required],
      digit5: ['', Validators.required],
      digit6: ['', Validators.required]
    });

    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  /** Keeps users from submitting different password and confirm-password values. */
  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  /** Requests the backend to generate and email a password-reset OTP. */
  onEmailSubmit() {
    if (this.emailForm.invalid) return;

    this.isLoadingEmail = true;
    this.errorMsg = '';
    this.successMsg = '';

    const email = this.emailForm.value.email;

    this.authService.sendPasswordResetOtp(email).subscribe({
      next: () => {
        this.isLoadingEmail = false;
        this.successMsg = 'OTP sent to your email! Please enter it below.';
      },
      error: (err) => {
        this.isLoadingEmail = false;
        this.errorMsg = 'Failed to send OTP. Please check the email address.';
      }
    });
  }

  /**
   * Moves to the new-password step after the user has entered six digits.
   * The backend validates the OTP during onResetSubmit so the password update happens in one secure call.
   */
  onOtpSubmit() {
    if (this.otpForm.invalid || this.emailForm.invalid) {
      this.errorMsg = 'Please enter both your Email and the 6-digit OTP.';
      return;
    }
    
    // Move to step 2 (Set New Password)
    this.step = 2;
    this.errorMsg = '';
    this.successMsg = 'OTP collected. Please set your new password.';
  }

  /** Auto-focuses the next OTP box to make six-digit entry easier. */
  onOtpInput(event: any, index: number) {
    const input = event.target;
    if (input.value && index < 6) {
      const nextInput = document.getElementById(`otp-${index + 1}`);
      if (nextInput) {
        nextInput.focus();
      }
    }
  }

  /** Moves focus backward when the user deletes an OTP digit. */
  onOtpBackspace(event: any, index: number) {
    if (event.key === 'Backspace' && !event.target.value && index > 1) {
      const prevInput = document.getElementById(`otp-${index - 1}`);
      if (prevInput) {
        prevInput.focus();
      }
    }
  }

  /** Sends email, OTP, and newPassword to the backend, then returns the user to login. */
  onResetSubmit() {
    if (this.resetForm.invalid || this.emailForm.invalid || this.otpForm.invalid) {
      this.resetForm.markAllAsTouched();
      this.errorMsg = 'Password must be at least 6 characters and both password fields must match.';
      this.successMsg = '';
      return;
    }

    this.isLoadingReset = true;
    this.errorMsg = '';

    const otpValues = this.otpForm.value;
    const otp = `${otpValues.digit1}${otpValues.digit2}${otpValues.digit3}${otpValues.digit4}${otpValues.digit5}${otpValues.digit6}`;

    const data = {
      email: this.emailForm.value.email,
      otp: otp,
      newPassword: this.resetForm.value.newPassword
    };

    this.authService.resetPassword(data).subscribe({
      next: () => {
        this.isLoadingReset = false;
        this.successMsg = 'Password successfully reset. Redirecting to login...';
        setTimeout(() => this.router.navigate(['/login']), 1200);
      },
      error: (err) => {
        this.isLoadingReset = false;
        this.errorMsg = 'Failed to reset password. OTP might be invalid or expired.';
      }
    });
  }
}
