"use client"

import { useCallback } from "react"
import { useUnifiedAuth } from "./useUnifiedAuth"
import { jobApplicationFlowService } from "@/lib/api/services/jobApplicationFlow.service"
import type {
  JobApplicationResponse,
  BeginStepResponse,
  JobApplicationProgress,
  BeginStepRequest,
} from "@/lib/api/services/jobApplicationFlow.service"
import type { ApiResponse } from "@/lib/api/config"

/**
 * Hook that wraps job application flow service with automatic token injection
 * Ensures all API calls are properly authenticated with Azure token
 */
export function useJobApplicationFlow() {
  const { getAccessToken } = useUnifiedAuth()

  // Apply for a job with authentication
  const applyForJob = useCallback(
    async (jobPostName: string, jobPostVersion: number): Promise<ApiResponse<JobApplicationResponse>> => {
      try {
        const token = await getAccessToken()
        if (!token) {
          throw new Error("Authentication required. Please sign in again.")
        }

        console.log("Applying for job with authentication:", jobPostName)
        return await jobApplicationFlowService.applyForJob(jobPostName, jobPostVersion, token)
      } catch (error) {
        console.error("Error in applyForJob:", error)
        throw error
      }
    },
    [getAccessToken],
  )

  // Get application progress with authentication
  const getApplicationProgress = useCallback(
    async (jobPostName: string, jobPostVersion: number): Promise<ApiResponse<JobApplicationProgress>> => {
      try {
        const token = await getAccessToken()
        if (!token) {
          throw new Error("Authentication required. Please sign in again.")
        }

        console.log("Getting application progress with authentication:", jobPostName)
        return await jobApplicationFlowService.getApplicationProgress(jobPostName, jobPostVersion, token)
      } catch (error) {
        console.error("Error in getApplicationProgress:", error)
        throw error
      }
    },
    [getAccessToken],
  )

  // Begin a step with authentication
  const beginStep = useCallback(
    async (
      jobPostName: string,
      jobPostVersion: number,
      stepRequest: BeginStepRequest,
    ): Promise<ApiResponse<BeginStepResponse>> => {
      try {
        const token = await getAccessToken()
        if (!token) {
          throw new Error("Authentication required. Please sign in again.")
        }

        console.log("Beginning application step with authentication:", jobPostName)
        return await jobApplicationFlowService.beginStep(jobPostName, jobPostVersion, stepRequest, token)
      } catch (error) {
        console.error("Error in beginStep:", error)
        throw error
      }
    },
    [getAccessToken],
  )

  // Get application by job post with authentication
  const getApplicationByJobPost = useCallback(
    async (jobPostName: string, jobPostVersion: number): Promise<ApiResponse<JobApplicationResponse>> => {
      try {
        const token = await getAccessToken()
        if (!token) {
          throw new Error("Authentication required. Please sign in again.")
        }

        return await jobApplicationFlowService.getApplicationByJobPost(jobPostName, jobPostVersion, token)
      } catch (error) {
        console.error("Error in getApplicationByJobPost:", error)
        throw error
      }
    },
    [getAccessToken],
  )

  // Get all my applications with authentication
  const getMyApplications = useCallback(async (): Promise<ApiResponse<JobApplicationResponse[]>> => {
    try {
      const token = await getAccessToken()
      if (!token) {
        throw new Error("Authentication required. Please sign in again.")
      }

      return await jobApplicationFlowService.getMyApplications(token)
    } catch (error) {
      console.error("Error in getMyApplications:", error)
      throw error
    }
  }, [getAccessToken])

  // Check application status with authentication
  const getApplicationStatus = useCallback(
    async (jobPostName: string, jobPostVersion: number): Promise<{ hasApplied: boolean; applicationId?: string }> => {
      try {
        const token = await getAccessToken()
        if (!token) {
          return { hasApplied: false }
        }

        console.log("Checking application status:", jobPostName)
        return await jobApplicationFlowService.getApplicationStatus(jobPostName, jobPostVersion, token)
      } catch (error) {
        console.error("Error checking application status:", error)
        // Return false on error to gracefully handle missing applications
        return { hasApplied: false }
      }
    },
    [getAccessToken],
  )

  return {
    applyForJob,
    getApplicationProgress,
    beginStep,
    getApplicationByJobPost,
    getMyApplications,
    getApplicationStatus,
  }
}
