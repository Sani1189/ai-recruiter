import { z } from "zod";

// Job post status (matches backend JobPostStatusEnum)
const jobPostStatusEnum = z.enum(["Draft", "Published", "Archived"]);

// Step 1: Job Post Details
const JobPostDetails = z.object({
  status: jobPostStatusEnum.default("Draft"),
  originCountryCode: z.string().max(2).optional().nullable(),
  countryExposureCountryCodes: z.array(z.string().max(2)).optional().default([]),
  jobTitle: z
    .string()
    .trim()
    .min(2, "Job title must be at least 2 characters")
    .max(200, "Job title must be at most 200 characters"),
  jobType: z.enum(["FullTime", "PartTime", "Contract", "Internship"]),
  experienceLevel: z.enum(["Entry", "Mid", "Senior", "Lead", "Executive"]),
  jobDescription: z
    .string()
    .trim()
    .min(10, "Job description must be at least 10 characters")
    .max(2000, "Job description must be at most 2000 characters"),
  minimumRequirements: z
    .array(
      z
        .string()
        .trim()
        .min(1, "Requirement cannot be empty")
        .max(100, "Requirement must be at most 100 characters"),
    )
    .min(1, "At least one minimum requirement is required")
    .max(20, "At most 20 minimum requirements are allowed"),
  policeReportRequired: z.boolean().default(false),
  maxAmountOfCandidatesRestriction: z
    .number()
    .int("Must be a whole number")
    .min(1, "Must be at least 1")
    .max(10000, "Cannot exceed 10,000")
    .default(1000),
});
type JobPostDetails = z.infer<typeof JobPostDetails>;

// Step 2: Job Steps Assignment
const JobStepAssignment = z.object({
  stepNumber: z.number().int().min(1),
  existingStepName: z.string().min(1, "Step name is required"),
  existingStepVersion: z.number().int().min(1).optional(), // Optional - if not provided, use latest
  useLatestVersion: z.boolean().default(true), // UI helper field
  // Additional fields for display purposes (not sent to backend)
  displayName: z.string().optional(),
  displayVersion: z.string().optional(),
  stepType: z.string().optional(),
  isInterview: z.boolean().optional(),
  interviewConfigName: z.string().optional(),
  interviewConfigVersion: z.number().optional(),
});
type JobStepAssignment = z.infer<typeof JobStepAssignment>;

const JobSteps = z.object({
  steps: z
    .array(JobStepAssignment)
    // MANDATORY: At least one step required
    .min(1, "At least one job step is required")
    // OPTIONAL: To make steps optional, change .min(1, ...) to:
    // .min(0, "You can add job steps or leave empty")
    .max(20, "Maximum 20 steps allowed"),
});
type JobSteps = z.infer<typeof JobSteps>;

// Step 3: Confirmation (no additional fields, just review)
const Confirmation = z.object({
  confirmed: z.boolean().default(false),
  // For update mode: whether to create a new version or update existing
  shouldUpdateVersion: z.boolean().default(false).optional(),
});
type Confirmation = z.infer<typeof Confirmation>;

// Complete form schema
const JobPostCreationForm = z.object({
  ...JobPostDetails.shape,
  ...JobSteps.shape,
  ...Confirmation.shape,
});
type JobPostCreationForm = z.infer<typeof JobPostCreationForm>;

export default JobPostCreationForm;
export { JobPostDetails, JobSteps, Confirmation };
export type { JobStepAssignment };
