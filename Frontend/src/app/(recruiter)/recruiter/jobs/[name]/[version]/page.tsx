"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import CandidatesTable from "@/components/pages/_recruiter/recruiter/jobs/CandidatesTable";
import JobInfo from "@/components/pages/_recruiter/recruiter/jobs/JobInfo";
import BackButton from "@/components/ui/back-button";
import { useApi } from "@/hooks/useApi";
import { JobPost as JobPostType } from "@/types/v2/type.view";

interface JobPost {
  name: string;
  version: number;
  jobTitle: string;
  jobType: "FullTime" | "PartTime" | "Contract" | "Internship";
  experienceLevel: "Entry" | "Mid" | "Senior" | "Lead" | "Executive";
  jobDescription: string;
  policeReportRequired?: boolean;
  maxAmountOfCandidatesRestriction?: number;
  minimumRequirements?: string[];
  candidateCount?: number;
  status?: string;
  originCountryCode?: string | null;
  countryExposureCountryCodes?: string[];
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
  assignedSteps?: Array<{
    stepNumber: number;
    status: string;
    stepDetails: {
      name: string;
      version: number;
      stepType: string;
      isInterview: boolean;
    };
  }>;
}

interface JobPostCandidate {
  applicationId: string;
  candidateId: string;
  candidateSerial: string;
  candidateName: string;
  candidateEmail: string;
  appliedAt: string;
  completedAt?: string;
  status: string;
}

export default function JobPage() {
  const params = useParams();
  const name = params.name as string;
  const version = params.version as string;
  const api = useApi();
  
  const [jobPost, setJobPost] = useState<JobPost | null>(null);
  const [candidates, setCandidates] = useState<JobPostCandidate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchJobData = async (showLoading = true) => {
    try {
      if (showLoading) setLoading(true);
      
      // Fetch job post with candidate count
      const jobResponse = await api.get(`/job/${name}/${version}/with-candidates`);
      setJobPost(jobResponse.data || jobResponse);
      
      // Fetch candidates list
      const candidatesResponse = await api.get(`/job/${name}/${version}/candidates`);
      setCandidates(candidatesResponse.data || candidatesResponse || []);
      
      setError(null);
    } catch (err: any) {
      console.error("Error fetching job data:", err);
      setError(err.message || "Failed to load job data");
      setJobPost(null);
      setCandidates([]);
    } finally {
      if (showLoading) setLoading(false);
    }
  };

  useEffect(() => {
    if (name && version) {
      fetchJobData();
    }
  }, [name, version]);

  if (loading) {
    return (
      <div className="container py-8">
        <div className="flex items-center justify-center">
          Loading job post...
        </div>
      </div>
    );
  }

  if (error || !jobPost) {
    return (
      <div className="container py-8">
        <BackButton />
        <div className="mt-4 text-red-500">
          {error || "Job not found"}
        </div>
      </div>
    );
  }

  return (
    <div className="container space-y-8 py-8">
      <BackButton />
      <JobInfo jobPost={jobPost} candidates={candidates} onRefreshNeeded={() => fetchJobData(false)} />
        <CandidatesTable 
          candidates={candidates} 
          jobPost={{
            ...jobPost, 
            id: `${jobPost.name}-${jobPost.version}`,
            minimumRequirements: jobPost.minimumRequirements || [],
            candidatesCount: jobPost.candidateCount || 0,
            confirmed: true,
            steps: jobPost.assignedSteps?.map(step => ({
              stepNumber: step.stepNumber,
              existingStepName: step.stepDetails.name,
              existingStepVersion: step.stepDetails.version,
              useLatestVersion: false,
              displayName: step.stepDetails.name,
              displayVersion: step.stepDetails.version.toString(),
              stepType: step.stepDetails.stepType,
              isInterview: step.stepDetails.isInterview,
            })) || []
          } as JobPostType} 
        />
    </div>
  );
}
