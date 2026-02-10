import { z } from 'zod';
import { PROMPT_CATEGORIES, COMMON_LOCALES } from '@/types/prompt';

// Prompt form schema
export const promptFormSchema = z.object({
  name: z.string()
    .min(1, 'Name is required')
    .max(100, 'Name cannot exceed 100 characters'),
  
  version: z.number()
    .int('Version must be an integer')
    .min(1, 'Version must be greater than 0'),
  
  category: z.enum(PROMPT_CATEGORIES, {
    message: 'Category is required'
  }),
  
  content: z.string()
    .min(1, 'Content is required')
    .max(50000, 'Content cannot exceed 50,000 characters'),
  
  locale: z.string()
    .max(10, 'Locale cannot exceed 10 characters')
    .optional()
    .or(z.literal('')),
  
  tags: z.array(z.string())
    .default([])
    .optional()
});

// Prompt query schema
export const promptQuerySchema = z.object({
  searchTerm: z.string().optional(),
  category: z.string().optional(),
  locale: z.string().optional(),
  name: z.string().optional(),
  createdAfter: z.string().optional(),
  createdBefore: z.string().optional(),
  pageNumber: z.number().int().min(1).default(1),
  pageSize: z.number().int().min(1).max(100).default(10),
  sortBy: z.string().default('createdAt'),
  sortDescending: z.boolean().default(true)
});

// Prompt create schema (without version - auto-generated)
export const promptCreateSchema = promptFormSchema.omit({ version: true });

// Prompt update schema
export const promptUpdateSchema = promptFormSchema;

// Type exports
export type PromptFormData = z.infer<typeof promptFormSchema>;
export type PromptQueryData = z.infer<typeof promptQuerySchema>;
export type PromptCreateData = z.infer<typeof promptCreateSchema>;
export type PromptUpdateData = z.infer<typeof promptUpdateSchema>;
