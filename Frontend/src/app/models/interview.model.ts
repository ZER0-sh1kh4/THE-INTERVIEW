/**
 * Purpose: TypeScript contracts for the interview feature.
 * These models keep the Angular service and components aligned with backend interview APIs.
 */

/**
 * User-facing start form shape requested by the product flow.
 * The service maps this into the current backend's title/domain DTO while keeping these fields available.
 */
export interface InterviewStartForm {
  role: string;
  experience: string;
  interviewType: 'Technical' | 'HR' | 'Mixed';
  techStack: string[];
  difficulty: 'Easy' | 'Medium' | 'Hard';
  numberOfQuestions: number;
}

/** Interview entity returned by POST /api/interviews/start. */
export interface Interview {
  id: number;
  userId: number;
  title: string;
  domain: string;
  type: string;
  status: string;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
}

/** Question shape returned by POST /api/interviews/{id}/begin. */
export interface InterviewQuestion {
  id: number;
  text: string;
  questionType: string;
  optionA?: string;
  optionB?: string;
  optionC?: string;
  optionD?: string;
  orderIndex: number;
}

/** Begin response wraps the generated or seeded questions for a session. */
export interface BeginInterviewResponse {
  interviewId: number;
  message: string;
  questions: InterviewQuestion[];
}

/** Single answer submitted to the backend. */
export interface InterviewAnswerSubmission {
  questionId: number;
  answerText: string;
}

/** Submit request expected by POST /api/interviews/submit. */
export interface SubmitInterviewRequest {
  interviewId: number;
  answers: InterviewAnswerSubmission[];
}

/** Result shape returned after final submit. */
export interface InterviewResult {
  totalScore: number;
  maxScore: number;
  percentage: number;
  grade: string;
  wrongQuestionIds: number[];
  feedback: string;
  isPremiumResult?: boolean;
  breakdown?: InterviewResultBreakdown[];
  strengths?: string[];
  weakAreas?: string[];
  suggestions?: string[];
}

export interface InterviewResultBreakdown {
  text: string;
  subtopic: string;
  yourAnswer: string;
  correctAnswer: string;
  isCorrect: boolean | null;
  score: number;
}
