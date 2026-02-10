"use client";

import { useState, useMemo } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import JobPostForm from "@/components/JobPostForm";
import BackButton from "@/components/ui/back-button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { useApi } from "@/hooks/useApi";
import { handleApiError, hasErrorCode } from "@/lib/api/errorHandler";

import JobPostCreationForm, { JobStepAssignment } from "@/schemas/job-posting";

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
  steps?: Array<{
    stepNumber: number;
    existingStepName: string;
    existingStepVersion?: number;
    stepType?: string;
    isInterview?: boolean;
    interviewConfigurationName?: string;
    interviewConfigurationVersion?: number;
  }>;
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
}

export default function EditJobPostClient({ jobPost }: { jobPost: JobPost }) {
  const router = useRouter();
  const api = useApi();
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [pendingData, setPendingData] = useState<JobPostCreationForm | null>(null);
  const [pendingPromise, setPendingPromise] = useState<{
    resolve: (value: void) => void;
    reject: (reason?: any) => void;
  } | null>(null);

  const handleFormSubmit = async (data: JobPostCreationForm): Promise<void> => {
    // Return a promise that resolves/rejects based on user's confirmation dialog action
    return new Promise((resolve, reject) => {
      setPendingData(data);
      setPendingPromise({ resolve, reject });
      setShowConfirmDialog(true);
    });
  };

  const handleConfirmUpdate = async () => {
    if (!pendingData || !pendingData.jobTitle) {
      toast.error("Form data is incomplete. Please try again.");
      pendingPromise?.reject(new Error('Incomplete form data'));
      return;
    }
    
    setShowConfirmDialog(false);

    try {
      toast.loading("Updating job post...");

      const payload = {
        name: jobPost.name,
        version: jobPost.version,
        jobTitle: pendingData.jobTitle,
        jobType: pendingData.jobType,
        experienceLevel: pendingData.experienceLevel,
        jobDescription: pendingData.jobDescription,
        maxAmountOfCandidatesRestriction: pendingData.maxAmountOfCandidatesRestriction,
        minimumRequirements: pendingData.minimumRequirements,
        policeReportRequired: pendingData.policeReportRequired,
        status: pendingData.status ?? "Draft",
        originCountryCode: pendingData.originCountryCode ?? null,
        countryExposureCountryCodes:
          pendingData.status === "Published"
            ? (pendingData.countryExposureCountryCodes ?? []).filter(Boolean)
            : [],
        steps: pendingData.steps?.map((step) => ({
          stepNumber: step.stepNumber,
          existingStepName: step.existingStepName,
          ...(step.useLatestVersion ? {} : { existingStepVersion: step.existingStepVersion }),
        })) || [],
        shouldUpdateVersion: pendingData.shouldUpdateVersion || false,
      };

      const response = await api.put(`/job/${jobPost.name}/${jobPost.version}`, payload);
      const updatedJob = response.data || response;

      toast.dismiss();
      toast.success("Job Post Updated Successfully!", {
        description: pendingData.shouldUpdateVersion
          ? `New version ${updatedJob.version} created.`
          : "Job post has been updated.",
      });

      // Resolve the promise to indicate success
      pendingPromise?.resolve();

      setTimeout(() => router.push("/recruiter/jobs"), 1500);
    } catch (error) {
      toast.dismiss();

      if (hasErrorCode(error, "JOB_POST_HAS_APPLICATIONS")) {
        toast.error("This job has applications and cannot be updated. Create a new version instead.");
        pendingPromise?.reject(error);
        return;
      }

      handleApiError(error, {
        customMessages: {
          'DUPLICATE_ENTRY': 'A job post with this configuration already exists.',
          'NOT_FOUND': 'Job post not found. It may have been deleted.',
        },
        defaultMessage: 'Failed to update job post. Please try again.',
      });
      pendingPromise?.reject(error);
    }
  };

  const handleCancelDialog = () => {
    setShowConfirmDialog(false);
    // Reject the promise when user cancels
    pendingPromise?.reject(new Error('User cancelled'));
  };

  // Transform backend steps to form format - useMemo to avoid recreating on every render
  const defaultValues = useMemo<JobPostCreationForm>(() => {
    const stepsData = jobPost.assignedSteps || jobPost.steps || [];
    const transformedSteps = stepsData.map((step: any) => {
      const stepInfo = step.stepDetails || step;
      return {
        stepNumber: step.stepNumber,
        existingStepName: stepInfo.name || step.existingStepName,
        existingStepVersion: stepInfo.version || step.existingStepVersion,
        useLatestVersion: !(stepInfo.version || step.existingStepVersion),
        displayName: stepInfo.name || step.existingStepName,
        displayVersion: (stepInfo.version || step.existingStepVersion) 
          ? `v${stepInfo.version || step.existingStepVersion}` 
          : "Latest",
        stepType: stepInfo.stepType || step.stepType,
        isInterview: stepInfo.isInterview ?? step.isInterview,
        interviewConfigName: stepInfo.interviewConfigurationName || step.interviewConfigurationName,
        interviewConfigVersion: stepInfo.interviewConfigurationVersion || step.interviewConfigurationVersion,
      };
    });

    const statusMap = ["Draft", "Published", "Archived"] as const;
    const rawStatus = jobPost.status;
    const status: "Draft" | "Published" | "Archived" =
      typeof rawStatus === "number"
        ? statusMap[rawStatus] ?? "Draft"
        : (rawStatus as "Draft" | "Published" | "Archived") ?? "Draft";

    const raw = jobPost as unknown as Record<string, unknown>;
    return {
      status,
      originCountryCode: (raw.originCountryCode ?? raw.OriginCountryCode ?? undefined) as string | undefined,
      countryExposureCountryCodes: (raw.countryExposureCountryCodes ?? raw.CountryExposureCountryCodes ?? []) as string[],
      jobTitle: jobPost.jobTitle,
      jobType: jobPost.jobType as any,
      experienceLevel: jobPost.experienceLevel as any,
      jobDescription: jobPost.jobDescription,
      minimumRequirements: jobPost.minimumRequirements || [""],
      policeReportRequired: jobPost.policeReportRequired || false,
      maxAmountOfCandidatesRestriction: jobPost.maxAmountOfCandidatesRestriction || 1000,
      steps: transformedSteps,
      confirmed: false,
      shouldUpdateVersion: false,
    };
  }, [jobPost]);

  return (
    <>
      <div className="container max-w-5xl py-8">
        <div className="mb-8">
          <BackButton />

          <h1 className="mb-2 text-3xl font-bold">Edit Job Post</h1>
          <p className="text-muted-foreground">
            Update your job post configuration. You can choose to update the existing version or create a new version.
          </p>
        </div>

        <JobPostForm
          variant="edit"
          onSubmit={handleFormSubmit}
          defaultValues={defaultValues}
        />
      </div>

      {/* Confirmation Dialog */}
      <AlertDialog open={showConfirmDialog} onOpenChange={(open) => {
        if (!open) handleCancelDialog();
      }}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Confirm Job Post Update</AlertDialogTitle>
            <AlertDialogDescription>
              {pendingData?.shouldUpdateVersion ? (
                <>
                  You are about to create a <strong>new version</strong> of this job post.
                  The current version will remain unchanged, and a new version will be created.
                </>
              ) : (
                <>
                  You are about to update the <strong>existing version</strong> of this job post.
                  This will modify the current version {jobPost.version}.
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={handleCancelDialog}>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmUpdate}>
              {pendingData?.shouldUpdateVersion ? "Create New Version" : "Update Existing Version"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
