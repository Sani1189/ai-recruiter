"use client";

import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Checkbox } from "@/components/ui/checkbox";
import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
} from "@/components/ui/form";
import { CheckCircle2, ExternalLink } from "lucide-react";

import { JobPostDetailsStepProps } from "./type";
import { JobStepAssignment } from "@/schemas/job-posting";

interface ConfirmationStepProps extends JobPostDetailsStepProps {
  variant?: "create" | "edit";
}

export default function ConfirmationStep({ form, variant = "create" }: ConfirmationStepProps) {
  const formData = form.getValues();

  return (
    <div className="space-y-6">
      <div className="text-center space-y-2">
        <div className="flex justify-center">
          <CheckCircle2 className="h-12 w-12 text-primary" />
        </div>
        <h3 className="text-2xl font-bold">Review Your Job Post</h3>
        <p className="text-muted-foreground">
          Please review all the information before creating the job post
        </p>
      </div>

      <Separator />

      {/* Job Post Details Section */}
      <Card>
        <CardHeader>
          <CardTitle>Job Post Details</CardTitle>
          <CardDescription>Basic information about the job posting</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <p className="text-sm font-medium text-muted-foreground">Job Title</p>
              <p className="text-base font-semibold">{formData.jobTitle || "—"}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">Job Type</p>
              <Badge variant="secondary">{formData.jobType || "—"}</Badge>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">
                Experience Level
              </p>
              <Badge variant="outline">{formData.experienceLevel || "—"}</Badge>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">
                Max Candidates
              </p>
              <p className="text-base font-semibold">
                {formData.maxAmountOfCandidatesRestriction || 0}
              </p>
            </div>
          </div>

          <div>
            <p className="text-sm font-medium text-muted-foreground mb-2">
              Job Description
            </p>
            <p className="text-sm bg-muted/50 p-3 rounded-md whitespace-pre-wrap">
              {formData.jobDescription || "—"}
            </p>
          </div>

          <div>
            <p className="text-sm font-medium text-muted-foreground mb-2">
              Minimum Requirements
            </p>
            <ul className="list-disc list-inside space-y-1">
              {formData.minimumRequirements?.map((req: string, index: number) => (
                <li key={index} className="text-sm">
                  {req}
                </li>
              )) || <li className="text-sm text-muted-foreground">None</li>}
            </ul>
          </div>

          <div className="flex items-center gap-2">
            <p className="text-sm font-medium text-muted-foreground">
              Police Report Required:
            </p>
            <Badge variant={formData.policeReportRequired ? "default" : "secondary"}>
              {formData.policeReportRequired ? "Yes" : "No"}
            </Badge>
          </div>
        </CardContent>
      </Card>

      {/* Job Steps Section */}
      <Card>
        <CardHeader>
          <CardTitle>Job Steps</CardTitle>
          <CardDescription>
            Steps assigned to this job post ({formData.steps?.length || 0} steps)
          </CardDescription>
        </CardHeader>
        <CardContent>
          {formData.steps && formData.steps.length > 0 ? (
            <div className="border rounded-lg">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-16">#</TableHead>
                    <TableHead>Step Name</TableHead>
                    <TableHead>Version</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Interview Config</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {formData.steps.map((step: JobStepAssignment, index: number) => (
                    <TableRow key={index}>
                      <TableCell className="font-medium">{step.stepNumber}</TableCell>
                      <TableCell className="font-mono text-sm">
                        {step.displayName}
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{step.displayVersion}</Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary">{step.stepType}</Badge>
                      </TableCell>
                      <TableCell>
                        {step.isInterview && step.interviewConfigName ? (
                          <a
                            href={`/recruiter/interviewConfigurations/${step.interviewConfigName}/${step.interviewConfigVersion}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="flex items-center gap-1 text-sm text-primary hover:underline"
                          >
                            {step.interviewConfigName} (v
                            {step.interviewConfigVersion})
                            <ExternalLink className="h-3 w-3" />
                          </a>
                        ) : (
                          <span className="text-sm text-muted-foreground">—</span>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground border rounded-lg border-dashed">
              No job steps added
            </div>
          )}
        </CardContent>
      </Card>

      {/* Version Update Option (Edit Mode Only) */}
      {variant === "edit" && (
        <Card className="border-primary/20 bg-primary/5">
          <CardHeader>
            <CardTitle className="text-lg">Version Update Option</CardTitle>
            <CardDescription>
              Choose how to update this job post
            </CardDescription>
          </CardHeader>
          <CardContent>
            <FormField
              control={form.control}
              name="shouldUpdateVersion"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border border-primary/20 bg-background p-4">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <div className="space-y-1 leading-none">
                    <FormLabel className="text-base font-semibold">
                      Create New Version
                    </FormLabel>
                    <FormDescription>
                      {field.value ? (
                        <>
                          ✅ A <strong>new version</strong> will be created. The current version
                          will remain unchanged and accessible.
                        </>
                      ) : (
                        <>
                          ⚠️ The <strong>existing version</strong> will be updated. This will
                          modify the current version and cannot be undone.
                        </>
                      )}
                    </FormDescription>
                  </div>
                </FormItem>
              )}
            />
          </CardContent>
        </Card>
      )}
    </div>
  );
}

