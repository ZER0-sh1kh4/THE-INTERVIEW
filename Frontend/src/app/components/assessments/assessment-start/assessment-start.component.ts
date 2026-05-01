import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AssessmentService } from '../../../services/assessment.service';
import { AuthService } from '../../../services/auth.service';
import { StartAssessmentRequest } from '../../../models/assessment.model';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

@Component({
  selector: 'app-assessment-start',
  standalone: true,
  imports: [CommonModule, FormsModule, UserNavbarComponent],
  templateUrl: './assessment-start.component.html',
  styleUrls: ['./assessment-start.component.css']
})
export class AssessmentStartComponent implements OnInit, OnDestroy {
  form: StartAssessmentRequest = {
    domain: 'C#',
    questionCount: 10,
    difficulty: 'Medium'
  };

  subtopicsInput: string = '';
  subtopics: string[] = [];

  difficulties = ['Easy', 'Medium', 'Hard', 'Expert'];
  questionCounts = [5, 10, 20, 30];

  isLoading = false;
  errorMsg = '';
  infoMsg = '';
  warmUpStatus = '';

  private warmUpTimer: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private assessmentService: AssessmentService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['domain']) {
        this.form.domain = params['domain'];
      }
    });

    // Free-tier gate: check assessment count immediately
    this.checkFreeUserLimit();

    // Step 2: Predictive Pre-Generation — warm up the cache
    // while the user reads the instructions and configures settings
    this.triggerWarmUp();
  }

  /** If a free user has already taken 2 assessments, redirect to subscription page */
  private checkFreeUserLimit(): void {
    const user = this.authService.currentUserValue;
    const isPremium = user?.isPremium === true || user?.isPremium === 'true';
    if (isPremium) return; // Premium users have unlimited access

    this.assessmentService.getMyAssessments().subscribe({
      next: (results) => {
        if (results.length >= 2) {
          this.router.navigate(['/premium'], {
            queryParams: { reason: 'assessment_limit' }
          });
        }
      },
      error: () => {
        // If we can't check, let the backend enforce the limit
      }
    });
  }

  ngOnDestroy(): void {
    if (this.warmUpTimer) {
      clearTimeout(this.warmUpTimer);
    }
  }

  /**
   * Sends a warm-up signal to the backend to pre-generate and cache questions.
   * Fires silently in the background — user sees a subtle indicator.
   */
  private triggerWarmUp() {
    this.warmUpStatus = 'warming';
    this.assessmentService.warmUpCache({
      domain: this.form.domain,
      difficulty: this.form.difficulty,
      targetCount: Math.min(this.form.questionCount, 10) // pre-cache up to 10
    }).subscribe({
      next: () => {
        this.warmUpStatus = 'ready';
      },
      error: () => {
        this.warmUpStatus = 'failed';
        // Silently fail — the seed data will still be available
      }
    });
  }

  /** Re-trigger warm-up when domain or difficulty changes */
  onConfigChange() {
    if (this.warmUpTimer) {
      clearTimeout(this.warmUpTimer);
    }
    // Debounce — wait 500ms after last change
    this.warmUpTimer = setTimeout(() => {
      this.triggerWarmUp();
    }, 500);
  }

  addSubtopic() {
    const topic = this.subtopicsInput.trim();
    if (topic && !this.subtopics.includes(topic)) {
      this.subtopics.push(topic);
    }
    this.subtopicsInput = '';
  }

  removeSubtopic(topic: string) {
    this.subtopics = this.subtopics.filter(t => t !== topic);
  }

  startAssessment() {
    if (!this.form.domain) return;

    this.isLoading = true;
    this.errorMsg = '';
    this.infoMsg = 'Preparing your assessment...';

    this.assessmentService.startAssessment(this.form).subscribe({
      next: (response) => {
        this.isLoading = false;
        // Session storage to persist the questions for the session page
        sessionStorage.setItem('current_assessment', JSON.stringify(response));
        this.router.navigate(['/assessments', response.assessmentId, 'session']);
      },
      error: (err) => {
        this.isLoading = false;
        this.infoMsg = '';
        this.errorMsg = err.error?.message || 'Failed to start assessment. Please try again.';
      }
    });
  }
}
