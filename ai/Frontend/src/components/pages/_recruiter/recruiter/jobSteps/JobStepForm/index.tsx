"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from "@/components/ui/form";

import { useApi } from "@/hooks/useApi";
import { handleApiError, hasErrorCode } from "@/lib/api/errorHandler";
import {
  CANDIDATE_STEP_TYPES,
  JobPostStep,
  PARTICIPANTS,
  RECRUITER_STEP_TYPES,
  STEP_TYPES,
} from "@/types/jobPostStep";
import { jobPostStepFormSchema, JobPostStepFormData } from "@/schemas/jobPostStep.schema";

interface JobStepFormProps {
  step?: JobPostStep;
  mode: 'create' | 'edit';
}

interface ConfigVersion {
  version: number;
  createdAt: string;
  createdBy?: string;
}

export default function JobStepForm({ step, mode }: JobStepFormProps) {
  const router = useRouter();
  const api = useApi();
  const [loading, setLoading] = useState(false);
  const [shouldUpdateVersion, setShouldUpdateVersion] = useState(false);
  const [availableConfigs, setAvailableConfigs] = useState<string[]>([]);
  const [availableVersions, setAvailableVersions] = useState<ConfigVersion[]>([]);
  const [useLatest, setUseLatest] = useState(true);
  const [availablePromptCategories, setAvailablePromptCategories] = useState<string[]>([]);
  const [selectedPromptCategory, setSelectedPromptCategory] = useState<string>("");
  const [availablePrompts, setAvailablePrompts] = useState<string[]>([]);
  const [availablePromptVersions, setAvailablePromptVersions] = useState<ConfigVersion[]>([]);
  const [useLatestPrompt, setUseLatestPrompt] = useState(true);
  const [isLoadingPromptCategories, setIsLoadingPromptCategories] = useState(false);
  const [isLoadingPromptsByCategory, setIsLoadingPromptsByCategory] = useState(false);
  const [isLoadingPromptVersions, setIsLoadingPromptVersions] = useState(false);

  const [availableAssessmentTemplates, setAvailableAssessmentTemplates] = useState<string[]>([]);
  const [availableAssessmentTemplateVersions, setAvailableAssessmentTemplateVersions] = useState<ConfigVersion[]>([]);
  const [useLatestAssessmentTemplate, setUseLatestAssessmentTemplate] = useState(true);

  // Simple caches to avoid repeated network calls
  const promptNamesByCategoryCache = useRef<Record<string, string[]>>({});
  const promptVersionsCache = useRef<Record<string, ConfigVersion[]>>({});
  const assessmentTemplateVersionsCache = useRef<Record<string, ConfigVersion[]>>({});

  const rawStepType = ((step as any)?.stepType as string | undefined) ?? undefined;
  const rawParticipant = ((step as any)?.participant as string | undefined) ?? undefined;

  const legacyStepTypeMap: Record<string, string> = {
    "Recruiter Decision": "Generic",
    "Recruiter Step": "Generic",
    // Back-compat: old step type name
    "Multiple Choice": "Questionnaire",
    "Assessment": "Questionnaire",
  };

  const normalizeStepTypeValue = (incoming?: string): string | undefined => {
    if (!incoming) return undefined;
    return legacyStepTypeMap[incoming] ?? incoming;
  };

  const normalizeParticipant = (): "Candidate" | "Recruiter" => {
    if (rawParticipant === "Candidate" || rawParticipant === "Recruiter") return rawParticipant;
    const normalizedLegacyType = normalizeStepTypeValue(rawStepType);
    const recruiterTypes = RECRUITER_STEP_TYPES as readonly string[];
    if (normalizedLegacyType && recruiterTypes.includes(normalizedLegacyType)) return "Recruiter";

    return "Candidate";
  };

  const defaultParticipant = normalizeParticipant() as any;

  const normalizeStepType = (participantValue: "Candidate" | "Recruiter"): string => {
    const allTypes = STEP_TYPES as readonly string[];
    const allowed = (participantValue === "Recruiter" ? RECRUITER_STEP_TYPES : CANDIDATE_STEP_TYPES) as readonly string[];

    if (rawStepType) {
      const normalizedLegacyType = normalizeStepTypeValue(rawStepType)!;
      if (allTypes.includes(normalizedLegacyType) && allowed.includes(normalizedLegacyType)) {
        return normalizedLegacyType;
      }
    }

    return allowed[0] ?? "";
  };

  const defaultStepType =
    (normalizeStepType(defaultParticipant) as any) || ("" as any);

  const form = useForm<JobPostStepFormData>({
    resolver: zodResolver(jobPostStepFormSchema) as any,
    defaultValues: {
      name: step?.name || "",
      version: step?.version ?? 1,
      participant: defaultParticipant,
      stepType: defaultStepType,
      // Candidate steps are always visible to the candidate; recruiter steps default hidden (user can turn on).
      showStepForCandidate:
        defaultParticipant === "Candidate"
          ? true
          : (typeof (step as any)?.showStepForCandidate === "boolean"
              ? (step as any)?.showStepForCandidate
              : false),
      displayTitle: step?.displayTitle ?? null,
      displayContent: step?.displayContent ?? null,
      showSpinner: step?.showSpinner ?? false,
      interviewConfigurationName: step?.interviewConfigurationName || "",
      interviewConfigurationVersion: step?.interviewConfigurationVersion || undefined,
      promptName: step?.promptName || null,
      promptVersion: step?.promptVersion || null,
      // Back-compat: step may come from older API shape (assessmentTemplate*) or new (questionnaireTemplate*)
      assessmentTemplateName:
        (step as any)?.questionnaireTemplateName ?? (step as any)?.assessmentTemplateName ?? null,
      assessmentTemplateVersion:
        (step as any)?.questionnaireTemplateVersion ?? (step as any)?.assessmentTemplateVersion ?? null,
    },
  });

  // Watch values used for conditional UI
  const configName = useWatch({ control: form.control, name: "interviewConfigurationName" });
  const stepType = useWatch({ control: form.control, name: "stepType" });
  const participant = useWatch({ control: form.control, name: "participant" });
  const showStepForCandidate = useWatch({ control: form.control, name: "showStepForCandidate" });
  const promptName = useWatch({ control: form.control, name: "promptName" });
  const assessmentTemplateName = useWatch({ control: form.control, name: "assessmentTemplateName" });

  // Track initial render so we don't overwrite persisted values on edit
  const didInitRef = useRef(false);
  const skipNextParticipantDefaultRef = useRef(false);
  const prevParticipantRef = useRef<"Candidate" | "Recruiter" | null>(null);

  // Hydrate form when editing and step data arrives (defaultValues are not reactive)
  useEffect(() => {
    if (mode !== "edit") return;
    if (!step) return;

    // Prevent the participant-change effect from overwriting persisted values during this hydration cycle.
    skipNextParticipantDefaultRef.current = true;

    const p = (step.participant === "Candidate" || step.participant === "Recruiter")
      ? step.participant
      : "Candidate";

    const normalizedStepType = normalizeStepTypeValue((step as any)?.stepType) ?? (p === "Recruiter" ? "Generic" : "Interview");
    const allowed = (p === "Recruiter" ? RECRUITER_STEP_TYPES : CANDIDATE_STEP_TYPES) as readonly string[];
    const stepTypeForForm = (allowed.includes(normalizedStepType) ? normalizedStepType : (allowed[0] ?? "")) as any;

    form.reset(
      {
        name: step.name ?? "",
        version: step.version ?? 1,
        participant: p as any,
        stepType: stepTypeForForm,
        showStepForCandidate:
          p === "Candidate"
            ? true
            : (typeof (step as any)?.showStepForCandidate === "boolean"
                ? (step as any)?.showStepForCandidate
                : false),
        displayTitle: (step.displayTitle ?? null) as any,
        displayContent: (step.displayContent ?? null) as any,
        showSpinner: (step.showSpinner ?? false) as any,
        interviewConfigurationName: (step.interviewConfigurationName ?? "") as any,
        interviewConfigurationVersion: (step.interviewConfigurationVersion ?? undefined) as any,
        promptName: (step.promptName ?? null) as any,
        promptVersion: (step.promptVersion ?? null) as any,
        assessmentTemplateName:
          (((step as any).questionnaireTemplateName ?? (step as any).assessmentTemplateName) ?? null) as any,
        assessmentTemplateVersion:
          (((step as any).questionnaireTemplateVersion ?? (step as any).assessmentTemplateVersion) ?? null) as any,
      },
      {
        keepDirty: false,
        keepTouched: false,
      } as any
    );

    // Keep internal participant tracking aligned so subsequent real user changes work as intended.
    prevParticipantRef.current = p;
  }, [mode, step]);

  // Ensure stepType always stays valid for participant
  useEffect(() => {
    const allowed = (participant === "Recruiter" ? RECRUITER_STEP_TYPES : CANDIDATE_STEP_TYPES) as readonly string[];
    if (!allowed.includes(stepType)) {
      form.setValue("stepType", (allowed[0] ?? "") as any, { shouldValidate: true, shouldDirty: true });
    }

    // Apply participant-based defaults ONLY when the user changes participant (not during hydration)
    const prev = prevParticipantRef.current;
    prevParticipantRef.current = participant;

    if (!didInitRef.current) return;
    if (skipNextParticipantDefaultRef.current) {
      skipNextParticipantDefaultRef.current = false;
      return;
    }
    if (prev && prev !== participant) {
      // Default: Candidate steps visible; Recruiter steps hidden (user may override)
      form.setValue("showStepForCandidate", participant === "Candidate", { shouldValidate: true, shouldDirty: true });
    }
  }, [participant]);

  // Derive isInterview purely from stepType. Interview config only applies to Interview step type.
  useEffect(() => {
    const isInterviewType = participant === "Candidate" && stepType === "Interview";
    // If not an interview step, clear interview configuration fields
    if (!isInterviewType) {
      form.setValue("interviewConfigurationName", "" as any, { shouldValidate: true, shouldDirty: true });
      form.setValue("interviewConfigurationVersion", undefined as any, { shouldValidate: true, shouldDirty: true });
    }
  }, [participant, stepType]);

  // Questionnaire template only applies to Candidate + Questionnaire step type.
  useEffect(() => {
  const isAssessmentType = participant === "Candidate" && stepType === "Questionnaire";
    if (isAssessmentType) return;

    form.setValue("assessmentTemplateName", null as any, { shouldValidate: true, shouldDirty: true });
    form.setValue("assessmentTemplateVersion", null as any, { shouldValidate: true, shouldDirty: true });
    setUseLatestAssessmentTemplate(true);
    setAvailableAssessmentTemplateVersions([]);
  }, [participant, stepType]);

  // Initialize guard after first paint
  useEffect(() => {
    didInitRef.current = true;
  }, []);

  // If Show Step is off, clear display fields (backend validates they must be empty/false)
  useEffect(() => {
    if (showStepForCandidate) return;
    form.setValue("displayTitle", null, { shouldValidate: true, shouldDirty: true });
    form.setValue("displayContent", null, { shouldValidate: true, shouldDirty: true });
    form.setValue("showSpinner", false, { shouldValidate: true, shouldDirty: true });
  }, [showStepForCandidate]);

  // Candidate participant does not use spinner configuration
  useEffect(() => {
    if (participant !== "Candidate") return;
    // Candidate steps are always visible to the candidate (switch is hidden in UI)
    form.setValue("showStepForCandidate", true, { shouldValidate: true, shouldDirty: true });
    form.setValue("showSpinner", false, { shouldValidate: true, shouldDirty: true });
  }, [participant]);

  const loadAvailableConfigs = async () => {
    try {
      const response = await api.get('/InterviewConfiguration');
      const configs = Array.isArray(response) ? response : (response.data || []);
      const uniqueNames = [...new Set(configs.map((c: any) => c.name as string))] as string[];
      setAvailableConfigs(uniqueNames);
    } catch (error) {
      console.error('Failed to load interview configurations:', error);
      setAvailableConfigs([]);
    }
  };

  const loadConfigVersions = async (configName: string) => {
    if (!configName) {
      setAvailableVersions([]);
      return;
    }

    try {
      // Get all configurations and filter by name
      const response = await api.get('/InterviewConfiguration');
      const allConfigs = Array.isArray(response) ? response : (response.data || []);
      const configVersions = allConfigs
        .filter((c: any) => c.name === configName)
        .map((c: any) => ({
          version: c.version,
          createdAt: c.createdAt,
          createdBy: c.createdBy
        }))
        .sort((a: any, b: any) => b.version - a.version); // Sort by version descending
      
      setAvailableVersions(configVersions);
    } catch (error) {
      console.error('Failed to load configuration versions:', error);
      setAvailableVersions([]);
    }
  };

  const handleConfigNameChange = (configName: string) => {
    form.setValue('interviewConfigurationName', configName);
    form.setValue('interviewConfigurationVersion', undefined);
    loadConfigVersions(configName);
  };

  const loadAvailableAssessmentTemplates = async () => {
    try {
      const query = new URLSearchParams({
        Status: "Published",
        PageNumber: "1",
        PageSize: "2000",
        SortBy: "name",
        SortDescending: "false",
      });
      const response = await api.get(`/QuestionnaireTemplate/filtered?${query.toString()}`);
      const data = response?.data ?? response;
      const items = data?.items ?? [];
      const uniqueNames = [...new Set(items.map((t: any) => t.name as string).filter(Boolean))] as string[];
      uniqueNames.sort();
      setAvailableAssessmentTemplates(uniqueNames);
    } catch (error) {
      console.error("Failed to load questionnaire templates:", error);
      setAvailableAssessmentTemplates([]);
    }
  };

  const loadAssessmentTemplateVersions = async (templateName: string) => {
    if (!templateName) {
      setAvailableAssessmentTemplateVersions([]);
      return;
    }

    const cached = assessmentTemplateVersionsCache.current[templateName];
    if (cached) {
      setAvailableAssessmentTemplateVersions(cached);
      return;
    }

    try {
      const response = await api.get(`/QuestionnaireTemplate/${templateName}/versions`);
      const allVersions = Array.isArray(response) ? response : (response.data || []);
      const versions = allVersions
        .filter((t: any) => t?.version)
        .map((t: any) => ({
          version: t.version,
          createdAt: t.createdAt,
          createdBy: t.createdBy,
        }))
        .sort((a: any, b: any) => b.version - a.version);

      assessmentTemplateVersionsCache.current[templateName] = versions;
      setAvailableAssessmentTemplateVersions(versions);
    } catch (error) {
      console.error("Failed to load questionnaire template versions:", error);
      setAvailableAssessmentTemplateVersions([]);
    }
  };

  const handleAssessmentTemplateNameChange = (templateName: string) => {
    form.setValue("assessmentTemplateName", templateName as any);
    form.setValue("assessmentTemplateVersion", null as any);
    // Default to latest whenever a new template is selected
    setUseLatestAssessmentTemplate(true);
    // Only load versions when "Use Latest" is unchecked (on demand)
    setAvailableAssessmentTemplateVersions([]);
  };

  const loadAvailablePromptCategories = async () => {
    setIsLoadingPromptCategories(true);
    try {
      const response = await api.get('/Prompt/categories');
      const categories = Array.isArray(response) ? response : (response.data || []);
      setAvailablePromptCategories((categories as string[]).filter(Boolean).sort());
    } catch (error) {
      console.error('Failed to load prompts:', error);
      setAvailablePromptCategories([]);
    } finally {
      setIsLoadingPromptCategories(false);
    }
  };

  const loadPromptsByCategory = async (category: string) => {
    if (!category) {
      setAvailablePrompts([]);
      return;
    }

    const cached = promptNamesByCategoryCache.current[category];
    if (cached) {
      setAvailablePrompts(cached);
      return;
    }

    setIsLoadingPromptsByCategory(true);
    try {
      const response = await api.get(`/Prompt/by-category/${category}`);
      const prompts = Array.isArray(response) ? response : (response.data || []);
      const uniqueNames = [...new Set(prompts.map((p: any) => p.name as string).filter(Boolean))] as string[];
      uniqueNames.sort();
      promptNamesByCategoryCache.current[category] = uniqueNames;
      setAvailablePrompts(uniqueNames);
    } catch (error) {
      console.error('Failed to load prompts by category:', error);
      setAvailablePrompts([]);
    } finally {
      setIsLoadingPromptsByCategory(false);
    }
  };

  const loadPromptVersions = async (promptName: string) => {
    if (!promptName) {
      setAvailablePromptVersions([]);
      return;
    }

    const cached = promptVersionsCache.current[promptName];
    if (cached) {
      setAvailablePromptVersions(cached);
      return;
    }

    setIsLoadingPromptVersions(true);
    try {
      const response = await api.get(`/Prompt/${promptName}/versions`);
      const allVersions = Array.isArray(response) ? response : (response.data || []);
      const promptVersions = allVersions
        .map((p: any) => ({
          version: p.version,
          createdAt: p.createdAt,
          createdBy: p.createdBy
        }))
        .sort((a: any, b: any) => b.version - a.version);

      promptVersionsCache.current[promptName] = promptVersions;
      setAvailablePromptVersions(promptVersions);
    } catch (error) {
      console.error('Failed to load prompt versions:', error);
      setAvailablePromptVersions([]);
    } finally {
      setIsLoadingPromptVersions(false);
    }
  };

  const handlePromptNameChange = (promptName: string) => {
    form.setValue('promptName', promptName);
    form.setValue('promptVersion', null);
    // Default to latest whenever a new prompt is selected
    setUseLatestPrompt(true);
    // Only load versions when "Use Latest" is unchecked (on demand)
    setAvailablePromptVersions([]);
  };

  useEffect(() => {
    loadAvailableConfigs();
    loadAvailablePromptCategories();
    loadAvailableAssessmentTemplates();
    
    if (mode === 'edit' && step?.interviewConfigurationName) {
      loadConfigVersions(step.interviewConfigurationName);
      setUseLatest(!step.interviewConfigurationVersion);
    }

    if (mode === "edit" && (step as any)?.assessmentTemplateName) {
      setUseLatestAssessmentTemplate(!(step as any)?.assessmentTemplateVersion);
      if ((step as any)?.assessmentTemplateVersion) {
        loadAssessmentTemplateVersions((step as any).assessmentTemplateName);
      }
    }

    const hydratePromptSelectionForEdit = async () => {
      if (mode !== 'edit' || !step?.promptName) return;

      try {
        // Determine category from the latest version (category is stable per prompt name)
        const response = await api.get(`/Prompt/${step.promptName}/latest`);
        const prompt = (response?.data ?? response) as any;
        const category = prompt?.category as string | undefined;
        if (category) {
          setSelectedPromptCategory(category);
          await loadPromptsByCategory(category);
        }
      } catch (error) {
        console.error('Failed to hydrate prompt category for edit:', error);
      }

      setUseLatestPrompt(!step.promptVersion);
      if (step.promptVersion) {
        // Only load versions if we're using a specific version
        loadPromptVersions(step.promptName);
      }
    };

    hydratePromptSelectionForEdit();
  }, [mode, step]);

  // Load prompt versions on demand only when Use Latest is unchecked.
  useEffect(() => {
    if (useLatestPrompt) return;
    if (!promptName) return;
    loadPromptVersions(promptName);
  }, [useLatestPrompt, promptName]);

  // Load assessment template versions on demand only when Use Latest is unchecked.
  useEffect(() => {
    if (useLatestAssessmentTemplate) return;
    if (!assessmentTemplateName) return;
    loadAssessmentTemplateVersions(assessmentTemplateName);
  }, [useLatestAssessmentTemplate, assessmentTemplateName]);

  const onSubmit = async (data: JobPostStepFormData) => {
    setLoading(true);
    try {
      // If "Use Latest" is checked, send null for version
      const submitData: any = {
        ...data,
        interviewConfigurationVersion: useLatest ? null : data.interviewConfigurationVersion,
        promptName: data.promptName ? data.promptName : null,
        promptVersion: data.promptName ? (useLatestPrompt ? null : data.promptVersion) : null,

        // Backend expects QuestionnaireTemplateName/Version (not assessmentTemplate*)
        questionnaireTemplateName: data.assessmentTemplateName ? data.assessmentTemplateName : null,
        questionnaireTemplateVersion: data.assessmentTemplateName
          ? (useLatestAssessmentTemplate ? null : data.assessmentTemplateVersion)
          : null,
      };

      // Do not send legacy field names to backend (they'll be ignored and validation will fail)
      delete submitData.assessmentTemplateName;
      delete submitData.assessmentTemplateVersion;

      if (mode === 'create') {
        await api.post('/JobStep', submitData);
      } else if (step) {
        const updateData = { ...submitData, shouldUpdateVersion };
        await api.put(`/JobStep/${step.name}/${step.version}`, updateData);
      }
      
      router.push('/recruiter/jobSteps');
    } catch (error) {
      if (hasErrorCode(error, "JOB_STEP_HAS_APPLICATIONS")) {
        toast.error("This step has applications and cannot be updated. Create a new version instead.");
      } else {
        handleApiError(error, { defaultMessage: "Failed to save job step. Please try again." });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-8">
      <div className="max-w-2xl mx-auto space-y-6">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push('/recruiter/jobSteps')}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Job Steps
          </Button>
        </div>

        <div className="space-y-2">
          <h1 className="text-3xl font-bold">
            {mode === 'create' ? 'Create New Job Step' : 'Edit Job Step'}
          </h1>
          <p className="text-muted-foreground">
            {mode === 'create' 
              ? 'Create a reusable job step template for your recruitment process'
              : 'Update the job step template details'
            }
          </p>
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name *</FormLabel>
                  <FormControl>
                    <Input 
                      placeholder="e.g., InitialScreening" 
                      {...field}
                      disabled={mode === 'edit'}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <FormField
                control={form.control}
                name="version"
                render={({ field }) => (
                  <FormItem className="w-full">
                    <FormLabel>Version *</FormLabel>
                    <FormControl>
                      <Input 
                        type="number" 
                        min="1"
                        {...field}
                        onChange={(e) => field.onChange(parseInt(e.target.value) || 1)}
                        disabled={mode === 'edit'}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="participant"
                render={({ field }) => (
                  <FormItem className="w-full">
                    <FormLabel>Participant *</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder="Select participant" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {PARTICIPANTS.map((p) => (
                          <SelectItem key={p} value={p}>
                            {p}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="stepType"
                render={({ field }) => (
                  <FormItem className="w-full">
                    <FormLabel>Step Type *</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder="Select step type" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {(participant === "Recruiter" ? RECRUITER_STEP_TYPES : CANDIDATE_STEP_TYPES).map((type) => (
                          <SelectItem key={type} value={type}>
                            {type}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="space-y-3 p-3 border rounded-lg bg-muted/30">
              <div className="flex items-center justify-between gap-4">
                <h3 className="text-sm font-semibold">Candidate Display (Optional)</h3>
                {participant === "Recruiter" && (
            <FormField
              control={form.control}
                    name="showStepForCandidate"
              render={({ field }) => (
                      <FormItem className="flex items-center gap-3 space-y-0">
                        <FormLabel className="text-sm font-medium m-0">Show Step</FormLabel>
                  <FormControl>
                          <Switch checked={field.value} onCheckedChange={field.onChange} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                )}
              </div>

              {showStepForCandidate && (
                <>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
                    <FormField
                      control={form.control}
                      name="displayTitle"
                      render={({ field }) => (
                        <FormItem className="md:col-span-2 w-full">
                          <FormLabel>Display title</FormLabel>
                          <FormControl>
                            <Input placeholder="e.g., Finished!" {...field} value={field.value ?? ""} />
                  </FormControl>
                          <FormMessage />
                </FormItem>
              )}
            />

                    {participant === "Recruiter" && (
                      <FormField
                        control={form.control}
                        name="showSpinner"
                        render={({ field }) => (
                          <FormItem className="md:col-span-1 w-full">
                            <FormLabel>Loading Indicator</FormLabel>
                            <FormControl>
                              <div className="h-10 w-full rounded-md border bg-background px-3 flex items-center justify-between">
                                <span className="text-sm text-muted-foreground">Show while waiting</span>
                                <Switch checked={field.value} onCheckedChange={field.onChange} />
                              </div>
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    )}
                  </div>

            <FormField
              control={form.control}
                    name="displayContent"
              render={({ field }) => (
                      <FormItem className="w-full">
                        <FormLabel>Display content</FormLabel>
                  <FormControl>
                          <Textarea
                            placeholder='e.g., "Wait for our recruiters to get to you..."'
                            {...field}
                            value={field.value ?? ""}
                    />
                  </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </>
              )}

              {/* Interview is derived from Step Type = Interview; no checkbox needed */}
                  </div>

            {participant === "Candidate" && stepType === "Interview" && (
              <div className="space-y-4 p-4 border rounded-lg bg-muted/30">
                <h3 className="text-sm font-semibold">Interview Configuration</h3>

                <FormField
                  control={form.control}
                  name="interviewConfigurationName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Configuration Name *</FormLabel>
                      <Select onValueChange={(value) => {
                        field.onChange(value);
                        handleConfigNameChange(value);
                      }} value={field.value || undefined}>
                        <FormControl>
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Select configuration..." />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {availableConfigs.map(name => (
                            <SelectItem key={name} value={name}>
                              {name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                </FormItem>
              )}
            />

                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="useLatest"
                    checked={useLatest}
                    onCheckedChange={(checked) => {
                      setUseLatest(checked === true);
                      if (checked) {
                        form.setValue('interviewConfigurationVersion', undefined);
                      }
                    }}
                    className="h-4 w-4"
                  />
                  <Label htmlFor="useLatest" className="text-xs">
                    Use Latest Version
                  </Label>
                </div>

                {!useLatest && availableVersions.length > 0 && configName && (
                  <FormField
                    control={form.control}
                    name="interviewConfigurationVersion"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Version *</FormLabel>
                        <Select onValueChange={(value) => field.onChange(Number(value))} value={field.value?.toString()}>
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue placeholder="Select version" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {availableVersions.map(version => (
                              <SelectItem key={version.version} value={version.version.toString()}>
                                v{version.version} - {new Date(version.createdAt).toLocaleDateString()}
                                {version.createdBy && ` - ${version.createdBy}`}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                )}
              </div>
            )}

            {participant === "Candidate" && stepType === "Questionnaire" && (
              <div className="space-y-4 p-4 border rounded-lg bg-muted/30">
                <h3 className="text-sm font-semibold">Questionnaire Template</h3>

                <FormField
                  control={form.control}
                  name="assessmentTemplateName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Template Name *</FormLabel>
                      <Select
                        onValueChange={(value) => {
                          field.onChange(value);
                          handleAssessmentTemplateNameChange(value);
                        }}
                        value={field.value || undefined}
                      >
                        <FormControl>
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Select template..." />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {availableAssessmentTemplates.map((name) => (
                            <SelectItem key={name} value={name}>
                              {name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="useLatestAssessment"
                    checked={useLatestAssessmentTemplate}
                    onCheckedChange={(checked) => {
                      setUseLatestAssessmentTemplate(checked === true);
                      if (checked) {
                        form.setValue("assessmentTemplateVersion", null as any);
                      }
                    }}
                    className="h-4 w-4"
                    disabled={!assessmentTemplateName}
                  />
                  <Label htmlFor="useLatestAssessment" className="text-xs">
                    Use Latest Version
                  </Label>
                </div>

                {!useLatestAssessmentTemplate && assessmentTemplateName && (
                  <FormField
                    control={form.control}
                    name="assessmentTemplateVersion"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Version *</FormLabel>
                        <Select
                          onValueChange={(value) => field.onChange(Number(value))}
                          value={field.value?.toString() || undefined}
                          disabled={availableAssessmentTemplateVersions.length === 0}
                        >
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue placeholder="Select version" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {availableAssessmentTemplateVersions.map((v) => (
                              <SelectItem key={v.version} value={v.version.toString()}>
                                v{v.version} - {new Date(v.createdAt).toLocaleDateString()}
                                {v.createdBy ? ` - ${v.createdBy}` : ""}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                )}
              </div>
            )}

            {(stepType === "Interview" || stepType === "Resume Upload") && (
              <div className="space-y-3 p-4 border rounded-lg bg-muted/30">
                <h3 className="text-sm font-semibold">Prompt (Optional)</h3>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-end">
                  <FormItem>
                    <FormLabel>Category</FormLabel>
                    <Select
                      onValueChange={(value) => {
                        setSelectedPromptCategory(value);
                        loadPromptsByCategory(value);
                        // reset dependent fields
                        form.setValue("promptName", null);
                        form.setValue("promptVersion", null);
                        setAvailablePromptVersions([]);
                        setUseLatestPrompt(true);
                      }}
                      value={selectedPromptCategory || undefined}
                      disabled={isLoadingPromptCategories}
                    >
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder={isLoadingPromptCategories ? "Loading..." : "Select category..."} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {isLoadingPromptCategories ? (
                          <SelectItem value="__loading_categories" disabled>
                            Loading...
                          </SelectItem>
                        ) : (
                          availablePromptCategories.map((category) => (
                            <SelectItem key={category} value={category}>
                              {category}
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                  </FormItem>

                  <FormField
                    control={form.control}
                    name="promptName"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Prompt</FormLabel>
                        <Select
                          onValueChange={(value) => {
                            field.onChange(value);
                            handlePromptNameChange(value);
                          }}
                          value={field.value || undefined}
                          disabled={!selectedPromptCategory || isLoadingPromptsByCategory}
                        >
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue
                                placeholder={
                                  isLoadingPromptsByCategory
                                    ? "Loading..."
                                    : selectedPromptCategory
                                      ? "Select prompt..."
                                      : "Select category first"
                                }
                              />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {isLoadingPromptsByCategory ? (
                              <SelectItem value="__loading_prompts" disabled>
                                Loading...
                              </SelectItem>
                            ) : (
                              availablePrompts.map((name) => (
                                <SelectItem key={name} value={name}>
                                  {name}
                                </SelectItem>
                              ))
                            )}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="space-y-2 md:col-span-2">
                    <div className="flex items-center justify-between gap-3">
                      <Label className="text-sm">Version</Label>
                      <label className="flex items-center gap-2 text-xs text-muted-foreground">
                        <Checkbox
                          id="useLatestPrompt"
                          checked={useLatestPrompt}
                          onCheckedChange={(checked) => {
                            setUseLatestPrompt(checked === true);
                            if (checked) {
                              form.setValue("promptVersion", null);
                            }
                          }}
                          className="h-4 w-4"
                          disabled={!promptName || isLoadingPromptVersions}
                        />
                        Use Latest
                      </label>
                    </div>

                    {!useLatestPrompt && promptName ? (
                      <FormField
                        control={form.control}
                        name="promptVersion"
                        render={({ field }) => (
                          <FormItem>
                            <Select
                              onValueChange={(value) => field.onChange(Number(value))}
                              value={field.value?.toString()}
                              disabled={isLoadingPromptVersions || availablePromptVersions.length === 0}
                            >
                              <FormControl>
                                <SelectTrigger className="w-full">
                                  <SelectValue placeholder={isLoadingPromptVersions ? "Loading..." : "Select version"} />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                {isLoadingPromptVersions ? (
                                  <SelectItem value="__loading_versions" disabled>
                                    Loading...
                                  </SelectItem>
                                ) : (
                                  availablePromptVersions.map((version) => (
                                    <SelectItem
                                      key={version.version}
                                      value={version.version.toString()}
                                    >
                                      v{version.version} - {new Date(version.createdAt).toLocaleDateString()}
                                      {version.createdBy ? ` - ${version.createdBy}` : ""}
                                    </SelectItem>
                                  ))
                                )}
                              </SelectContent>
                            </Select>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    ) : (
                      <div className="h-10 flex items-center text-sm text-muted-foreground border border-input bg-background rounded-md px-3">
                        {promptName ? "Latest version will be used" : "Select a prompt first"}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            )}

            {mode === 'edit' && (
              <div className="flex flex-row items-start space-x-3 space-y-0 gap-3">
                <Checkbox
                  id="shouldUpdateVersion"
                  checked={shouldUpdateVersion}
                  onCheckedChange={(checked) => setShouldUpdateVersion(checked === true)}
                />
                <div className="space-y-1 leading-none">
                  <Label htmlFor="shouldUpdateVersion">
                    Update version
                  </Label>
                  <p className="text-sm text-muted-foreground">
                    Check this to create a new version of the job step
                  </p>
                </div>
              </div>
            )}

            <div className="flex gap-4">
              <Button type="submit" disabled={loading}>
                {loading ? 'Saving...' : mode === 'create' ? 'Create Job Step' : 'Update Job Step'}
              </Button>
              <Button 
                type="button" 
                variant="outline" 
                onClick={() => router.push('/recruiter/jobSteps')}
              >
                Cancel
              </Button>
            </div>
          </form>
        </Form>
      </div>
    </div>
  );
}

