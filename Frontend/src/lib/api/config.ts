import { env } from "../config/env";

// Enterprise-grade API Configuration
export const API_CONFIG = {
  baseURL: env.apiBaseUrl,
  saasURL: env.saasApiBaseUrl,
  timeout: env.api.timeout,
  retryAttempts: env.api.retryAttempts,
  retryDelay: env.api.retryDelay,
  cacheTimeout: env.api.cacheTimeout,
  maxConcurrentRequests: env.api.maxConcurrentRequests,
  enableLogging: env.isDevelopment,
  enableMetrics: true,
} as const;

// API Endpoints - Simplified for Next.js file-based routing
export const API_ENDPOINTS = {
  // Only define endpoints that don't match your Next.js routes
  health: "/health",
  uploads: "/uploads",
} as const;

// Dynamic endpoint builder for Next.js routing
export const createApiEndpoint = (path: string) => path;

// Common endpoint patterns (optional helpers)
export const API_PATTERNS = {
  candidateById: (id: string) => `/recruiter/candidates/${id}`,
  interviewById: (id: string) => `/interviews/${id}`,
} as const;

// HTTP Status Codes
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  UNPROCESSABLE_ENTITY: 422,
  INTERNAL_SERVER_ERROR: 500,
  SERVICE_UNAVAILABLE: 503,
} as const;

// API Response Types
export interface ApiResponse<T = any> {
  data: T;
  message?: string;
  success: boolean;
  errors?: string[];
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
  };
}

export interface PaginatedApiResponse<T> extends ApiResponse<T[]> {
  pagination: {
    hasNextPage: boolean;
    hasPreviousPage: false;
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

// API Error Types
export class ApiError extends Error {
  constructor(
    public status: number,
    public message: string,
    public errors?: string[],
    public code?: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export class NetworkError extends Error {
  constructor(message: string = "Network error occurred") {
    super(message);
    this.name = "NetworkError";
  }
}

export class TimeoutError extends Error {
  constructor(message: string = "Request timeout") {
    super(message);
    this.name = "TimeoutError";
  }
}
