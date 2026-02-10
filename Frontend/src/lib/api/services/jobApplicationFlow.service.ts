import { apiClient } from "../client"
import type { ApiResponse } from "../config"

export interface JobApplicationResponse {
  id: string
  jobPostName: string
  jobPostVersion: number
  candidateId: string
  status: string
  createdAt: string
  updatedAt: string
}

export interface BeginStepRequest {
  stepName: string
  stepVersion?: number | null
  stepNumber: number
}

export interface BeginStepResponse {
  jobApplicationId: string
  jobApplicationStepId: string
  interviewId?: string | null
  stepStatus?: string
}

export interface JobApplicationProgress {
  jobApplication: JobApplicationResponse | null
  steps: Array<{
    id: string
    jobPostStepName: string
    jobPostStepVersion: number
    status: string
    stepNumber: number
  }>
}

export class JobApplicationFlowService {
  // Apply for a job
  async applyForJob(
    jobPostName: string,
    jobPostVersion: number,
    azureToken?: string,
  ): Promise<ApiResponse<JobApplicationResponse>> {
    return apiClient.post<JobApplicationResponse>(
      `/candidate/job-application`,
      {
        jobPostName,
        jobPostVersion,
      },
      {},
      azureToken,
    )
  }

  // Get application progress without creating anything
  async getApplicationProgress(
    jobPostName: string,
    jobPostVersion: number,
    azureToken?: string,
  ): Promise<ApiResponse<JobApplicationProgress>> {
    return apiClient.get<JobApplicationProgress>(
      `/candidate/job-application/my-application/jobpost/${encodeURIComponent(jobPostName)}/${jobPostVersion}/progress`,
      {},
      azureToken,
    )
  }

  // Begin a step in the application flow
  async beginStep(
    jobPostName: string,
    jobPostVersion: number,
    stepRequest: BeginStepRequest,
    azureToken?: string,
  ): Promise<ApiResponse<BeginStepResponse>> {
    return apiClient.post<BeginStepResponse>(
      `/candidate/job-application/jobpost/${encodeURIComponent(jobPostName)}/${jobPostVersion}/steps/begin`,
      stepRequest,
      {},
      azureToken,
    )
  }

  // Get my application by job post
  async getApplicationByJobPost(
    jobPostName: string,
    jobPostVersion: number,
    azureToken?: string,
  ): Promise<ApiResponse<JobApplicationResponse>> {
    return apiClient.get<JobApplicationResponse>(
      `/candidate/job-application/my-application/jobpost/${encodeURIComponent(jobPostName)}/${jobPostVersion}`,
      {},
      azureToken,
    )
  }

  // Get all my applications
  async getMyApplications(azureToken?: string): Promise<ApiResponse<JobApplicationResponse[]>> {
    return apiClient.get<JobApplicationResponse[]>(`/candidate/job-application/my-applications`, {}, azureToken)
  }

  async getApplicationStatus(
    jobPostName: string,
    jobPostVersion: number,
    azureToken?: string,
  ): Promise<{ hasApplied: boolean; applicationId?: string }> {
    try {
      const response = await this.getApplicationByJobPost(jobPostName, jobPostVersion, azureToken)
      const appData = (response as any)?.data ?? response
      if (appData && appData.id) {
        return {
          hasApplied: true,
          applicationId: appData.id,
        }
      }
      return { hasApplied: false }
    } catch (error: any) {
      // If 404, user hasn't applied yet
      if (error?.statusCode === 404 || error?.status === 404) {
        return { hasApplied: false }
      }
      console.error("Error checking application status:", error)
      throw error
    }
  }
}

export const jobApplicationFlowService = new JobApplicationFlowService()
