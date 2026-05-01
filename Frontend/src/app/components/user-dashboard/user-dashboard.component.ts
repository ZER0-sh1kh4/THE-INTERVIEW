import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { catchError, forkJoin, Observable, of, filter, take } from 'rxjs';
import { AssessmentResult } from '../../models/assessment.model';
import { Interview, InterviewResult } from '../../models/interview.model';
import { User } from '../../models/user.model';
import { AssessmentService } from '../../services/assessment.service';
import { AuthService } from '../../services/auth.service';
import { InterviewService } from '../../services/interview.service';

interface DashboardStat {
  label: string;
  value: string;
}

interface RecentInterviewRow {
  domain: string;
  status: string;
  date: string;
  link?: string;
}

interface RecentAssessmentRow {
  domain: string;
  grade: string;
  date: string;
}

/**
 * Purpose: Candidate dashboard that stays synced with auth, interview, and assessment APIs.
 * It refreshes on every dashboard visit so returning from result pages shows current counts.
 */
import { UserNavbarComponent } from '../shared/user-navbar/user-navbar.component';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, UserNavbarComponent],
  providers: [DatePipe],
  templateUrl: './user-dashboard.component.html',
  styleUrl: './user-dashboard.component.css'
})
export class UserDashboardComponent implements OnInit {
  currentUser$: Observable<User | null | undefined>;
  stats: DashboardStat[] = [
    { label: 'Completed Interviews', value: '...' },
    { label: 'Completed Assessments', value: '...' },
    { label: 'Performance Grade', value: '...' },
    { label: 'Premium Status', value: '...' }
  ];

  recentInterviews: RecentInterviewRow[] = [
    { domain: 'No interviews yet', status: 'Start practice', date: '-' }
  ];

  recentAssessments: RecentAssessmentRow[] = [
    { domain: 'No assessments yet', grade: '-', date: '-' }
  ];

  constructor(
    private authService: AuthService,
    private interviewService: InterviewService,
    private assessmentService: AssessmentService,
    private datePipe: DatePipe,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.stats[3].value = this.getPlanLabel(user);
      }
    });

    this.authService.currentUser$.pipe(
      filter(user => !!user),
      take(1)
    ).subscribe(() => {
      this.loadDashboardData();
    });
  }

  getDisplayName(user: User | null): string {
    if (!user) return 'Candidate';
    if (user.fullName) return user.fullName;
    if (user.email) return user.email.split('@')[0].replace(/[._-]+/g, ' ');
    return 'Candidate';
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 17) return 'Good afternoon';
    return 'Good evening';
  }

  getPlanLabel(user: User | null): string {
    return user?.isPremium === true ? 'Premium' : 'Free';
  }

  getStatusLink(row: RecentInterviewRow): string | null {
    return row.link || null;
  }

  private loadDashboardData(): void {
    this.interviewService.getMyInterviews(true).pipe(
      catchError(() => of([] as Interview[]))
    ).subscribe(interviews => {
      this.applyInterviewActivity(interviews);
      this.cdr.detectChanges();
    });

    this.assessmentService.getMyAssessments(true).pipe(
      catchError(() => of([] as AssessmentResult[]))
    ).subscribe(assessments => {
      this.applyAssessmentActivity(assessments);
      this.cdr.detectChanges();
    });
  }

  private applyInterviewActivity(interviews: Interview[]): void {
    if (!interviews || interviews.length === 0) {
      this.recentInterviews = [{ domain: 'No interviews yet', status: 'Start practice', date: '-' }];
      this.stats[0].value = '0';
      this.stats[2].value = '-';
      return;
    }

    const sorted = [...interviews].sort((a, b) =>
      new Date(b.completedAt || b.startedAt || b.createdAt).getTime() -
      new Date(a.completedAt || a.startedAt || a.createdAt).getTime()
    );

    this.recentInterviews = sorted.slice(0, 5).map(interview => this.mapInterviewRow(interview));

    const completed = sorted.filter(interview => interview.status.toLowerCase() === 'completed');
    this.stats[0].value = String(completed.length);

    if (!completed.length) {
      this.stats[2].value = '-';
      return;
    }

    forkJoin(
      completed.map(interview =>
        this.interviewService.getInterviewResult(interview.id).pipe(
          catchError(() => of(null as InterviewResult | null))
        )
      )
    ).subscribe(results => {
      const grades = results
        .filter((result): result is InterviewResult => !!result)
        .map(result => result.grade);

      this.stats[2].value = this.pickBestGrade(grades);
      this.cdr.detectChanges();
    });
  }

  private applyAssessmentActivity(assessments: AssessmentResult[]): void {
    if (!assessments || assessments.length === 0) {
      this.recentAssessments = [{ domain: 'No assessments yet', grade: '-', date: '-' }];
      this.stats[1].value = '0';
      return;
    }

    const sorted = [...assessments].sort((a, b) => b.assessmentId - a.assessmentId);
    this.recentAssessments = sorted.slice(0, 5).map(assessment => ({
      domain: assessment.domain,
      grade: assessment.grade,
      date: assessment.percentage ? `${assessment.percentage.toFixed(0)}%` : '-'
    }));

    this.stats[1].value = String(assessments.length);
  }

  private mapInterviewRow(interview: Interview): RecentInterviewRow {
    const link = interview.status.toLowerCase() === 'completed'
      ? `/interviews/${interview.id}/result`
      : `/interviews/${interview.id}/session`;

    return {
      domain: interview.title || interview.domain,
      status: interview.status,
      date: this.datePipe.transform(interview.completedAt || interview.startedAt || interview.createdAt, 'dd MMM yyyy') || '-',
      link
    };
  }

  private pickBestGrade(grades: string[]): string {
    const rank = ['A+', 'A', 'B', 'C', 'D', 'F'];
    const sortedGrades = grades
      .filter(grade => rank.includes(grade))
      .sort((a, b) => rank.indexOf(a) - rank.indexOf(b));

    return sortedGrades[0] || '-';
  }
}
