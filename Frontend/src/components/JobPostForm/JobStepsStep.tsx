"use client";

import { useState, useEffect } from "react";
import { useFieldArray } from "react-hook-form";
import { Plus, Trash2, ExternalLink } from "lucide-react";
import { toast } from "sonner";

import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { useApi } from "@/hooks/useApi";
import { JobPostDetailsStepProps } from "./type";
import { JobStepAssignment } from "@/schemas/job-posting";

// Type for Job Step from API
interface JobStep {
  name: string;
  version: number;
  stepType: string;
  isInterview: boolean;
  interviewConfigurationName?: string;
  interviewConfigurationVersion?: number;
  createdAt?: string;
  updatedAt?: string;
}

//Job Step Versions
interface JobStepVersion {
  version: number;
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
  updatedBy?: string;
}


export default function JobStepsStep({ form }: JobPostDetailsStepProps) {
  const api = useApi();
  const [availableSteps, setAvailableSteps] = useState<JobStep[]>([]);
  const [selectedStepName, setSelectedStepName] = useState<string>("");
  const [useLatest, setUseLatest] = useState<boolean>(true);
  const [selectedVersion, setSelectedVersion] = useState<number | undefined>();
  const [stepVersions, setStepVersions] = useState<JobStepVersion[]>([]);
  const [isLoadingSteps, setIsLoadingSteps] = useState(false);
  const [isLoadingVersions, setIsLoadingVersions] = useState(false);

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "steps",
  });

  // Fetch all available job steps (latest versions only)
  useEffect(() => {
    const fetchJobSteps = async () => {
      setIsLoadingSteps(true);
      try {
        // Fetch all latest job steps
        const response = await api.get("/jobstep/dropdown");
        const data = response.data || response;

        setAvailableSteps(data);
      } catch (error) {
        console.error("Failed to fetch job steps:", error);
      } finally {
        setIsLoadingSteps(false);
      }
    };

    fetchJobSteps();
  }, []);

  // Fetch versions when a step is selected and "Use Latest" is unchecked
  useEffect(() => {
    if (selectedStepName && !useLatest) {
      const fetchVersions = async () => {
        setIsLoadingVersions(true);
        try {
          const response = await api.get<JobStepVersion[]>(`/jobstep/${selectedStepName}/versions`);
          const versions = response.data || response || [];
          setStepVersions(versions);
        } catch (error) {
          console.error("Failed to fetch step versions:", error);
          setStepVersions([]);
        } finally {
          setIsLoadingVersions(false);
        }
      };

      fetchVersions();
    } else {
      setStepVersions([]);
    }
  }, [selectedStepName, useLatest]);

  // Handle adding a job step
  const handleAddStep = () => {
    if (!selectedStepName) return;

    const selectedStep = availableSteps.find((s) => s.name === selectedStepName);
    if (!selectedStep) return;

    const versionToUse = useLatest ? undefined : selectedVersion;
    
    // Check if step already exists - use fields directly for better performance
    const isDuplicate = fields.some((field) => {
      const step = field as JobStepAssignment;
      return step.existingStepName === selectedStepName &&
        (useLatest ? step.useLatestVersion : step.existingStepVersion === versionToUse && !step.useLatestVersion);
    });

    if (isDuplicate) {
      toast.error("Step already added", {
        description: `The step "${selectedStepName}" with ${useLatest ? "latest version" : `version ${versionToUse}`} has already been added to this job post.`,
      });
      return;
    }

    const newStep: JobStepAssignment = {
      stepNumber: fields.length + 1,
      existingStepName: selectedStepName,
      ...(useLatest ? {} : { existingStepVersion: versionToUse }),
      useLatestVersion: useLatest,
      // Display fields - use selectedStep properties regardless of version
      displayName: selectedStepName,
      displayVersion: useLatest ? "Latest" : `v${versionToUse}`,
      stepType: selectedStep.stepType,
      isInterview: selectedStep.isInterview,
      // Only include interview config fields if they have values
      ...(selectedStep.interviewConfigurationName && { 
        interviewConfigName: selectedStep.interviewConfigurationName 
      }),
      ...(selectedStep.interviewConfigurationVersion && { 
        interviewConfigVersion: selectedStep.interviewConfigurationVersion 
      }),
    };

    append(newStep);

    // Reset selection
    setSelectedStepName("");
    setUseLatest(true);
    setSelectedVersion(undefined);
  };

  // Handle removing a step and reordering
  const handleRemoveStep = (index: number) => {
    remove(index);
    
    // Reorder step numbers
    const currentSteps = form.getValues("steps");
    currentSteps.forEach((step: any, idx: number) => {
      form.setValue(`steps.${idx}.stepNumber`, idx + 1);
    });
  };

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-semibold mb-2">Add Job Steps</h3>
        <p className="text-sm text-muted-foreground mb-4">
          Select existing job steps to add to this job post. Steps will be executed
          in the order they are added.
        </p>
      </div>

      {/* Add Job Step Form */}
      <div className="border rounded-lg p-4 space-y-4 bg-muted/20">
        {/* All three fields in one row */}
        <div className="grid gap-3 lg:grid-cols-3 md:grid-cols-2 grid-cols-1">
          {/* Job Step Selector */}
          <div className="space-y-2">
            <label className="text-sm font-medium">Select Job Step *</label>
            <Select
              value={selectedStepName}
              onValueChange={(value) => {
                setSelectedStepName(value);
                setSelectedVersion(undefined);
              }}
              disabled={isLoadingSteps}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={isLoadingSteps ? "Loading..." : "Choose a job step"} />
              </SelectTrigger>
              <SelectContent>
                {availableSteps.map((step) => (
                  <SelectItem key={step.name} value={step.name}>
                    {step.name} (v{step.version})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Use Latest Version Checkbox */}
          <div className="space-y-2">
            <label className="text-sm font-medium text-muted-foreground">Version</label>
            <label className="hover:bg-accent/50 has-[[aria-checked=true]]:border-primary has-[[aria-checked=true]]:bg-primary/5 flex items-start gap-3 rounded-lg border px-3 py-2 cursor-pointer h-9">
              <Checkbox
                id="useLatest"
                checked={useLatest}
                onCheckedChange={(checked) => {
                  setUseLatest(checked as boolean);
                  if (checked) setSelectedVersion(undefined);
                }}
                disabled={!selectedStepName}
              />
              <div className="grid gap-1.5 font-normal">
                <p className="text-sm leading-none font-medium">
                  Use Latest Version
                </p>
              </div>
            </label>
          </div>

          {/* Version Selector (conditional) */}
          {!useLatest && selectedStepName ? (
            <div className="space-y-2">
              <label className="text-sm font-medium">Select Version *</label>
              <Select
                value={selectedVersion?.toString()}
                onValueChange={(value) => setSelectedVersion(parseInt(value))}
                disabled={isLoadingVersions}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder={isLoadingVersions ? "Loading..." : "Choose version"} />
                </SelectTrigger>
                <SelectContent>
                  {stepVersions.map((step) => (
                    <SelectItem key={step.version} value={step.version.toString()}>
                      Version {step.version}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          ) : (
            <div className="space-y-2">
              <label className="text-sm font-medium text-muted-foreground">Version</label>
              <div className="h-9 flex items-center text-sm text-muted-foreground border border-input bg-background rounded-md px-3">
                {selectedStepName ? "Latest version will be used" : "Select a step first"}
              </div>
            </div>
          )}
        </div>

        {/* Add Button */}
        <Button
          type="button"
          onClick={handleAddStep}
          disabled={!selectedStepName || (!useLatest && !selectedVersion)}
          className="w-full"
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Step to Job
        </Button>
      </div>

      {/* Job Steps Table */}
      {fields.length > 0 && (
        <div className="border rounded-lg">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-16">#</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Version</TableHead>
                <TableHead>Step Type</TableHead>
                <TableHead>Interview Config</TableHead>
                <TableHead className="w-24 text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {fields.map((field, index) => {
                const step = field as JobStepAssignment;
                return (
                  <TableRow key={field.id}>
                    <TableCell className="font-medium">{step.stepNumber}</TableCell>
                    <TableCell className="font-mono text-sm">{step.displayName}</TableCell>
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
                          {step.interviewConfigName} (v{step.interviewConfigVersion})
                          <ExternalLink className="h-3 w-3" />
                        </a>
                      ) : (
                        <span className="text-sm text-muted-foreground">—</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => handleRemoveStep(index)}
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </div>
      )}

      {fields.length === 0 && (
        <div className="text-center py-8 text-muted-foreground border rounded-lg border-dashed">
          No job steps added yet. Add steps to define the recruitment process.
        </div>
      )}

      {/* Validation Error Message */}
      <FormField
        control={form.control}
        name="steps"
        render={() => (
          <FormItem>
            <FormMessage />
          </FormItem>
        )}
      />
    </div>
  );
}

