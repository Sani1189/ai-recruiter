"use client";

import { Mic, X, Info } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

import useInterviewer, { CallStatus } from "@/hooks/useInterviewer";
import { cn } from "@/lib/utils";
import { InterviewView } from "@/types/v2/type.view";
import { InterviewConfigurationWithPrompts } from "@/types/interviewConfiguration";
import { useAuthStore } from "@/stores/useAuthStore";

type InterviewControlsProps = {
  jobPost: InterviewView;
  isInterviewStartable: boolean;
  setInterviewStarted: (started: boolean) => void;
  conversationToken: string;
  sessionPayload?: string | null;
  onInterviewComplete?: (payload?: { duration?: number; durationMs?: number; conversationId?: string }) => void;
  isCompleted?: boolean;
  interviewConfig?: InterviewConfigurationWithPrompts;
  stepLabel?: string;
  requiresAuth?: boolean; // If true, user must be authenticated to start interview
};

export default function InterviewControls({
  jobPost: interview,
  isInterviewStartable,
  setInterviewStarted,
  conversationToken,
  sessionPayload,
  onInterviewComplete,
  isCompleted = false,
  interviewConfig,
  stepLabel,
  requiresAuth = false,
}: InterviewControlsProps) {
  const router = useRouter();
  const { user, userType } = useAuthStore();
  const [hasShownCompletionToast, setHasShownCompletionToast] = useState(false);
  const [startTime, setStartTime] = useState<number | null>(null);
  const [endTime, setEndTime] = useState<number | null>(null);
  
  const isAuthenticatedCandidate = user && userType === 'candidate';

  // Prevent repeatedly notifying the parent when the effect re-runs while ACTIVE.
  const prevCallStatusRef = useRef<CallStatus>(CallStatus.INACTIVE);
  
  const deriveDurationFromSessionPayload = (payload?: string | null): number | undefined => {
    if (!payload) return undefined;
    try {
      const parsed = JSON.parse(payload) as any;
      const dynamic =
        parsed?.dynamic_variables ??
        parsed?.conversation_initiation_client_data?.dynamic_variables ??
        parsed?.conversationInitiationClientData?.dynamic_variables;
      const raw = dynamic?.duration;
      const n = raw != null ? Number(raw) : NaN;
      return Number.isFinite(n) && n > 0 ? n : undefined;
    } catch {
      return undefined;
    }
  };

  // Prefer backend-defined duration: config -> session payload -> default
  const interviewDuration =
    interviewConfig?.duration ??
    deriveDurationFromSessionPayload(sessionPayload) ??
    30;

  const { messages, callStatus, isSpeaking, action, error, conversationId, showHelpMessage } = useInterviewer(
    conversationToken,
    interviewConfig,
    {
      ...interview,
      // Surface an adjusted first message hint via jobPost field; hook reads this via prompt variables
      stepLabel,
    } as any,
    sessionPayload,
    interviewDuration
  );

  useEffect(() => {
    // If the interview is already completed, set the call status to finished
    if (isCompleted && callStatus === CallStatus.INACTIVE) {
      // Don't show toast on initial render for completed interviews
      if (!hasShownCompletionToast) {
        setHasShownCompletionToast(true);
        return;
      }
    }
    
    const prevStatus = prevCallStatusRef.current;
    prevCallStatusRef.current = callStatus;

    let title = "Interview Finished";
    let description =
      "Thank you for your time. Your responses have been recorded.";

    switch (callStatus) {
      case CallStatus.ACTIVE:
        title = "Interview in Progress";
        description = "Best of luck! Listen carefully and respond naturally.";
        
        // Notify parent only on transition into ACTIVE
        if (prevStatus !== CallStatus.ACTIVE) {
          setInterviewStarted(true);
        }
        break;

      case CallStatus.CONNECTING:
        title = "Connecting...";
        description = "Please wait while we connect you to the AI interviewer.";
        break;

      case CallStatus.FINISHED:
        title = "Interview Finished";
        description =
          "Thank you for your time. Your responses have been recorded.";

        // Call the completion callback when interview is finished (only once)
        if (onInterviewComplete && !hasShownCompletionToast) {
          setHasShownCompletionToast(true);
          const deltaMs = startTime ? Math.max(0, Date.now() - startTime) : undefined;
          const durationMinutes = deltaMs !== undefined ? Math.max(1, Math.round(deltaMs / 60000)) : undefined;
          onInterviewComplete({ duration: durationMinutes, durationMs: deltaMs, conversationId: conversationId || undefined });
        }
        break;

      default:
        return;
    }

    toast.success(title, {
      description,
    });
  }, [callStatus, onInterviewComplete, isCompleted, hasShownCompletionToast, setInterviewStarted, startTime, conversationId]);

  const isCallActive = callStatus === CallStatus.ACTIVE;
  const isCallConnecting = callStatus === CallStatus.CONNECTING;
  const isCallFinished = callStatus === CallStatus.FINISHED || isCompleted;
  const isCallDisabled = callStatus === CallStatus.INACTIVE || isCallFinished;

  const handleStart = () => {
    if (!isInterviewStartable) return;
    
    // Check authentication if required
    if (requiresAuth && !isAuthenticatedCandidate) {
      toast.error("Authentication Required", {
        description: "Please sign in to start the interview.",
      });
      router.push(`/sign-in?redirect=${encodeURIComponent(window.location.pathname)}`);
      return;
    }

    // Don't call setInterviewStarted(true) here - wait for connection to succeed
    // It will be called in useEffect when callStatus becomes ACTIVE
    setHasShownCompletionToast(false); // Reset completion toast state
    setStartTime(Date.now());
    action.connect();
  };

  const handleEnd = () => {
    setInterviewStarted(false);
    setEndTime(Date.now());
    action.disconnect();
    // Optionally compute duration and surface via messages/props if needed
  };

  return (
    <Card className="shadow-card relative overflow-hidden">
      {/* Disable overlay when not startable */}
      {!isInterviewStartable && (
        <div className="absolute inset-0 bg-black/40 backdrop-blur z-10">
          <div className="flex h-full items-center justify-center text-white p-6">
            <div className="text-center">
              <h1 className="text-2xl font-bold mb-2">
                {requiresAuth && !isAuthenticatedCandidate
                  ? "Please sign in to start the interview."
                  : "Please upload your resume to start the interview."}
              </h1>
              {requiresAuth && !isAuthenticatedCandidate && (
                <Button
                  onClick={() => router.push(`/sign-in?redirect=${encodeURIComponent(window.location.pathname)}`)}
                  className="mt-4"
                >
                  Sign In
                </Button>
              )}
            </div>
          </div>
        </div>
      )}

      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Mic className="text-brand h-5 w-5" />
          Interview Session
          {interviewConfig && (
            <span className="text-sm font-normal text-muted-foreground ml-2">
              ({interviewConfig.name})
            </span>
          )}
        </CardTitle>
        
        {/* Display interview configuration details */}
        {/* {interviewConfig && (
          <div className="mt-2 flex flex-wrap gap-2">
            {interviewConfig.tone && (
              <Badge variant="outline">Tone: {interviewConfig.tone}</Badge>
            )}
            {interviewConfig.focusArea && (
              <Badge variant="outline">Focus: {interviewConfig.focusArea}</Badge>
            )}
            {interviewConfig.duration && (
              <Badge variant="outline">{interviewConfig.duration} min</Badge>
            )}
          </div>
        )} */}
      </CardHeader>

      {isCallFinished ? (
        <CardContent className="space-y-6 py-12 text-center">
          <h3 className="mb-2 text-xl font-semibold">Interview Finished</h3>
          <p className="text-muted-foreground mb-6">
            Thank you for your time. Your responses have been recorded.
          </p>
        </CardContent>
      ) : (
        <CardContent className="space-y-6 py-5 text-center">
          <div
            className={cn(
              "mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-gradient-to-r transition-all duration-300",
              {
                "animate-pulse from-red-500 to-red-600": isCallActive,
                "from-brand to-brand-secondary": !isCallActive,
              },
            )}
          >
            <Mic className="h-12 w-12 text-white" />
          </div>

          <h3 className="mb-2 text-xl font-semibold">
            {isCallActive ? "Interview in Progress" : "Ready to Start?"}
          </h3>

          <p className="text-muted-foreground mx-auto mb-6 max-w-lg">
            {isCallActive ? (
              "Listen carefully and respond naturally to the AI interviewer."
            ) : (
              <>
                Make sure you're in a quiet environment and have uploaded your
                resume. The interview will go for approximately{" "}
                <strong>{interviewDuration} minutes</strong>.
              </>
            )}
          </p>

          {isCallConnecting && (
            <p className="text-muted-foreground mb-4">
              Connecting to the AI interviewer...
            </p>
          )}

          {error && (
            <p className="text-red-500">
              Error:{" "}
              {(error as Error).message || "An unexpected error occurred."}
            </p>
          )}

          {showHelpMessage && isCallActive && (
            <div className="mx-auto max-w-lg mb-4 rounded-lg border border-blue-200 bg-blue-50 p-4 text-left">
              <div className="flex items-start gap-3">
                <Info className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
                <div className="flex-1">
                  <p className="text-sm text-blue-900 font-medium mb-1">
                    Having technical issues?
                  </p>
                  <p className="text-sm text-blue-800">
                    If you experience audio problems, can't hear the interviewer, or the conversation stops responding, 
                    please refresh this page to restart the interview. Your progress will be saved.
                  </p>
                </div>
                <button
                  onClick={() => action.dismissHelpMessage()}
                  className="text-blue-600 hover:text-blue-800 flex-shrink-0"
                  aria-label="Dismiss help message"
                >
                  <X className="h-4 w-4" />
                </button>
              </div>
            </div>
          )}

          <Button
            size="lg"
            onClick={
              !isInterviewStartable
                ? undefined
                : isCallActive
                  ? handleEnd
                  : handleStart
            }
            variant={isCallActive ? "destructive" : "default"}
            disabled={isCallFinished}
          >
            {isCallActive
              ? "End Interview"
              : isCallFinished
                ? "Interview Completed"
                : "Start Interview"}
          </Button>

          {isCallActive && messages.length > 0 && (
            <p className="text-muted-foreground mt-4">
              {">"} {messages[messages.length - 1]}
            </p>
          )}
        </CardContent>
      )}
    </Card>
  );
}