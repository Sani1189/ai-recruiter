export const ASSESSMENT_TEMPLATE_TYPES = [
  "Quiz",
  "Personality",
  "Form",
] as const;
export type AssessmentTemplateType = (typeof ASSESSMENT_TEMPLATE_TYPES)[number];

// Quiz supports choice-based questions with answer keys for automated scoring.
// Keep quiz authoring minimal: just SingleChoice/MultiChoice.
export const QUIZ_QUESTION_TYPES = ["SingleChoice", "MultiChoice"] as const;
export const PERSONALITY_QUESTION_TYPES = ["Likert"] as const;
// Form is the "superset" template type: it can contain all question types.
// These are the default / commonly used "form" question types (used for default selection UX).
export const FORM_QUESTION_TYPES = [
  "Text",
  "Textarea",
  "Radio",
  "Checkbox",
  "Dropdown",
] as const;

export const ASSESSMENT_QUESTION_TYPES = [
  ...QUIZ_QUESTION_TYPES,
  ...PERSONALITY_QUESTION_TYPES,
  ...FORM_QUESTION_TYPES,
] as const;

export type AssessmentQuestionType = (typeof ASSESSMENT_QUESTION_TYPES)[number];

export interface AssessmentOption {
  name: string;
  version: number;
  order: number;
  label?: string | null;
  /**
   * Stable blob URL (no SAS). Access is controlled server-side when needed.
   */
  mediaUrl?: string | null;
  /**
   * Optional File record id for secure access (download SAS can be generated server-side).
   */
  mediaFileId?: string | null;
  /**
   * Quiz only.
   * Candidate payload must not include this.
   */
  isCorrect?: boolean | null;
  /**
   * Quiz only (optional).
   */
  score?: number | null;
  /**
   * Optional generic weight for non-quiz scoring/aggregation.
   * For Personality use `wa`. For Quiz use `score`.
   */
  weight?: number | null;
  /**
   * Personality only (Wa).
   */
  wa?: number | null;
}

export interface AssessmentQuestion {
  name: string;
  version: number;
  order: number;
  questionType: AssessmentQuestionType;
  isRequired?: boolean | null;
  promptText?: string | null;
  /**
   * Stable blob URL (no SAS). Optional media for the question prompt.
   */
  mediaUrl?: string | null;
  /**
   * Optional File record id for secure access (download SAS can be generated server-side).
   */
  mediaFileId?: string | null;
  /**
   * Personality only (Ws).
   */
  ws?: number | null;
  /**
   * Personality only (e.g. extrovert_introvert).
   */
  traitKey?: string | null;
  options?: AssessmentOption[];
}

export interface AssessmentSection {
  id: string;
  order: number;
  title?: string | null;
  questions: AssessmentQuestion[];
}

export interface AssessmentTemplate {
  name: string;
  version: number;
  templateType: AssessmentTemplateType;
  status?: "Draft" | "Published" | "Archived" | string;
  isDeleted?: boolean;
  sectionsCount?: number;
  questionsCount?: number;
  title?: string | null;
  description?: string | null;
  timeLimitSeconds?: number | null;
  isPublished?: boolean;
  publishedAt?: string | null;
  shouldUpdateVersion?: boolean | null;
  sections: AssessmentSection[];
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string | null;
  updatedBy?: string | null;
}
