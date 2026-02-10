// Auth Service - Simplified for Next.js
import { apiClient } from '../client';
import { ApiResponse } from '../config';

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  roles: string[];
  company?: string;
  avatar?: string;
  createdAt: string;
  lastLoginAt?: string;
}

export class AuthService {
  // Get current user profile
  async getProfile(): Promise<ApiResponse<UserProfile>> {
    return apiClient.get<UserProfile>('/auth/profile');
  }
  // Get current user profile - creates user if not exists (works for both recruiters and candidates)
  async getMe(azureToken?: string): Promise<ApiResponse<UserProfile>> {
    return apiClient.get<UserProfile>('/UserProfile/me', {}, azureToken);
  }

  // Register user (creates user if not exists) - alternative to /me
  async registerUser(azureToken?: string): Promise<ApiResponse<UserProfile>> {
    return apiClient.post<UserProfile>('/UserProfile/register', {}, {}, azureToken);
  }
  // Logout (if needed on backend)
  async logout(): Promise<ApiResponse<void>> {
    return apiClient.post<void>('/auth/logout', {});
  }
}

export const authService = new AuthService();
