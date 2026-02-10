export const PARTICIPANTS = ["Candidate", "Recruiter"] as const;
export type Participant = typeof PARTICIPANTS[number];

// Candidate step types (API string values).
// Canonical name: "Questionnaire". Legacy values: "Assessment", "Multiple Choice".
export const CANDIDATE_STEP_TYPES = ["Interview", "Resume Upload", "Questionnaire"] as const;
export type CandidateStepType = typeof CANDIDATE_STEP_TYPES[number];

export const RECRUITER_STEP_TYPES = ["Ranking", "Documentation", "Generic"] as const;
export type RecruiterStepType = typeof RECRUITER_STEP_TYPES[number];

export const STEP_TYPES = [...CANDIDATE_STEP_TYPES, ...RECRUITER_STEP_TYPES] as const;
export type StepType = typeof STEP_TYPES[number];

export interface JobPostStep {
  name: string;
  version: number;
  stepType: StepType;
  participant: Participant;
  showStepForCandidate: boolean;
  displayTitle?: string | null;
  displayContent?: string | null;
  showSpinner: boolean;
  interviewConfigurationName?: string | null;
  interviewConfigurationVersion?: number | null;
  promptName?: string | null;
  promptVersion?: number | null;
  // New canonical API fields
  questionnaireTemplateName?: string | null;
  questionnaireTemplateVersion?: number | null;
  // Legacy frontend fields (kept for compatibility)
  assessmentTemplateName?: string | null;
  assessmentTemplateVersion?: number | null;
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface JobPostStepFormData {
  name: string;
  version: number;
  participant: Participant;
  stepType: StepType;
  showStepForCandidate: boolean;
  displayTitle?: string | null;
  displayContent?: string | null;
  showSpinner: boolean;
  interviewConfigurationName?: string;
  interviewConfigurationVersion?: number;
  promptName?: string | null;
  promptVersion?: number | null;
  assessmentTemplateName?: string | null;
  assessmentTemplateVersion?: number | null;
}

