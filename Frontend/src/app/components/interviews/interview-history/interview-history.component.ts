import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { Interview, InterviewResult } from '../../../models/interview.model';
import { InterviewService } from '../../../services/interview.service';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

interface InterviewHistoryRow {
  id: number;
  title: string;
  domain: string;
  type: string;
  status: string;
  date: string;
  grade: string;
  percentage: number | null;
  actionLabel: string;
  actionLink: string;
}

/**
 * Purpose: Displays all previous interview sessions in an archive-style page.
 * It adapts the supplied interview_history UI into this project's existing editorial theme.
 */
@Component({
  selector: 'app-interview-history',
  standalone: true,
  imports: [CommonModule, RouterModule, UserNavbarComponent],
  providers: [DatePipe],
  templateUrl: './interview-history.component.html',
  styleUrl: './interview-history.component.css'
})
export class InterviewHistoryComponent implements OnInit {
  interviews: Interview[] = [];
  rows: InterviewHistoryRow[] = [];
  visibleRows: InterviewHistoryRow[] = [];
  selectedStatus = 'All';
  isLoading = true;
  errorMsg = '';

  statusOptions = ['All', 'Pending', 'In Progress', 'Completed'];

  constructor(
    private interviewService: InterviewService,
    private datePipe: DatePipe,
    private router: Router
  ) {}

  ngOnInit(): void {
    const cached = this.interviewService.getCachedInterviews();
    if (cached.length) {
      this.applyInterviews(cached);
      this.isLoading = false;
    }

    this.loadHistory();
  }

  get totalCount(): number {
    return this.interviews.length;
  }

  get completedCount(): number {
    return this.interviews.filter(interview => interview.status.toLowerCase() === 'completed').length;
  }

  get averageScore(): string {
    const scoredRows = this.rows.filter(row => row.percentage !== null);
    if (!scoredRows.length) {
      return '-';
    }

    const average = scoredRows.reduce((sum, row) => sum + (row.percentage || 0), 0) / scoredRows.length;
    return `${average.toFixed(0)}%`;
  }

  get inProgressCount(): number {
    return this.interviews.filter(interview => interview.status.toLowerCase() === 'inprogress').length;
  }

  loadHistory(): void {
    if (!this.interviews.length) {
      this.isLoading = true;
    }
    this.errorMsg = '';

    this.interviewService.getMyInterviews().pipe(
      catchError(() => {
        const cached = this.interviewService.getCachedInterviews();
        if (cached.length) {
          return of(cached);
        }

        this.errorMsg = 'Unable to load interview history right now.';
        return of([] as Interview[]);
      })
    ).subscribe(interviews => {
      this.applyInterviews(interviews);
      this.isLoading = false;
      this.loadCompletedResults(interviews);
    });
  }

  setStatus(status: string): void {
    this.selectedStatus = status;
    this.applyFilter();
  }

  startNewInterview(): void {
    this.router.navigate(['/interviews/start']);
  }

  private applyInterviews(interviews: Interview[]): void {
    this.interviews = [...interviews].sort((a, b) =>
      new Date(b.completedAt || b.startedAt || b.createdAt).getTime() -
      new Date(a.completedAt || a.startedAt || a.createdAt).getTime()
    );
    this.rows = this.interviews.map(interview => this.mapRow(interview));
    this.applyFilter();
  }

  private applyFilter(): void {
    this.visibleRows = this.selectedStatus === 'All'
      ? this.rows
      : this.rows.filter(row => {
          const s1 = row.status.toLowerCase().replace(/\s/g, '');
          const s2 = this.selectedStatus.toLowerCase().replace(/\s/g, '');
          return s1 === s2;
        });
  }

  private loadCompletedResults(interviews: Interview[]): void {
    const completed = interviews.filter(interview => interview.status.toLowerCase() === 'completed');
    if (!completed.length) {
      return;
    }

    forkJoin(
      completed.map(interview =>
        this.interviewService.getInterviewResult(interview.id).pipe(
          catchError(() => of(null as InterviewResult | null))
        )
      )
    ).subscribe(results => {
      const resultByInterviewId = new Map<number, InterviewResult>();
      completed.forEach((interview, index) => {
        const result = results[index];
        if (result) {
          resultByInterviewId.set(interview.id, result);
        }
      });

      this.rows = this.rows.map(row => {
        const result = resultByInterviewId.get(row.id);
        return result
          ? { ...row, grade: result.grade, percentage: result.percentage }
          : row;
      });
      this.applyFilter();
    });
  }

  private mapRow(interview: Interview): InterviewHistoryRow {
    const status = interview.status || 'Pending';
    const isCompleted = status.toLowerCase() === 'completed';
    const isPending = status.toLowerCase() === 'pending';

    return {
      id: interview.id,
      title: interview.title || 'Interview Session',
      domain: this.getDomainLabel(interview),
      type: interview.type || 'Interview',
      status,
      date: this.datePipe.transform(interview.completedAt || interview.startedAt || interview.createdAt, 'MMM d, y') || '-',
      grade: isCompleted ? '-' : 'N/A',
      percentage: null,
      actionLabel: isCompleted ? 'View Result' : isPending ? 'Begin' : 'Continue',
      actionLink: isCompleted ? `/interviews/${interview.id}/result` : `/interviews/${interview.id}/session`
    };
  }

  private getDomainLabel(interview: Interview): string {
    const parts = (interview.domain || '').split('|').map(part => part.trim()).filter(Boolean);
    return parts.length ? parts.slice(0, 3).join(' / ') : interview.domain || 'General Interview';
  }
}
