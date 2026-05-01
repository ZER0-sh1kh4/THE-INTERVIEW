/**
 * Purpose: Frontend contracts for assessment result data shown on the candidate dashboard.
 * The backend can return either camelCase or PascalCase, so the service normalizes into this shape.
 */
export interface AssessmentResult {
  assessmentId: number;
  domain: string;
  score: number;
  maxScore: number;
  percentage: number;
  grade: string;
}

export interface StartAssessmentRequest {
  domain: string;
  questionCount: number;
  difficulty: string;
}

export interface QuestionDto {
  id: number;
  text: string;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  orderIndex: number;
}

export interface StartAssessmentResponse {
  assessmentId: number;
  timeLimitMinutes: number;
  expiresAt: string;
  questions: QuestionDto[];
  totalExpected: number;
  hasMore: boolean;
  warnings?: number;
  answers?: { [questionId: number]: string };
}

export interface AnswerSubmission {
  questionId: number;
  selectedOption: string;
}

export interface SubmitAssessmentRequest {
  assessmentId: number;
  answers: AnswerSubmission[];
  totalExpected?: number;
}
