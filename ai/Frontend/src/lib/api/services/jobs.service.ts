// Jobs Service - Next.js optimized
import { apiClient } from '../client';
import { API_PATTERNS, ApiResponse } from '../config';

export interface JobPost {
  id: string;
  title: string;
  description: string;
  requirements: string[];
  location: string;
  salaryRange?: {
    min: number;
    max: number;
    currency: string;
  };
  employmentType: 'full-time' | 'part-time' | 'contract' | 'internship';
  status: 'draft' | 'published' | 'closed' | 'archived';
  recruiterId: string;
  createdAt: string;
  updatedAt: string;
  applicationCount: number;
}

export class JobsService {
  // Get jobs - automatic token injection
  async getJobs(page: number = 1, pageSize: number = 10): Promise<ApiResponse<JobPost[]>> {
    return apiClient.get<JobPost[]>(`/recruiter/jobs?page=${page}&pageSize=${pageSize}`);
  }

  async getJob(id: string): Promise<ApiResponse<JobPost>> {
    return apiClient.get<JobPost>(`/recruiter/jobs/${id}`);
  }

  async createJob(job: Omit<JobPost, 'id' | 'recruiterId' | 'createdAt' | 'updatedAt' | 'applicationCount'>): Promise<ApiResponse<JobPost>> {
    return apiClient.post<JobPost>('/recruiter/jobs', job);
  }

  async updateJob(id: string, job: Partial<JobPost>): Promise<ApiResponse<JobPost>> {
    return apiClient.put<JobPost>(`/recruiter/jobs/${id}`, job);
  }

  async deleteJob(id: string): Promise<ApiResponse<void>> {
    return apiClient.delete<void>(`/recruiter/jobs/${id}`);
  }
}

export const jobsService = new JobsService();

