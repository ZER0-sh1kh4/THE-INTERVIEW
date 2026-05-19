import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AssessmentService } from './assessment.service';
import { environment } from '../../environments/environment';

describe('AssessmentService', () => {
  let service: AssessmentService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/assessments`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AssessmentService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AssessmentService);
    httpMock = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getMyAssessments()', () => {
    it('should GET and return normalized assessment results', () => {
      service.getMyAssessments().subscribe(results => {
        expect(results.length).toBe(1);
        expect(results[0].domain).toBe('Angular');
        expect(results[0].grade).toBe('A');
      });
      httpMock.expectOne(apiUrl).flush({ success: true, message: '', data: [
        { assessmentId: 1, domain: 'Angular', score: 9, maxScore: 10, percentage: 90, grade: 'A' }
      ]});
    });

    it('should return cached results without re-fetching', () => {
      service.getMyAssessments().subscribe();
      httpMock.expectOne(apiUrl).flush({ success: true, message: '', data: [
        { assessmentId: 1, domain: 'Cached', score: 5, maxScore: 10, percentage: 50, grade: 'C' }
      ]});

      service.getMyAssessments().subscribe(results => {
        expect(results[0].domain).toBe('Cached');
      });
      httpMock.expectNone(apiUrl);
    });
  });

  describe('getCachedAssessments()', () => {
    it('should return empty array when nothing is cached', () => {
      expect(service.getCachedAssessments()).toEqual([]);
    });

    it('should return data from sessionStorage', () => {
      sessionStorage.setItem('the_interview_my_assessments',
        JSON.stringify([{ assessmentId: 5, domain: 'React', score: 7, maxScore: 10, percentage: 70, grade: 'B' }])
      );
      expect(service.getCachedAssessments().length).toBe(1);
      expect(service.getCachedAssessments()[0].domain).toBe('React');
    });

    it('should return empty array for invalid JSON', () => {
      sessionStorage.setItem('the_interview_my_assessments', 'broken');
      expect(service.getCachedAssessments()).toEqual([]);
    });
  });

  describe('startAssessment()', () => {
    it('should POST and return the unwrapped response', () => {
      service.startAssessment({ domain: 'Angular', questionCount: 10, difficulty: 'Medium' }).subscribe(res => {
        expect(res.assessmentId).toBe(100);
      });
      const req = httpMock.expectOne(`${apiUrl}/start`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: {
        assessmentId: 100, timeLimitMinutes: 30, expiresAt: '', questions: [], totalExpected: 10, hasMore: false
      }});
    });
  });

  describe('submitAssessment()', () => {
    it('should POST answers', () => {
      service.submitAssessment({ assessmentId: 100, answers: [{ questionId: 1, selectedOption: 'A' }] }).subscribe();
      const req = httpMock.expectOne(`${apiUrl}/submit`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: { score: 8 } });
    });
  });

  describe('getNextBatch()', () => {
    it('should GET with correct query params', () => {
      service.getNextBatch(100, 3, 3).subscribe(questions => {
        expect(questions.length).toBe(2);
      });
      httpMock.expectOne(`${apiUrl}/100/next-batch?currentCount=3&batchSize=3`).flush({
        success: true, message: '', data: [
          { id: 4, text: 'Q4', optionA: 'A', optionB: 'B', optionC: 'C', optionD: 'D', orderIndex: 3 },
          { id: 5, text: 'Q5', optionA: 'A', optionB: 'B', optionC: 'C', optionD: 'D', orderIndex: 4 }
        ]
      });
    });
  });
});
