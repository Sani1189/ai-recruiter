"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import EditJobPostClient from "@/components/pages/_recruiter/recruiter/jobs/edit/EditJobPostClient";
import BackButton from "@/components/ui/back-button";
import { useApi } from "@/hooks/useApi";

interface JobStepAssignment {
  stepNumber: number;
  existingStepName: string;
  existingStepVersion?: number;
  stepType?: string;
  isInterview?: boolean;
  interviewConfigurationName?: string;
  interviewConfigurationVersion?: number;
}

interface JobPost {
  name: string;
  version: number;
  jobTitle: string;
  jobType: string;
  experienceLevel: string;
  jobDescription: string;
  policeReportRequired?: boolean;
  maxAmountOfCandidatesRestriction?: number;
  minimumRequirements?: string[];
  status?: string;
  originCountryCode?: string | null;
  countryExposureCountryCodes?: string[];
  steps?: JobStepAssignment[];
  // Backend actually sends assignedSteps with nested structure
  assignedSteps?: Array<{
    stepNumber: number;
    status?: string;
    stepDetails: {
      name: string;
      version: number;
      stepType: string;
      isInterview: boolean;
      recruiterCompletesStepManually?: boolean;
      interviewConfigurationName?: string;
      interviewConfigurationVersion?: number;
    };
  }>;
  candidatesCount?: number;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export default function EditJobPage() {
  const params = useParams();
  const name = params.name as string;
  const version = params.version as string;
  const api = useApi();
  
  const [jobPost, setJobPost] = useState<JobPost | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchJobPost() {
      try {
        setLoading(true);
        const response = await api.get(`/job/${name}/${version}`);
        setJobPost(response.data || response);
        setError(null);
      } catch (err: any) {
        console.error("Error fetching job post:", err);
        setError(err.message || "Failed to load job post");
        setJobPost(null);
      } finally {
        setLoading(false);
      }
    }

    if (name && version) {
      fetchJobPost();
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

  return <EditJobPostClient jobPost={jobPost} />;
}

