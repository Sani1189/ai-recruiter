import { Candidate, Interview, JobPost, Score, UserProfile } from "./type";

/* ========= View models (app-level rich shapes for UI) ========= */

interface CandidateView extends Omit<Candidate, "userId"> {
  userProfile: UserProfile; // hydrated from "userId"
  score: Score; // average score across all interviews
  comment?: Comment; // top-level comment for candidate (if any)
}

interface Comment {
  content: string;
  entityId: string;
  entityType: number;
  parentCommentId: string | null;
  id: string;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
}
interface InterviewView
  extends Omit<Interview, "jobPostId" | "candidateId" | "currentStepIndex"> {
  jobPost: JobPost; // hydrated from "jobPostId"
  candidate: CandidateView; // hydrated from "candidateId"
  currentStep: any; // hydrated from "currentStepIndex"
  comment?: Comment;
}

export * from "./type";
export type { CandidateView, InterviewView, Comment };
