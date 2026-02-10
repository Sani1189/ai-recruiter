// Candidates Service - Next.js optimized
import { apiClient } from '../client';
import { API_PATTERNS, ApiResponse } from '../config';
import { ApiListParams, ListResponse } from '../types';

export interface Candidate {
  id: string;
  name: string;
  email: string;
  phone?: string;
  resumeUrl?: string;
  skills: string[];
  experience: number;
  location?: string;
  status: 'active' | 'inactive' | 'blacklisted';
  appliedJobs: string[];
  createdAt: string;
  lastActiveAt?: string;
}

export class CandidatesService {
  // Simple method - just pass searchParams object
  async getCandidates(searchParams: Record<string, any> = {}): Promise<ListResponse<Candidate>> {
    const queryString = new URLSearchParams();
    
    // Add all non-empty values from searchParams
    Object.entries(searchParams).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        queryString.set(key, value.toString());
      }
    });
    
    const url = queryString.toString() 
      ? `/recruiter/candidates?${queryString.toString()}`
      : '/recruiter/candidates';
      
    return apiClient.get<Candidate[]>(url) as Promise<ListResponse<Candidate>>;
  }

  async getCandidate(id: string): Promise<ApiResponse<Candidate>> {
    return apiClient.get<Candidate>(API_PATTERNS.candidateById(id));
  }

  // Get job applications for a candidate
  async getCandidateJobApplications(candidateId: string): Promise<any> {
    return apiClient.get<any[]>(`/api/jobapplication/candidate/${candidateId}`);
  }

  // Get job application with steps and interviews for a specific candidate and job post
  async getApplicationDetails(
    jobPostName: string,
    jobPostVersion: number,
    candidateId: string
  ): Promise<any> {
    return apiClient.get<any>(
      `/jobapplication/jobpost/${encodeURIComponent(jobPostName)}/${jobPostVersion}/candidate/${candidateId}`
    );
  }

  async updateCandidateStatus(id: string, status: Candidate['status']): Promise<ApiResponse<Candidate>> {
    return apiClient.patch<Candidate>(`/recruiter/candidates/${id}/status`, { status });
  }
}

export const candidatesService = new CandidatesService();
