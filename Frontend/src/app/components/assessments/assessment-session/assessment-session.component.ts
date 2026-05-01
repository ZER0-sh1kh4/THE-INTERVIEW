import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { AssessmentService } from '../../../services/assessment.service';
import { StartAssessmentResponse, QuestionDto, SubmitAssessmentRequest, AnswerSubmission } from '../../../models/assessment.model';

@Component({
  selector: 'app-assessment-session',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './assessment-session.component.html',
  styleUrls: ['./assessment-session.component.css']
})
export class AssessmentSessionComponent implements OnInit, OnDestroy {
  assessmentId!: number;
  sessionData!: StartAssessmentResponse;
  
  questions: QuestionDto[] = [];
  currentIndex: number = 0;
  
  // map of questionId to selected option (A, B, C, D)
  answers: { [questionId: number]: string } = {};

  timerMinutes = 0;
  timerSeconds = 0;
  private timerInterval: any;
  private expiresAt!: Date;

  warnings = 0;
  maxWarnings = 3;
  showWarningModal = false;
  warningMessage = '';

  isSubmitting = false;

  // Lazy loading state
  totalExpected = 0;
  isFetchingMore = false;
  fetchAttempted = false;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private assessmentService: AssessmentService
  ) {}

  ngOnInit(): void {
    const sessionStr = sessionStorage.getItem('current_assessment');
    if (!sessionStr) {
      this.router.navigate(['/assessments/domain']);
      return;
    }

    this.sessionData = JSON.parse(sessionStr);
    this.assessmentId = this.sessionData.assessmentId;
    this.questions = this.sessionData.questions || [];
    this.totalExpected = this.sessionData.totalExpected || this.questions.length;
    this.expiresAt = new Date(this.sessionData.expiresAt);
    this.warnings = this.sessionData.warnings || 0;
    this.answers = this.sessionData.answers || {};

    this.enterFullscreen();
    this.startTimer();

    // If the backend flagged that more questions are available, fetch them in background
    if (this.sessionData.hasMore && this.questions.length < this.totalExpected) {
      this.fetchNextBatch();
    }
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  // --- LAZY LOADING (Step 3: Batches of 3) ---
  fetchNextBatch() {
    if (this.isFetchingMore || this.isSubmitting) return;
    this.isFetchingMore = true;

    const currentCount = this.questions.length;
    const needed = this.totalExpected - currentCount;
    const batchSize = Math.min(needed, 3); // Step 3: Small batches to avoid rate limits

    this.assessmentService.getNextBatch(this.assessmentId, currentCount, batchSize).subscribe({
      next: (newQuestions) => {
        if (newQuestions && newQuestions.length > 0) {
          // Avoid duplicates by ID
          const existingIds = new Set(this.questions.map(q => q.id));
          const uniqueNew = newQuestions.filter(q => !existingIds.has(q.id));
          this.questions.push(...uniqueNew);

          // Update session storage
          this.sessionData.questions = this.questions;
          sessionStorage.setItem('current_assessment', JSON.stringify(this.sessionData));
        }
        this.isFetchingMore = false;

        // If still need more, wait 3s then fetch again (spaces out API calls)
        if (this.questions.length < this.totalExpected) {
          setTimeout(() => this.fetchNextBatch(), 3000);
        }
      },
      error: () => {
        this.isFetchingMore = false;
        // Don't crash — user can still take the test with whatever questions we have
      }
    });
  }

  @HostListener('document:visibilitychange')
  onVisibilityChange() {
    if (document.hidden && !this.isSubmitting) {
      this.handleCheatingViolation('Tab switch detected.');
    }
  }

  handleCheatingViolation(reason: string) {
    this.warnings++;

    // Save to session storage immediately so it survives refresh
    this.sessionData.warnings = this.warnings;
    sessionStorage.setItem('current_assessment', JSON.stringify(this.sessionData));

    if (this.warnings >= this.maxWarnings) {
      this.warningMessage = `Violation ${this.warnings}/${this.maxWarnings}: ${reason}. Auto-submitting assessment.`;
      this.showWarningModal = true;
      setTimeout(() => this.submitAssessment(), 2000); // 2-second grace period
    } else {
      this.warningMessage = `Warning ${this.warnings}/${this.maxWarnings}: ${reason}. Please stay on this page.`;
      this.showWarningModal = true;
    }
  }

  dismissWarning() {
    this.showWarningModal = false;
    this.enterFullscreen();
  }

  enterFullscreen() {
    const elem = document.documentElement as any;
    if (elem.requestFullscreen) {
      elem.requestFullscreen().catch(() => {});
    } else if (elem.webkitRequestFullscreen) { /* Safari */
      elem.webkitRequestFullscreen().catch(() => {});
    } else if (elem.msRequestFullscreen) { /* IE11 */
      elem.msRequestFullscreen().catch(() => {});
    }
  }

  // --- TIMER ---
  startTimer() {
    this.updateTimer();
    this.timerInterval = setInterval(() => {
      this.updateTimer();
    }, 1000);
  }

  updateTimer() {
    const now = new Date().getTime();
    const distance = this.expiresAt.getTime() - now;

    if (distance <= 0) {
      clearInterval(this.timerInterval);
      this.timerMinutes = 0;
      this.timerSeconds = 0;
      if (!this.isSubmitting) {
        this.submitAssessment();
      }
    } else {
      this.timerMinutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
      this.timerSeconds = Math.floor((distance % (1000 * 60)) / 1000);
    }
  }

  // --- NAVIGATION ---
  get currentQuestion(): QuestionDto {
    return this.questions[this.currentIndex];
  }

  selectOption(option: string) {
    this.answers[this.currentQuestion.id] = option;
    this.sessionData.answers = this.answers;
    sessionStorage.setItem('current_assessment', JSON.stringify(this.sessionData));
  }

  nextQuestion() {
    if (this.currentIndex < this.questions.length - 1) {
      this.currentIndex++;
    }
  }

  prevQuestion() {
    if (this.currentIndex > 0) {
      this.currentIndex--;
    }
  }

  goToQuestion(index: number) {
    this.currentIndex = index;
  }

  // --- SUBMISSION ---
  submitAssessment() {
    if (this.isSubmitting) return;
    this.isSubmitting = true;
    clearInterval(this.timerInterval);

    const submissions: AnswerSubmission[] = Object.keys(this.answers).map(qId => ({
      questionId: Number(qId),
      selectedOption: this.answers[Number(qId)]
    }));

    const request: SubmitAssessmentRequest = {
      assessmentId: this.assessmentId,
      answers: submissions,
      totalExpected: this.totalExpected
    };

    this.assessmentService.submitAssessment(request).subscribe({
      next: () => {
        sessionStorage.removeItem('current_assessment');
        // Exit fullscreen
        if (document.exitFullscreen) {
          document.exitFullscreen().catch(() => {});
        }
        this.router.navigate(['/assessments', this.assessmentId, 'result']);
      },
      error: () => {
        // Fallback — navigate anyway so user isn't stuck
        sessionStorage.removeItem('current_assessment');
        if (document.exitFullscreen) {
          document.exitFullscreen().catch(() => {});
        }
        this.router.navigate(['/assessments', this.assessmentId, 'result']);
      }
    });
  }
}
