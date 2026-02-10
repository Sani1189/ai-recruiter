import { CandidateNotificationType } from "@/enums";
import JobPostCreationForm from "@/schemas/job-posting";
import { UserProfileSchema } from "@/schemas/user-profile";
import type { z } from "zod";

/* ========= Leaf types ========= */

type Score = {
  avg: number;
  english: number;
  technical: number;
  communication: number;
  problemSolving: number;
};

type Feedback = {
  detailed: string;
  summary: string;
  strengths: string[];
  weaknesses: string[];
};

/* ========= Core entities ========= */

/** Top-Level Entity
 *
 * Represents a job posting in the system.
 *
 * One `JobPost` ==>> Many `Interviews`s
 * - One `JobPost` can be associated with many `Interview`s
 */
type JobPostFormShape = z.infer<typeof JobPostCreationForm>;

type JobPost = Omit<
  JobPostFormShape,
  "confirmed" | "steps" | "maxAmountOfCandidatesRestriction" | "jobType"
> & {
  id: string;
  name?: string;
  version?: number;
  createdAt: string;
  candidatesCount: number;
  jobType: JobPostCreationForm["jobType"] | string;
  tone?: string;
  focusArea?: string;
  probingDepth?: string;
  instructions?: string;
  duration?: number;
  interviewSteps?: string[];
  confirmed?: boolean;
  steps?: JobPostFormShape["steps"];
  maxAmountOfCandidatesRestriction?: number;
};

/** Top-Level Entity
 *
 * Represents a user profile in the system.
 *
 * One `UserProfile` ==>> One `Candidate`
 * - One `UserProfile` can be associated with only one `Candidate`
 */
interface UserProfile extends UserProfileSchema {
  id: string;
  resumeUrl: string;
}

/** First-Level Entity
 *
 * A `Candidate` will be created when a `UserProfile`
 * gets associated with a `JobPost` for the first time
 *
 * One `Candidate` ==>> One `UserProfile`
 * - One `Candidate` can be associated with only one `UserProfile`
 *
 * One `Candidate` ==>> Many `Interview`s
 * - One `Candidate` can be associated with many `Interview`s
 */
interface Candidate {
  id: string;
  userId: UserProfile["id"];
}

/** First-Level Entity
 *
 * An `Interview` will be created when a `Candidate`
 * attends a `JobPost` interview
 *
 * One `Interview` ==>> One `Candidate`
 * - One `Interview` can be associated with only one `Candidate`
 *
 * One `Interview` ==>> One `JobPost`
 * - One `Interview` can be associated with only one `JobPost`
 *
 *
 * `Interview` will be distinct with `Candidate` and `JobPost`
 */
interface Interview {
  id: string;
  jobPostId: JobPost["id"];
  candidateId: Candidate["id"];

  // Interview specific data
  score: Score;
  feedback: Feedback;
  interviewAudioUrl: string;
  interviewQuestions: string[];
  completedAt: string;
  duration: number;
  currentStepIndex: number;
}

/* TODO: Need refactor */
interface UserNotification {
  id: string;
  type: keyof typeof CandidateNotificationType;
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  actionUrl?: string;
}

type ConstantValueType = {
  value: string;
  text: string;
};

export type {
  Candidate,
  ConstantValueType,
  Feedback,
  Interview,
  JobPost,
  Score,
  UserNotification,
  UserProfile,
};
