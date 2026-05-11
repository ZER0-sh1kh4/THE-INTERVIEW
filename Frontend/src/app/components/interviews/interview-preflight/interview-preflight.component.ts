import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { InterviewService } from '../../../services/interview.service';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

/**
 * Purpose: Pre-interview environment validation page.
 * Checks microphone, browser speech APIs, and fetches questions from Gemini
 * BEFORE the user enters the live interview session. This avoids mid-interview
 * permission popups and gives the AI generation pipeline a head start.
 */
@Component({
  selector: 'app-interview-preflight',
  standalone: true,
  imports: [CommonModule, RouterModule, UserNavbarComponent],
  templateUrl: './interview-preflight.component.html',
  styleUrl: './interview-preflight.component.css'
})
export class InterviewPreflightComponent implements OnInit, OnDestroy {
  interviewId = 0;

  /** Each check as a flat array for easy *ngFor rendering */
  checkList: { key: string; label: string; description: string; status: string }[] = [
    { key: 'browser', label: 'Browser Compatibility', description: 'Verifying browser supports required media APIs', status: 'pending' },
    { key: 'microphone', label: 'Microphone Access', description: 'Requesting microphone permission for voice recording', status: 'pending' },
    { key: 'speechRecognition', label: 'Speech-to-Text Engine', description: 'Checking speech-to-text capability for live transcription', status: 'pending' },
    { key: 'speechSynthesis', label: 'Text-to-Speech Engine', description: 'Checking text-to-speech for reading questions aloud', status: 'pending' },
    { key: 'questions', label: 'AI Question Generation', description: 'Loading AI-generated interview questions from Gemini', status: 'pending' }
  ];

  /** Overall status */
  allPassed = false;
  hasCriticalFailure = false;
  errorDetails = '';
  questionsReady = false;
  questionCount = 0;
  questionsChecking = false;

  /** Mic stream reference for cleanup */
  private micStream: MediaStream | null = null;
  private timeoutTimer: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private interviewService: InterviewService,
    private cdr: ChangeDetectorRef,
    private zone: NgZone
  ) {}

  ngOnInit(): void {
    this.interviewId = Number(this.route.snapshot.paramMap.get('id'));
    this.runAllChecks();
  }

  ngOnDestroy(): void {
    this.releaseMic();
    if (this.timeoutTimer) clearTimeout(this.timeoutTimer);
  }

  /** Runs all validation checks sequentially. */
  async runAllChecks(): Promise<void> {
    // Reset
    this.allPassed = false;
    this.hasCriticalFailure = false;
    this.errorDetails = '';
    this.checkList.forEach(c => c.status = 'pending');
    this.cdr.detectChanges();

    // 1. Browser compatibility
    this.setCheck('browser', 'checking');
    await this.delay(500);
    this.setCheck('browser', this.checkBrowser() ? 'pass' : 'fail');

    // 2. Microphone permission
    this.setCheck('microphone', 'checking');
    await this.delay(400);
    await this.checkMicrophone();

    // 3. Speech Recognition API
    this.setCheck('speechRecognition', 'checking');
    await this.delay(400);
    this.setCheck('speechRecognition', this.checkSpeechRecognition() ? 'pass' : 'warn');

    // 4. Text-to-Speech
    this.setCheck('speechSynthesis', 'checking');
    await this.delay(400);
    this.setCheck('speechSynthesis', this.checkSpeechSynthesis() ? 'pass' : 'warn');

    // 5. Question generation (kicked off in background)
    this.setCheck('questions', 'checking');
    this.questionsChecking = true;
    this.cdr.detectChanges();
    this.loadQuestions();

    // Evaluate overall (questions may still be loading)
    this.evaluateOverall();
  }

  /** Helper to update a check status and trigger change detection. */
  private setCheck(key: string, status: string): void {
    const check = this.checkList.find(c => c.key === key);
    if (check) check.status = status;
    this.cdr.detectChanges();
  }

  /** Get the status of a specific check. */
  getCheckStatus(key: string): string {
    return this.checkList.find(c => c.key === key)?.status ?? 'pending';
  }

  /** Get icon for each check state. */
  getIcon(status: string): string {
    switch (status) {
      case 'pass': return 'check_circle';
      case 'fail': return 'cancel';
      case 'warn': return 'warning';
      case 'checking': return 'sync';
      default: return 'radio_button_unchecked';
    }
  }

  /** Checks basic browser requirements. */
  private checkBrowser(): boolean {
    return !!(window.MediaRecorder || navigator.mediaDevices?.getUserMedia);
  }

  /** Requests microphone permission. */
  private async checkMicrophone(): Promise<void> {
    if (!navigator.mediaDevices?.getUserMedia) {
      this.setCheck('microphone', 'fail');
      this.errorDetails = 'Your browser does not support microphone access.';
      return;
    }

    try {
      this.releaseMic();
      this.micStream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.setCheck('microphone', 'pass');
      // Release immediately — we just needed to confirm permission
      this.releaseMic();
    } catch {
      this.setCheck('microphone', 'fail');
      this.errorDetails = 'Microphone permission was denied. Please allow microphone access in your browser settings and retry.';
    }
  }

  /** Checks if the browser supports Web Speech Recognition. */
  private checkSpeechRecognition(): boolean {
    return !!((window as any).SpeechRecognition || (window as any).webkitSpeechRecognition);
  }

  /** Checks if the browser supports text-to-speech synthesis. */
  private checkSpeechSynthesis(): boolean {
    return !!window.speechSynthesis;
  }

  /** Begins loading questions from Gemini via the backend with a safety timeout. */
  private loadQuestions(): void {
    // Check sessionStorage first — questions might already be cached from a previous attempt
    const cacheKey = `interview_questions_${this.interviewId}`;
    const cached = sessionStorage.getItem(cacheKey);
    if (cached) {
      try {
        const parsed = JSON.parse(cached);
        if (parsed && parsed.length > 0) {
          this.setCheck('questions', 'pass');
          this.questionsReady = true;
          this.questionsChecking = false;
          this.questionCount = parsed.length;
          this.evaluateOverall();
          return;
        }
      } catch { /* fall through */ }
    }

    // Safety timeout: if questions haven't loaded in 30s, let the user proceed
    this.timeoutTimer = setTimeout(() => {
      this.zone.run(() => {
        if (this.getCheckStatus('questions') === 'checking') {
          this.setCheck('questions', 'warn');
          this.questionsChecking = false;
          this.errorDetails = 'Question generation is taking longer than expected. You can proceed — they will load in the session.';
          this.evaluateOverall();
        }
      });
    }, 30000);

    // Hit the backend to generate questions
    this.interviewService.loadSessionQuestions(this.interviewId).subscribe({
      next: session => {
        clearTimeout(this.timeoutTimer);
        if (session.questions && session.questions.length > 0) {
          sessionStorage.setItem(cacheKey, JSON.stringify(session.questions));
          this.setCheck('questions', 'pass');
          this.questionsReady = true;
          this.questionCount = session.questions.length;
        } else {
          this.setCheck('questions', 'warn');
          this.errorDetails = 'AI returned no questions. You can proceed — questions will be generated in the session.';
        }
        this.questionsChecking = false;
        this.evaluateOverall();
      },
      error: () => {
        clearTimeout(this.timeoutTimer);
        // Try fallback GET
        this.interviewService.getInterview(this.interviewId).subscribe({
          next: fallback => {
            if (fallback.questions && fallback.questions.length > 0) {
              sessionStorage.setItem(cacheKey, JSON.stringify(fallback.questions));
              this.setCheck('questions', 'pass');
              this.questionsReady = true;
              this.questionCount = fallback.questions.length;
            } else {
              this.setCheck('questions', 'warn');
              this.errorDetails = 'Questions are still generating. You can proceed — they will load in the session.';
            }
            this.questionsChecking = false;
            this.evaluateOverall();
          },
          error: () => {
            this.setCheck('questions', 'warn');
            this.questionsChecking = false;
            this.errorDetails = 'Question pre-loading failed (AI may be rate-limited). You can still proceed — they will load in the session.';
            this.evaluateOverall();
          }
        });
      }
    });
  }

  /** Determines overall readiness. */
  private evaluateOverall(): void {
    const values = this.checkList.map(c => c.status);
    this.hasCriticalFailure = this.getCheckStatus('microphone') === 'fail';
    this.allPassed = values.every(v => v === 'pass' || v === 'warn');
    this.cdr.detectChanges();
  }

  /** Navigate into the live session. */
  proceedToSession(): void {
    this.router.navigate(['/interviews', this.interviewId, 'session']);
  }

  /** Re-run all checks. */
  async retryChecks(): Promise<void> {
    await this.runAllChecks();
  }

  private releaseMic(): void {
    this.micStream?.getTracks().forEach(t => t.stop());
    this.micStream = null;
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
