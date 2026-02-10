import { apiClient } from '../client';
import { ApiResponse } from '../config';

export interface DashboardStats {
  totalJobOpenings: number;
  totalCandidates: number;
  avgOverallScore: number;
  avgDurationMinutes: number;
}

export class DashboardService {
  async getDashboardStats(token: string): Promise<ApiResponse<DashboardStats>> {
    return apiClient.get<DashboardStats>(
      '/dashboard/stats',
      { requireAuth: true },
      token
    );
  }
}

export const dashboardService = new DashboardService();
