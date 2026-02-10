import { z } from "zod";
import {
  ASSESSMENT_TEMPLATE_TYPES,
  PERSONALITY_QUESTION_TYPES,
  QUIZ_QUESTION_TYPES,
} from "@/types/assessmentTemplate";

const templateTypeSchema = z.enum(ASSESSMENT_TEMPLATE_TYPES);

const emptyStringToNull = <T extends z.ZodTypeAny>(schema: T) =>
  z.preprocess((v) => (typeof v === "string" && v.trim() === "" ? null : v), schema);

const optionSchema = z.object({
  name: z.string().min(1),
  version: z.number().int().min(1),
  order: z.number().int().min(1),
  label: emptyStringToNull(z.string().max(1000).nullable()).optional(),
  mediaUrl: emptyStringToNull(z.string().url().nullable()).optional(),
  mediaFileId: emptyStringToNull(z.string().uuid().nullable()).optional(),
  isCorrect: z.boolean().optional().nullable(),
  score: z.number().optional().nullable(),
  weight: z.number().optional().nullable(),
  wa: z.number().optional().nullable(),
});

const questionSchema = z.object({
  name: z.string().min(1),
  version: z.number().int().min(1),
  order: z.number().int().min(1),
  questionType: z.string().min(1),
  isRequired: z.boolean().optional().nullable(),
  promptText: emptyStringToNull(z.string().nullable()).optional(),
  mediaUrl: emptyStringToNull(z.string().url().nullable()).optional(),
  mediaFileId: emptyStringToNull(z.string().uuid().nullable()).optional(),
  ws: z.number().optional().nullable(),
  traitKey: z.string().max(100).optional().nullable(),
  options: z.array(optionSchema).optional(),
});

const sectionSchema = z.object({
  id: z.string().min(1),
  order: z.number().int().min(1),
  title: z.string().max(255).optional().nullable(),
  questions: z.array(questionSchema).default([]),
});

export const assessmentTemplateFormSchema = z
  .object({
    name: z.string().min(1).max(255),
    version: z.number().int().min(1),
    templateType: templateTypeSchema,
    title: emptyStringToNull(z.string().max(255).nullable()).optional(),
    description: emptyStringToNull(z.string().nullable()).optional(),
    timeLimitSeconds: z.number().int().min(1).optional().nullable(),
    sections: z.array(sectionSchema).min(1),
  })
  .superRefine((data, ctx) => {
    // Enforce allowed question types and required fields per templateType.
    for (const [sIndex, section] of data.sections.entries()) {
      for (const [qIndex, q] of section.questions.entries()) {
        const pathBase = ["sections", sIndex, "questions", qIndex] as const;
        const qType = q.questionType as string;
        const needsOptions =
          qType === "SingleChoice" ||
          qType === "MultiChoice" ||
          qType === "Radio" ||
          qType === "Checkbox" ||
          qType === "Likert" ||
          qType === "Dropdown";

        if (data.templateType === "Quiz") {
          if (!QUIZ_QUESTION_TYPES.includes(q.questionType as any)) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "Question type is not allowed for Quiz templates",
              path: [...pathBase, "questionType"],
            });
          }
          if (!q.options || q.options.length < 2) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "Quiz questions must have at least 2 options",
              path: [...pathBase, "options"],
            });
          }
        }

        if (data.templateType === "Personality") {
          if (!PERSONALITY_QUESTION_TYPES.includes(q.questionType as any)) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "Question type is not allowed for Personality templates",
              path: [...pathBase, "questionType"],
            });
          }
          if (!q.options || q.options.length < 2) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "Personality questions must have at least 2 options",
              path: [...pathBase, "options"],
            });
          }
        }

        if (data.templateType === "Form") {
          // Form is the "superset" template: allow any question type.
        }

        // Common rules for option-based questions: option label is required (clean authoring & candidate UX).
        if (needsOptions) {
          if (!q.options || q.options.length < 2) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "This question must have at least 2 options",
              path: [...pathBase, "options"],
            });
          } else {
            for (const [oIndex, opt] of q.options.entries()) {
              const label = (opt?.label ?? "").toString().trim();
              if (!label) {
                ctx.addIssue({
                  code: z.ZodIssueCode.custom,
                  message: "Option label is required",
                  path: [...pathBase, "options", oIndex, "label"],
                });
              }
              if (qType === "Likert" && (opt?.wa === null || opt?.wa === undefined)) {
                ctx.addIssue({
                  code: z.ZodIssueCode.custom,
                  message: "Wa is required for Likert options",
                  path: [...pathBase, "options", oIndex, "wa"],
                });
              }
            }
          }
        }

        // Question-type specific requirements (independent of template type).
        if (qType === "Likert") {
          if (q.ws === null || q.ws === undefined) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "Ws (question weight) is required for Likert questions",
              path: [...pathBase, "ws"],
            });
          }
          if (!q.traitKey) {
            ctx.addIssue({
              code: z.ZodIssueCode.custom,
              message: "TraitKey is required for Likert questions",
              path: [...pathBase, "traitKey"],
            });
          }
        }
      }
    }
  });

export type AssessmentTemplateFormData = z.infer<typeof assessmentTemplateFormSchema>;


