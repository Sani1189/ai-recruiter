// Prompt types and interfaces

export interface Prompt {
  name: string;
  version: number;
  category: string;
  content: string;
  locale?: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  rowVersion?: string;
  shouldUpdateVersion?: boolean | null;
}

export interface PromptListQuery {
  searchTerm?: string;
  category?: string;
  locale?: string;
  name?: string;
  createdAfter?: string;
  createdBefore?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending: boolean;
}

export interface PromptListResponse {
  items: Prompt[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Prompt categories enum
export const PROMPT_CATEGORIES = [
  "instructions",
  "tonality",
  "rubric",
  "question_bank",
  "personality",
  "cv_extraction",
  "scoring",
  "other",
] as const;

export type PromptCategory = (typeof PROMPT_CATEGORIES)[number];

// Common locales
export const COMMON_LOCALES = [
  "en-US",
  "en-GB",
  "en-CA",
  "en-AU",
  "en-IN",
  "fr-FR",
  "fr-CA",
  "de-DE",
  "es-ES",
  "es-MX",
  "es-AR",
  "es-CO",
  "es-CL",
  "es-PE",
  "it-IT",
  "pt-PT",
  "pt-BR",
  "nl-NL",
  "sv-SE",
  "no-NO",
  "da-DK",
  "fi-FI",
  "pl-PL",
  "cs-CZ",
  "hu-HU",
  "ro-RO",
  "bg-BG",
  "el-GR",
  "uk-UA",
  "ru-RU",
  "zh-CN",
  "zh-TW",
  "ja-JP",
  "ko-KR",
  "hi-IN",
  "bn-BD",
  "bn-IN",
  "th-TH",
  "vi-VN",
  "id-ID",
  "ms-MY",
  "tl-PH",
  "ar-SA",
  "ar-EG",
  "he-IL",
  "fa-IR",
  "tr-TR",
  "ur-PK",
  "sw-KE",
  "am-ET",
] as const;

export type Locale = (typeof COMMON_LOCALES)[number];
