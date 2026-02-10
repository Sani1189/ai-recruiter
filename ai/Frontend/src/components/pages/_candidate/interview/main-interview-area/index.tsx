"use client";

import { useState } from "react";

import InterviewControls from "./InterviewControls";
import InterviewHeader from "./InterviewHeader";
import ResumeUploadArea from "./ResumeUploadArea";

import { InterviewView } from "@/types/v2/type.view";

export default function MainInterviewArea({
  interview,
  conversationToken,
}: {
  interview: InterviewView;
  conversationToken: string;
}) {
  const [isInterviewStartable, setIsInterviewStartable] = useState(false);
  const [interviewStarted, setInterviewStarted] = useState(false);

  // Resume upload step is a fixed step in the candidate flow
  const jobPostName = interview.jobPost?.name;
  const jobPostVersion = interview.jobPost?.version;
  const resumeUploadStepName = "resume upload";

  if (!jobPostName || !jobPostVersion) {
    return (
      <div className="space-y-4 lg:col-span-2">
        <InterviewHeader
          jobTitle={interview.jobPost.jobTitle}
          jobType={interview.jobPost.jobType}
          experienceLevel={interview.jobPost.experienceLevel}
          tone={(interview.jobPost as any).tone}
          focusArea={(interview.jobPost as any).focusArea}
        />
        <div className="rounded-md border p-4 text-sm text-muted-foreground">
          Job post identifiers are missing (name/version). Resume upload cannot be initialized.
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 lg:col-span-2">
      <InterviewHeader
        jobTitle={interview.jobPost.jobTitle}
        jobType={interview.jobPost.jobType}
        experienceLevel={interview.jobPost.experienceLevel}
        tone={(interview.jobPost as any).tone}
        focusArea={(interview.jobPost as any).focusArea}
      />
      <ResumeUploadArea
        setIsInterviewStartable={setIsInterviewStartable}
        interviewStarted={interviewStarted}
        jobPost={{ name: jobPostName, version: jobPostVersion }}
        stepName={resumeUploadStepName}
        stepVersion={null}
      />
      <InterviewControls
        isInterviewStartable={isInterviewStartable}
        setInterviewStarted={setInterviewStarted}
        jobPost={interview}
        conversationToken={conversationToken}
      />
    </div>
  );
}
