import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { SubscriptionService } from './subscription.service';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

describe('SubscriptionService', () => {
  let service: SubscriptionService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/subscriptions`;

  beforeEach(() => {
    const spy = {
      currentUserValue: null,
      setToken: vi.fn(),
      getMe: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        SubscriptionService,
        { provide: AuthService, useValue: spy },
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(SubscriptionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('subscribe()', () => {
    it('should POST and return checkout session data', () => {
      service.subscribe().subscribe(res => {
        expect(res.checkoutSessionId).toBe('cs_test_123');
        expect(res.amount).toBe(999);
      });
      const req = httpMock.expectOne(`${apiUrl}/subscribe`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: {
        checkoutSessionId: 'cs_test_123', checkoutUrl: 'https://stripe.com',
        amount: 999, currency: 'inr', mode: 'payment', message: 'Session created'
      }});
    });
  });

  describe('confirmPayment()', () => {
    it('should POST payment confirmation details', () => {
      service.confirmPayment('sess_1', 'pi_1', 'sig_1').subscribe();
      const req = httpMock.expectOne(`${apiUrl}/confirm`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.paymentSessionId).toBe('sess_1');
      expect(req.request.body.paymentReferenceId).toBe('pi_1');
      req.flush({ success: true, message: '', data: {} });
    });
  });

  describe('getMySubscriptions()', () => {
    it('should GET user subscriptions', () => {
      service.getMySubscriptions().subscribe(subs => {
        expect(subs.length).toBe(1);
        expect(subs[0].plan).toBe('Premium');
      });
      const req = httpMock.expectOne(`${apiUrl}/my`);
      expect(req.request.method).toBe('GET');
      req.flush({ success: true, message: '', data: [
        { id: 1, userId: 1, plan: 'Premium', price: 999, status: 'Active',
          sagaState: 'Completed', startDate: '', endDate: '', createdAt: '' }
      ]});
    });
  });

  describe('getMyPayments()', () => {
    it('should GET payment history', () => {
      service.getMyPayments().subscribe(payments => {
        expect(payments.length).toBe(1);
        expect(payments[0].amount).toBe(999);
      });
      const req = httpMock.expectOne(`${apiUrl}/my/payments`);
      expect(req.request.method).toBe('GET');
      req.flush({ success: true, message: '', data: [
        { id: 1, subscriptionId: 1, userId: 1, amount: 999, currency: 'inr',
          stripeSessionId: '', stripePaymentIntentId: '', status: 'Paid', createdAt: '' }
      ]});
    });
  });

  describe('cancelSubscription()', () => {
    it('should POST cancel and return message', () => {
      service.cancelSubscription().subscribe(msg => {
        expect(msg).toBe('Subscription cancelled');
      });
      const req = httpMock.expectOne(`${apiUrl}/cancel`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: 'Subscription cancelled', data: {} });
    });
  });
});
