import { z } from "zod";

const InterviewSessionForm = z.object({
  termsOfService: z.boolean().refine((value) => value === true, {
    message: "You must accept the terms of service",
  }),
  resumeFile: z
    .array(z.custom<File>()) // Array to allow for future expansion
    .length(1, "You must upload exactly one resume file")
    .refine(
      (files) =>
        files.every(
          (file) => file.size <= 5 * 1024 * 1024, // 5 MB
        ),
      {
        message: "Resume file must be at most 5 MB",
        path: ["resumeFile"],
      },
    ),
});
type InterviewSessionForm = z.infer<typeof InterviewSessionForm>;

export { InterviewSessionForm };
