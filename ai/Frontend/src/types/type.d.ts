import { CandidateNotificationType } from "@/enums";
import JobPostCreationForm from "@/schemas/job-posting";
import { UserProfileSchema } from "@/schemas/user-profile";

type Prettify<T> = {
  [K in keyof T]: T[K];
} & {};

type EvnVariablesType = {
  VERCEL_PROJECT_PRODUCTION_URL: string;
  OPENAI_API_KEY: string;
  ELEVENLABS_API_KEY: string;
};

interface JobPost extends JobPostCreationForm {
  id: string;
  createdAt: string;
  candidatesCount: number;
}

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

interface Interview extends JobPost {
  id: string;
  score: Score;
  feedback: Feedback;
  interviewAudioUrl: string;
  interviewQuestions: string[];
  completedAt: string;
  duration: number;
  currentStep: JobPost["interviewSteps"][number]; // importing it from `JobPost` indicates that we need to check against the defined steps everytime we set or modify `currentStep`
}

interface UserProfile extends UserProfileSchema {
  id: string;
  resumeUrl: string;
  roles?: string[];
}

interface Candidate {
  id: string;
  profile: UserProfile;
  interviews: Interview[];
  score: Score;
  feedback: Feedback;
}

interface UserNotification {
  id: string;
  type: keyof typeof CandidateNotificationType;
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  actionUrl?: string;
}

export type {
  Candidate,
  EvnVariablesType,
  Interview,
  JobPost,
  Prettify,
  UserNotification,
  UserProfile,
};
