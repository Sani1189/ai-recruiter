// Interviews Service - Next.js optimized
import { apiClient } from '../client';
import { API_PATTERNS, ApiResponse } from '../config';

export interface Interview {
  id: string;
  jobPostId: string;
  candidateId: string;
  scheduledAt: string;
  duration: number; // in minutes
  type: 'phone' | 'video' | 'in-person' | 'technical';
  status: 'scheduled' | 'completed' | 'cancelled' | 'rescheduled';
  feedback?: string;
  rating?: number;
  interviewerId: string;
  createdAt: string;
}

export class InterviewsService {
  async getInterviews(page: number = 1, pageSize: number = 10): Promise<ApiResponse<Interview[]>> {
    return apiClient.get<Interview[]>(`/recruiter/interviews?page=${page}&pageSize=${pageSize}`);
  }

  async scheduleInterview(interview: Omit<Interview, 'id' | 'createdAt'>): Promise<ApiResponse<Interview>> {
    return apiClient.post<Interview>('/recruiter/interviews', interview);
  }

  async updateInterview(id: string, interview: Partial<Interview>): Promise<ApiResponse<Interview>> {
    return apiClient.put<Interview>(API_PATTERNS.interviewById(id), interview);
  }

  async cancelInterview(id: string, reason?: string): Promise<ApiResponse<void>> {
    return apiClient.patch<void>(`/recruiter/interviews/${id}/cancel`, { reason });
  }
}

export const interviewsService = new InterviewsService();
