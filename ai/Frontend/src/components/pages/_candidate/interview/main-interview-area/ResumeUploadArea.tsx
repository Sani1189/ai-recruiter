"use client";

import { FileText, Loader2, Upload, X } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import {
  FileUpload,
  FileUploadDropzone,
  FileUploadItem,
  FileUploadItemDelete,
  FileUploadItemMetadata,
  FileUploadItemPreview,
  FileUploadList,
  FileUploadTrigger,
} from "@/components/ui/file-upload";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { InterviewSessionForm } from "@/schemas/interview-session";
import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useAuthStore } from "@/stores/useAuthStore";
import { useApi } from "@/hooks/useApi";
import { toast } from "sonner";
import { uploadToSasUrl } from "@/lib/fileUtils";

type ResumeUploadAreaProps = {
  setIsInterviewStartable: (startable: boolean) => void;
  interviewStarted: boolean;
  onBeforeSubmit?: () => Promise<boolean> | boolean;
  onUploadComplete?: (result: any) => void;
  jobPost: {
    name: string;
    version: number;
  };
  stepName: string;
  stepVersion?: number | null; // Optional - null means use latest version
  isCompleted?: boolean;
  displayTitle?: string | null;
  displayContent?: string | null;
  showSpinner?: boolean;
};
export default function ResumeUploadArea({
  setIsInterviewStartable,
  interviewStarted,
  onBeforeSubmit,
  onUploadComplete,
  jobPost,
  stepName,
  stepVersion,
  isCompleted = false,
  displayTitle = null,
  displayContent = null,
  showSpinner = false,
}: ResumeUploadAreaProps) {
  const [validated, setValidated] = useState(false);
  const [uploading, setUploading] = useState(false);
  const api = useApi();

  const form = useForm<InterviewSessionForm>({
    resolver: zodResolver(InterviewSessionForm),
    defaultValues: {
      termsOfService: false,
      resumeFile: [],
    },
  });

  const onSubmit = async (data: InterviewSessionForm) => {
    if (onBeforeSubmit) {
      const allowed = await Promise.resolve(onBeforeSubmit());
      if (!allowed) return;
    }

    if (!data.resumeFile || data.resumeFile.length === 0) {
      toast.error("Please select a file to upload");
      return;
    }

    if (!jobPost?.name || !jobPost?.version) {
      toast.error("Job post information is required");
      return;
    }

    if (!stepName) {
      toast.error("Step name is required");
      return;
    }

    const file = data.resumeFile[0];
    setUploading(true);

    try {
      // Get upload URL - unified method for all step types
      const uploadUrlResponse = await api.post('/candidate/job-application/steps/upload/get-upload-url', {
        jobPostName: jobPost.name,
        jobPostVersion: jobPost.version,
        stepName,
        stepVersion: stepVersion ?? null, // null means use latest version
        fileName: file.name,
        contentType: file.type || 'application/pdf',
        fileSize: file.size,
      });

      const uploadData = (uploadUrlResponse as any)?.data ?? uploadUrlResponse;
      if (!uploadData.uploadUrl || !uploadData.blobPath) {
        throw new Error('Failed to get upload URL from server');
      }

      // Upload directly to Azure using SAS URL
      await uploadToSasUrl(file, uploadData.uploadUrl, (progress) => {
        // Progress tracking can be added here if needed
      });

      // Notify backend that upload is complete - unified method for all step types
      const completeResponse = await api.post('/candidate/job-application/steps/upload/complete', {
        jobPostName: jobPost.name,
        jobPostVersion: jobPost.version,
        stepName,
        stepVersion: stepVersion ?? null, // null means use latest version
        blobPath: uploadData.blobPath,
        originalFileName: file.name,
        contentType: file.type || 'application/pdf',
        fileSize: file.size,
      });

      const completeResult = (completeResponse as any)?.data ?? completeResponse;

      if (completeResult?.jobApplicationStepId) {
        toast.success("Resume uploaded successfully", {
          description: "Your resume has been saved and you can now proceed to the interview.",
        });
        onUploadComplete?.(completeResult);
        setIsInterviewStartable(true);
        setValidated(true);
      }
    } catch (error) {
      console.error('Failed to upload resume:', error);
      const errorMessage = error instanceof Error ? error.message : "Please try again or contact support if the issue persists.";
      toast.error("Failed to upload resume", {
        description: errorMessage,
      });
    } finally {
      setUploading(false);
    }
  };

  const handleFileChange = (files: File[]) => {
    // if interview has started, do not allow file changes
    if (interviewStarted) {
      return;
    }

    // if files length is 0 (files are removed)
    if (files.length === 0) {
      // Only reset if resume hasn't been uploaded successfully yet
      if (!validated) {
        setIsInterviewStartable(false);
        setValidated(false);
        form.reset();
      }
      // Don't reset isInterviewStartable if resume was already uploaded successfully
      return;
    }

    form.setValue("resumeFile", files);
  };

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileText className="text-brand h-5 w-5" />
          {displayTitle || "Upload Your Resume"}
          {showSpinner && !isCompleted && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
        </CardTitle>
        {displayContent && (
          <p className="text-sm text-muted-foreground whitespace-pre-wrap">{displayContent}</p>
        )}
      </CardHeader>

      <CardContent>
        {isCompleted ? (
          <div className="py-6">
            <p className="text-green-700 font-medium">Resume step completed successfully.</p>
            <p className="text-sm text-muted-foreground">You can proceed to the next step.</p>
          </div>
        ) : (
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(onSubmit)}
            className="w-full space-y-4"
          >
            <FormField
              control={form.control}
              name="resumeFile"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Resume File</FormLabel>

                  <FormControl>
                    <FileUpload
                      value={field.value}
                      onValueChange={handleFileChange}
                      accept="application/pdf"
                      maxFiles={1}
                      maxSize={5 * 1024 * 1024}
                      onFileReject={(_, message) => {
                        form.setError(field.name, {
                          message,
                        });
                      }}
                    >
                      {field.value.length === 0 && (
                        <FileUploadDropzone className="py-12">
                          <div className="flex flex-col items-center gap-1 text-center">
                            <div className="flex items-center justify-center rounded-full border p-2.5">
                              <Upload className="text-muted-foreground size-6" />
                            </div>

                            <p className="text-sm font-medium">
                              Drag & drop files here
                            </p>

                            <p className="text-muted-foreground text-xs">
                              Or click to browse (max 1 file, up to 5MB each)
                            </p>
                          </div>

                          <FileUploadTrigger asChild>
                            <Button
                              variant="outline"
                              size="sm"
                              className="mt-2 w-fit"
                            >
                              Browse files
                            </Button>
                          </FileUploadTrigger>
                        </FileUploadDropzone>
                      )}

                      <FileUploadList>
                        {field.value.map((file, index) => (
                          <FileUploadItem key={index} value={file}>
                            <FileUploadItemPreview />
                            <FileUploadItemMetadata />
                            {!interviewStarted && (
                              <FileUploadItemDelete asChild>
                                <Button
                                  variant="ghost"
                                  size="icon"
                                  className="size-7"
                                >
                                  <X />
                                  <span className="sr-only">Delete</span>
                                </Button>
                              </FileUploadItemDelete>
                            )}
                          </FileUploadItem>
                        ))}
                      </FileUploadList>
                    </FileUpload>
                  </FormControl>

                  <FormMessage />
                </FormItem>
              )}
            />

            {!validated && (
              <FormField
                control={form.control}
                name="termsOfService"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="hover:bg-accent/50 has-[[aria-checked=true]]:border-primary has-[[aria-checked=true]]:bg-primary/5 dark:has-[[aria-checked=true]]:border-primary dark:has-[[aria-checked=true]]:bg-primary flex items-start gap-3 rounded-lg border p-3">
                      <FormControl>
                        <Checkbox
                          className="data-[state=checked]:border-primary data-[state=checked]:bg-primary dark:data-[state=checked]:border-primary dark:data-[state=checked]:bg-primary data-[state=checked]:text-white"
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>

                      <div className="grid gap-1.5 font-normal">
                        <p className="text-sm leading-none font-medium">
                          I have read and agree to the{" "}
                          <Link
                            href="/terms-of-service"
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-primary hover:underline"
                          >
                            Terms of Service
                          </Link>
                        </p>

                        <p className="text-muted-foreground text-sm">
                          You must accept the terms of service to proceed.
                        </p>
                      </div>
                    </FormLabel>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            {!validated && (
              <Button
                type="submit"
                className="mt-4"
                disabled={form.formState.isSubmitting || uploading}
              >
                {(form.formState.isSubmitting || uploading) && (
                  <Loader2 className="h-4 w-4 animate-spin mr-2" />
                )}
                {uploading ? 'Uploading...' : 'Submit'}
              </Button>
            )}
          </form>
        </Form>
        )}
      </CardContent>
    </Card>
  );
}
