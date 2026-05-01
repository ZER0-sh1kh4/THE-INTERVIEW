import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { InterviewService } from '../../services/interview.service';
import { AssessmentService } from '../../services/assessment.service';
import { UserNavbarComponent } from '../shared/user-navbar/user-navbar.component';

/**
 * Purpose: Displays the authenticated user's profile in the editorial design system.
 * Shows avatar, identity details, membership status, session stats, and a change-password section.
 * Supports inline editing of display name with backend persistence.
 */
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, UserNavbarComponent],
  providers: [DatePipe],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  currentUser$: Observable<User | null | undefined>;
  user: User | null = null;

  /** Stats pulled from cached interview data. */
  sessionsCompleted = 0;
  totalInterviews = 0;
  totalAssessments = 0;
  inProgressCount = 0;

  /** Active section state. */
  activeSection: 'account' = 'account';

  /** Formatted member-since date. */
  memberSince = '';

  /** Edit profile state. */
  isEditingProfile = false;
  editName = '';
  profileMsg = '';
  profileError = '';
  isSavingProfile = false;

  constructor(
    private authService: AuthService,
    private interviewService: InterviewService,
    private assessmentService: AssessmentService,
    private datePipe: DatePipe,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    this.currentUser$.subscribe(user => {
      this.user = user ?? null;
    });

    this.loadStats();
  }

  /** Generates avatar initials from the user's display name or email. */
  getInitials(user: User | null): string {
    if (!user) return '?';
    const name = this.getDisplayName(user);
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  /** Returns the user-friendly display name, preferring fullName over email-derived name. */
  getDisplayName(user: User | null): string {
    if (!user) return 'Candidate';
    if (user.fullName && user.fullName.trim().length > 0) {
      return user.fullName.trim()
        .replace(/\b\w/g, c => c.toUpperCase());
    }
    if (!user.email) return 'Candidate';
    return user.email
      .split('@')[0]
      .replace(/[._-]+/g, ' ')
      .replace(/\b\w/g, c => c.toUpperCase());
  }

  getRoleLabel(user: User | null): string {
    if (!user?.role) return 'Candidate';
    return user.role.charAt(0).toUpperCase() + user.role.slice(1);
  }

  getPlanLabel(user: User | null): string {
    return user?.isPremium === true ? 'PREMIUM' : 'FREE';
  }

  isPremium(user: User | null): boolean {
    return user?.isPremium === true;
  }

  setSection(section: 'account'): void {
    this.activeSection = section;
    this.profileMsg = '';
    this.profileError = '';
    this.cancelEdit();
  }

  // ─── Edit Profile ──────────────────────────────────────────────────

  /** Enters inline edit mode, pre-filling the current display name. */
  startEdit(): void {
    this.isEditingProfile = true;
    this.editName = this.getDisplayName(this.user);
    this.profileMsg = '';
    this.profileError = '';
  }

  /** Cancels edit mode without saving. */
  cancelEdit(): void {
    this.isEditingProfile = false;
    this.editName = '';
    this.profileError = '';
  }

  /** Validates and saves the updated display name via API. */
  saveProfile(): void {
    this.profileMsg = '';
    this.profileError = '';

    const trimmed = this.editName?.trim() || '';
    if (trimmed.length < 2) {
      this.profileError = 'Name must be at least 2 characters.';
      return;
    }
    if (trimmed.length > 100) {
      this.profileError = 'Name must be under 100 characters.';
      return;
    }

    this.isSavingProfile = true;
    this.authService.updateProfile({ fullName: trimmed }).subscribe({
      next: () => {
        this.isSavingProfile = false;
        this.isEditingProfile = false;
        this.profileMsg = 'Profile updated successfully.';
        setTimeout(() => this.profileMsg = '', 4000);
      },
      error: (err) => {
        this.isSavingProfile = false;
        this.profileError = err?.error?.message || 'Unable to update profile. Please try again.';
      }
    });
  }

  /** Re-fetches user data from the backend. */
  refreshProfile(): void {
    this.profileMsg = '';
    this.authService.getMe().subscribe({
      next: () => {
        this.profileMsg = 'Profile refreshed.';
        setTimeout(() => this.profileMsg = '', 3000);
      },
      error: () => {
        this.profileError = 'Unable to refresh profile.';
      }
    });
  }

  // ─── Stats ─────────────────────────────────────────────────────────

  /** Loads interview stats from cache or fetches fresh data. */
  private loadStats(): void {
    const cached = this.interviewService.getCachedInterviews();
    if (cached.length) {
      this.applyStats(cached);
    }

    this.interviewService.getMyInterviews().subscribe({
      next: interviews => this.applyStats(interviews),
      error: () => {} // silently keep cached stats
    });

    const cachedAssessments = this.assessmentService.getCachedAssessments();
    if (cachedAssessments.length) {
      this.totalAssessments = cachedAssessments.length;
    }

    this.assessmentService.getMyAssessments().subscribe({
      next: assessments => this.totalAssessments = assessments.length,
      error: () => {} // silently keep cached stats
    });
  }

  private applyStats(interviews: any[]): void {
    this.totalInterviews = interviews.length;
    this.sessionsCompleted = interviews.filter(
      i => (i.status || '').toLowerCase() === 'completed'
    ).length;
    this.inProgressCount = interviews.filter(
      i => (i.status || '').toLowerCase() === 'inprogress'
    ).length;
  }



  navigateToPremium(): void {
    this.router.navigate(['/premium']);
  }
}
