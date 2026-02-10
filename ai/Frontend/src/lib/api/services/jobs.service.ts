// Jobs Service - Next.js optimized
import { apiClient } from '../client';
import { API_PATTERNS, ApiResponse } from '../config';

export interface JobPost {
  id: string;
  name: string;
  version: number;
  title: string;
  jobTitle: string;
  description: string;
  jobDescription: string;
  requirements: string;
  whatWeOffer: string;
  industry: string;
  introText: string;
  companyInfo: string;
  location: string;
  salaryRange?: {
    min: number;
    max: number;
    currency: string;
  };
  employmentType: 'full-time' | 'part-time' | 'contract' | 'internship';
  jobType: string;
  status: 'draft' | 'published' | 'closed' | 'archived';
  recruiterId: string;
  currentBoardColumnId?: string;
  createdAt: string;
  updatedAt: string;
  applicationCount: number;
}

export interface KanbanBoardColumn {
  id: string;
  recruiterId: string;
  columnName: string;
  sequence: number;
  isVisible: boolean;
  createdAt: string;
  updatedAt: string;
}

export class JobsService {
  // Get jobs - automatic token injection
  async getJobs(page: number = 1, pageSize: number = 10): Promise<ApiResponse<JobPost[]>> {
    return apiClient.get<JobPost[]>(`/job?page=${page}&pageSize=${pageSize}`);
  }

  async getJob(name: string, version: number): Promise<ApiResponse<JobPost>> {
    return apiClient.get<JobPost>(`/job/${name}/${version}`);
  }

  async createJob(job: Partial<JobPost>): Promise<ApiResponse<JobPost>> {
    return apiClient.post<JobPost>('/job', job);
  }

  async updateJob(name: string, version: number, job: Partial<JobPost>): Promise<ApiResponse<JobPost>> {
    return apiClient.put<JobPost>(`/job/${name}/${version}`, job);
  }

  async deleteJob(name: string, version: number): Promise<ApiResponse<void>> {
    return apiClient.delete<void>(`/job/${name}/${version}`);
  }

  // Kanban Board Column APIs
  async getColumnsByRecruiter(recruiterId: string): Promise<ApiResponse<KanbanBoardColumn[]>> {
    return apiClient.get<KanbanBoardColumn[]>(`/KanbanBoardColumn/recruiter/${recruiterId}`);
  }

  async createColumn(recruiterId: string, column: Omit<KanbanBoardColumn, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApiResponse<KanbanBoardColumn>> {
    return apiClient.post<KanbanBoardColumn>(`/KanbanBoardColumn/recruiter/${recruiterId}`, column);
  }

  async updateColumn(columnId: string, column: Partial<KanbanBoardColumn>): Promise<ApiResponse<KanbanBoardColumn>> {
    return apiClient.put<KanbanBoardColumn>(`/KanbanBoardColumn/${columnId}`, column);
  }

  async deleteColumn(columnId: string): Promise<ApiResponse<void>> {
    return apiClient.delete<void>(`/KanbanBoardColumn/${columnId}`);
  }

  async reorderColumns(recruiterId: string, ordering: Array<{ columnId: string; sequence: number }>): Promise<ApiResponse<void>> {
    return apiClient.post<void>(`/KanbanBoardColumn/recruiter/${recruiterId}/reorder`, ordering);
  }

  async moveJobToColumn(name: string, version: number, columnId: string): Promise<ApiResponse<JobPost>> {
    return apiClient.put<JobPost>(`/job/${name}/${version}/move-to-column/${columnId}`, {});
  }

  async getJobsByColumn(recruiterId: string, page: number = 1, pageSize: number = 100): Promise<ApiResponse<Record<string, JobPost[]>>> {
    return apiClient.get<Record<string, JobPost[]>>(`/job/by-column/${recruiterId}?page=${page}&pageSize=${pageSize}`);
  }
}

export const jobsService = new JobsService();

