import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { InterviewService } from './interview.service';
import { environment } from '../../environments/environment';

describe('InterviewService', () => {
  let service: InterviewService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/interviews`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [InterviewService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(InterviewService);
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

  describe('startInterview()', () => {
    it('should POST the form and return a normalized Interview', () => {
      const form = {
        role: 'Frontend Developer', experience: '2 Years',
        interviewType: 'Technical' as const, techStack: ['Angular', 'TypeScript'],
        difficulty: 'Medium' as const, numberOfQuestions: 10
      };

      service.startInterview(form).subscribe(interview => {
        expect(interview.id).toBe(42);
        expect(interview.status).toBe('Pending');
      });

      const req = httpMock.expectOne(`${apiUrl}/start`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: {
        id: 42, userId: 1, title: 'Frontend Developer Technical Interview',
        domain: 'Frontend Developer | 2 Years', type: 'Technical', status: 'Pending', createdAt: '2026-05-11'
      }});
    });
  });

  describe('beginInterview()', () => {
    it('should POST to begin and return normalized session with questions', () => {
      service.beginInterview(42).subscribe(session => {
        expect(session.interviewId).toBe(42);
        expect(session.questions.length).toBe(2);
        expect(session.questions[0].text).toBe('What is Angular?');
      });

      const req = httpMock.expectOne(`${apiUrl}/42/begin`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: {
        interviewId: 42, totalExpected: 2, message: 'Generated.',
        questions: [
          { id: 1, text: 'What is Angular?', questionType: 'Open', orderIndex: 0 },
          { id: 2, text: 'Explain RxJS.', questionType: 'Open', orderIndex: 1 }
        ]
      }});
    });

    it('should normalize PascalCase API responses', () => {
      service.beginInterview(10).subscribe(session => {
        expect(session.interviewId).toBe(10);
        expect(session.questions[0].text).toBe('PascalCase Question');
      });

      const req = httpMock.expectOne(`${apiUrl}/10/begin`);
      req.flush({ Data: {
        InterviewId: 10, TotalExpected: 1, Message: 'OK',
        Questions: [{ Id: 1, Text: 'PascalCase Question', QuestionType: 'MCQ', OrderIndex: 0 }]
      }});
    });
  });

  describe('getMyInterviews()', () => {
    it('should GET and return a list of normalized interviews', () => {
      service.getMyInterviews().subscribe(interviews => {
        expect(interviews.length).toBe(2);
        expect(interviews[0].title).toBe('Interview 1');
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush({ success: true, message: '', data: [
        { id: 1, userId: 1, title: 'Interview 1', domain: 'Angular', type: 'Technical', status: 'Pending', createdAt: '2026-05-01' },
        { id: 2, userId: 1, title: 'Interview 2', domain: 'React', type: 'HR', status: 'Completed', createdAt: '2026-05-02' }
      ]});
    });

    it('should return cached interviews on second call without forceRefresh', () => {
      service.getMyInterviews().subscribe();
      httpMock.expectOne(apiUrl).flush({ success: true, message: '', data: [
        { id: 1, userId: 1, title: 'Cached', domain: '', type: '', status: 'Pending', createdAt: '' }
      ]});

      service.getMyInterviews().subscribe(interviews => {
        expect(interviews.length).toBe(1);
        expect(interviews[0].title).toBe('Cached');
      });
      httpMock.expectNone(apiUrl);
    });
  });

  describe('getCachedInterviews()', () => {
    it('should return empty array when nothing is cached', () => {
      expect(service.getCachedInterviews()).toEqual([]);
    });

    it('should return cached interviews from sessionStorage', () => {
      sessionStorage.setItem('the_interview_my_interviews', JSON.stringify([
        { id: 99, userId: 1, title: 'Cached Interview', domain: '', type: '', status: '', createdAt: '' }
      ]));
      expect(service.getCachedInterviews().length).toBe(1);
    });

    it('should return empty array for invalid JSON', () => {
      sessionStorage.setItem('the_interview_my_interviews', 'broken');
      expect(service.getCachedInterviews()).toEqual([]);
    });
  });

  describe('submitInterview()', () => {
    it('should POST answers and return a normalized result', () => {
      const request = {
        interviewId: 42,
        answers: [{ questionId: 1, answerText: 'Angular is a framework.' }]
      };

      service.submitInterview(request).subscribe(result => {
        expect(result.totalScore).toBe(8);
        expect(result.grade).toBe('A');
      });

      const req = httpMock.expectOne(`${apiUrl}/submit`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: {
        totalScore: 8, maxScore: 10, percentage: 80, grade: 'A',
        wrongQuestionIds: [], feedback: 'Great!', breakdown: [], strengths: [], weakAreas: [], suggestions: []
      }});
    });
  });

  describe('getInterviewResult()', () => {
    it('should GET and return a normalized result', () => {
      service.getInterviewResult(42).subscribe(result => {
        expect(result.grade).toBe('B');
      });

      const req = httpMock.expectOne(`${apiUrl}/42/result`);
      expect(req.request.method).toBe('GET');
      req.flush({ success: true, message: '', data: {
        totalScore: 6, maxScore: 10, percentage: 60, grade: 'B', wrongQuestionIds: [2], feedback: 'Good.'
      }});
    });
  });

  describe('warmUpCache()', () => {
    it('should POST warm-up request', () => {
      service.warmUpCache({ domain: 'Angular', targetCount: 10 }).subscribe();
      const req = httpMock.expectOne(`${apiUrl}/warm-up`);
      expect(req.request.method).toBe('POST');
      req.flush({ success: true, message: '', data: {} });
    });
  });
});
