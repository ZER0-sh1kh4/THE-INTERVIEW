import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { vi, type Mocked } from 'vitest';
import { AuthService } from '../../services/auth.service';

import { ResetPasswordComponent } from './reset-password.component';

describe('ResetPasswordComponent', () => {
  let component: ResetPasswordComponent;
  let fixture: ComponentFixture<ResetPasswordComponent>;
  let authServiceSpy: Mocked<Pick<AuthService, 'sendPasswordResetOtp' | 'resetPassword'>>;
  let routerSpy: Mocked<Pick<Router, 'navigate'>>;

  beforeEach(async () => {
    authServiceSpy = {
      sendPasswordResetOtp: vi.fn().mockReturnValue(of({})),
      resetPassword: vi.fn().mockReturnValue(of({}))
    };
    routerSpy = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ResetPasswordComponent],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        provideRouter([])
      ]
    }).compileComponents();

    routerSpy = {
      navigate: vi.spyOn(TestBed.inject(Router), 'navigate')
    };
    fixture = TestBed.createComponent(ResetPasswordComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
