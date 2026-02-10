"use client";

import { Fragment, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Check, Loader2, Send, ChevronLeft, ChevronRight } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { useApi } from "@/hooks/useApi";
import { useAuthStore } from "@/stores/useAuthStore";
import { cn } from "@/lib/utils";
import { env } from "@/lib/config/env";
import { generateConversationTokenToken } from "@/scripts/token";
import {
  beginCandidateJobApplicationStep,
  getMyJobApplicationProgress,
} from "@/lib/api/services/candidateJobApplicationFlow.service";

import ResumeUploadArea from "./main-interview-area/ResumeUploadArea";
import InterviewControls from "./main-interview-area/InterviewControls";
import AssessmentStep from "./main-interview-area/AssessmentStep";

type InterviewStepperProps = {
  jobPost: any;
  requireCandidateAuth?: () => boolean;
};

type StepKind = "resume" | "interview" | "assessment" | "display";

type UnderlyingStepRef = {
  name: string;
  version: number | null;
};

type NormalizedStep = {
  id: string;
  label: string;
  kind: StepKind;
  stepNumber: number;
  jobPostStepName: string;
  jobPostStepVersion: number | null;
  underlyingSteps: UnderlyingStepRef[];
  /**
   * Display-only steps that should not block progression can be auto-completed client-side.
   * Internal/recruiter-decision placeholder steps must NOT be auto-completed.
   */
  autoComplete?: boolean;
  interviewConfigurationName?: string | null;
  interviewConfigurationVersion?: number | null;
  displayTitle?: string | null;
  displayContent?: string | null;
  showSpinner?: boolean;
};

type StepState = {
  started: boolean;
  completed: boolean;
};

type BeginStepResponse = {
  jobApplicationId: string;
  jobApplicationStepId: string;
  interviewId?: string | null;
  stepStatus?: string;
};

// In-memory cache to avoid duplicate token calls in strict/dev or fast re-renders
const tokenPromiseCache = new Map<
  string,
  Promise<{ token: string; conversationId?: string | null; sessionPayload?: string | null }>
>();

const isResumeLike = (name?: string, stepType?: string) => {
  // Prefer explicit stepType (new model). Fall back to name heuristics for older data.
  if (stepType && stepType.trim().toLowerCase() === "resume upload") return true;
  if (!name) return false;
  const normalized = name.toLowerCase();
  return normalized.includes("resume") || normalized.includes("upload") || normalized.includes("cv");
};

const isInterviewLike = (stepType?: string, isInterviewFlag?: boolean) => {
  if (typeof stepType === "string" && stepType.trim().toLowerCase() === "interview") return true;
  return Boolean(isInterviewFlag);
};

const isAssessmentLike = (stepType?: string) => {
  if (!stepType) return false;
  const t = stepType.trim().toLowerCase();
  return t === "questionnaire" || t === "assessment" || t === "multiple choice";
};

const normalizeSteps = (assignedSteps: any[] | undefined): NormalizedStep[] => {
  const raw = Array.isArray(assignedSteps) ? [...assignedSteps] : [];

  const shouldShowToCandidate = (s: any): boolean => {
    // Candidate participant steps are ALWAYS shown to the candidate (even if legacy/incorrect flags exist)
    const participant = s?.stepDetails?.participant;
    if (participant === "Candidate") return true;

    const explicit = s?.stepDetails?.showStepForCandidate;
    if (typeof explicit === "boolean") return explicit;

    // Backward compatibility (until all clients + DB are migrated)
    if (participant === "Recruiter") return false;

    return !s?.stepDetails?.recruiterCompletesStepManually;
  };

  const ordered = raw.sort((a: any, b: any) => (a.stepNumber ?? 0) - (b.stepNumber ?? 0));
  const normalized: NormalizedStep[] = [];

  const hiddenBuffer: any[] = [];

  const pushRecruiterDecisionPlaceholder = () => {
    if (hiddenBuffer.length === 0) return;

    const underlyingSteps: UnderlyingStepRef[] = hiddenBuffer
      .map((s: any) => ({
        name: (s?.stepDetails?.name ?? s?.stepName ?? "").toString(),
        version: (s?.stepDetails?.version ?? s?.stepVersion ?? null) as number | null,
      }))
      .filter((x) => Boolean(x.name));

    const first = hiddenBuffer[0];
    const firstName = (first?.stepDetails?.name ?? first?.stepName ?? "recruiter decision") as string;
    const firstVersion = (first?.stepDetails?.version ?? first?.stepVersion ?? null) as number | null;

    normalized.push({
      id: `recruiter-decision-${first?.stepNumber ?? normalized.length + 1}`,
      label: "Recruiter decision",
      kind: "display",
      stepNumber: first?.stepNumber ?? normalized.length + 1,
      jobPostStepName: firstName,
      jobPostStepVersion: firstVersion,
      underlyingSteps,
      autoComplete: false,
      displayTitle: "Recruiter decision",
      displayContent: "Please wait while our recruiters review your application.",
      showSpinner: true,
    });

    hiddenBuffer.length = 0;
  };

  ordered.forEach((s: any, index: number) => {
    if (!shouldShowToCandidate(s)) {
      // Keep hidden recruiter/internal steps so we can surface a single placeholder step that blocks progression.
      hiddenBuffer.push(s);
      return;
    }

    // If we have hidden steps before this visible step, insert a single placeholder
    pushRecruiterDecisionPlaceholder();

    const name = (s.stepDetails?.name ?? s.stepName ?? "").toString();
    const version = (s.stepDetails?.version ?? s.stepVersion ?? null) as number | null;
    const underlyingSteps: UnderlyingStepRef[] = name ? [{ name, version }] : [];

    const displayTitle = s.stepDetails?.displayTitle ?? null;
    const displayContent = s.stepDetails?.displayContent ?? null;
    const showSpinner = Boolean(s.stepDetails?.showSpinner);

    if (isResumeLike(s.stepDetails?.name, s.stepDetails?.stepType)) {
    normalized.push({
      id: "resume-upload",
        label: displayTitle || name || "Resume Upload",
      kind: "resume",
        stepNumber: s.stepNumber ?? 1,
        jobPostStepName: name || "resume upload",
        jobPostStepVersion: version,
        underlyingSteps,
        displayTitle,
        displayContent,
        showSpinner,
    });
      return;
  }

    // Interview steps are identified by StepType=Interview (fallback to legacy isInterview flag)
    if (isInterviewLike(s?.stepDetails?.stepType, s?.stepDetails?.isInterview)) {
    normalized.push({
        id: `${name || "interview"}-${index}`,
        label: displayTitle || name || `Interview Step ${normalized.length + 1}`,
      kind: "interview",
      stepNumber: s.stepNumber ?? normalized.length + 1,
        jobPostStepName: name,
        jobPostStepVersion: version,
        underlyingSteps,
      interviewConfigurationName: s.stepDetails?.interviewConfigurationName,
      interviewConfigurationVersion: s.stepDetails?.interviewConfigurationVersion,
        displayTitle,
        displayContent,
        showSpinner,
      });
      return;
    }

    // Questionnaire steps
    if (isAssessmentLike(s?.stepDetails?.stepType)) {
      normalized.push({
        id: `${name || "assessment"}-${index}`,
        label: displayTitle || name || `Questionnaire Step ${normalized.length + 1}`,
        kind: "assessment",
        stepNumber: s.stepNumber ?? normalized.length + 1,
        jobPostStepName: name,
        jobPostStepVersion: version,
        underlyingSteps,
        displayTitle,
        displayContent,
        showSpinner,
      });
      return;
    }

    // Display-only steps (e.g., recruiter info/decision steps explicitly shown to the candidate)
    normalized.push({
      id: `${name || "step"}-${index}`,
      label: displayTitle || name || `Step ${normalized.length + 1}`,
      kind: "display",
      stepNumber: s.stepNumber ?? normalized.length + 1,
      jobPostStepName: name,
      jobPostStepVersion: version,
      underlyingSteps,
      autoComplete: true,
      displayTitle,
      displayContent,
      showSpinner,
    });
  });

  // If we end with hidden recruiter/internal steps, show a final placeholder
  pushRecruiterDecisionPlaceholder();

  return normalized;
};

const normalizeStatus = (status?: string) => (status ?? "").toLowerCase();
const isCompletedStatus = (status?: string) => normalizeStatus(status) === "completed";
const isStartedStatus = (status?: string) => {
  const s = normalizeStatus(status);
  return s === "inprogress" || s === "completed";
};

export default function InterviewStepper({ jobPost, requireCandidateAuth }: InterviewStepperProps) {
  const api = useApi();
  const { user, userType } = useAuthStore();
  const isCandidate = Boolean(user && userType === "candidate");

  const steps = useMemo<NormalizedStep[]>(() => normalizeSteps(jobPost?.assignedSteps), [jobPost?.assignedSteps]);

  const [currentStepIndex, setCurrentStepIndex] = useState(0);
  const [stepStates, setStepStates] = useState<Record<string, StepState>>({});

  const [applicationId, setApplicationId] = useState<string | null>(null);
  const [applicationCompleted, setApplicationCompleted] = useState(false);

  const [jobApplicationStepIds, setJobApplicationStepIds] = useState<Record<string, string>>({});
  const [interviewIds, setInterviewIds] = useState<Record<string, string>>({});

  const [stepTokens, setStepTokens] = useState<Record<string, string>>({});
  const [stepSessionPayloads, setStepSessionPayloads] = useState<Record<string, string | null>>({});
  const [conversationIds, setConversationIds] = useState<Record<string, string>>({});
  const [tokenErrors, setTokenErrors] = useState<Record<string, string>>({});
  const [tokenLoading, setTokenLoading] = useState<Record<string, boolean>>({});

  const currentStep = steps[currentStepIndex];
  const progressKeyRef = useRef<string | null>(null);
  const beginInFlightRef = useRef<Record<string, Promise<BeginStepResponse | null> | null>>({});
  const beganStepRef = useRef<Record<string, boolean>>({});

  useEffect(() => {
    if (currentStepIndex >= steps.length) setCurrentStepIndex(0);
  }, [currentStepIndex, steps.length]);

  // Display-only steps should never block progression; treat them as completed.
  useEffect(() => {
    setStepStates((prev) => {
      const next = { ...prev };
      for (const s of steps) {
        if (s.kind !== "display") continue;
        if (s.autoComplete === false) continue;
        next[s.id] = { started: true, completed: true };
      }
      return next;
    });
  }, [steps]);

  const updateStepState = useCallback((stepId: string, updates: Partial<StepState>) => {
    setStepStates((prev) => {
      const previous = prev[stepId] ?? { started: false, completed: false };
      return { ...prev, [stepId]: { ...previous, ...updates } };
    });
  }, []);

  // Load progress snapshot (does NOT create JobApplication)
  useEffect(() => {
    if (!isCandidate) return;
    if (!jobPost?.name || !jobPost?.version) return;

    const stepsKey = steps
      .map((s) =>
        (s.underlyingSteps ?? [])
          .map((u) => `${u.name}:${u.version ?? "latest"}`)
          .join(",")
      )
      .join("|");
    const key = `${jobPost.name}-${jobPost.version}-${stepsKey}`;
    if (progressKeyRef.current === key) return;
    progressKeyRef.current = key;

    const run = async () => {
      try {
        const data = await getMyJobApplicationProgress(api as any, jobPost.name, jobPost.version);

        const app = data?.jobApplication ?? null;
        const existingSteps = Array.isArray(data?.steps) ? data.steps : [];

        if (!app?.id) {
          setApplicationId(null);
          setApplicationCompleted(false);
          setStepStates({});
          setJobApplicationStepIds({});
          setInterviewIds({});
          return;
        }

        setApplicationId(app.id);
        setApplicationCompleted(Boolean(app.completedAt));

        const byKey = new Map<string, any>();
        const byName = new Map<string, any[]>();
        existingSteps.forEach((s: any) => {
          const k = `${s.jobPostStepName}::${s.jobPostStepVersion}`;
          byKey.set(k, s);
          const list = byName.get(s.jobPostStepName) ?? [];
          list.push(s);
          byName.set(s.jobPostStepName, list);
        });
        // For "latest"/unspecified step versions, we want the most recent version for the same step name.
        byName.forEach((list, name) => {
          list.sort((a, b) => (b.jobPostStepVersion ?? 0) - (a.jobPostStepVersion ?? 0));
          byName.set(name, list);
        });

        const nextStates: Record<string, StepState> = {};
        const nextStepIds: Record<string, string> = {};

        const resolveMatch = (name: string, version: number | null) => {
          if (!name) return null;
          if (version && version > 0) return byKey.get(`${name}::${version}`) ?? null;
          return byName.get(name)?.[0] ?? null;
        };

        steps.forEach((s) => {
          // Auto-complete display-only informational steps
          if (s.kind === "display" && s.autoComplete !== false) {
            nextStates[s.id] = { started: true, completed: true };
            return;
          }

          const underlying = Array.isArray(s.underlyingSteps) ? s.underlyingSteps : [];
          if (underlying.length === 0) return;

          const matches = underlying.map((u) => resolveMatch(u.name, u.version));
          if (matches.some((m) => !m)) return;

          nextStates[s.id] = {
            started: matches.some((m) => isStartedStatus(m.status)),
            completed: matches.every((m) => isCompletedStatus(m.status)),
          };

          // Store a representative JobApplicationStepId for potential follow-up calls (interview/resume steps use this)
          const representative = matches.find(Boolean);
          if (representative?.id) nextStepIds[s.id] = representative.id;
        });

        setStepStates((prev) => ({ ...prev, ...nextStates }));
        setJobApplicationStepIds((prev) => ({ ...prev, ...nextStepIds }));

        const firstIncompleteIndex = steps.findIndex((s) => !nextStates[s.id]?.completed);
        if (firstIncompleteIndex >= 0) setCurrentStepIndex(firstIncompleteIndex);
      } catch (e) {
        console.error("Failed to load application progress", e);
        // Don't hard-fail UI; keep working with local state
      }
    };

    void run();
  }, [api, isCandidate, jobPost?.name, jobPost?.version, steps]);

  const canAdvance = useMemo(() => {
    const step = steps[currentStepIndex];
    if (!step) return false;
    return Boolean(stepStates[step.id]?.completed);
  }, [currentStepIndex, steps, stepStates]);

  const canStartCurrent = useMemo(() => {
    const step = steps[currentStepIndex];
    if (!step) return false;
    if (!isCandidate) return false;
    if (step.kind === "resume") return true;
    if (step.kind === "display") return true;
    const previous = steps.slice(0, currentStepIndex);
    return previous.every((s) => stepStates[s.id]?.completed);
  }, [currentStepIndex, isCandidate, steps, stepStates]);

  const tryExtractConversationId = (token: string): string | null => {
    try {
      const parts = token.split(".");
      if (parts.length !== 3) return null;
      const payload = JSON.parse(atob(parts[1].replace(/-/g, "+").replace(/_/g, "/")));
      return payload?.conversation_id || payload?.conversationId || payload?.conversation?.id || null;
    } catch {
      return null;
    }
  };

  const ensureTokenForStep = useCallback(
    async (step: NormalizedStep) => {
      if (!step || step.kind !== "interview") return;
      if (stepTokens[step.id]) return;
      if (!step.interviewConfigurationName) {
        setTokenErrors((prev) => ({ ...prev, [step.id]: "Interview configuration is missing for this step." }));
        return;
      }

      const cacheKey = `${jobPost?.name ?? ""}-${jobPost?.version ?? ""}-${step.id}-token-${applicationId ?? "noapp"}`;
      const cached = tokenPromiseCache.get(cacheKey);
      if (cached) {
        const result = await cached;
        setStepTokens((prev) => ({ ...prev, [step.id]: result.token }));
        if (result.conversationId) setConversationIds((prev) => ({ ...prev, [step.id]: result.conversationId! }));
        if (result.sessionPayload) setStepSessionPayloads((prev) => ({ ...prev, [step.id]: result.sessionPayload! }));
        return;
      }

      setTokenLoading((prev) => ({ ...prev, [step.id]: true }));
      setTokenErrors((prev) => ({ ...prev, [step.id]: "" }));

      const promise = generateConversationTokenToken(api as any, {
        interviewConfigurationName: step.interviewConfigurationName,
        interviewConfigurationVersion: step.interviewConfigurationVersion ?? undefined,
        jobPostName: jobPost?.name,
        jobPostVersion: jobPost?.version,
        jobApplicationId: applicationId ?? undefined,
        stepName: step.jobPostStepName,
        stepDisplayTitle: step.displayTitle || step.label,
      });

      tokenPromiseCache.set(cacheKey, promise as any);

      try {
        const result: any = await promise;
        setStepTokens((prev) => ({ ...prev, [step.id]: result.token }));
        const decoded = result.conversationId ?? tryExtractConversationId(result.token);
        if (decoded) setConversationIds((prev) => ({ ...prev, [step.id]: decoded }));
        if (result.sessionPayload) setStepSessionPayloads((prev) => ({ ...prev, [step.id]: result.sessionPayload }));
      } catch (e) {
        console.error("Failed to generate conversation token", e);
        setTokenErrors((prev) => ({ ...prev, [step.id]: "Failed to prepare interview token. Please try again." }));
        tokenPromiseCache.delete(cacheKey);
      } finally {
        setTokenLoading((prev) => ({ ...prev, [step.id]: false }));
      }
    },
    [api, applicationId, jobPost?.name, jobPost?.version, stepTokens]
  );

  // Pre-fetch token when step becomes startable/current
  useEffect(() => {
    const step = steps[currentStepIndex];
    if (!step || step.kind !== "interview") return;
    if (!canStartCurrent) return;
    void ensureTokenForStep(step);
  }, [canStartCurrent, currentStepIndex, ensureTokenForStep, steps]);

  const beginStep = useCallback(
    async (step: NormalizedStep): Promise<BeginStepResponse | null> => {
      if (!step?.jobPostStepName) return null;
      if (!jobPost?.name || !jobPost?.version) return null;

      const payload = {
        stepName: step.jobPostStepName,
        stepVersion: step.jobPostStepVersion ?? null,
        stepNumber: step.stepNumber,
      };

      return await beginCandidateJobApplicationStep(api as any, jobPost.name, jobPost.version, payload);
    },
    [api, jobPost?.name, jobPost?.version]
  );

  // Auto-begin assessment steps when they become startable/current (candidates must submit to proceed).
  useEffect(() => {
    const step = steps[currentStepIndex];
    if (!step || step.kind !== "assessment") return;
    if (!canStartCurrent) return;

    // If we've already created/began this step for the current session, skip.
    if (beganStepRef.current[step.id]) return;
    if (jobApplicationStepIds[step.id]) return;

    const run = async () => {
      try {
        if (beginInFlightRef.current[step.id]) {
          await beginInFlightRef.current[step.id];
          return;
        }

        const promise = beginStep(step);
        beginInFlightRef.current[step.id] = promise;
        const result = await promise;
        beginInFlightRef.current[step.id] = null;
        if (!result?.jobApplicationId || !result?.jobApplicationStepId) return;

        setApplicationId(result.jobApplicationId);
        setJobApplicationStepIds((prev) => ({ ...prev, [step.id]: result.jobApplicationStepId }));
        updateStepState(step.id, { started: true });
        beganStepRef.current[step.id] = true;
      } catch (e) {
        console.error("Failed to begin assessment step", e);
        beginInFlightRef.current[step.id] = null;
        updateStepState(step.id, { started: false });
        toast.error("Failed to start assessment");
      }
    };

    void run();
  }, [beginStep, canStartCurrent, currentStepIndex, jobApplicationStepIds, steps, updateStepState]);

  const handleResumeUploadComplete = useCallback(
    (stepId: string, completeResult: any) => {
      const appId = completeResult?.jobApplicationId;
      const jobAppStepId = completeResult?.jobApplicationStepId;
      if (appId && typeof appId === "string") setApplicationId(appId);

      if (jobAppStepId && typeof jobAppStepId === "string") {
        setJobApplicationStepIds((prev) => ({ ...prev, [stepId]: jobAppStepId }));
      }

      updateStepState(stepId, { started: true, completed: true });
    },
    [updateStepState]
  );

  const handleInterviewStart = useCallback(
    async (started: boolean, step: NormalizedStep) => {
      if (!started) {
        updateStepState(step.id, { started: false });
        return;
      }

      // Guard: if we've already begun this step for this session, do nothing.
      if (beganStepRef.current[step.id]) {
        return;
      }
      if (beginInFlightRef.current[step.id]) {
        await beginInFlightRef.current[step.id];
        return;
      }

      try {
        const promise = beginStep(step);
        beginInFlightRef.current[step.id] = promise;
        const result = await promise;
        beginInFlightRef.current[step.id] = null;
        if (!result?.jobApplicationId || !result?.jobApplicationStepId) return;

        setApplicationId(result.jobApplicationId);
        setJobApplicationStepIds((prev) => ({ ...prev, [step.id]: result.jobApplicationStepId }));
        if (result.interviewId) setInterviewIds((prev) => ({ ...prev, [step.id]: result.interviewId! }));

        updateStepState(step.id, { started: true });
        beganStepRef.current[step.id] = true;
      } catch (e) {
        console.error("Failed to begin step", e);
        beginInFlightRef.current[step.id] = null;
        updateStepState(step.id, { started: false });
        toast.error("Failed to start step");
      }
    },
    [beginStep, updateStepState]
  );

  const ensureInterviewIdFallback = useCallback(
    async (step: NormalizedStep, appId: string, stepId: string): Promise<string | null> => {
      if (interviewIds[step.id]) return interviewIds[step.id];
      try {
        const existing = await api.get(`/Interview/applications/${appId}/steps/${stepId}/interviews`);
        const list = (existing as any)?.data ?? existing;
        if (Array.isArray(list) && list[0]?.id) {
          setInterviewIds((prev) => ({ ...prev, [step.id]: list[0].id }));
          return list[0].id;
        }
        const created = await api.post(`/Interview/applications/${appId}/steps/${stepId}/interviews`, {});
        const createdId = (created as any)?.data?.id ?? (created as any)?.id;
        if (createdId) {
          setInterviewIds((prev) => ({ ...prev, [step.id]: createdId }));
          return createdId;
        }
      } catch (e) {
        console.error("Failed to ensure interview id", e);
      }
      return null;
    },
    [api, interviewIds]
  );

  const handleInterviewComplete = useCallback(
    async (payload?: { duration?: number; durationMs?: number; conversationId?: string }) => {
      const step = steps[currentStepIndex];
      if (!step || step.kind !== "interview") return;

      updateStepState(step.id, { completed: true });

      const appId = applicationId;
      if (!appId) return;

      const stepId = jobApplicationStepIds[step.id];
      if (!stepId) return;

      const interviewId = interviewIds[step.id] ?? (await ensureInterviewIdFallback(step, appId, stepId));
      if (!interviewId) return;

      const convId = payload?.conversationId || conversationIds[step.id];
      const apiBase = env.apiBaseUrl.replace(/\/+$/, "");
      const transcriptUrl = convId ? `${apiBase}/candidate/ai-interview/conversations/${convId}` : null;
      const interviewAudioUrl = convId ? `${apiBase}/candidate/ai-interview/conversations/${convId}/audio` : null;
      const durationMs = payload?.durationMs ?? (payload?.duration ? payload.duration * 60000 : undefined);

      try {
        await api.put(`/Interview/applications/${appId}/steps/${stepId}/interviews/${interviewId}/complete`, {
          duration: durationMs,
          transcriptUrl,
          interviewAudioUrl,
          interviewQuestions: [],
          notes: convId ? `ConversationId=${convId}; durationMs=${durationMs ?? ""}` : "Interview completed",
        });
      } catch (e) {
        console.error("Failed to complete interview", e);
      }
    },
    [
      api,
      applicationId,
      conversationIds,
      currentStepIndex,
      ensureInterviewIdFallback,
      interviewIds,
      jobApplicationStepIds,
      steps,
      updateStepState,
    ]
  );

  const handleSubmitApplication = useCallback(async () => {
    const appId = applicationId;
    if (!appId) {
      toast.error("No application found to submit. Please complete a step first.");
      return;
    }
    try {
      await api.put(`/candidate/job-application/${appId}/submit`);
      setApplicationCompleted(true);
      toast.success("Application submitted", {
        description: "Your application has been successfully submitted.",
      });
    } catch (e) {
      console.error("Failed to submit application", e);
      toast.error("Failed to submit application");
    }
  }, [api, applicationId]);

  if (!steps.length) {
    return (
      <Card className="shadow-card">
        <CardContent className="py-10">
          <p className="text-muted-foreground text-center">No steps are configured for this interview.</p>
        </CardContent>
      </Card>
    );
  }

  if (applicationCompleted) {
    return (
      <Card className="shadow-card">
        <CardContent className="pt-6">
          <CardHeader className="px-0 pt-0">
            <CardTitle className="text-xl">You have already applied for this job</CardTitle>
          </CardHeader>
          <p className="text-muted-foreground">Your application has been submitted. You can close this page.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-8">
      <StepNavigation
        steps={steps}
        currentStepIndex={currentStepIndex}
        stepStates={stepStates}
        onSelect={(index) => {
          if (index <= currentStepIndex) setCurrentStepIndex(index);
        }}
      />

      <Card className="shadow-card">
        <CardContent className="pt-6">
          <StepContent
            step={currentStep}
            stepState={stepStates[currentStep?.id] ?? { completed: false, started: false }}
            jobPost={jobPost}
            canStart={canStartCurrent}
            requireCandidateAuth={requireCandidateAuth}
            onResumeUploadComplete={handleResumeUploadComplete}
            onInterviewStart={(started) => void handleInterviewStart(started, currentStep)}
            onInterviewComplete={handleInterviewComplete}
            onAssessmentSubmitted={() => {
              if (!currentStep) return;
              updateStepState(currentStep.id, { started: true, completed: true });
              toast.success("Questionnaire submitted");
            }}
            jobApplicationStepId={currentStep ? jobApplicationStepIds[currentStep.id] ?? null : null}
            conversationToken={currentStep ? stepTokens[currentStep.id] || "" : ""}
            sessionPayload={currentStep ? stepSessionPayloads[currentStep.id] : null}
            isCandidate={isCandidate}
            tokenLoading={currentStep ? Boolean(tokenLoading[currentStep.id]) : false}
            tokenError={currentStep ? tokenErrors[currentStep.id] : ""}
            hasToken={currentStep ? Boolean(stepTokens[currentStep.id]) : false}
          />

          <div className="flex items-center justify-end gap-4 mt-8">
            <Button
              variant="outline"
              type="button"
              disabled={currentStepIndex === 0}
              onClick={() => setCurrentStepIndex((prev) => Math.max(0, prev - 1))}
            >
              Back
            </Button>

            {currentStepIndex < steps.length - 1 ? (
              <Button
                onClick={() => setCurrentStepIndex((prev) => Math.min(steps.length - 1, prev + 1))}
                disabled={!canAdvance}
              >
                Next
              </Button>
            ) : (
              canAdvance && (
                <Button onClick={handleSubmitApplication} className="bg-green-600 hover:bg-green-700">
                  <Send className="h-4 w-4 mr-2" />
                  Finish
                </Button>
              )
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

type StepNavigationProps = {
  steps: NormalizedStep[];
  currentStepIndex: number;
  stepStates: Record<string, StepState>;
  onSelect: (index: number) => void;
};

function StepNavigation({ steps, currentStepIndex, stepStates, onSelect }: StepNavigationProps) {
  const containerRef = useRef<HTMLOListElement>(null);
  const [showLeft, setShowLeft] = useState(false);
  const [showRight, setShowRight] = useState(false);

  const updateArrows = () => {
    const el = containerRef.current;
    if (!el) return;
    setShowLeft(el.scrollLeft > 10);
    setShowRight(el.scrollLeft < el.scrollWidth - el.clientWidth - 10);
  };

  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;
    updateArrows();
    el.addEventListener("scroll", updateArrows);
    const ro = new ResizeObserver(updateArrows);
    ro.observe(el);
    return () => {
      el.removeEventListener("scroll", updateArrows);
      ro.disconnect();
    };
  }, []);

  useEffect(() => {
    containerRef.current?.children[currentStepIndex]?.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "center" });
  }, [currentStepIndex]);

  return (
    <nav aria-label="Interview Steps" className="relative">
      {showLeft && (
        <Button
          type="button"
          variant="outline"
          size="icon"
          className="absolute left-0 top-1/2 -translate-y-1/2 z-10 h-8 w-8 bg-background shadow-md"
          onClick={() => containerRef.current?.scrollBy({ left: -200, behavior: "smooth" })}
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>
      )}
      <ol
        ref={containerRef}
        className="flex items-center gap-2 overflow-x-auto px-8 [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]"
        aria-orientation="horizontal"
      >
        {steps.map((step, index, array) => {
          const isCompleted = stepStates[step.id]?.completed;
          const isCurrent = index === currentStepIndex;
          return (
            <Fragment key={step.id}>
              <li className="flex flex-shrink-0 items-center gap-2">
                <Button
                  role="tab"
                  type="button"
                  variant={index <= currentStepIndex ? "default" : "secondary"}
                  disabled={index > currentStepIndex}
                  className={cn("flex size-10 items-center justify-center rounded-full", index > currentStepIndex && "bg-slate-200 text-slate-600")}
                  onClick={() => onSelect(index)}
                  aria-current={isCurrent ? "step" : undefined}
                >
                  {isCompleted ? <Check className="h-5 w-5" /> : index + 1}
                </Button>
                <span className="text-sm font-medium whitespace-nowrap">
                  {step.label}
                  {isCurrent && step.showSpinner && !isCompleted && <Loader2 className="ml-2 h-4 w-4 animate-spin inline" />}
                </span>
              </li>
              {index < array.length - 1 && (
                <Separator className={cn("flex-1 h-0.5 min-w-[2rem]", index < currentStepIndex ? "bg-primary" : "bg-gray-300")} />
              )}
            </Fragment>
          );
        })}
      </ol>
      {showRight && (
        <Button
          type="button"
          variant="outline"
          size="icon"
          className="absolute right-0 top-1/2 -translate-y-1/2 z-10 h-8 w-8 bg-background shadow-md"
          onClick={() => containerRef.current?.scrollBy({ left: 200, behavior: "smooth" })}
        >
          <ChevronRight className="h-4 w-4" />
        </Button>
      )}
    </nav>
  );
}

type StepContentProps = {
  step?: NormalizedStep;
  stepState: StepState;
  jobPost: any;
  canStart: boolean;
  requireCandidateAuth?: () => boolean;
  onResumeUploadComplete: (stepId: string, completeResult: any) => void;
  onInterviewStart: (started: boolean) => void;
  onInterviewComplete: (payload?: { duration?: number; durationMs?: number; conversationId?: string }) => void;
  onAssessmentSubmitted: () => void;
  jobApplicationStepId: string | null;
  conversationToken: string;
  sessionPayload?: string | null;
  isCandidate: boolean;
  tokenLoading: boolean;
  tokenError?: string;
  hasToken: boolean;
};

function StepContent({
  step,
  stepState,
  jobPost,
  canStart,
  requireCandidateAuth,
  onResumeUploadComplete,
  onInterviewStart,
  onInterviewComplete,
  onAssessmentSubmitted,
  jobApplicationStepId,
  conversationToken,
  sessionPayload,
  isCandidate,
  tokenLoading,
  tokenError,
  hasToken,
}: StepContentProps) {
  if (!step) return null;

  // Auth gate
  if (!isCandidate) {
    const message = step.kind === "resume" ? "Please sign in to upload your resume." : "Please sign in to start the interview.";
    return (
      <div className="relative w-full min-h-[320px] sm:min-h-[360px]">
        <div className="absolute inset-0 bg-black/40 backdrop-blur z-10 flex items-center justify-center">
          <div className="text-center text-white space-y-3 px-6">
            <p className="text-lg font-semibold">{message}</p>
            <Button
              onClick={() => window.location.assign(`/sign-in?redirect=${encodeURIComponent(window.location.pathname)}`)}
            >
              Sign In
            </Button>
          </div>
        </div>
      </div>
    );
  }

  if (step.kind === "resume") {
    if (!step.jobPostStepName) {
      return (
        <div className="p-4 text-center text-muted-foreground">
          <p>Resume upload step configuration is missing. Please contact support.</p>
        </div>
      );
    }

    return (
      <ResumeUploadArea
        setIsInterviewStartable={() => {
          /* Stepper owns progression; keep for backward compat */
        }}
        interviewStarted={false}
        onBeforeSubmit={() => (requireCandidateAuth ? requireCandidateAuth() : true)}
        onUploadComplete={(result) => onResumeUploadComplete(step.id, result)}
        jobPost={jobPost}
        stepName={step.jobPostStepName}
        stepVersion={step.jobPostStepVersion ?? null}
        isCompleted={stepState.completed}
        displayTitle={step.displayTitle ?? null}
        displayContent={step.displayContent ?? null}
        showSpinner={Boolean(step.showSpinner)}
      />
    );
  }

  if (step.kind === "display") {
    return (
      <Card className="shadow-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {step.displayTitle || step.label}
            {step.showSpinner && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {step.displayContent ? (
            <p className="text-muted-foreground whitespace-pre-wrap">{step.displayContent}</p>
          ) : (
            <p className="text-muted-foreground">No additional details for this step.</p>
          )}
        </CardContent>
      </Card>
    );
  }

  if (step.kind === "assessment") {
    return (
      <AssessmentStep
        jobApplicationStepId={jobApplicationStepId}
        isCompleted={stepState.completed}
        onSubmitted={onAssessmentSubmitted}
      />
    );
  }

  if (tokenLoading) {
    return (
      <div className="space-y-4 py-8 text-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto" />
        <p className="text-muted-foreground">Preparing your interview. Please wait...</p>
      </div>
    );
  }

  if (tokenError) {
    return (
      <div className="space-y-4 py-8 text-center">
        <p className="text-destructive font-medium">{tokenError}</p>
        <p className="text-muted-foreground text-sm">Try going back and returning to this step, or refresh the page.</p>
      </div>
    );
  }

  return (
    <InterviewControls
      key={step.id}
      isInterviewStartable={canStart && hasToken}
      setInterviewStarted={onInterviewStart}
      jobPost={jobPost}
      conversationToken={conversationToken}
      sessionPayload={sessionPayload}
      onInterviewComplete={onInterviewComplete}
      isCompleted={stepState.completed}
      stepLabel={step.label}
      requiresAuth={!isCandidate}
    />
  );
}


