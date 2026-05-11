import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, NgZone, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { InterviewAnswerSubmission, InterviewQuestion } from '../../../models/interview.model';
import { InterviewService } from '../../../services/interview.service';

/**
 * Purpose: Runs the live AI mock interview session.
 * All questions are displayed on a single scrollable page.
 * Supports mute/unmute for AI voice, per-question recording, and sidebar submit progress.
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
  activeQuestionIndex = 0;
  answers: Record<number, string> = {};
  transcript = '';
  transcriptStatus = 'Microphone ready.';
  recognitionSupported = false;

  isLoading = true;
  isRecording = false;
  isSpeaking = false;
  isSpeakingIndex = -1;
  isMuted = false;
  isSubmitting = false;
  submitProgressMsg = 'Submitting responses...';
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
          if (!this.isMuted) {
            setTimeout(() => this.speakQuestion(0), 500);
          }
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

  /** Calls the backend for session questions and triggers background fetch for remaining. */
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
          const totalExpected = session.totalExpected || this.questions.length;

          // Cache to sessionStorage for refresh resilience
          if (this.questions.length > 0) {
            sessionStorage.setItem(`interview_questions_${this.interviewId}`, JSON.stringify(this.questions));
          }
          if (this.questions.length > 0 && !this.isMuted) {
            setTimeout(() => this.speakQuestion(0), 500);
          } else if (this.questions.length === 0) {
            this.errorMsg = 'Gemini did not return questions for this attempt. Click Generate Questions Again in a moment.';
          }

          // LAZY LOADING: If we don't have all questions yet, fetch the rest in background
          if (this.questions.length < totalExpected) {
            this.fetchRemainingQuestions();
          }

          this.cdr.detectChanges();
        });
      },
      error: error => {
        this.interviewService.getInterview(this.interviewId).subscribe({
          next: fallbackSession => {
            this.zone.run(() => {
              this.questions = fallbackSession.questions || [];
              if (this.questions.length > 0) {
                sessionStorage.setItem(`interview_questions_${this.interviewId}`, JSON.stringify(this.questions));
                this.isLoading = false;
                if (!this.isMuted) {
                  setTimeout(() => this.speakQuestion(0), 500);
                }
                const totalExpected = fallbackSession.totalExpected || this.questions.length;
                if (this.questions.length < totalExpected) {
                  this.fetchRemainingQuestions();
                }
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

  /** Silently fetches remaining questions in the background while the user answers the first batch. */
  private fetchRemainingQuestions(): void {
    this.interviewService.fetchMoreQuestions(this.interviewId).subscribe({
      next: session => {
        this.zone.run(() => {
          if (session.questions && session.questions.length > this.questions.length) {
            this.questions = session.questions;
            sessionStorage.setItem(`interview_questions_${this.interviewId}`, JSON.stringify(this.questions));
          }
          this.cdr.detectChanges();
        });
      },
      error: err => {
        console.warn('Background fetch-more failed:', err);
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

  // ─── Mute/Unmute ───────────────────────────────────────────────────────

  /** Toggles mute state for AI voice. Stops any current speech if muting. */
  toggleMute(): void {
    this.isMuted = !this.isMuted;
    if (this.isMuted) {
      window.speechSynthesis?.cancel();
      this.isSpeaking = false;
      this.isSpeakingIndex = -1;
    }
    this.cdr.detectChanges();
  }

  // ─── Single-Page Q&A Helpers ───────────────────────────────────────────

  nextQuestion(): void {
    if (this.activeQuestionIndex < this.questions.length - 1) {
      window.speechSynthesis?.cancel();
      this.isSpeaking = false;
      this.isSpeakingIndex = -1;
      this.activeQuestionIndex++;
      this.cdr.detectChanges();
      
      // Auto-speak the new question
      setTimeout(() => this.speakQuestion(this.activeQuestionIndex), 100);
    }
  }

  previousQuestion(): void {
    if (this.activeQuestionIndex > 0) {
      window.speechSynthesis?.cancel();
      this.isSpeaking = false;
      this.isSpeakingIndex = -1;
      this.activeQuestionIndex--;
      this.cdr.detectChanges();
      
      // Auto-speak the new question
      setTimeout(() => this.speakQuestion(this.activeQuestionIndex), 100);
    }
  }

  /** Sets the active question index (used when focusing a textarea). */
  setActiveQuestion(index: number): void {
    if (this.activeQuestionIndex !== index) {
      window.speechSynthesis?.cancel();
      this.isSpeaking = false;
      this.isSpeakingIndex = -1;
    }
    this.activeQuestionIndex = index;
    this.cdr.detectChanges();
  }

  /** Called by the template when a user types in any question's textarea. */
  onAnswerChange(questionId: number, value: string): void {
    this.answers[questionId] = value;
    sessionStorage.setItem(`interview_answers_${this.interviewId}`, JSON.stringify(this.answers));
  }

  /** Returns word count for a given question ID. */
  getWordCount(questionId: number): number {
    const text = (this.answers[questionId] || '').trim();
    return text ? text.split(/\s+/).length : 0;
  }

  /** Overall progress based on answered questions. */
  get overallProgress(): number {
    if (!this.questions.length) return 0;
    return (this.answeredCount / this.questions.length) * 100;
  }

  /** Count of answered questions. */
  get answeredCount(): number {
    return this.questions.filter(q => (this.answers[q.id] || '').trim()).length;
  }

  get missingQuestionNumbers(): number[] {
    return this.questions
      .filter(q => !(this.answers[q.id] || '').trim())
      .map((q, _, arr) => this.questions.indexOf(q) + 1);
  }

  /** Scrolls to a question block from the sidebar navigator. */
  scrollToQuestion(index: number): void {
    if (this.activeQuestionIndex !== index) {
      window.speechSynthesis?.cancel();
      this.isSpeaking = false;
      this.isSpeakingIndex = -1;
    }
    this.activeQuestionIndex = index;
    const el = document.getElementById('q-' + index);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
    this.cdr.detectChanges();
  }

  // ─── Fullscreen Management ───────────────────────────────────────────────

  /** Requests browser fullscreen on session entry. */
  private enterFullscreen(): void {
    const docEl = document.documentElement;
    const requestFs = docEl.requestFullscreen
      || (docEl as any).webkitRequestFullscreen
      || (docEl as any).msRequestFullscreen;

    if (requestFs) {
      requestFs.call(docEl).catch(() => {});
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

    this.stopRecording();

    const payload = {
      interviewId: this.interviewId,
      answers: this.buildSubmitAnswers()
    };

    this.isSubmitting = true;
    this.submitProgressMsg = 'Auto-submitting due to policy violation...';
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
        this.exitFullscreen();
        this.router.navigate(['/interviews', this.interviewId, 'result']);
      }
    });
  }

  // ─── Speech TTS ──────────────────────────────────────────────────────────

  /** Reads a specific question aloud using browser text-to-speech. Respects mute state. */
  speakQuestion(index: number): void {
    if (this.isMuted || !window.speechSynthesis) return;

    // Toggle behavior: if already speaking THIS question, stop it.
    if (this.isSpeaking && this.isSpeakingIndex === index) {
      window.speechSynthesis.cancel();
      this.isSpeaking = false;
      this.isSpeakingIndex = -1;
      return;
    }

    const question = this.questions[index];
    if (!question) return;

    window.speechSynthesis.cancel();
    const utterance = new SpeechSynthesisUtterance(question.text);
    utterance.rate = 0.92;
    utterance.pitch = 1;
    utterance.onstart = () => this.zone.run(() => { this.isSpeaking = true; this.isSpeakingIndex = index; });
    utterance.onend = () => this.zone.run(() => { this.isSpeaking = false; this.isSpeakingIndex = -1; });
    utterance.onerror = () => this.zone.run(() => { this.isSpeaking = false; this.isSpeakingIndex = -1; });
    window.speechSynthesis.speak(utterance);
  }

  // Keep old method name working for backward compatibility
  speakCurrentQuestion(): void {
    this.speakQuestion(this.activeQuestionIndex);
  }

  // ─── Speech STT (Recording) ──────────────────────────────────────────────

  /** Toggles recording for a specific question. */
  toggleRecordingFor(index: number): void {
    // If recording a different question, stop first
    if (this.isRecording && this.activeQuestionIndex !== index) {
      this.stopRecording();
    }

    this.activeQuestionIndex = index;
    this.transcript = this.answers[this.questions[index]?.id] || '';
    this.toggleRecording();
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
        this.transcriptStatus = 'Listening continuously.';
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
        // Save to the active question
        const activeQ = this.questions[this.activeQuestionIndex];
        if (activeQ) {
          this.answers[activeQ.id] = this.transcript;
          sessionStorage.setItem(`interview_answers_${this.interviewId}`, JSON.stringify(this.answers));
        }
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
          this.transcriptStatus = 'Reconnecting...';
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

  /** Starts or stops live speech-to-text recording. */
  async toggleRecording(): Promise<void> {
    if (!this.recognitionSupported || !this.recognition) {
      this.errorMsg = 'Speech-to-text is not supported in this browser. Please type your answer.';
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

  // ─── Submit ─────────────────────────────────────────────────────────────

  get canSubmit(): boolean {
    return this.questions.length > 0;
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
    if (!this.canSubmit) {
      this.errorMsg = 'Cannot submit: no questions loaded.';
      return;
    }

    this.stopRecording();
    this.isSubmitting = true;
    this.submitProgressMsg = 'Submitting responses...';
    this.errorMsg = '';
    this.cdr.detectChanges();

    // Live progress updates shown in the sidebar
    let elapsed = 0;
    const progressMessages = [
      'Submitting responses...',
      'AI is evaluating your answers...',
      'Scoring question batch 1...',
      'Scoring question batch 2...',
      'Almost done — finalizing results...',
      'Still processing — this can take up to a minute...',
    ];
    const progressInterval = setInterval(() => {
      elapsed++;
      const msgIndex = Math.min(elapsed, progressMessages.length - 1);
      this.submitProgressMsg = progressMessages[msgIndex];
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
        this.submitProgressMsg = `Done! Grade ${result.grade} — ${result.percentage.toFixed(0)}%`;
        this.submitMsg = 'submitted';
        this.cdr.detectChanges();
        setTimeout(() => {
          this.exitFullscreen();
          this.router.navigate(['/interviews', this.interviewId, 'result']);
        }, 700);
      },
      error: error => {
        clearInterval(progressInterval);
        this.isSubmitting = false;
        this.submitProgressMsg = '';
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

  // ─── Private Helpers ────────────────────────────────────────────────────

  private async ensureMicrophoneAccess(): Promise<boolean> {
    if (!navigator.mediaDevices?.getUserMedia) {
      this.errorMsg = 'Microphone access is not available in this browser.';
      this.cdr.detectChanges();
      return false;
    }

    try {
      this.releaseMicrophone();
      this.microphoneStream = await navigator.mediaDevices.getUserMedia({ audio: true });
      return true;
    } catch {
      this.errorMsg = 'Microphone permission was blocked. Please allow microphone access and try again.';
      this.cdr.detectChanges();
      return false;
    }
  }

  private releaseMicrophone(): void {
    this.microphoneStream?.getTracks().forEach(track => track.stop());
    this.microphoneStream = null;
  }

  private commitPendingTranscript(): void {
    if (!this.pendingTranscript) return;

    this.transcript = this.pendingTranscript;
    const activeQ = this.questions[this.activeQuestionIndex];
    if (activeQ) {
      this.answers[activeQ.id] = this.transcript;
      sessionStorage.setItem(`interview_answers_${this.interviewId}`, JSON.stringify(this.answers));
    }
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
        this.transcriptStatus = 'Listening paused. Tap Record to continue.';
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
