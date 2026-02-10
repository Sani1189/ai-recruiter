"use client";

import { useState } from "react";
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
import { handleApiError, hasErrorCode, isErrorType } from "@/lib/api/errorHandler";

import { getInterviewLink } from "@/lib/job";
import JobPostCreationForm from "@/schemas/job-posting";

export default function CreatePage() {
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

  const handleConfirmCreate = async () => {
    if (!pendingData || !pendingData.jobTitle) {
      toast.error("Form data is incomplete. Please try again.");
      pendingPromise?.reject(new Error('Incomplete form data'));
      return;
    }

    setShowConfirmDialog(false);

    try {
      toast.loading("Creating job post...");

      // Transform frontend form data to backend DTO structure
      const jobPostDto = {
        name: pendingData.jobTitle
          .toLowerCase()
          .replace(/[^a-z0-9]+/g, '-')
          .replace(/^-+|-+$/g, ''),
        version: 1,
        jobTitle: pendingData.jobTitle,
        jobType: pendingData.jobType,
        experienceLevel: pendingData.experienceLevel,
        jobDescription: pendingData.jobDescription,
        maxAmountOfCandidatesRestriction: pendingData.maxAmountOfCandidatesRestriction,
        minimumRequirements: pendingData.minimumRequirements,
        policeReportRequired: pendingData.policeReportRequired,
        status: pendingData.status ?? "Draft",
        originCountryCode: pendingData.originCountryCode || null,
        countryExposureCountryCodes:
          pendingData.status === "Published"
            ? (pendingData.countryExposureCountryCodes ?? []).filter(Boolean)
            : [],
        steps: pendingData.steps?.map((step) => ({
          stepNumber: step.stepNumber,
          existingStepName: step.existingStepName,
          ...(step.useLatestVersion ? {} : { existingStepVersion: step.existingStepVersion }),
        })) || [],
      };

      // Call the API
      const response = await api.post("/job", jobPostDto);
      const createdJob = response.data || response;

      toast.dismiss();
      toast.success("Job Post Created Successfully!", {
        description: `Job "${createdJob.jobTitle}" has been created.`,
      });

      // Copy interview link to clipboard
      const interviewLink = getInterviewLink(createdJob.name, createdJob.version);
      navigator.clipboard.writeText(interviewLink);

      // Resolve the promise to indicate success
      pendingPromise?.resolve();

      // Redirect to jobs page
      setTimeout(() => {
        router.push("/recruiter/jobs");
      }, 1000);
    } catch (error: any) {
      toast.dismiss();
      
      // Use centralized error handler with custom messages for specific scenarios
      handleApiError(error, {
        customMessages: {
          'DUPLICATE_ENTRY': 'A job post with this name and version already exists. Please use a different job title or update the existing job post.',
          'INVALID_ARGUMENT': 'Please check your input and try again.',
          'VALIDATION_ERROR': 'Please correct the validation errors and try again.',
        },
        customTitles: {
          'DUPLICATE_ENTRY': 'Job Post Already Exists',
          'INVALID_ARGUMENT': 'Invalid Input',
          'VALIDATION_ERROR': 'Validation Failed',
        },
        defaultMessage: 'Failed to create job post. Please try again.',
      });

      // Reject the promise to preserve form values
      pendingPromise?.reject(error);
    }
  };

  const handleCancelDialog = () => {
    setShowConfirmDialog(false);
    // Reject the promise when user cancels
    pendingPromise?.reject(new Error('User cancelled'));
  };

  const defaultValues: JobPostCreationForm = {
    status: "Draft",
    originCountryCode: undefined,
    countryExposureCountryCodes: [],
    jobTitle: "",
    jobType: "FullTime",
    experienceLevel: "Mid",
    jobDescription: "",
    minimumRequirements: [""],
    policeReportRequired: false,
    maxAmountOfCandidatesRestriction: 1000,
    steps: [],
    confirmed: false,
  };

  return (
    <>
      <div className="container max-w-5xl py-8">
        <div className="mb-8">
          <BackButton />

          <h1 className="mb-2 text-3xl font-bold">Create New Job Post</h1>
          <p className="text-muted-foreground">
            Configure your AI interview parameters and generate a shareable link
          </p>
        </div>

        <JobPostForm
          variant="create"
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
            <AlertDialogTitle>Confirm Job Post Creation</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to create this job post? This will make it available
              for candidates to apply.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={handleCancelDialog}>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmCreate}>
              Yes, Create Job Post
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
