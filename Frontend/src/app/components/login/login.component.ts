import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

/**
 * Purpose: Handles the login screen and routes successful users into the app dashboard.
 * The form stays small and beginner-friendly: email, password, loading state, and error text.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  /** Reactive form with validation rules for the login payload expected by the backend. */
  loginForm: FormGroup;
  isLoading = false;
  errorMsg = '';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  /** Lets the user switch the password input between hidden and visible text. */
  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  /** Submits credentials, stores the JWT through AuthService, and opens the appropriate dashboard. */
  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMsg = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        // Wait for getMe() to populate user claims, then route based on role
        this.authService.currentUser$.subscribe(user => {
          if (user) {
            if (user.role === 'Admin') {
              this.router.navigate(['/admin']);
            } else {
              this.router.navigate(['/user-dashboard']);
            }
          }
        });
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMsg = 'Invalid credentials. Please try again.';
      }
    });
  }
}
