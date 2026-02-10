import { UseFormReturn } from "react-hook-form";
import type { z } from "zod";

import JobPostCreationFormSchema from "@/schemas/job-posting";

export type JobPostCreationFormValues = z.infer<typeof JobPostCreationFormSchema>;
export type JobPostFormValues = Omit<JobPostCreationFormValues, "minimumRequirements"> & {
  minimumRequirements: string[];
};

// Use the full form type for all steps to preserve data across steps
export type JobPostDetailsStepProps = {
  form: UseFormReturn<JobPostFormValues, any, JobPostFormValues>;
};
