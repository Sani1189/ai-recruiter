import { CandidateNotificationType } from "@/enums";

export type EvnVariablesType = {
  NEXT_PUBLIC_AGENT_ID: string;
  ELEVENLABS_API_KEY: string;
  AGENT_ID: string;
  OPENAI_API_KEY: string;
  VERCEL_PROJECT_PRODUCTION_URL: string;

  // API Configuration
  NEXT_PUBLIC_API_BASE_URL: string;
  NEXT_PUBLIC_SAAS_API_BASE_URL: string;

  // Azure AD Configuration (Recruiter/Admin Authentication)
  NEXT_PUBLIC_AZURE_AD_CLIENT_ID: string;
  NEXT_PUBLIC_AZURE_AD_TENANT_ID: string;
  NEXT_PUBLIC_AZURE_AD_AUTHORITY: string;
  NEXT_PUBLIC_AZURE_AD_SCOPE: string;
  NEXT_PUBLIC_REDIRECT_URI: string;

  // Azure B2C Configuration (Candidate Authentication)
  NEXT_PUBLIC_B2C_CLIENT_ID: string;
  NEXT_PUBLIC_B2C_TENANT_ID: string;
  NEXT_PUBLIC_B2C_AUTHORITY: string;
  NEXT_PUBLIC_B2C_API_SCOPE: string;
};

export interface UserProfile {
  id: string;
  name: string | null;
  email: string | null;
  phoneNumber: string | null;
  age: number | null;
  nationality: string | null;
  profilePictureUrl: string | null;
  bio: string | null;
  resumeUrl?: string | null;
  jobTypePreferences: string[] | null;
  remotePreferences: string[] | null;
  openToRelocation: boolean | null;
  createdAt: string | null;
  updatedAt?: string | null;
  roles?: string[];
  education?: Education[];
  workExperience?: Experience[];
  projects?: Project[];
  speakingLanguages?: LanguageSkill[];
  programmingLanguages?: LanguageSkill[];
}

export interface Education {
  id?: string;
  userProfileId?: string;
  degree: string | null;
  fieldOfStudy: string | null;
  institution: string | null;
  graduationYear: number | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}

export interface Skill {
  id?: string;
  userProfileId?: string;
  skillName: string | null;
  proficiencyLevel: string | null;
  yearsOfExperience: number | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}

export interface Experience {
  id?: string;
  userProfileId?: string;
  company?: string | null;
  jobTitle?: string | null;
  role?: string | null;
  startDate: string | null;
  endDate: string | null;
  description?: string | null;
  responsibilities?: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}

export interface Project {
  id?: string;
  userProfileId?: string;
  title: string | null;
  description: string | null;
  technologies: string[] | null;
  url: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}

export interface LanguageSkill {
  id?: string;
  userProfileId?: string;
  language: string | null;
  proficiency: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
}
export interface UserNotification {
  id: string;
  type: keyof typeof CandidateNotificationType;
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  actionUrl?: string;
}
