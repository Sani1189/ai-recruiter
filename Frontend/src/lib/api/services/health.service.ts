// Health Service - for testing API connectivity
import { apiClient } from '../client';
import { API_ENDPOINTS, ApiResponse } from '../config';

export interface HealthResponse {
  status: string;
  timestamp: string;
  version: string;
}

export interface HealthApiResponse {
  data: HealthResponse;
  success: boolean;
  message: string;
}

export class HealthService {
  // Get health status (no auth required) with optimized caching
  async getHealth(): Promise<ApiResponse<HealthResponse>> {
    const response = await apiClient.get<HealthResponse>(API_ENDPOINTS.health, { 
      requireAuth: false,
      cacheStrategy: 'cache-first',
      cacheTTL: 30000, // Cache health checks for 30 seconds
      timeout: 5000, // Short timeout for health checks
      enableMetrics: true
    });
    
    return response;
  }

  // Force refresh health status (bypass cache)
  async getHealthForceRefresh(): Promise<ApiResponse<HealthResponse>> {
    const response = await apiClient.get<HealthResponse>(API_ENDPOINTS.health, { 
      requireAuth: false,
      cacheStrategy: 'no-cache',
      timeout: 5000
    });
    
    return response;
  }
}

export const healthService = new HealthService();
