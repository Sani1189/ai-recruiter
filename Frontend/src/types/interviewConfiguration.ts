// InterviewConfiguration types and interfaces

export interface InterviewConfiguration {
  name: string;
  version: number;
  modality: string;
  tone?: string;
  probingDepth?: string;
  focusArea?: string;
  duration?: number;
  language?: string;
  instructionPromptName: string;
  personalityPromptName: string;
  questionsPromptName: string;
  instructionPromptVersion?: number;
  personalityPromptVersion?: number;
  questionsPromptVersion?: number;
  active: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface InterviewConfigurationWithPrompts {
  name: string;
  version: number;
  modality: string;
  tone?: string;
  probingDepth?: string;
  focusArea?: string;
  duration?: number;
  language?: string;
  instructionPromptName: string;
  personalityPromptName: string;
  questionsPromptName: string;
  instructionPromptVersion?: number;
  personalityPromptVersion?: number;
  questionsPromptVersion?: number;
  active: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  instructionPrompt?: Prompt;
  personalityPrompt?: Prompt;
  questionsPrompt?: Prompt;
}

export interface Prompt {
  name: string;
  version: number;
  category: string;
  content: string;
  locale?: string;
  tags: string[];
  createdAt: string;
}

export interface PromptVersion {
  version: number;
  content: string;
  createdAt: string;
  createdBy?: string;
}

export interface InterviewConfigurationListQuery {
  searchTerm?: string;
  modality?: string;
  active?: boolean;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending: boolean;
}

export interface InterviewConfigurationListResponse {
  items: InterviewConfiguration[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// InterviewConfiguration modalities enum
export const INTERVIEW_MODALITIES = [
  'voice',
  'text', 
  'coding',
  'assignment'
] as const;

export type InterviewModality = typeof INTERVIEW_MODALITIES[number];

// Common languages
export const COMMON_LANGUAGES = [
  'en-US',
  'en-GB', 
  'es-ES',
  'fr-FR',
  'de-DE',
  'it-IT',
  'pt-BR',
  'zh-CN',
  'ja-JP',
  'ko-KR'
] as const;

export type Language = typeof COMMON_LANGUAGES[number];
