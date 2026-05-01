import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { InterviewResult } from '../../../models/interview.model';
import { InterviewService } from '../../../services/interview.service';

/**
 * Purpose: Shows the final interview result after backend scoring.
 * Free users see the summary/upgrade state; premium users see detailed AI analysis.
 */
@Component({
  selector: 'app-interview-result',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './interview-result.component.html',
  styleUrl: './interview-result.component.css'
})
export class InterviewResultComponent implements OnInit, OnDestroy {
  interviewId = 0;
  result: InterviewResult | null = null;
  isLoading = true;
  errorMsg = '';
  private loadingGuardTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private interviewService: InterviewService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.interviewId = Number(params.get('id'));
      this.loadResult();
    });
  }

  ngOnDestroy(): void {
    this.clearLoadingGuard();
  }

  get isPremiumResult(): boolean {
    return this.result?.isPremiumResult === true;
  }

  get scorePercent(): number {
    return Math.max(0, Math.min(100, this.result?.percentage ?? 0));
  }

  get dashOffset(): number {
    const circumference = 603;
    return circumference - (circumference * this.scorePercent) / 100;
  }

  get resultTitle(): string {
    return this.isPremiumResult ? 'Assessment Complete.' : 'Performance Summary';
  }

  get reviewLabel(): string {
    return this.isPremiumResult ? 'Premium Review' : 'Standard Class';
  }

  loadResult(): void {
    this.isLoading = true;
    this.errorMsg = '';
    this.result = null;
    this.startLoadingGuard();

    this.interviewService.getInterviewResult(this.interviewId).pipe(
      finalize(() => {
        this.isLoading = false;
        this.clearLoadingGuard();
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: result => {
        this.result = result;
        this.cdr.detectChanges();
      },
      error: error => {
        this.errorMsg = error?.error?.message || error?.message || 'Unable to load the interview result right now.';
        this.cdr.detectChanges();
      }
    });
  }

  startNewInterview(): void {
    this.router.navigate(['/interviews/start']);
  }

  viewDashboard(): void {
    this.router.navigate(['/user-dashboard']);
  }

  private startLoadingGuard(): void {
    this.clearLoadingGuard();
    this.loadingGuardTimer = setTimeout(() => {
      if (!this.isLoading) {
        return;
      }

      this.isLoading = false;
      this.errorMsg = 'Result request is taking too long. Refresh this page or open View Dashboard to see the completed session.';
      this.cdr.detectChanges();
    }, 12000);
  }

  private clearLoadingGuard(): void {
    if (this.loadingGuardTimer) {
      clearTimeout(this.loadingGuardTimer);
      this.loadingGuardTimer = null;
    }
  }
}
