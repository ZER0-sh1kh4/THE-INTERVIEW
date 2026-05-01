import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/user.model';
import { AuthService } from './auth.service';

/**
 * Purpose: Handles all subscription, payment, and premium lifecycle operations.
 * Communicates with the SubscriptionService backend via the API Gateway.
 */

export interface Subscription {
  id: number;
  userId: number;
  plan: string;
  price: number;
  status: string;
  sagaState: string;
  startDate: string;
  endDate: string;
  createdAt: string;
}

export interface PaymentRecord {
  id: number;
  subscriptionId: number;
  userId: number;
  amount: number;
  currency: string;
  stripeSessionId: string;
  stripePaymentIntentId: string;
  status: string;
  createdAt: string;
}

export interface SubscribeResponse {
  checkoutSessionId: string;
  checkoutUrl?: string;
  publishableKey?: string;
  amount: number;
  currency: string;
  mode: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private apiUrl = `${environment.apiUrl}/subscriptions`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  /**
   * Creates a Stripe Checkout session for the premium plan.
   * In Stripe mode, returns a checkoutUrl to redirect the user.
   * In simulated mode, returns a checkoutSessionId for manual confirm.
   */
  subscribe(): Observable<SubscribeResponse> {
    return this.http.post<ApiResponse<SubscribeResponse>>(`${this.apiUrl}/subscribe`, {})
      .pipe(map(res => res.data));
  }

  /**
   * Confirms a successful payment after the user returns from Stripe Checkout.
   */
  confirmPayment(sessionId: string, paymentIntentId: string, signature: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/confirm`, {
      paymentSessionId: sessionId,
      paymentReferenceId: paymentIntentId,
      signature: signature
    }).pipe(map(res => res.data));
  }

  /**
   * Fetches the current user's subscription list.
   */
  getMySubscriptions(): Observable<Subscription[]> {
    return this.http.get<ApiResponse<Subscription[]>>(`${this.apiUrl}/my`)
      .pipe(map(res => res.data));
  }

  /**
   * Fetches the current user's payment history.
   */
  getMyPayments(): Observable<PaymentRecord[]> {
    return this.http.get<ApiResponse<PaymentRecord[]>>(`${this.apiUrl}/my/payments`)
      .pipe(map(res => res.data));
  }

  /**
   * Cancels the active subscription.
   */
  cancelSubscription(): Observable<string> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/cancel`, {})
      .pipe(map(res => res.message));
  }

  /**
   * Refreshes the JWT after a premium upgrade so isPremium claims are current.
   * Stores the new token and refreshes the user observable.
   */
  refreshClaims(): Observable<any> {
    return this.http.post<ApiResponse<{ token: string }>>(`${environment.apiUrl}/auth/refresh-claims`, {})
      .pipe(
        tap(res => {
          if (res?.data?.token) {
            this.authService.setToken(res.data.token);
          }
        }),
        switchMap(() => this.authService.getMe())
      );
  }
}
