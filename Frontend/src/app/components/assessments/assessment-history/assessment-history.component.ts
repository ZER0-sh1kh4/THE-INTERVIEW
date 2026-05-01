import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AssessmentService } from '../../../services/assessment.service';
import { AuthService } from '../../../services/auth.service';
import { AssessmentResult } from '../../../models/assessment.model';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

interface AssessmentHistoryRow {
  id: number;
  domain: string;
  scoreDisplay: string;
  percentage: number;
  classification: string;
  actionLabel: string;
  actionLink: string;
}

@Component({
  selector: 'app-assessment-history',
  standalone: true,
  imports: [CommonModule, RouterModule, UserNavbarComponent],
  templateUrl: './assessment-history.component.html',
  styleUrls: ['./assessment-history.component.css']
})
export class AssessmentHistoryComponent implements OnInit {
  assessments: AssessmentResult[] = [];
  visibleRows: AssessmentHistoryRow[] = [];
  isLoading = true;
  isPremium = false;
  errorMsg = '';

  constructor(
    private assessmentService: AssessmentService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user: any) => {
      this.isPremium = user?.isPremium || false;
    });

    const cached = this.assessmentService.getCachedAssessments();
    if (cached.length > 0) {
      this.applyAssessments(cached);
      this.isLoading = false;
    }

    this.loadHistory();
  }

  loadHistory(): void {
    if (!this.assessments.length) {
      this.isLoading = true;
    }
    this.errorMsg = '';
    this.assessmentService.getMyAssessments().subscribe({
      next: (results) => {
        this.applyAssessments(results);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        if (!this.assessments.length) {
            this.errorMsg = 'Unable to load assessment history right now.';
        }
        this.cdr.detectChanges();
      }
    });
  }

  get totalCount(): number {
    return this.assessments.length;
  }

  get averageScore(): string {
    if (!this.assessments.length) return '-';
    const average = this.assessments.reduce((sum, a) => sum + a.percentage, 0) / this.assessments.length;
    return `${average.toFixed(0)}%`;
  }

  get bestDomain(): string {
    if (!this.assessments.length) return '-';
    return this.assessments.reduce((best, a) => a.percentage > best.percentage ? a : best).domain;
  }

  private applyAssessments(assessments: AssessmentResult[]): void {
    this.assessments = assessments;
    this.visibleRows = this.assessments.map(a => ({
        id: a.assessmentId,
        domain: a.domain,
        scoreDisplay: `${a.score} / ${a.maxScore}`,
        percentage: a.percentage,
        classification: this.getClassification(a.percentage),
        actionLabel: 'View Result',
        actionLink: `/assessments/${a.assessmentId}/result`
    }));
  }

  getClassification(percentage: number): string {
    if (percentage >= 90) return 'ELITE';
    if (percentage >= 75) return 'PROFICIENT';
    if (percentage >= 60) return 'MODERATE';
    if (percentage >= 40) return 'DEVELOPING';
    return 'NEEDS WORK';
  }

  takeNewAssessment() {
    this.router.navigate(['/assessments/domain']);
  }
}
