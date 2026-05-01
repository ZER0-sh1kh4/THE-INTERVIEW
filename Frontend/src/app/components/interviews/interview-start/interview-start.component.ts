import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InterviewStartForm } from '../../../models/interview.model';
import { InterviewService } from '../../../services/interview.service';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

/**
 * Purpose: Collects configuration for a personalized AI mock interview.
 * Uses ngModel for a beginner-friendly form and calls InterviewService to create the backend session.
 */
@Component({
  selector: 'app-interview-start',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, UserNavbarComponent],
  templateUrl: './interview-start.component.html',
  styleUrl: './interview-start.component.css'
})
export class InterviewStartComponent implements OnInit, OnDestroy {
  /** Options displayed by the setup form. */
  experienceLevels = ['Fresher', '1-3 years', '3-5 years', '5+ years'];
  interviewTypes: InterviewStartForm['interviewType'][] = ['Technical', 'HR', 'Mixed'];
  techOptions = ['Angular', 'Node.js', 'Java', 'SQL', 'TypeScript', 'AWS', 'C#', '.NET'];
  difficulties: InterviewStartForm['difficulty'][] = ['Easy', 'Medium', 'Hard'];
  questionCounts = [5, 10, 20];

  /** ngModel-backed form state sent to the service on submit. */
  form: InterviewStartForm = {
    role: '',
    experience: '1-3 years',
    interviewType: 'Technical',
    techStack: ['Angular', 'TypeScript'],
    difficulty: 'Medium',
    numberOfQuestions: 5
  };

  customSkill = '';
  isLoading = false;
  errorMsg = '';
  infoMsg = '';
  warmUpStatus = '';
  private warmUpTimer: any;

  constructor(private interviewService: InterviewService, private router: Router) {}

  ngOnInit(): void {
    // Warm-up is deferred until the user types a meaningful role name.
  }

  ngOnDestroy(): void {
    if (this.warmUpTimer) {
      clearTimeout(this.warmUpTimer);
    }
  }

  /**
   * Constructs the domain string used for JIT caching.
   */
  private buildDomainString(): string {
    return [
      this.form.role,
      this.form.experience,
      this.form.interviewType,
      this.form.difficulty,
      `${this.form.numberOfQuestions} Questions`,
      this.form.techStack.join(', ')
    ].filter(Boolean).join(' | ');
  }

  /**
   * Sends a warm-up signal to the backend to pre-generate questions.
   */
  private triggerWarmUp() {
    // Only trigger if role is at least 5 characters — avoids spamming the AI on every keystroke.
    if (this.form.role.trim().length < 5 || this.form.techStack.length === 0) return;

    this.warmUpStatus = 'warming';
    this.interviewService.warmUpCache({
      domain: this.buildDomainString(),
      targetCount: 3
    }).subscribe({
      next: () => { this.warmUpStatus = 'ready'; },
      error: () => { this.warmUpStatus = 'failed'; }
    });
  }

  /** Re-trigger warm-up when form changes with debounce */
  onConfigChange() {
    if (this.warmUpTimer) clearTimeout(this.warmUpTimer);
    // Wait 1.5 seconds after the user stops typing before calling the AI.
    this.warmUpTimer = setTimeout(() => this.triggerWarmUp(), 1500);
  }

  /** Adds or removes a technology chip from the selected stack. */
  toggleTech(skill: string): void {
    const normalizedSkill = skill.trim();
    if (!normalizedSkill) {
      return;
    }

    this.form.techStack = this.form.techStack.includes(normalizedSkill)
      ? this.form.techStack.filter(item => item !== normalizedSkill)
      : [...this.form.techStack, normalizedSkill];
    
    this.onConfigChange();
  }

  /** Adds a custom skill chip typed by the user. */
  addCustomSkill(): void {
    if (!this.customSkill.trim()) {
      return;
    }

    this.toggleTech(this.customSkill);
    this.customSkill = '';
  }

  /** Creates the interview, then navigates to the live session route. */
  startInterview(): void {
    if (!this.form.role.trim() || this.form.techStack.length === 0) {
      this.errorMsg = 'Please enter a job role and select at least one tech stack item.';
      return;
    }

    this.isLoading = true;
    this.errorMsg = '';
    this.infoMsg = 'Preparing your interview...';

    this.interviewService.createInterviewSession(this.form).subscribe({
      next: interview => {
        this.isLoading = false;
        if (!interview.id) {
          this.errorMsg = 'Interview was created, but the session id could not be resolved. Please try again.';
          return;
        }

        this.router.navigate(['/interviews', interview.id, 'session']);
      },
      error: error => {
        this.isLoading = false;
        this.infoMsg = '';
        const message = error?.error?.message || error?.error?.Message || 'Unable to start interview. Please try again.';
        if (error?.status === 403 || message.toLowerCase().includes('upgrade to premium')) {
          this.router.navigate(['/premium'], {
            queryParams: { reason: 'interview-limit' },
            state: { upgradeMessage: message }
          });
          return;
        }

        this.errorMsg = message;
      }
    });
  }
}
