import { z } from "zod";
import {
  CANDIDATE_STEP_TYPES,
  PARTICIPANTS,
  RECRUITER_STEP_TYPES,
  STEP_TYPES,
} from "@/types/jobPostStep";

export const jobPostStepFormSchema = z.object({
  name: z.string().min(1, "Name is required").max(255, "Name must be less than 255 characters"),
  version: z.number().int().min(1, "Version must be at least 1"),
  participant: z.enum(PARTICIPANTS).default("Candidate"),
  stepType: z.enum(STEP_TYPES),
  showStepForCandidate: z.boolean().default(true),
  displayTitle: z.string().max(255, "Display title must be less than 255 characters").optional().nullable(),
  displayContent: z.string().optional().nullable(),
  showSpinner: z.boolean().default(false),
  interviewConfigurationName: z.string().optional().nullable(),
  interviewConfigurationVersion: z.number().int().min(1).optional().nullable(),
  promptName: z.string().optional().nullable(),
  promptVersion: z.number().int().min(1).optional().nullable(),
  assessmentTemplateName: z.string().optional().nullable(),
  assessmentTemplateVersion: z.number().int().min(1).optional().nullable(),
}).superRefine((data, ctx) => {
  // Keep step type aligned with participant (matches backend validation)
  const allowed = (data.participant === "Recruiter" ? RECRUITER_STEP_TYPES : CANDIDATE_STEP_TYPES) as readonly string[];

  if (!allowed.includes(data.stepType)) {
    ctx.addIssue({
      code: "custom",
      path: ["stepType"],
      message: "Step type is not valid for the selected participant",
    });
    }

  // Candidate steps are always visible to the candidate (ShowStepForCandidate is only meaningful for recruiter steps)
  if (data.participant === "Candidate" && data.showStepForCandidate === false) {
    ctx.addIssue({
      code: "custom",
      path: ["showStepForCandidate"],
      message: "Candidate steps are always visible to the candidate",
    });
  }

  if (data.participant === "Candidate" && data.stepType === "Interview" && !data.interviewConfigurationName) {
    ctx.addIssue({
      code: "custom",
    path: ["interviewConfigurationName"],
      message: "Interview Configuration is required when Step Type is 'Interview'",
    });
  }

  if (data.participant === "Candidate" && data.stepType === "Questionnaire" && !data.assessmentTemplateName) {
    ctx.addIssue({
      code: "custom",
      path: ["assessmentTemplateName"],
      message: "Questionnaire Template is required when Step Type is 'Questionnaire'",
    });
  }

  // Only Questionnaire steps can have assessmentTemplateName/Version (legacy field names retained for now)
  if (data.participant === "Recruiter" || data.stepType !== "Questionnaire") {
    if (data.assessmentTemplateName) {
      ctx.addIssue({
        code: "custom",
        path: ["assessmentTemplateName"],
        message: "Only Questionnaire steps can have a questionnaire template",
      });
    }
    if (data.assessmentTemplateVersion) {
      ctx.addIssue({
        code: "custom",
        path: ["assessmentTemplateVersion"],
        message: "Only Questionnaire steps can have a questionnaire template version",
      });
    }
  }

  if (!data.showStepForCandidate) {
    if (data.displayTitle) {
      ctx.addIssue({ code: "custom", path: ["displayTitle"], message: "Display title must be empty when Show Step is off" });
    }
    if (data.displayContent) {
      ctx.addIssue({ code: "custom", path: ["displayContent"], message: "Display content must be empty when Show Step is off" });
    }
    if (data.showSpinner) {
      ctx.addIssue({ code: "custom", path: ["showSpinner"], message: "Show spinner must be off when Show Step is off" });
    }
  }

  // Spinner is not configurable for candidate steps
  if (data.participant === "Candidate" && data.showSpinner) {
    ctx.addIssue({ code: "custom", path: ["showSpinner"], message: "Spinner is not supported for candidate steps" });
  }
});

export type JobPostStepFormData = z.infer<typeof jobPostStepFormSchema>;


