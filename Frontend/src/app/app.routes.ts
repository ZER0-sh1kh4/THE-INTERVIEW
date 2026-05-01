import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';
import { InterviewStartComponent } from './components/interviews/interview-start/interview-start.component';
import { InterviewSessionComponent } from './components/interviews/interview-session/interview-session.component';
import { InterviewResultComponent } from './components/interviews/interview-result/interview-result.component';
import { InterviewHistoryComponent } from './components/interviews/interview-history/interview-history.component';
import { PremiumComponent } from './components/premium/premium.component';
import { ProfileComponent } from './components/profile/profile.component';
import { AssessmentDomainComponent } from './components/assessments/assessment-domain/assessment-domain.component';
import { AssessmentStartComponent } from './components/assessments/assessment-start/assessment-start.component';
import { AssessmentSessionComponent } from './components/assessments/assessment-session/assessment-session.component';
import { AssessmentResultComponent } from './components/assessments/assessment-result/assessment-result.component';
import { AssessmentHistoryComponent } from './components/assessments/assessment-history/assessment-history.component';
import { SubscriptionSuccessComponent } from './components/subscription-success/subscription-success.component';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';

// Admin components
import { AdminDashboardComponent } from './admin/components/admin-dashboard/admin-dashboard.component';
import { AdminUsersComponent } from './admin/components/admin-users/admin-users.component';
import { AdminInterviewsComponent } from './admin/components/admin-interviews/admin-interviews.component';
import { AdminAssessmentsComponent } from './admin/components/admin-assessments/admin-assessments.component';
import { AdminQuestionsComponent } from './admin/components/admin-questions/admin-questions.component';

/**
 * Purpose: Defines the app's top-level pages.
 * Public routes handle landing/auth, while user-dashboard is protected by the JWT guard.
 * Admin routes are protected by both authGuard and adminGuard.
 */
export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'user-dashboard', component: UserDashboardComponent, canActivate: [authGuard] },
  { path: 'interviews/start', component: InterviewStartComponent, canActivate: [authGuard] },
  { path: 'interviews/history', component: InterviewHistoryComponent, canActivate: [authGuard] },
  { path: 'interviews/:id/session', component: InterviewSessionComponent, canActivate: [authGuard] },
  { path: 'interviews/:id/result', component: InterviewResultComponent, canActivate: [authGuard] },
  { path: 'assessments/domain', component: AssessmentDomainComponent, canActivate: [authGuard] },
  { path: 'assessments/history', component: AssessmentHistoryComponent, canActivate: [authGuard] },
  { path: 'assessments/start', component: AssessmentStartComponent, canActivate: [authGuard] },
  { path: 'assessments/:id/session', component: AssessmentSessionComponent, canActivate: [authGuard] },
  { path: 'assessments/:id/result', component: AssessmentResultComponent, canActivate: [authGuard] },
  { path: 'premium', component: PremiumComponent, canActivate: [authGuard] },
  { path: 'subscription/success', component: SubscriptionSuccessComponent, canActivate: [authGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },

  // ─── Admin Routes ───
  { path: 'admin', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/users', component: AdminUsersComponent, canActivate: [adminGuard] },
  { path: 'admin/interviews', component: AdminInterviewsComponent, canActivate: [adminGuard] },
  { path: 'admin/assessments', component: AdminAssessmentsComponent, canActivate: [adminGuard] },
  { path: 'admin/questions', component: AdminQuestionsComponent, canActivate: [adminGuard] },

  { path: '**', redirectTo: '' }
];

