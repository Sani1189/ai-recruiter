import { z } from 'zod';
import { INTERVIEW_MODALITIES, COMMON_LANGUAGES } from '@/types/interviewConfiguration';

// InterviewConfiguration form schema
export const interviewConfigurationFormSchema = z.object({
  name: z.string()
    .min(1, 'Name is required')
    .max(100, 'Name cannot exceed 100 characters'),
  
  version: z.number()
    .int('Version must be an integer')
    .min(1, 'Version must be greater than 0'),
  
  modality: z.enum(INTERVIEW_MODALITIES, {
    message: 'Modality is required'
  }),
  
  tone: z.string()
    .max(100, 'Tone cannot exceed 100 characters')
    .optional()
    .or(z.literal('')),
  
  probingDepth: z.string()
    .max(100, 'Probing depth cannot exceed 100 characters')
    .optional()
    .or(z.literal('')),
  
  focusArea: z.string()
    .max(100, 'Focus area cannot exceed 100 characters')
    .optional()
    .or(z.literal('')),
  
  duration: z.number()
    .int('Duration must be an integer')
    .min(1, 'Duration must be greater than 0')
    .optional()
    .or(z.literal(0)),
  
  language: z.enum(COMMON_LANGUAGES, {
    message: 'Invalid language'
  }).optional()
    .or(z.literal('')),
  
  instructionPromptName: z.string()
    .min(1, 'Instruction prompt name is required')
    .max(255, 'Instruction prompt name cannot exceed 255 characters'),
  
  instructionPromptVersion: z.number()
    .int('Instruction prompt version must be an integer')
    .min(1, 'Instruction prompt version must be greater than 0')
    .optional(),
  
  personalityPromptName: z.string()
    .min(1, 'Personality prompt name is required')
    .max(255, 'Personality prompt name cannot exceed 255 characters'),
  
  personalityPromptVersion: z.number()
    .int('Personality prompt version must be an integer')
    .min(1, 'Personality prompt version must be greater than 0')
    .optional(),
  
  questionsPromptName: z.string()
    .min(1, 'Questions prompt name is required')
    .max(255, 'Questions prompt name cannot exceed 255 characters'),
  
  questionsPromptVersion: z.number()
    .int('Questions prompt version must be an integer')
    .min(1, 'Questions prompt version must be greater than 0')
    .optional(),
  
  active: z.boolean().default(true)
});

// InterviewConfiguration query schema
export const interviewConfigurationQuerySchema = z.object({
  searchTerm: z.string().optional(),
  modality: z.string().optional(),
  active: z.boolean().optional(),
  pageNumber: z.number().int().min(1).default(1),
  pageSize: z.number().int().min(1).max(100).default(10),
  sortBy: z.string().default('createdAt'),
  sortDescending: z.boolean().default(true)
});

// InterviewConfiguration create schema (without version - auto-generated)
export const interviewConfigurationCreateSchema = interviewConfigurationFormSchema.omit({ version: true });

// InterviewConfiguration update schema
export const interviewConfigurationUpdateSchema = interviewConfigurationFormSchema;

// Type exports
export type InterviewConfigurationFormData = z.infer<typeof interviewConfigurationFormSchema>;
export type InterviewConfigurationQueryData = z.infer<typeof interviewConfigurationQuerySchema>;
export type InterviewConfigurationCreateData = z.infer<typeof interviewConfigurationCreateSchema>;
export type InterviewConfigurationUpdateData = z.infer<typeof interviewConfigurationUpdateSchema>;
