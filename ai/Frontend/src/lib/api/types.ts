// Common API types for pagination and filtering

export interface PaginationParams {
  page: number;
  pageSize: number;
}

export interface SearchParams {
  search?: string;
  filters?: Record<string, any>;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface ApiListParams extends PaginationParams, SearchParams {}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface ListResponse<T> {
  data: T[];
  pagination: PaginationMeta;
  message?: string;
  success: boolean;
  errors?: string[];
}

export type CacheStrategy = 'no-cache' | 'cache-first' | 'network-first' | 'cache-only';

export type RequestPriority = 'low' | 'normal' | 'high';

// Retry and Circuit Breaker Configuration Types
export interface RetryConfig {
  maxAttempts: number;
  baseDelay: number;
  maxDelay: number;
  backoffFactor: number;
  retryCondition: (error: any) => boolean;
}

export interface CircuitBreakerConfig {
  failureThreshold: number;
  resetTimeout: number;
  monitoringPeriod: number;
}