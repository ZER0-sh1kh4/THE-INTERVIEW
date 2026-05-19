import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { vi, type Mocked } from 'vitest';
import { User } from '../../../models/user.model';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';

import { UserNavbarComponent } from './user-navbar.component';

describe('UserNavbarComponent', () => {
  let component: UserNavbarComponent;
  let fixture: ComponentFixture<UserNavbarComponent>;
  let authServiceSpy: Mocked<Pick<AuthService, 'logout'>> & Pick<AuthService, 'currentUser$'>;
  let notificationServiceSpy: Mocked<Pick<NotificationService, 'fetchNotifications' | 'markAllAsRead'>> &
    Pick<NotificationService, 'notifications$' | 'unreadCount$'>;
  let routerSpy: Mocked<Pick<Router, 'navigate' | 'navigateByUrl'>>;

  beforeEach(async () => {
    authServiceSpy = {
      currentUser$: of<User | null>({ userId: '1', email: 'test@example.com', role: 'Candidate', isPremium: false }),
      logout: vi.fn()
    };
    notificationServiceSpy = {
      notifications$: of([]),
      unreadCount$: new BehaviorSubject(0).asObservable(),
      fetchNotifications: vi.fn().mockReturnValue(of([])),
      markAllAsRead: vi.fn().mockReturnValue(of(null))
    };
    routerSpy = {
      navigate: vi.fn(),
      navigateByUrl: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [UserNavbarComponent],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    routerSpy = {
      navigate: vi.spyOn(TestBed.inject(Router), 'navigate'),
      navigateByUrl: vi.spyOn(TestBed.inject(Router), 'navigateByUrl')
    };
    fixture = TestBed.createComponent(UserNavbarComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
