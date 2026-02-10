import { ApiResponse } from "@/lib/api/config";

type ApiClientLike = {
  get: <T = any>(url: string, options?: Record<string, unknown>) => Promise<ApiResponse<T> | T>;
  post: <T = any>(url: string, body?: any, options?: Record<string, unknown>) => Promise<ApiResponse<T> | T>;
};

function extractResponse<T>(response: ApiResponse<T> | T): T {
  if (typeof response === "object" && response !== null && "data" in response) {
    return (response as ApiResponse<T>).data;
  }
  return response as T;
}

export type CandidateJobApplicationProgress = {
  jobApplication: { id: string; completedAt?: string | null } | null;
  steps: Array<{
    id: string;
    jobPostStepName: string;
    jobPostStepVersion: number;
    status: string;
    stepNumber: number;
  }>;
};

export type BeginCandidateJobApplicationStepRequest = {
  stepName: string;
  stepVersion: number | null;
  stepNumber: number;
};

export type BeginCandidateJobApplicationStepResponse = {
  jobApplicationId: string;
  jobApplicationStepId: string;
  interviewId?: string | null;
  stepStatus?: string;
};

export async function getMyJobApplicationProgress(
  api: ApiClientLike,
  jobPostName: string,
  jobPostVersion: number
): Promise<CandidateJobApplicationProgress> {
  const response = await api.get<CandidateJobApplicationProgress>(
    `/candidate/job-application/my-application/jobpost/${jobPostName}/${jobPostVersion}/progress`
  );
  return extractResponse(response);
}

export async function beginCandidateJobApplicationStep(
  api: ApiClientLike,
  jobPostName: string,
  jobPostVersion: number,
  request: BeginCandidateJobApplicationStepRequest
): Promise<BeginCandidateJobApplicationStepResponse> {
  const response = await api.post<BeginCandidateJobApplicationStepResponse>(
    `/candidate/job-application/jobpost/${jobPostName}/${jobPostVersion}/steps/begin`,
    request
  );
  return extractResponse(response);
}



