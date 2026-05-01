import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AssessmentService } from '../../../services/assessment.service';
import { AuthService } from '../../../services/auth.service';
import { UserNavbarComponent } from '../../shared/user-navbar/user-navbar.component';

@Component({
  selector: 'app-assessment-result',
  standalone: true,
  imports: [CommonModule, UserNavbarComponent],
  templateUrl: './assessment-result.component.html',
  styleUrls: ['./assessment-result.component.css']
})
export class AssessmentResultComponent implements OnInit {
  assessmentId!: number;
  resultData: any = null;
  isPremium = false;
  isLoading = true;
  errorMsg = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private assessmentService: AssessmentService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.assessmentId = +this.route.snapshot.params['id'];
    console.log('[ResultComponent] Init for assessment ID:', this.assessmentId);

    this.authService.currentUser$.subscribe((user: any) => {
      this.isPremium = user?.isPremium === true || user?.isPremium === 'true' || user?.isPremium === 'True';
      this.cdr.detectChanges();
    });

    if (this.assessmentId) {
      this.fetchResult();
    } else {
      this.isLoading = false;
      this.errorMsg = 'No assessment ID found.';
      this.cdr.detectChanges();
    }
  }

  fetchResult() {
    this.isLoading = true;
    this.errorMsg = '';
    this.cdr.detectChanges();

    this.assessmentService.getAssessmentResult(this.assessmentId).subscribe({
      next: (res) => {
        console.log('[ResultComponent] Raw response:', JSON.stringify(res));

        if (!res) {
          this.errorMsg = 'No result data was returned by the server.';
          this.isLoading = false;
          this.cdr.detectChanges();
          return;
        }

        this.resultData = {
          score: res.score ?? res.Score ?? 0,
          maxScore: res.maxScore ?? res.MaxScore ?? 0,
          percentage: res.percentage ?? res.Percentage ?? 0,
          grade: res.grade ?? res.Grade ?? '-',
          wrongQuestionIds: res.wrongQuestionIds ?? res.WrongQuestionIds ?? [],
          feedback: res.feedback ?? res.Feedback ?? '',
          details: (res.answerReview ?? res.AnswerReview ?? res.breakdown ?? res.Breakdown ?? []).map((d: any) => ({
            text: d.text ?? d.Text ?? d.questionText ?? '',
            subtopic: d.subtopic ?? d.Subtopic ?? '',
            selectedOption: d.selectedOption ?? d.SelectedOption ?? d.yourAnswer ?? d.YourAnswer ?? '',
            correctOption: d.correctOption ?? d.CorrectOption ?? d.correctAnswer ?? d.CorrectAnswer ?? '',
            isCorrect: d.isCorrect ?? d.IsCorrect ?? false,
            score: d.score ?? d.Score ?? 0
          })),
          weakAreas: res.weakAreas ?? res.WeakAreas ?? [],
          strengths: res.strengths ?? res.Strengths ?? [],
          suggestions: res.suggestions ?? res.Suggestions ?? []
        };

        console.log('[ResultComponent] Normalized:', this.resultData);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('[ResultComponent] Error:', err);
        this.errorMsg = err?.error?.message || err?.message || 'Failed to load result.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getGradeClass(): string {
    const g = this.resultData?.grade;
    if (g === 'A+' || g === 'A') return 'grade-a';
    if (g === 'B') return 'grade-b';
    if (g === 'C') return 'grade-c';
    return 'grade-f';
  }

  getMasteryTitle(): string {
    const p = this.resultData?.percentage ?? 0;
    if (p >= 90) return 'Exceptional Strategic Mastery';
    if (p >= 75) return 'Strong Proficiency';
    if (p >= 60) return 'Moderate Performance';
    if (p >= 40) return 'Developing Competency';
    return 'Needs Significant Improvement';
  }

  getMasteryDescription(): string {
    const p = this.resultData?.percentage ?? 0;
    if (p >= 90) return 'Your performance reflects a high degree of cognitive agility and domain mastery. You successfully navigated complex questions with focus on measurable outcomes.';
    if (p >= 75) return 'You demonstrate strong competency across most areas. Minor refinements in specific subtopics will elevate your performance to elite levels.';
    if (p >= 60) return 'Your foundational understanding is solid but several areas need deeper exploration. Focus on the weak areas identified below.';
    return 'Significant gaps exist in your knowledge. Focused study on the refinement areas will yield substantial improvements.';
  }

  getCorrectDetails(): any[] {
    return (this.resultData?.details || []).filter((d: any) => d.isCorrect);
  }

  upgradeToPremium() {
    this.router.navigate(['/premium']);
  }

  goBack() {
    this.router.navigate(['/assessments/domain']);
  }

  goToHistory() {
    this.router.navigate(['/assessments/history']);
  }
}
