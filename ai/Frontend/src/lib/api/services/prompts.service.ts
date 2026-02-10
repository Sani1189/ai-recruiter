// Prompts Service - Next.js optimized
import { apiClient } from '../client';
import { API_PATTERNS, ApiResponse } from '../config';
import { Prompt, PromptListQuery, PromptListResponse } from '@/types/prompt';

export class PromptsService {
  // Get all prompts
  async getPrompts(): Promise<ApiResponse<Prompt[]>> {
    return apiClient.get<Prompt[]>('/prompt');
  }

  // Get filtered prompts with pagination
  async getFilteredPrompts(query: PromptListQuery): Promise<PromptListResponse> {
    const queryParams = new URLSearchParams();
    
    // Map frontend query to backend DTO format
    const backendQuery = {
      SearchTerm: query.searchTerm,
      Category: query.category,
      Locale: query.locale,
      Name: query.name,
      CreatedAfter: query.createdAfter,
      CreatedBefore: query.createdBefore,
      PageNumber: query.pageNumber,
      PageSize: query.pageSize,
      SortBy: query.sortBy,
      SortDescending: query.sortDescending,
    };
    
    // Add all non-empty values from backend query
    Object.entries(backendQuery).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        queryParams.set(key, value.toString());
      }
    });
    
    const url = queryParams.toString() 
      ? `/prompt/filtered?${queryParams.toString()}`
      : '/prompt/filtered';
      
    const response = await apiClient.get<PromptListResponse>(url);
    return response.data || { items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false };
  }

  // Get prompt by name and version
  async getPrompt(name: string, version: number): Promise<ApiResponse<Prompt>> {
    return apiClient.get<Prompt>(`/prompt/${name}/${version}`);
  }

  // Get latest prompt by name
  async getLatestPrompt(name: string): Promise<ApiResponse<Prompt>> {
    return apiClient.get<Prompt>(`/prompt/${name}/latest`);
  }

  // Get prompts by category
  async getPromptsByCategory(category: string): Promise<ApiResponse<Prompt[]>> {
    return apiClient.get<Prompt[]>(`/prompt/by-category/${category}`);
  }

  // Get prompts by locale
  async getPromptsByLocale(locale: string): Promise<ApiResponse<Prompt[]>> {
    return apiClient.get<Prompt[]>(`/prompt/by-locale/${locale}`);
  }

  // Create new prompt
  async createPrompt(prompt: Omit<Prompt, 'createdAt' | 'updatedAt'>): Promise<ApiResponse<Prompt>> {
    return apiClient.post<Prompt>('/prompt', prompt);
  }

  // Update prompt
  async updatePrompt(name: string, version: number, prompt: Partial<Prompt>): Promise<ApiResponse<Prompt>> {
    return apiClient.put<Prompt>(`/prompt/${name}/${version}`, prompt);
  }

  // Delete prompt
  async deletePrompt(name: string, version: number): Promise<ApiResponse<void>> {
    return apiClient.delete<void>(`/prompt/${name}/${version}`);
  }

  // Check if prompt exists
  async promptExists(name: string, version: number): Promise<boolean> {
    try {
      await apiClient.get(`/prompt/${name}/${version}`, { requireAuth: true });
      return true;
    } catch {
      return false;
    }
  }
}

export const promptsService = new PromptsService();
