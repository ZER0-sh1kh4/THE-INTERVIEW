import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, NgZone, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { InterviewAnswerSubmission, InterviewQuestion } from '../../../models/interview.model';
import { InterviewService } from '../../../services/interview.service';

/**
 * Purpose: Runs the live AI mock interview session.
 * It displays Gemini-generated questions, reads them using TTS, captures speech using STT, and submits transcripts.
 * Enforces fullscreen mode and detects tab/screen switches with anti-cheating warnings.
 */
@Component({
  selector: 'app-interview-session',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './interview-session.component.html',
  styleUrl: './interview-session.component.css'
})
export class InterviewSessionComponent implements OnInit, OnDestroy {
  interviewId = 0;
  questions: InterviewQuestion[] = [];
  currentQuestionIndex = 0;
  answers: Record<number, string> = {};
  transcript = '';
  transcriptStatus = 'Microphone ready. Press Start Recording when you are ready to answer.';
  recognitionSupported = false;

  isLoading = true;
  isRecording = false;
  isSpeaking = false;
  errorMsg = '';
  submitMsg = '';
  resultSummary = '';

  /** Anti-cheating: tab switch tracking */
  tabSwitchCount = 0;
  showWarningOverlay = false;
  warningTitle = '';
  warningMessage = '';
  isAutoSubmitting = false;

  private recognition: any = null;
  private recordingBaseTranscript = '';
  private pendingTranscript = '';
  private microphoneStream: MediaStream | null = null;
  private stopFallbackTimer: ReturnType<typeof setTimeout> | null = null;
  private shouldKeepRecording = false;
  private manuallyStopping = false;
  private visibilityHandler: (() => void) | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private interviewService: InterviewService,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  /** Loads generated questions for the session id in the route. */
  ngOnInit(): void {
    this.interviewId = Number(this.route.snapshot.paramMap.get('id'));
    this.setupSpeechRecognition();

    // Check sessionStorage first — makes the session refresh-proof
    const cacheKey = `interview_questions_${this.interviewId}`;
    const cached = sessionStorage.getItem(cacheKey);
    if (cached) {
      try {
        const parsed = JSON.parse(cached) as InterviewQuestion[];
        if (parsed && parsed.length > 0) {
          this.questions = parsed;
          // Restore saved answers so the user picks up exactly where they left off
          const savedAnswers = sessionStorage.getItem(`interview_answers_${this.interviewId}`);
          if (savedAnswers) {
            try { this.answers = JSON.parse(savedAnswers); } catch { /* ignore corrupt data */ }
          }
          this.isLoading = false;
          this.loadCurrentTranscript();
          setTimeout(() => this.speakCurrentQuestion(), 500);
          this.enterFullscreen();
          this.setupVisibilityDetection();
          return;
        }
      } catch {
        // Invalid cache, fall through to API call
      }
    }

    this.loadQuestions();
    this.enterFullscreen();
    this.setupVisibilityDetection();
  }

  /** Calls the backend for session questions and guarantees the loading state eventually clears. */
  loadQuestions(): void {
    this.isLoading = true;
    this.errorMsg = '';
    window.speechSynthesis?.cancel();

    this.interviewService.loadSessionQuestions(this.interviewId).pipe(
      finalize(() => {
        this.zone.run(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe({
      next: session => {
        this.zone.run(() => {
          this.questions = session.questions || [];
          // Cache to sessionStorage for refresh resilience
          if (this.questions.length > 0) {
            sessionStorage.setItem(`interview_questions_${this.interviewId}`, JSON.stringify(this.questions));
          }
          this.loadCurrentTranscript();
          if (this.questions.length > 0) {
            setTimeout(() => this.speakCurrentQuestion(), 500);
          } else {
            this.errorMsg = 'Gemini did not return questions for this attempt. Click Generate Questions Again in a moment.';
          }
          this.cdr.detectChanges();
        });
      },
      error: error => {
        // Safe fallback: if POST /begin timed out but succeeded in the background,
        // GET /interviews/{id} will retrieve the already-saved questions.
        this.interviewService.getInterview(this.interviewId).subscribe({
          next: fallbackSession => {
            this.zone.run(() => {
              this.questions = fallbackSession.questions || [];
              if (this.questions.length > 0) {
                sessionStorage.setItem(`interview_questions_${this.interviewId}`, JSON.stringify(this.questions));
                this.isLoading = false;
                this.loadCurrentTranscript();
                setTimeout(() => this.speakCurrentQuestion(), 500);
              } else {
                this.errorMsg = error?.error?.message || 'Gemini question generation timed out. Click Generate Questions Again.';
              }
              this.cdr.detectChanges();
            });
          },
          error: () => {
            this.zone.run(() => {
              this.errorMsg = error?.error?.message || 'Gemini question generation timed out. Click Generate Questions Again to start the AI generation flow again.';
              this.cdr.detectChanges();
            });
          }
        });
      }
    });
  }

  /** Stops voice APIs and cleans up fullscreen/visibility listeners when the component is destroyed. */
  ngOnDestroy(): void {
    this.stopRecording();
    this.releaseMicrophone();
    window.speechSynthesis?.cancel();
    this.removeVisibilityDetection();
    this.exitFullscreen();
  }

  // ─── Fullscreen Management ───────────────────────────────────────────────

  /** Requests browser fullscreen on session entry. */
  private enterFullscreen(): void {
    const docEl = document.documentElement;
    const requestFs = docEl.requestFullscreen
      || (docEl as any).webkitRequestFullscreen
      || (docEl as any).msRequestFullscreen;

    if (requestFs) {
      requestFs.call(docEl).catch(() => {
        // Fullscreen may be blocked by browser policy; proceed anyway.
      });
    }
  }

  /** Exits fullscreen when leaving the session. */
  private exitFullscreen(): void {
    if (document.fullscreenElement) {
      document.exitFullscreen?.().catch(() => {});
    }
  }

  /** Re-enters fullscreen if the user tries to escape during an active interview. */
  @HostListener('document:fullscreenchange')
  onFullscreenChange(): void {
    if (!document.fullscreenElement && this.questions.length > 0 && !this.isAutoSubmitting && !this.submitMsg) {
      // User tried to exit fullscreen during interview — re-enter.
      setTimeout(() => this.enterFullscreen(), 200);
    }
  }

  // ─── Tab Switch / Visibility Detection ───────────────────────────────────

  /** Listens for tab/window visibility changes using the Page Visibility API. */
  private setupVisibilityDetection(): void {
    this.visibilityHandler = () => {
      if (document.hidden && this.questions.length > 0 && !this.isAutoSubmitting && !this.submitMsg) {
        this.zone.run(() => this.handleTabSwitch());
      }
    };
    document.addEventListener('visibilitychange', this.visibilityHandler);
  }

  /** Removes the visibility listener on destroy. */
  private removeVisibilityDetection(): void {
    if (this.visibilityHandler) {
      document.removeEventListener('visibilitychange', this.visibilityHandler);
      this.visibilityHandler = null;
    }
  }

  /** Processes a detected tab switch: show warnings or auto-submit on 3rd violation. */
  private handleTabSwitch(): void {
    this.tabSwitchCount++;

    if (this.tabSwitchCount === 1) {
      this.warningTitle = 'Warning — Tab Switch Detected';
      this.warningMessage = 'Switching tabs during an interview is not allowed. This is your first warning. Please stay on this page.';
      this.showWarningOverlay = true;
    } else if (this.tabSwitchCount === 2) {
      this.warningTitle = 'Final Warning — Tab Switch Detected';
      this.warningMessage = 'You have switched tabs again. This is your LAST warning. One more switch and your interview will be auto-submitted.';
      this.showWarningOverlay = true;
    } else if (this.tabSwitchCount >= 3) {
      this.warningTitle = 'Interview Auto-Submitted';
      this.warningMessage = 'You switched tabs 3 times. Your interview has been automatically submitted for evaluation.';
      this.showWarningOverlay = true;
      this.autoSubmitInterview();
    }

    this.cdr.detectChanges();
  }

  /** Dismisses the warning popup (only available for warnings 1 & 2). */
  dismissWarning(): void {
    if (this.tabSwitchCount < 3) {
      this.showWarningOverlay = false;
      this.cdr.detectChanges();
    }
  }

  // ─── Auto Submit ─────────────────────────────────────────────────────────

  /** Force-submits the interview after 3 tab switches, regardless of missing answers. */
  private autoSubmitInterview(): void {
    if (this.isAutoSubmitting) return;
    this.isAutoSubmitting = true;

    this.commitPendingTranscript();
    this.saveCurrentAnswer();
    this.stopRecording();

    const payload = {
      interviewId: this.interviewId,
      answers: this.buildSubmitAnswers()
    };

    this.submitMsg = 'Auto-submitting interview due to policy violation...';
    this.errorMsg = '';
    this.cdr.detectChanges();

    this.interviewService.submitInterview(payload).subscribe({
      next: result => {
        this.submitMsg = 'Interview auto-submitted.';
        this.resultSummary = `Grade ${result.grade} — ${result.percentage.toFixed(0)}%`;
        this.cdr.detectChanges();
        setTimeout(() => {
          this.exitFullscreen();
          this.router.navigate(['/interviews', this.interviewId, 'result']);
        }, 1200);
      },
      error: () => {
        // Even on error, redirect to result page.
        this.exitFullscreen();
        this.router.navigate(['/interviews', this.interviewId, 'result']);
      }
    });
  }

  // ─── Core Session Logic (unchanged) ──────────────────────────────────────

  /** Current question convenience getter for the template. */
  get currentQuestion(): InterviewQuestion | null {
    return this.questions[this.currentQuestionIndex] || null;
  }

  /** Progress percentage used by the top progress bar. */
  get progressPercent(): number {
    if (!this.questions.length) {
      return 0;
    }

    return ((this.currentQuestionIndex + 1) / this.questions.length) * 100;
  }

  get canSubmit(): boolean {
    return this.questions.length > 0;
  }

  get missingQuestionNumbers(): number[] {
    return this.questions
      .filter(q => !(this.answers[q.id] || '').trim())
      .map(q => q.orderIndex || q.id);
  }

  /** Word count for the live transcription panel. */
  get wordCount(): number {
    return this.transcript.trim() ? this.transcript.trim().split(/\s+/).length : 0;
  }

  /** Initializes browser speech recognition when supported. */
  private setupSpeechRecognition(): void {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (!SpeechRecognition) {
      this.recognitionSupported = false;
      this.transcriptStatus = 'Speech-to-text is not supported in this browser. You can type manually.';
      return;
    }

    this.recognitionSupported = true;
    this.recognition = new SpeechRecognition();
    this.recognition.continuous = true;
    this.recognition.interimResults = true;
    this.recognition.maxAlternatives = 1;
    this.recognition.lang = 'en-US';

    this.recognition.onstart = () => {
      this.zone.run(() => {
        this.isRecording = true;
        this.manuallyStopping = false;
        this.transcriptStatus = 'Listening continuously. Speak naturally; the transcript updates live.';
        this.pendingTranscript = this.transcript;
        this.cdr.detectChanges();
      });
    };

    this.recognition.onresult = (event: any) => {
      let finalText = this.recordingBaseTranscript;
      let interimText = '';

      for (let index = 0; index < event.results.length; index++) {
        const result = event.results[index];
        if (result.isFinal) {
          finalText += `${result[0].transcript} `;
        } else {
          interimText += result[0].transcript;
        }
      }

      this.zone.run(() => {
        this.pendingTranscript = `${finalText}${interimText}`.trim();
        this.transcript = this.pendingTranscript;
        this.saveCurrentAnswer();
        this.transcriptStatus = interimText ? 'Listening continuously. Live transcript is updating.' : 'Listening continuously. Transcript saved so far.';
        this.cdr.detectChanges();
      });
    };

    this.recognition.onerror = (event: any) => {
      this.zone.run(() => {
        if (event?.error === 'no-speech' && this.shouldKeepRecording && !this.manuallyStopping) {
          this.transcriptStatus = 'Still listening. Speak when ready.';
          this.restartRecognition(250);
          return;
        }

        this.isRecording = false;
        this.shouldKeepRecording = false;
        this.releaseMicrophone();
        this.errorMsg = this.getRecognitionErrorMessage(event?.error);
        this.transcriptStatus = this.errorMsg ? 'Recording unavailable. You can type your answer manually.' : 'Recording stopped.';
        this.cdr.detectChanges();
      });
    };

    this.recognition.onend = () => {
      this.zone.run(() => {
        this.commitPendingTranscript();
        this.clearStopFallback();

        if (this.shouldKeepRecording && !this.manuallyStopping) {
          this.isRecording = false;
          this.transcriptStatus = 'Listening paused briefly... reconnecting.';
          this.cdr.detectChanges();
          this.restartRecognition(180);
          return;
        }

        this.isRecording = false;
        this.shouldKeepRecording = false;
        this.manuallyStopping = false;
        this.releaseMicrophone();
        this.transcriptStatus = 'Recording stopped.';
        this.cdr.detectChanges();
      });
    };
  }

  /** Reads the current AI question aloud using browser text-to-speech. */
  speakCurrentQuestion(): void {
    if (!this.currentQuestion || !window.speechSynthesis) {
      return;
    }

    window.speechSynthesis.cancel();
    const utterance = new SpeechSynthesisUtterance(this.currentQuestion.text);
    utterance.rate = 0.92;
    utterance.pitch = 1;
    utterance.onstart = () => this.zone.run(() => (this.isSpeaking = true));
    utterance.onend = () => this.zone.run(() => (this.isSpeaking = false));
    utterance.onerror = () => this.zone.run(() => (this.isSpeaking = false));
    window.speechSynthesis.speak(utterance);
  }

  /** Starts or stops live speech-to-text recording. */
  async toggleRecording(): Promise<void> {
    if (!this.recognitionSupported || !this.recognition) {
      this.errorMsg = 'Speech-to-text is not supported in this browser. Please type your answer.';
      this.transcriptStatus = 'Microphone unavailable in this browser.';
      this.cdr.detectChanges();
      return;
    }

    if (this.isRecording) {
      this.stopRecording();
      return;
    }

    this.errorMsg = '';
    this.shouldKeepRecording = true;
    this.manuallyStopping = false;
    this.transcriptStatus = 'Requesting microphone access...';
    this.cdr.detectChanges();

    const hasAccess = await this.ensureMicrophoneAccess();
    if (!hasAccess) {
      return;
    }

    try {
      this.recordingBaseTranscript = this.transcript ? `${this.transcript.trim()} ` : '';
      this.pendingTranscript = this.transcript;
      this.recognition.start();
    } catch (error: any) {
      this.isRecording = false;
      this.shouldKeepRecording = false;
      this.errorMsg = error?.message || 'Unable to start microphone recording.';
      this.transcriptStatus = 'Unable to start recording. You can type your answer manually.';
      this.clearStopFallback();
      this.releaseMicrophone();
      this.cdr.detectChanges();
    }
  }

  /** Stops browser speech recognition if it is active. */
  stopRecording(): void {
    this.shouldKeepRecording = false;
    this.manuallyStopping = true;

    if (!this.isRecording && !this.pendingTranscript) {
      this.transcriptStatus = 'Recording stopped.';
      this.cdr.detectChanges();
      return;
    }

    this.transcriptStatus = 'Stopping recording...';
    this.cdr.detectChanges();

    if (this.recognition) {
      try {
        this.recognition.stop();
      } catch {
        // Some engines throw if stop() is called while already idle.
      }

      this.clearStopFallback();
      this.stopFallbackTimer = setTimeout(() => {
        try {
          this.recognition.abort?.();
        } catch {
          // Ignore abort fallback errors.
        }

        this.zone.run(() => {
          this.commitPendingTranscript();
          this.isRecording = false;
          this.shouldKeepRecording = false;
          this.manuallyStopping = false;
          this.transcriptStatus = 'Recording stopped.';
          this.releaseMicrophone();
          this.cdr.detectChanges();
        });
      }, 900);
    }
  }

  /** Jumps to a specific question from the right sidebar navigator. */
  goToQuestion(index: number): void {
    if (index < 0 || index >= this.questions.length || index === this.currentQuestionIndex) {
      return;
    }

    this.saveCurrentAnswer();
    this.stopRecording();
    this.currentQuestionIndex = index;
    this.loadCurrentTranscript();
    this.speakCurrentQuestion();
    this.cdr.detectChanges();
  }

  /** Requests microphone permission without starting the transcript engine. */
  async checkMicrophoneAccess(): Promise<void> {
    this.errorMsg = '';
    this.transcriptStatus = 'Checking microphone permission...';
    this.cdr.detectChanges();

    const hasAccess = await this.ensureMicrophoneAccess();
    if (hasAccess) {
      this.transcriptStatus = 'Microphone access granted. Press Start Recording to begin.';
      this.releaseMicrophone();
      this.cdr.detectChanges();
    }
  }

  /** Saves the current textarea/transcript as the answer for the active question and persists to sessionStorage. */
  saveCurrentAnswer(): void {
    if (!this.currentQuestion) {
      return;
    }

    this.answers[this.currentQuestion.id] = this.transcript;
    // Persist answers to sessionStorage so they survive refresh/crash
    sessionStorage.setItem(`interview_answers_${this.interviewId}`, JSON.stringify(this.answers));
  }

  /** Loads a previously saved answer when moving between questions. */
  private loadCurrentTranscript(): void {
    this.transcript = this.currentQuestion ? this.answers[this.currentQuestion.id] || '' : '';
    this.pendingTranscript = this.transcript;
  }

  /** Moves to the previous question after saving the current transcript. */
  previousQuestion(): void {
    this.saveCurrentAnswer();
    this.stopRecording();
    if (this.currentQuestionIndex > 0) {
      this.currentQuestionIndex--;
      this.loadCurrentTranscript();
      this.speakCurrentQuestion();
      this.cdr.detectChanges();
    }
  }

  /** Moves to the next question after saving the current transcript. */
  nextQuestion(): void {
    this.saveCurrentAnswer();
    this.stopRecording();
    if (this.currentQuestionIndex < this.questions.length - 1) {
      this.currentQuestionIndex++;
      this.loadCurrentTranscript();
      this.speakCurrentQuestion();
      this.cdr.detectChanges();
    }
  }

  /** Builds the backend submit payload from saved transcripts. */
  private buildSubmitAnswers(): InterviewAnswerSubmission[] {
    return this.questions.map(question => ({
      questionId: question.id,
      answerText: this.answers[question.id] || ''
    }));
  }

  /** Submits every answer to the backend for persistence and scoring. */
  submitInterview(): void {
    this.commitPendingTranscript();
    this.saveCurrentAnswer();
    if (!this.canSubmit) {
      this.errorMsg = 'Cannot submit: no questions loaded.';
      return;
    }

    this.stopRecording();
    this.submitMsg = 'Submitting responses...';
    this.errorMsg = '';
    this.cdr.detectChanges();

    // Live progress updates so the user knows the app isn't frozen
    let elapsed = 0;
    const progressMessages = [
      'Submitting responses...',
      'AI is evaluating your answers...',
      'Scoring question batch 1...',
      'Scoring question batch 2...',
      'Almost done — finalizing results...',
      'Still processing — this can take up to a minute for longer interviews...',
    ];
    const progressInterval = setInterval(() => {
      elapsed++;
      const msgIndex = Math.min(elapsed, progressMessages.length - 1);
      this.submitMsg = progressMessages[msgIndex];
      this.cdr.detectChanges();
    }, 5000);

    this.interviewService.submitInterview({
      interviewId: this.interviewId,
      answers: this.buildSubmitAnswers()
    }).subscribe({
      next: result => {
        clearInterval(progressInterval);
        sessionStorage.removeItem(`interview_questions_${this.interviewId}`);
        sessionStorage.removeItem(`interview_answers_${this.interviewId}`);
        this.submitMsg = 'Interview submitted successfully.';
        this.resultSummary = `Grade ${result.grade} - ${result.percentage.toFixed(0)}%`;
        this.cdr.detectChanges();
        setTimeout(() => {
          this.exitFullscreen();
          this.router.navigate(['/interviews', this.interviewId, 'result']);
        }, 700);
      },
      error: error => {
        clearInterval(progressInterval);
        this.submitMsg = '';
        this.errorMsg = error?.error?.message || 'Unable to submit interview. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  /** Ends the session by returning to the dashboard without submitting. */
  endInterview(): void {
    this.stopRecording();
    this.exitFullscreen();
    this.router.navigate(['/user-dashboard']);
  }

  private async ensureMicrophoneAccess(): Promise<boolean> {
    if (!navigator.mediaDevices?.getUserMedia) {
      this.errorMsg = 'Microphone access is not available in this browser.';
      this.transcriptStatus = 'Microphone API unavailable.';
      this.cdr.detectChanges();
      return false;
    }

    try {
      this.releaseMicrophone();
      this.microphoneStream = await navigator.mediaDevices.getUserMedia({ audio: true });
      return true;
    } catch {
      this.errorMsg = 'Microphone permission was blocked. Please allow microphone access and try again.';
      this.transcriptStatus = 'Microphone permission blocked.';
      this.cdr.detectChanges();
      return false;
    }
  }

  private releaseMicrophone(): void {
    this.microphoneStream?.getTracks().forEach(track => track.stop());
    this.microphoneStream = null;
  }

  private commitPendingTranscript(): void {
    if (!this.pendingTranscript || !this.currentQuestion) {
      return;
    }

    this.transcript = this.pendingTranscript;
    this.saveCurrentAnswer();
  }

  private clearStopFallback(): void {
    if (this.stopFallbackTimer) {
      clearTimeout(this.stopFallbackTimer);
      this.stopFallbackTimer = null;
    }
  }

  private restartRecognition(delayMs = 0): void {
    if (!this.shouldKeepRecording || !this.recognition) {
      return;
    }

    const startAgain = () => {
      try {
        this.recordingBaseTranscript = this.pendingTranscript ? `${this.pendingTranscript.trim()} ` : '';
        this.recognition.start();
      } catch {
        this.isRecording = false;
        this.transcriptStatus = 'Listening paused. Tap Start Recording to continue.';
        this.cdr.detectChanges();
      }
    };

    delayMs > 0 ? setTimeout(startAgain, delayMs) : startAgain();
  }

  private getRecognitionErrorMessage(errorCode?: string): string {
    switch (errorCode) {
      case 'not-allowed':
      case 'service-not-allowed':
        return 'Microphone permission was blocked. Please allow microphone access and try again.';
      case 'audio-capture':
        return 'No microphone was detected. Please check your microphone and try again.';
      case 'network':
        return 'Speech recognition had a network issue. Please try again.';
      case 'no-speech':
        return 'No speech was detected. Try speaking a little louder and closer to the microphone.';
      case 'aborted':
        return '';
      default:
        return 'Microphone access failed. You can type your answer manually.';
    }
  }
}
