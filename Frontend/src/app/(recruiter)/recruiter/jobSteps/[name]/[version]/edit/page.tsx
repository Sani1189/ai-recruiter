"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";

import { useApi } from "@/hooks/useApi";
import { JobPostStep } from "@/types/jobPostStep";
import JobStepForm from "@/components/pages/_recruiter/recruiter/jobSteps/JobStepForm";

export default function EditJobStepPage() {
  const params = useParams();
  const api = useApi();
  const [step, setStep] = useState<JobPostStep | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStep = async () => {
      try {
        const response = await api.get(`/JobStep/${params.name}/${params.version}`);
        setStep(response.data || response);
      } catch (error) {
        console.error('Failed to fetch job step:', error);
      } finally {
        setLoading(false);
      }
    };

    if (params.name && params.version) {
      fetchStep();
    }
  }, [params.name, params.version]);

  if (loading) {
    return (
      <div className="container py-8">
        <div className="text-center text-muted-foreground">Loading...</div>
      </div>
    );
  }

  if (!step) {
    return (
      <div className="container py-8">
        <div className="text-center text-muted-foreground">Job step not found</div>
      </div>
    );
  }

  return <JobStepForm step={step} mode="edit" />;
}

