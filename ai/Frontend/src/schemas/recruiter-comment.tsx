import { z } from "zod";

const CandidateCommentForm = z.object({
  comment: z
    .string()
    .trim()
    .min(10, {
      message: "Comment must be at least 10 characters.",
    })
    .max(160, {
      message: "Comment must not be longer than 160 characters.",
    }),
});
type CandidateCommentForm = z.infer<typeof CandidateCommentForm>;

export { CandidateCommentForm };
