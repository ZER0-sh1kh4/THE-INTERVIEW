/**
 * Admin-specific TypeScript models mirroring the backend entity shapes.
 */

export interface AdminUser {
  id: number;
  fullName: string;
  email: string;
  role: string;
  isPremium: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface AdminInterview {
  id: number;
  userId: number;
  title: string;
  domain: string;
  type: string;
  status: string;
  startedAt: string;
  completedAt: string;
  createdAt: string;
}

export interface AdminAssessment {
  id: number;
  assessmentId: number;
  userId: number;
  domain: string;
  score: number;
  maxScore: number;
  percentage: number;
  grade: string;
  isPremiumResult: boolean;
  createdAt: string;
}

export interface MCQQuestion {
  id: number;
  domain: string;
  text: string;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  correctOption: string;
  subtopic: string;
  marks: number;
  orderIndex: number;
}

export interface AdminSubscription {
  id: number;
  userId: number;
  plan: string;
  price: number;
  status: string;
  startDate: string;
  endDate: string;
  createdAt: string;
}

export interface AdminPayment {
  id: number;
  subscriptionId: number;
  userId: number;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
}
