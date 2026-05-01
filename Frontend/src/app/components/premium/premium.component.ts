import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SubscriptionService, Subscription, PaymentRecord } from '../../services/subscription.service';
import { UserNavbarComponent } from '../shared/user-navbar/user-navbar.component';
import { catchError, of } from 'rxjs';

/**
 * Purpose: Full subscription management page — pricing plans, current subscription
 * status, Stripe checkout flow, payment history, and cancel subscription.
 * Adapted from Stitch UI reference to match the existing app editorial theme.
 */
@Component({
  selector: 'app-premium',
  standalone: true,
  imports: [CommonModule, RouterModule, UserNavbarComponent],
  templateUrl: './premium.component.html',
  styleUrl: './premium.component.css'
})
export class PremiumComponent implements OnInit {
  isPremiumUser = false;
  highlightReason = '';

  /** Current subscription state */
  currentSubscription: Subscription | null = null;
  subscriptionLoading = true;

  /** Payment history */
  payments: PaymentRecord[] = [];
  paymentsLoading = true;

  /** Checkout flow state */
  isUpgrading = false;
  upgradeError = '';

  /** Cancel flow state */
  showCancelDialog = false;
  isCancelling = false;
  cancelError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private subscriptionService: SubscriptionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.isPremiumUser = user?.isPremium === true;
      this.cdr.detectChanges();
    });

    this.route.queryParamMap.subscribe(params => {
      const reason = params.get('reason');
      if (reason === 'interview-limit') {
        this.highlightReason = 'Your free interview has already been used. Premium unlocks unlimited interview sessions.';
      } else if (reason === 'assessment_limit') {
        this.highlightReason = 'You\'ve completed your 2 free assessments! Upgrade to Premium for unlimited assessments with detailed analytics.';
      } else {
        this.highlightReason = '';
      }
    });

    this.loadSubscription();
    this.loadPayments();
  }

  /** Loads the user's active subscription from the backend. */
  loadSubscription(): void {
    this.subscriptionLoading = true;
    this.subscriptionService.getMySubscriptions().pipe(
      catchError(() => of([] as Subscription[]))
    ).subscribe(subs => {
      const active = subs.find(s => s.status === 'Active' && s.plan === 'Premium');
      this.currentSubscription = active || null;
      this.subscriptionLoading = false;
      this.cdr.detectChanges();
    });
  }

  /** Loads the user's payment history from the backend. */
  loadPayments(): void {
    this.paymentsLoading = true;
    this.subscriptionService.getMyPayments().pipe(
      catchError(() => of([] as PaymentRecord[]))
    ).subscribe(payments => {
      this.payments = payments.sort((a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );
      this.paymentsLoading = false;
      this.cdr.detectChanges();
    });
  }

  /**
   * Initiates Stripe Checkout: calls POST /api/subscriptions/subscribe,
   * receives a sessionUrl, and redirects the user to Stripe.
   */
  upgradeToPremium(): void {
    this.isUpgrading = true;
    this.upgradeError = '';
    this.cdr.detectChanges();

    this.subscriptionService.subscribe().subscribe({
      next: (response) => {
        if (response.checkoutUrl) {
          // Stripe mode: redirect to Stripe Checkout
          window.location.href = response.checkoutUrl;
        } else if (response.mode === 'Simulated') {
          // Demo mode: auto-confirm and refresh claims
          this.subscriptionService.confirmPayment(
            response.checkoutSessionId,
            response.checkoutSessionId,
            'simulated'
          ).subscribe({
            next: () => {
              this.subscriptionService.refreshClaims().subscribe({
                next: () => {
                  this.isUpgrading = false;
                  this.router.navigate(['/subscription/success'], {
                    queryParams: { session_id: response.checkoutSessionId }
                  });
                },
                error: () => {
                  this.isUpgrading = false;
                  this.router.navigate(['/subscription/success'], {
                    queryParams: { session_id: response.checkoutSessionId }
                  });
                }
              });
            },
            error: (err) => {
              this.upgradeError = err?.error?.message || 'Payment confirmation failed.';
              this.isUpgrading = false;
              this.cdr.detectChanges();
            }
          });
        } else {
          this.upgradeError = 'Unable to create checkout session. Please try again.';
          this.isUpgrading = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.upgradeError = err?.error?.message || 'Failed to start checkout. Please try again.';
        this.isUpgrading = false;
        this.cdr.detectChanges();
      }
    });
  }

  /** Opens the cancel confirmation dialog. */
  promptCancel(): void {
    this.showCancelDialog = true;
    this.cancelError = '';
  }

  /** Closes the cancel confirmation dialog without action. */
  dismissCancel(): void {
    this.showCancelDialog = false;
  }

  /** Executes the subscription cancellation. */
  confirmCancel(): void {
    this.isCancelling = true;
    this.cancelError = '';
    this.cdr.detectChanges();

    this.subscriptionService.cancelSubscription().subscribe({
      next: () => {
        this.showCancelDialog = false;
        this.isCancelling = false;
        this.currentSubscription = null;
        this.isPremiumUser = false;

        // Refresh claims so the JWT reflects the downgrade
        this.subscriptionService.refreshClaims().pipe(
          catchError(() => of(null))
        ).subscribe(() => {
          this.loadSubscription();
          this.loadPayments();
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.cancelError = err?.error?.message || 'Failed to cancel subscription.';
        this.isCancelling = false;
        this.cdr.detectChanges();
      }
    });
  }

  /** Formats a date string for display. */
  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  /** Returns a CSS class name for the payment status badge. */
  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'success': return 'status-success';
      case 'pending': return 'status-pending';
      case 'failed': return 'status-failed';
      default: return '';
    }
  }

  goBack(): void {
    this.router.navigate(['/user-dashboard']);
  }

  goToInterviews(): void {
    this.router.navigate(['/interviews/start']);
  }
}
