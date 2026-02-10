import { apiClient, EnhancedRequestOptions } from '../client';
import { ApiResponse } from '../config';

export interface GetUploadUrlRequest {
  jobPostName: string;
  jobPostVersion: number;
  stepName: string;
  stepVersion: number;
  fileName: string;
  contentType: string;
  fileSize: number;
}

export interface GetUploadUrlResponse {
  uploadUrl: string;
  blobPath: string;
  expiresInMinutes: number;
}

export interface CompleteUploadRequest {
  jobPostName: string;
  jobPostVersion: number;
  stepName: string;
  stepVersion: number;
  blobPath: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
}

export interface UploadStepFileResult {
  jobApplicationStepId: string;
  fileId: string;
  jobApplicationStepFilesId: string;
  fileName?: string;
  fileUrl?: string;
  fileSize: number;
}

class JobApplicationStepFilesService {
  /**
   * Get upload SAS URL for direct client-to-Azure upload (best practice)
   * Generic method that works for any step type
   */
  async getUploadUrl(
    request: GetUploadUrlRequest,
    token?: string,
    options?: Omit<EnhancedRequestOptions, 'method' | 'body'>
  ): Promise<GetUploadUrlResponse> {
    this.validateGetUploadUrlRequest(request);

    const response = await apiClient.post<GetUploadUrlResponse>(
      '/candidate/job-application/steps/upload/get-upload-url',
      request,
      { requireAuth: true, ...options },
      token
    );

    return this.extractData<GetUploadUrlResponse>(response);
  }

  /**
   * Complete upload after direct upload to Azure (saves metadata to database)
   * Generic method that works for any step type
   */
  async completeUpload(
    request: CompleteUploadRequest,
    token?: string,
    options?: Omit<EnhancedRequestOptions, 'method' | 'body'>
  ): Promise<UploadStepFileResult> {
    this.validateCompleteUploadRequest(request);

    const response = await apiClient.post<UploadStepFileResult>(
      '/candidate/job-application/steps/upload/complete',
      request,
      { requireAuth: true, ...options },
      token
    );

    return this.extractData<UploadStepFileResult>(response);
  }

  /**
   * Direct upload through backend (for legacy/internal use)
   * Generic method that works for any step type
   */
  async uploadStepFile(
    file: File,
    jobPostName: string,
    jobPostVersion: number,
    stepName: string,
    stepVersion: number,
    token?: string,
    options?: Omit<EnhancedRequestOptions, 'method' | 'body'>
  ): Promise<UploadStepFileResult> {
    if (!file || file.size === 0) {
      throw new Error('File is required and must not be empty');
    }

    if (!stepName || stepVersion < 1) {
      throw new Error('Step name and version are required');
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('jobPostName', jobPostName);
    formData.append('jobPostVersion', jobPostVersion.toString());
    formData.append('stepName', stepName);
    formData.append('stepVersion', stepVersion.toString());

    const response = await apiClient.request<UploadStepFileResult>(
      '/candidate/job-application/steps/upload',
      {
        method: 'POST',
        body: formData,
        requireAuth: true,
        ...options,
      },
      token
    );

    return this.extractData<UploadStepFileResult>(response);
  }


  private validateGetUploadUrlRequest(request: GetUploadUrlRequest): void {
    if (!request.jobPostName?.trim()) {
      throw new Error('Job post name is required');
    }
    if (request.jobPostVersion < 1) {
      throw new Error('Job post version must be greater than 0');
    }
    if (!request.stepName?.trim()) {
      throw new Error('Step name is required');
    }
    if (request.stepVersion < 1) {
      throw new Error('Step version must be greater than 0');
    }
    if (!request.fileName?.trim()) {
      throw new Error('File name is required');
    }
    if (request.fileSize <= 0) {
      throw new Error('File size must be greater than 0');
    }
  }

  private validateCompleteUploadRequest(request: CompleteUploadRequest): void {
    if (!request.jobPostName?.trim()) {
      throw new Error('Job post name is required');
    }
    if (request.jobPostVersion < 1) {
      throw new Error('Job post version must be greater than 0');
    }
    if (!request.stepName?.trim()) {
      throw new Error('Step name is required');
    }
    if (request.stepVersion < 1) {
      throw new Error('Step version must be greater than 0');
    }
    if (!request.blobPath?.trim()) {
      throw new Error('Blob path is required');
    }
    if (!request.originalFileName?.trim()) {
      throw new Error('Original file name is required');
    }
  }

  private extractData<T>(response: ApiResponse<T> | T): T {
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as ApiResponse<T>).data;
    }
    return response as T;
  }
}

export const jobApplicationStepFilesService = new JobApplicationStepFilesService();

