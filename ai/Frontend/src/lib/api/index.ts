// API Module Exports
export { apiClient } from './client';
// Removed interceptor - using unified auth directly
export {
  API_CONFIG,
  API_ENDPOINTS,
  HTTP_STATUS,
  ApiError,
  NetworkError,
  TimeoutError,
} from './config';

export type {
  ApiResponse,
  PaginatedResponse,
} from './config';

// Export modular services
export {
  authService,
  jobsService,
  candidatesService,
  interviewsService,
  dashboardService,
  healthService,
  type UserProfile,
  type JobPost,
  type Candidate,
  type Interview,
  type DashboardStats,
  type HealthResponse,
} from './services';

// Re-export default client for convenience
export { default } from './client';
