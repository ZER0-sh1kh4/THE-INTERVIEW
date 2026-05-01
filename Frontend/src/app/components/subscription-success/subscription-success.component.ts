import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SubscriptionService } from '../../services/subscription.service';
import { catchError, of } from 'rxjs';

/**
 * Purpose: Handles the Stripe Checkout return flow after a successful payment.
 * Confirms the payment with the backend, refreshes JWT claims to set isPremium=true,
 * and shows a premium activation confirmation screen.
 */
@Component({
  selector: 'app-subscription-success',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './subscription-success.component.html',
  styleUrl: './subscription-success.component.css'
})
export class SubscriptionSuccessComponent implements OnInit {
  isProcessing = true;
  isSuccess = false;
  errorMsg = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private subscriptionService: SubscriptionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const sessionId = params.get('session_id') || '';

      if (!sessionId) {
        this.errorMsg = 'Missing payment session. Please try the upgrade again.';
        this.isProcessing = false;
        this.cdr.detectChanges();
        return;
      }

      this.confirmAndRefresh(sessionId);
    });
  }

  /**
   * Step 3 & 4: Confirms the payment, then refreshes claims to upgrade the JWT.
   */
  private confirmAndRefresh(sessionId: string): void {
    // Confirm the payment with the backend
    this.subscriptionService.confirmPayment(sessionId, sessionId, 'stripe').pipe(
      catchError(err => {
        // Payment may have already been confirmed via webhook — proceed anyway
        console.warn('Confirm returned error, might be already processed:', err);
        return of(null);
      })
    ).subscribe(() => {
      // Step 4: Refresh JWT claims so isPremium=true
      // Poll up to 5 times (waiting for the webhook to process)
      this.pollForPremiumStatus(0);
    });
  }

  private pollForPremiumStatus(attempt: number): void {
    if (attempt >= 5) {
      this.errorMsg = 'Payment received, but premium activation is delayed. Please refresh your profile in a few minutes.';
      this.isProcessing = false;
      this.cdr.detectChanges();
      return;
    }

    setTimeout(() => {
      this.subscriptionService.refreshClaims().subscribe({
        next: (user) => {
          if (user && user.isPremium) {
            this.isProcessing = false;
            this.isSuccess = true;
            this.cdr.detectChanges();
          } else {
            this.pollForPremiumStatus(attempt + 1);
          }
        },
        error: () => this.pollForPremiumStatus(attempt + 1)
      });
    }, 2000); // Check every 2 seconds
  }

  goToDashboard(): void {
    this.router.navigate(['/user-dashboard']);
  }

  startInterview(): void {
    this.router.navigate(['/interviews/start']);
  }

  takeAssessment(): void {
    this.router.navigate(['/assessments/domain']);
  }

  goToPlans(): void {
    this.router.navigate(['/premium']);
  }
}
