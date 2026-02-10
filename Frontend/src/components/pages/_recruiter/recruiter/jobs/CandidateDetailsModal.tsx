"use client";

import { useState, useEffect, useRef } from "react";
import {
  User,
  Mail,
  Calendar,
  FileText,
  Headphones,
  CheckCircle2,
  Clock,
  AlertCircle,
  Loader2,
  ChevronLeft,
  ChevronRight,
  Bot,
  Briefcase,
  BarChart3,
  Lightbulb,
  Hammer,
  Star,
  Phone,
  Globe,
  Plane,
  Laptop,
  BookOpen,
  Zap,
  ArrowRight,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";
import { interviewAudioService, AudioPlaylistItem } from "@/lib/services/interviewAudioService";
import { useApi } from "@/hooks/useApi";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { profileService } from "@/lib/api/services/profile.service";
import InterviewAudioPlayer from "@/components/InterviewAudioPlayer";

interface JobStepCandidate {
  applicationId: string;
  candidateId: string;
  candidateSerial: string;
  candidateName: string;
  candidateEmail: string;
  appliedAt: string;
  completedAt?: string;
  status: string;
  currentStep?: number;
  completedStepsCount?: number;
  candidateCvFilePath?: string;
}

interface CandidateDetailsModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  candidate: JobStepCandidate | null;
  applicationId?: string;
  allSteps?: any[];
  jobPostName?: string;
  jobPostVersion?: number;
  onCandidatePromoted?: (updatedCandidate: JobStepCandidate) => void;
}

type StepTranscript = {
  stepId: string;
  stepName: string;
  stepNumber: number;
  interviews: Array<{
    interviewId: string;
    transcriptUrl: string;
    transcript: any[];
    loading: boolean;
    error: string | null;
  }>;
};

export default function CandidateDetailsModal({
  open,
  onOpenChange,
  candidate,
  allSteps = [],
  jobPostName = "",
  jobPostVersion = 1,
  onCandidatePromoted,
}: CandidateDetailsModalProps) {
  const [activeTab, setActiveTab] = useState("info");
  const [applicationData, setApplicationData] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [stepsData, setStepsData] = useState<StepTranscript[]>([]);
  const [transcriptError, setTranscriptError] = useState<string | null>(null);
  const [playlist, setPlaylist] = useState<AudioPlaylistItem[]>([]);
  const [isPromoting, setIsPromoting] = useState(false);
  const [promoteError, setPromoteError] = useState<string | null>(null);
  const [promoteSuccess, setPromoteSuccess] = useState<string | null>(null);
  const [localCandidate, setLocalCandidate] = useState<JobStepCandidate | null>(null);
  
  // Assessment data
  const [assessmentScorings, setAssessmentScorings] = useState<any[]>([]);
  const [derivedFeedback, setDerivedFeedback] = useState<{
    positiveSummary: string;
    negativeSummary: string;
    strengthList: string[];
    weaknessList: string[];
  }>({
    positiveSummary: "",
    negativeSummary: "",
    strengthList: [],
    weaknessList: [],
  });
  const [isLoadingAssessment, setIsLoadingAssessment] = useState(false);
  
  const api = useApi();
  const { getAccessToken } = useUnifiedAuth();
  const scrollRef = useRef<HTMLDivElement>(null);
  const [candidateProfile, setCandidateProfile] = useState<any>(null);

  // Sync local candidate state with prop candidate
  useEffect(() => {
    if (candidate) {
      setLocalCandidate(candidate);
    }
  }, [candidate]);

  // Fetch assessment data (scorings, strengths, weaknesses, summaries)
  useEffect(() => {
    async function fetchAssessmentData() {
      if (!candidate?.candidateId) return;

      try {
        setIsLoadingAssessment(true);
        
        // Fetch candidate details with assessment data
        const candidateResponse = await api.get(`/candidate/${candidate.candidateId}`);
        const candidateData = candidateResponse.data || candidateResponse;
        
        // Deduplicate by FixedCategory (use max score per category)
        const scorings: any[] = candidateData.scorings || [];
        const groupedMap = new Map<string, number>();
        scorings.forEach((s: any) => {
          const label = (s.fixedCategory || s.category || "Other").toString().trim();
          const val = typeof s.score === "number" ? s.score : null;
          if (val == null) return;
          const current = groupedMap.get(label);
          groupedMap.set(label, current == null ? val : Math.max(current, val));
        });

        const toTitle = (text: string) =>
          text
            .split(" ")
            .map((w) => (w ? w[0].toUpperCase() + w.slice(1) : ""))
            .join(" ");

        const groupedScorings = Array.from(groupedMap.entries()).map(([label, score]) => ({
          fixedCategory: toTitle(label),
          score,
        }));

        setAssessmentScorings(groupedScorings);

        // Summaries (positive/negative) and key strengths
        const summaries: any[] = candidateData.summaries || [];
        const findByType = (prefix: string) =>
          summaries.find((s) => s.type?.toLowerCase().startsWith(prefix))?.text || "";
        const positiveSummary = findByType("positive");
        const negativeSummary = findByType("negative");
        const weaknessSummary = findByType("weakness");

        const keyStrengths: any[] = candidateData.keyStrengths || [];
        const strengthList = Array.from(
          new Set(
            keyStrengths
              .map(
                (s) =>
                  s.description?.trim() ||
                  s.strengthName?.trim() ||
                  null,
              )
              .filter(Boolean),
          ),
        );

        const weaknessList = weaknessSummary
          ? [weaknessSummary]
          : negativeSummary
            ? [negativeSummary]
            : [];
        
        setDerivedFeedback({
          positiveSummary,
          negativeSummary,
          strengthList,
          weaknessList,
        });
      } catch (err) {
        console.error("Error fetching assessment data:", err);
      } finally {
        setIsLoadingAssessment(false);
      }
    }

    fetchAssessmentData();
  }, [candidate?.candidateId]);

  // Fetch application data when modal opens - only once per open
  useEffect(() => {
    if (!open || !candidate || !jobPostName || applicationData) {
      return;
    }

    const fetchApplicationData = async () => {
      try {
        setIsLoading(true);
        
        const data = await interviewAudioService.getJobApplicationWithInterviews(
          api,
          jobPostName,
          jobPostVersion,
          candidate.candidateId
        );

        if (data) {
          setApplicationData(data);
          // Create audio playlist
          const audioPlaylist = interviewAudioService.createPlaylist(
            data,
            candidate.candidateName
          );
          setPlaylist(audioPlaylist);
        }

        // Fetch candidate profile for additional details
        try {
          const profileResponse = await api.get(`/candidate/${candidate.candidateId}`);
          const profileData = profileResponse.data || profileResponse;
          const userProfileId = profileData?.userProfile?.id;
          
          // Now fetch the separate data using profileService methods with userProfileId
          const token = getAccessToken ? await getAccessToken() : null;
          
          if (userProfileId && token) {
            const [experiences, educations, skills] = await Promise.all([
              profileService.getExperienceByUserProfileId(token, userProfileId).catch(() => []),
              profileService.getEducationByUserProfileId(token, userProfileId).catch(() => []),
              profileService.getSkillsByUserProfileId(token, userProfileId).catch(() => [])
            ]);
            
            const enrichedProfileData = {
              ...profileData,
              experiences: experiences || [],
              educations: educations || [],
              skills: skills || []
            };
            
            setCandidateProfile(enrichedProfileData);
          } else {
            setCandidateProfile(profileData);
          }
        } catch (profileError) {
          // Still set the basic profile data even if extended data fails
          const profileData = {}; // Declare profileData here
          setCandidateProfile(profileData);
        }
      } catch (error) {
        // Error fetching application details
      } finally {
        setIsLoading(false);
      }
    };

    fetchApplicationData();
  }, [open, candidate]); // Only depend on open and candidateId to prevent infinite loops

  // Reset data when modal closes
  useEffect(() => {
    if (!open) {
      setApplicationData(null);
      setCandidateProfile(null);
      setStepsData([]);
      setPlaylist([]);
      setTranscriptError(null);
      setPromoteError(null);
      setPromoteSuccess(null);
      setIsPromoting(false);
    }
  }, [open]);

  // Promote candidate to next step
  const handlePromoteToNextStep = async () => {
    if (!localCandidate || !allSteps || allSteps.length === 0) return;

    try {
      setIsPromoting(true);
      setPromoteError(null);

      // Get current step
      const currentStep = localCandidate.currentStep || (localCandidate.completedStepsCount || 0) + 1;
      
      // Check if current step exists
      const step = allSteps.find(s => s.stepNumber === currentStep);
      if (!step) {
        setPromoteError("Current step not found");
        return;
      }

      // Check if current step is a recruiter step
      const isRecruiterStep = step.stepDetails?.participant && step.stepDetails.participant.toLowerCase() === "recruiter";
      if (!isRecruiterStep) {
        setPromoteError("This step is not a recruiter step. Only recruiter steps can be promoted.");
        return;
      }

      // Check if next step exists
      const nextStepNumber = currentStep + 1;
      const nextStep = allSteps.find(s => s.stepNumber === nextStepNumber);
      if (!nextStep) {
        setPromoteError("No next step available for promotion");
        return;
      }

      // Call API to promote the candidate
      // This assumes your backend has an endpoint to update the candidate's step
      const payload = {
        JobPostName: jobPostName,
        JobPostVersion: jobPostVersion,
        CurrentStep: currentStep,
        NextStep: nextStepNumber,
        MarkAsComplete: true,
      };
      
      console.log("[v0] Promoting candidate with payload:", {
        applicationId: localCandidate.applicationId,
        payload,
        candidate: {
          currentStep: localCandidate.currentStep,
          completedStepsCount: localCandidate.completedStepsCount,
        }
      });

      const response = await api.put(
        `/JobApplication/${localCandidate.applicationId}/promote-step`,
        payload
      );

      if (response) {
        // Update local candidate state immediately for instant UI update
        const updatedCandidate = {
          ...localCandidate,
          currentStep: nextStepNumber,
          completedStepsCount: (localCandidate.completedStepsCount || 0) + 1,
        };
        
        // Update local state immediately
        setLocalCandidate(updatedCandidate);
        
        console.log("[v0] Candidate promoted successfully:", updatedCandidate);
        
        // Call parent callback to update the Kanban view and table
        if (onCandidatePromoted) {
          onCandidatePromoted(updatedCandidate);
        }
        
        // Show success message and keep modal open for user to see the change
        setPromoteError(null);
        setPromoteSuccess(`Candidate promoted to Step ${nextStepNumber}`);
        
        // Auto-hide success message after 3 seconds
        setTimeout(() => {
          setPromoteSuccess(null);
        }, 3000);
      }
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || error?.message || "Failed to promote candidate";
      setPromoteError(errorMsg);
      console.error("[v0] Error promoting candidate:", error);
    } finally {
      setIsPromoting(false);
    }
  };

  // Load transcripts when user clicks transcript tab
  const loadTranscripts = async () => {
    try {
      setTranscriptError(null);

      if (!applicationData?.steps) {
        setTranscriptError("No interview steps found");
        return;
      }

      // Build steps with interviews that have transcriptUrl
      const stepsWithInterviews: StepTranscript[] = applicationData.steps
        .filter((step: any) => step.interviews && step.interviews.length > 0)
        .map((step: any) => ({
          stepId: step.step?.id || `step-${step.step?.stepNumber}`,
          stepName: step.step?.jobPostStepName || `Step ${step.step?.stepNumber || ""}`,
          stepNumber: step.step?.stepNumber || 0,
          interviews: step.interviews
            .filter((inv: any) => inv.transcriptUrl)
            .map((inv: any) => ({
              interviewId: inv.id,
              transcriptUrl: inv.transcriptUrl,
              transcript: [],
              loading: false,
              error: null,
            })),
        }))
        .filter((step: StepTranscript) => step.interviews.length > 0)
        .sort((a: StepTranscript, b: StepTranscript) => a.stepNumber - b.stepNumber);

      if (stepsWithInterviews.length === 0) {
        setTranscriptError("No transcripts available for any interview");
        return;
      }

      setStepsData(stepsWithInterviews);

      // Load transcripts for all interviews
      for (const step of stepsWithInterviews) {
        for (const interview of step.interviews) {
          await loadTranscriptForInterview(step.stepId, interview.interviewId, interview.transcriptUrl);
        }
      }
    } catch (err) {
      setTranscriptError("Failed to load transcripts");
    }
  };

  const loadTranscriptForInterview = async (
    stepId: string,
    interviewId: string,
    transcriptUrl: string
  ) => {
    // Update loading state for this specific interview
    setStepsData((prev) =>
      prev.map((step) =>
        step.stepId === stepId
          ? {
              ...step,
              interviews: step.interviews.map((inv) =>
                inv.interviewId === interviewId
                  ? { ...inv, loading: true, error: null }
                  : inv
              ),
            }
          : step
      )
    );

    try {
      const response = await api.get(transcriptUrl);
      const data = (response as any)?.data || response;

      const transcriptData = data?.transcript || [];

      if (Array.isArray(transcriptData) && transcriptData.length > 0) {
        setStepsData((prev) =>
          prev.map((step) =>
            step.stepId === stepId
              ? {
                  ...step,
                  interviews: step.interviews.map((inv) =>
                    inv.interviewId === interviewId
                      ? { ...inv, transcript: transcriptData, loading: false, error: null }
                      : inv
                  ),
                }
              : step
          )
        );
      } else {
        setStepsData((prev) =>
          prev.map((step) =>
            step.stepId === stepId
              ? {
                  ...step,
                  interviews: step.interviews.map((inv) =>
                    inv.interviewId === interviewId
                      ? {
                          ...inv,
                          loading: false,
                          error: "Transcript not found in response",
                        }
                      : inv
                  ),
                }
              : step
          )
        );
      }
    } catch (err) {
      setStepsData((prev) =>
        prev.map((step) =>
          step.stepId === stepId
            ? {
                ...step,
                interviews: step.interviews.map((inv) =>
                  inv.interviewId === interviewId
                    ? { ...inv, loading: false, error: "Failed to load transcript" }
                    : inv
                ),
              }
            : step
        )
      );
    }
  };

  const loadTranscript = async (stepId: string, interviewId: string) => {
    const step = stepsData.find((s) => s.stepId === stepId);
    const interview = step?.interviews.find((i) => i.interviewId === interviewId);

    if (!interview || interview.transcript.length > 0) return;

    setStepsData((prev) =>
      prev.map((s) =>
        s.stepId === stepId
          ? {
              ...s,
              interviews: s.interviews.map((inv) =>
                inv.interviewId === interviewId
                  ? { ...inv, loading: true }
                  : inv
              ),
            }
          : s
      )
    );

    try {
      const transcriptData = await fetch(interview.transcriptUrl, {
        method: "GET",
        headers: {
          Authorization: `Bearer ${await getAccessToken()}`,
        },
      }).then((res) => res.json());

      setStepsData((prev) =>
        prev.map((s) =>
          s.stepId === stepId
            ? {
                ...s,
                interviews: s.interviews.map((inv) =>
                  inv.interviewId === interviewId
                    ? {
                        ...inv,
                        transcript: Array.isArray(transcriptData)
                          ? transcriptData
                          : transcriptData?.transcript || [],
                        loading: false,
                        error: null,
                      }
                    : inv
                ),
              }
            : s
        )
      );
    } catch (err) {
      setStepsData((prev) =>
        prev.map((s) =>
          s.stepId === stepId
            ? {
                ...s,
                interviews: s.interviews.map((inv) =>
                  inv.interviewId === interviewId
                    ? { ...inv, loading: false, error: "Failed to load transcript" }
                    : inv
                ),
              }
            : s
        )
      );
    }
  };

  if (!candidate) {
    return null;
  }

  const formatDate = (dateString?: string) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getStatusColor = (status: string) => {
    const lower = status.toLowerCase();
    if (lower === "completed") return "bg-green-500/10 text-green-700";
    if (lower === "in progress" || lower === "pending")
      return "bg-blue-500/10 text-blue-700";
    if (lower === "failed" || lower === "rejected")
      return "bg-red-500/10 text-red-700";
    return "bg-gray-500/10 text-gray-700";
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>
            Candidate Details: {candidate.candidateName}
          </DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center p-12">
            <div>
              <Loader2 className="h-8 w-8 animate-spin text-primary mx-auto" />
              <p className="mt-2 text-sm text-muted-foreground">
                Loading candidate details...
              </p>
            </div>
          </div>
        ) : (
          <Tabs
            value={activeTab}
            onValueChange={setActiveTab}
            className="flex-1 flex flex-col min-h-0"
          >
            <TabsList className="w-full justify-start border-b rounded-none bg-transparent p-0 overflow-x-auto">
              <TabsTrigger
                value="info"
                className="rounded-none border-b-2 border-b-transparent data-[state=active]:border-b-primary flex items-center gap-2"
              >
                <User className="h-4 w-4" />
                Basic Info
              </TabsTrigger>
              <TabsTrigger
                value="profile"
                className="rounded-none border-b-2 border-b-transparent data-[state=active]:border-b-primary flex items-center gap-2"
              >
                <Briefcase className="h-4 w-4" />
                Dashboard
              </TabsTrigger>
              <TabsTrigger
                value="assessment"
                className="rounded-none border-b-2 border-b-transparent data-[state=active]:border-b-primary flex items-center gap-2"
              >
                <BarChart3 className="h-4 w-4" />
                Assessment
              </TabsTrigger>
              <TabsTrigger
                value="steps"
                className="rounded-none border-b-2 border-b-transparent data-[state=active]:border-b-primary flex items-center gap-2"
              >
                <CheckCircle2 className="h-4 w-4" />
                Application Steps
              </TabsTrigger>
              <TabsTrigger
                value="interview"
                className="rounded-none border-b-2 border-b-transparent data-[state=active]:border-b-primary flex items-center gap-2"
              >
                <Headphones className="h-4 w-4" />
                Interview
              </TabsTrigger>
            </TabsList>

            {/* Tab 1: Basic Info */}
            <TabsContent value="info" className="flex-1 overflow-y-auto space-y-4 mt-6">
              <Card>
                <CardHeader>
                  <CardTitle className="text-base flex items-center gap-2">
                    <User className="h-4 w-4" />
                    Candidate Information
                  </CardTitle>
                </CardHeader>
                <CardContent className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-xs text-muted-foreground">Serial</p>
                    <p className="text-sm font-medium">{localCandidate?.candidateSerial}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">Name</p>
                    <p className="text-sm font-medium">{localCandidate?.candidateName}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground flex items-center gap-1">
                      <Mail className="h-3 w-3" /> Email
                    </p>
                    <p className="text-sm font-medium">{localCandidate?.candidateEmail}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground flex items-center gap-1">
                      <Calendar className="h-3 w-3" /> Applied
                    </p>
                    <p className="text-sm font-medium">{formatDate(localCandidate?.appliedAt)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">Current Step</p>
                    <p className="text-sm font-medium">
                      Step {localCandidate?.currentStep || localCandidate?.completedStepsCount ? (localCandidate?.completedStepsCount || 0) + 1 : 1}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">Completed Steps</p>
                    <p className="text-sm font-medium">
                      {localCandidate?.completedStepsCount || 0}
                    </p>
                  </div>
                </CardContent>
              </Card>

              {/* Promote to Next Step Card */}
              {allSteps && allSteps.length > 0 && (
                <Card className="border-blue-500/50 bg-blue-500/5">
                  <CardHeader>
                    <CardTitle className="text-base flex items-center gap-2">
                      <ArrowRight className="h-4 w-4 text-blue-600" />
                      Candidate Pipeline
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="space-y-2">
                      <p className="text-sm text-muted-foreground">
                        {(() => {
                          const currentStep = localCandidate?.currentStep || (localCandidate?.completedStepsCount || 0) + 1;
                          const step = allSteps.find(s => s.stepNumber === currentStep);
                          const isRecruiterStep = step?.stepDetails?.participant && step.stepDetails.participant.toLowerCase() === "recruiter";
                          
                          if (!step) return "Current step information not available";
                          if (!isRecruiterStep) return `Current step: ${step.stepDetails?.name || `Step ${currentStep}`} (${step.stepDetails?.participant || "candidate"} step)`;
                          return `Current step: ${step.stepDetails?.name || `Step ${currentStep}`} (Recruiter step)`;
                        })()}
                      </p>
                    </div>

                    {(() => {
                      const currentStep = localCandidate?.currentStep || (localCandidate?.completedStepsCount || 0) + 1;
                      const step = allSteps.find(s => s.stepNumber === currentStep);
                      const isRecruiterStep = step?.stepDetails?.participant && step.stepDetails.participant.toLowerCase() === "recruiter";
                      const nextStep = allSteps.find(s => s.stepNumber === currentStep + 1);

                      if (!isRecruiterStep) {
                        return (
                          <div className="p-3 rounded-lg bg-amber-500/10 border border-amber-500/20">
                            <p className="text-sm text-amber-700 dark:text-amber-300">
                              This step is assigned to the candidate. Promotion will be available once the recruiter step is ready.
                            </p>
                          </div>
                        );
                      }

                      return (
                        <div className="space-y-3">
                          {promoteSuccess && (
                            <div className="p-3 rounded-lg bg-green-500/10 border border-green-500/20">
                              <p className="text-sm text-green-700 dark:text-green-300">{promoteSuccess}</p>
                            </div>
                          )}
                          {promoteError && (
                            <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/20">
                              <p className="text-sm text-red-700 dark:text-red-300">{promoteError}</p>
                            </div>
                          )}
                          {nextStep ? (
                            <>
                              <p className="text-xs text-muted-foreground">
                                Next step: <span className="font-medium">{nextStep.stepDetails?.name || `Step ${nextStep.stepNumber}`}</span>
                              </p>
                              <Button
                                onClick={handlePromoteToNextStep}
                                disabled={isPromoting}
                                className="w-full gap-2"
                              >
                                {isPromoting ? (
                                  <>
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                    Promoting...
                                  </>
                                ) : (
                                  <>
                                    <ArrowRight className="h-4 w-4" />
                                    Promote to Next Step
                                  </>
                                )}
                              </Button>
                            </>
                          ) : (
                            <div className="p-3 rounded-lg bg-green-500/10 border border-green-500/20">
                              <p className="text-sm text-green-700 dark:text-green-300">
                                ✓ Candidate has completed all steps
                              </p>
                            </div>
                          )}
                        </div>
                      );
                    })()}
                  </CardContent>
                </Card>
              )}

              {/* Additional Candidate Information */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Personal Information</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <div className="flex items-center gap-2 mb-1">
                        <Phone className="h-4 w-4 text-muted-foreground" />
                        <p className="text-xs text-muted-foreground font-medium">Phone Number</p>
                      </div>
                      <p className="text-sm font-medium">
                        {candidateProfile?.userProfile?.phoneNumber || "Not provided"}
                      </p>
                    </div>
                    <div>
                      <div className="flex items-center gap-2 mb-1">
                        <Calendar className="h-4 w-4 text-muted-foreground" />
                        <p className="text-xs text-muted-foreground font-medium">Age</p>
                      </div>
                      <p className="text-sm font-medium">
                        {candidateProfile?.userProfile?.age ? `${candidateProfile.userProfile.age} years` : "Not provided"}
                      </p>
                    </div>
                    <div>
                      <div className="flex items-center gap-2 mb-1">
                        <Globe className="h-4 w-4 text-muted-foreground" />
                        <p className="text-xs text-muted-foreground font-medium">Nationality</p>
                      </div>
                      <p className="text-sm font-medium">
                        {candidateProfile?.userProfile?.nationality || "Not provided"}
                      </p>
                    </div>
                    <div>
                      <div className="flex items-center gap-2 mb-1">
                        <Plane className="h-4 w-4 text-muted-foreground" />
                        <p className="text-xs text-muted-foreground font-medium">Open to Relocation</p>
                      </div>
                      <p className="text-sm font-medium">
                        {candidateProfile?.userProfile?.openToRelocation ? "Yes" : "No"}
                      </p>
                    </div>
                  </div>

                  <Separator />

                  <div className="grid grid-cols-1 gap-4">
                    <div>
                      <div className="flex items-center gap-2 mb-1">
                        <Briefcase className="h-4 w-4 text-muted-foreground" />
                        <p className="text-xs text-muted-foreground font-medium">Job Type Preferences</p>
                      </div>
                      <div className="flex gap-2 flex-wrap">
                        {candidateProfile?.userProfile?.jobTypePreferences && candidateProfile.userProfile.jobTypePreferences.length > 0 ? (
                          candidateProfile.userProfile.jobTypePreferences.map((type: string, idx: number) => (
                            <Badge key={idx} variant="secondary" className="text-xs">
                              {type}
                            </Badge>
                          ))
                        ) : (
                          <p className="text-xs text-muted-foreground">Not specified</p>
                        )}
                      </div>
                    </div>
                    <div>
                      <div className="flex items-center gap-2 mb-1">
                        <Laptop className="h-4 w-4 text-muted-foreground" />
                        <p className="text-xs text-muted-foreground font-medium">Remote Work Preferences</p>
                      </div>
                      <div className="flex gap-2 flex-wrap">
                        {candidateProfile?.userProfile?.remotePreferences && candidateProfile.userProfile.remotePreferences.length > 0 ? (
                          candidateProfile.userProfile.remotePreferences.map((pref: string, idx: number) => (
                            <Badge key={idx} variant="secondary" className="text-xs">
                              {pref}
                            </Badge>
                          ))
                        ) : (
                          <p className="text-xs text-muted-foreground">Not specified</p>
                        )}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            {/* Tab 2: Dashboard (Education, Experience, Skills) */}
            <TabsContent value="profile" className="flex-1 overflow-y-auto space-y-4 mt-6">
              {/* CV Download */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-base flex items-center gap-2">
                    <FileText className="h-4 w-4" />
                    Curriculum Vitae
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {candidateProfile?.userProfile?.resumeUrl || candidate?.candidateCvFilePath ? (
                    <a
                      href={candidateProfile?.userProfile?.resumeUrl || candidate?.candidateCvFilePath || "#"}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="inline-flex items-center gap-2 px-4 py-2 bg-primary/10 hover:bg-primary/20 text-primary rounded-lg transition-colors"
                    >
                      <FileText className="h-4 w-4" />
                      Download CV
                    </a>
                  ) : (
                    <p className="text-sm text-muted-foreground">No CV available</p>
                  )}
                </CardContent>
              </Card>

              {/* Experience Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-base flex items-center gap-2">
                    <Briefcase className="h-4 w-4" />
                    Professional Experience
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {candidateProfile?.experiences && candidateProfile.experiences.length > 0 ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                      {candidateProfile.experiences.map((exp: any, idx: number) => {
                        const startYear = exp.startDate ? new Date(exp.startDate).getFullYear() : "";
                        const endYear = exp.endDate ? new Date(exp.endDate).getFullYear() : "Present";
                        const duration = startYear && endYear ? `${startYear} - ${endYear}` : "";
                        const title = exp.title || exp.jobTitle || "Position";
                        const company = exp.organization || exp.company || "Company";
                        
                        return (
                          <div
                            key={idx}
                            className="group relative p-3 rounded-lg border border-border bg-card hover:bg-secondary/80 dark:hover:bg-secondary/40 hover:border-primary/50 transition-all duration-200 cursor-help overflow-hidden"
                          >
                            {/* Hover detail overlay */}
                            <div className="absolute inset-0 bg-gradient-to-b from-secondary/60 to-secondary/30 dark:from-muted/60 dark:to-muted/30 opacity-0 group-hover:opacity-100 transition-opacity duration-200 p-3 flex flex-col justify-between rounded-lg pointer-events-none">
                              {exp.description && (
                                <p className="text-xs text-foreground line-clamp-3 font-medium">{exp.description}</p>
                              )}
                              <div className="flex items-center gap-2 text-xs text-foreground">
                                {exp.location && <span>{exp.location}</span>}
                              </div>
                            </div>

                            {/* Main content */}
                            <div className="relative z-10 group-hover:opacity-0 transition-opacity duration-200">
                              <p className="font-semibold text-sm truncate">{title}</p>
                              <p className="text-xs text-muted-foreground truncate">{company}</p>
                              {duration && (
                                <Badge variant="secondary" className="text-xs mt-2">
                                  {duration}
                                </Badge>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground text-center py-4">No experience data available</p>
                  )}
                </CardContent>
              </Card>

              {/* Education Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-base flex items-center gap-2">
                    <BookOpen className="h-4 w-4" />
                    Education
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {candidateProfile?.educations && candidateProfile.educations.length > 0 ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                      {candidateProfile.educations.map((edu: any, idx: number) => {
                        const graduationYear = edu.endDate ? new Date(edu.endDate).getFullYear() : "";
                        const degree = edu.degree || edu.degreeType || "Degree";
                        const institution = edu.institution || edu.school || "Institution";
                        
                        return (
                          <div
                            key={idx}
                            className="group relative p-3 rounded-lg border border-border bg-card hover:bg-blue-50/50 dark:hover:bg-blue-950/30 hover:border-blue-400/50 dark:hover:border-blue-600/50 transition-all duration-200 cursor-help overflow-hidden"
                          >
                            {/* Hover detail overlay */}
                            <div className="absolute inset-0 bg-gradient-to-b from-blue-100/60 to-blue-50/30 dark:from-blue-950/60 dark:to-blue-900/30 opacity-0 group-hover:opacity-100 transition-opacity duration-200 p-3 flex flex-col justify-between rounded-lg pointer-events-none">
                              {edu.fieldOfStudy && (
                                <p className="text-xs text-foreground font-medium line-clamp-2">{edu.fieldOfStudy}</p>
                              )}
                              <div className="flex flex-col gap-1 text-xs text-foreground">
                                {edu.location && <span>{edu.location}</span>}
                                {graduationYear && <span>Graduated: {graduationYear}</span>}
                              </div>
                            </div>

                            {/* Main content */}
                            <div className="relative z-10 group-hover:opacity-0 transition-opacity duration-200">
                              <p className="font-semibold text-sm truncate">{degree}</p>
                              <p className="text-xs text-muted-foreground truncate">{institution}</p>
                              {graduationYear && (
                                <Badge variant="secondary" className="text-xs mt-2">
                                  {graduationYear}
                                </Badge>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground text-center py-4">No education data available</p>
                  )}
                </CardContent>
              </Card>

              {/* Skills Section */}
              <Card>
                <CardHeader>
                  <CardTitle className="text-base flex items-center gap-2">
                    <Zap className="h-4 w-4" />
                    Skills & Expertise
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {candidateProfile?.skills && candidateProfile.skills.length > 0 ? (
                    <div className="flex flex-wrap gap-2">
                      {candidateProfile.skills.map((skill: any, idx: number) => {
                        const yearsExp = skill.yearsExperience ? `${skill.yearsExperience}y` : "";
                        const proficiency = skill.proficiency ? skill.proficiency : "";
                        const displayText = yearsExp || proficiency;
                        
                        return (
                          <div
                            key={idx}
                            className="group relative"
                          >
                            <Badge
                              variant="outline"
                              className="text-xs cursor-help hover:border-amber-500 dark:hover:border-amber-600 hover:bg-amber-50 dark:hover:bg-amber-950/30 hover:text-amber-700 dark:hover:text-amber-300 transition-all duration-200 bg-card border border-border"
                            >
                              <span className="font-medium">{skill.skillName}</span>
                              {displayText && <span className="ml-1 opacity-70">{displayText}</span>}
                            </Badge>
                            
                            {/* Tooltip on hover */}
                            {(skill.category || proficiency) && (
                              <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-2 py-1.5 bg-amber-900 dark:bg-amber-100 text-amber-50 dark:text-amber-950 text-xs rounded-md opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none whitespace-nowrap z-50">
                                {skill.category && <div className="font-medium">{skill.category}</div>}
                                {proficiency && <div>{proficiency}</div>}
                                <div className="absolute top-full left-1/2 -translate-x-1/2 border-4 border-transparent border-t-amber-900 dark:border-t-amber-100" />
                              </div>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground text-center py-4">No skills data available</p>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            {/* Tab 3: Assessment & Evaluation (Scores, Strengths, Weaknesses, AI Feedback) */}
            <TabsContent value="assessment" className="flex-1 overflow-y-auto space-y-4 mt-6">
              {isLoadingAssessment ? (
                <div className="flex items-center justify-center p-8">
                  <Loader2 className="h-6 w-6 animate-spin text-primary" />
                </div>
              ) : (
                <>
                  {/* Scores Card */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-base flex items-center gap-2">
                        <BarChart3 className="h-4 w-4" />
                        Assessment Scores
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      {assessmentScorings.length > 0 ? (
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                          {(() => {
                            const avgScore = assessmentScorings.length > 0
                              ? Math.round(
                                  assessmentScorings.reduce((acc, s) => acc + s.score, 0) / assessmentScorings.length
                                )
                              : 0;
                            
                            return (
                              <>
                                {/* Average Score */}
                                <div className="rounded-lg border border-green-500 bg-green-500/5 p-3">
                                  <p className="text-xs text-muted-foreground mb-1">Average</p>
                                  <p className="text-lg font-bold text-green-600">{avgScore}/10</p>
                                </div>
                                
                                {/* Display all scorings */}
                                {assessmentScorings.map((scoring, idx) => (
                                  <div
                                    key={idx}
                                    className="rounded-lg border border-blue-500 bg-blue-500/5 p-3"
                                  >
                                    <p className="text-xs text-muted-foreground mb-1 truncate">
                                      {scoring.fixedCategory}
                                    </p>
                                    <p className="text-lg font-bold text-blue-600">{scoring.score}/10</p>
                                  </div>
                                ))}
                              </>
                            );
                          })()}
                        </div>
                      ) : (
                        <div className="text-center py-6 text-sm text-muted-foreground">
                          <p>No assessment scores available yet</p>
                        </div>
                      )}
                    </CardContent>
                  </Card>

                  {/* Strengths Card */}
                  <Card className="border-green-500">
                    <CardHeader>
                      <CardTitle className="text-base flex items-center gap-2">
                        <Lightbulb className="h-4 w-4 text-green-500" />
                        Strengths
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      {derivedFeedback.strengthList.length > 0 ? (
                        <ul className="space-y-2">
                          {derivedFeedback.strengthList.map((strength, idx) => (
                            <li key={idx} className="flex items-start gap-2 text-sm">
                              <span className="inline-block h-2 w-2 rounded-full bg-green-500 mt-1.5 flex-shrink-0" />
                              <span className="text-foreground">{strength}</span>
                            </li>
                          ))}
                        </ul>
                      ) : (
                        <p className="text-center text-sm text-muted-foreground py-2">
                          No strengths recorded yet
                        </p>
                      )}
                    </CardContent>
                  </Card>

                  {/* Weaknesses Card */}
                  <Card className="border-yellow-500">
                    <CardHeader>
                      <CardTitle className="text-base flex items-center gap-2">
                        <Hammer className="h-4 w-4 text-yellow-500" />
                        Areas for Improvement
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      {derivedFeedback.weaknessList.length > 0 ? (
                        <ul className="space-y-2">
                          {derivedFeedback.weaknessList.map((weakness, idx) => (
                            <li key={idx} className="flex items-start gap-2 text-sm">
                              <span className="inline-block h-2 w-2 rounded-full bg-yellow-500 mt-1.5 flex-shrink-0" />
                              <span className="text-foreground">{weakness}</span>
                            </li>
                          ))}
                        </ul>
                      ) : (
                        <p className="text-sm text-muted-foreground text-center py-2">
                          No areas for improvement recorded yet
                        </p>
                      )}
                    </CardContent>
                  </Card>

                  {/* AI Feedback Card */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-base flex items-center gap-2">
                        <Star className="h-4 w-4" />
                        AI Feedback
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      <div>
                        <label className="text-xs text-muted-foreground font-medium block mb-2">
                          Positive Summary
                        </label>
                        <div className="bg-muted/50 rounded-lg p-3 text-sm min-h-20">
                          {derivedFeedback.positiveSummary ? (
                            <p className="text-foreground">{derivedFeedback.positiveSummary}</p>
                          ) : (
                            <p className="text-muted-foreground">No positive feedback available yet</p>
                          )}
                        </div>
                      </div>
                      <Separator />
                      <div>
                        <label className="text-xs text-muted-foreground font-medium block mb-2">
                          Areas for Improvement
                        </label>
                        <div className="bg-muted/50 rounded-lg p-3 text-sm min-h-20">
                          {derivedFeedback.negativeSummary ? (
                            <p className="text-foreground">{derivedFeedback.negativeSummary}</p>
                          ) : (
                            <p className="text-muted-foreground">No improvement areas recorded yet</p>
                          )}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                </>
              )}
            </TabsContent>

            {/* Tab 4: Application Steps */}
            <TabsContent value="steps" className="flex-1 overflow-y-auto space-y-4 mt-6">
              {isLoading ? (
                <div className="flex items-center justify-center p-8">
                  <Loader2 className="h-6 w-6 animate-spin text-primary" />
                </div>
              ) : applicationData?.steps && applicationData.steps.length > 0 ? (
                <div className="space-y-3">
                  {applicationData.steps.map((stepData: any) => (
                    <Card
                      key={stepData.step.id}
                      className={`border-l-4 ${
                        stepData.step.status.toLowerCase() === "completed"
                          ? "border-l-green-500"
                          : stepData.step.status.toLowerCase() === "in progress"
                            ? "border-l-blue-500"
                            : "border-l-gray-400"
                      }`}
                    >
                      <CardContent className="p-4">
                        <div className="flex items-start justify-between gap-4">
                          <div className="flex-1">
                            <div className="flex items-center gap-3 mb-2">
                              {stepData.step.status.toLowerCase() === "completed" ? (
                                <CheckCircle2 className="h-4 w-4 text-green-600" />
                              ) : stepData.step.status.toLowerCase() === "in progress" ? (
                                <Clock className="h-4 w-4 text-blue-600" />
                              ) : (
                                <AlertCircle className="h-4 w-4 text-gray-600" />
                              )}
                              <div>
                                <h4 className="font-semibold text-sm">
                                  {stepData.step.jobPostStepName}
                                </h4>
                                <p className="text-xs text-muted-foreground">
                                  Step {stepData.step.stepNumber}
                                </p>
                              </div>
                            </div>

                            <div className="grid grid-cols-2 gap-3 mt-3 ml-7">
                              {stepData.step.startedAt && (
                                <div>
                                  <p className="text-xs text-muted-foreground">Started</p>
                                  <p className="text-xs font-medium">
                                    {formatDate(stepData.step.startedAt)}
                                  </p>
                                </div>
                              )}

                              {stepData.step.completedAt && (
                                <div>
                                  <p className="text-xs text-muted-foreground">Completed</p>
                                  <p className="text-xs font-medium">
                                    {formatDate(stepData.step.completedAt)}
                                  </p>
                                </div>
                              )}
                            </div>

                            {stepData.interviews && stepData.interviews.length > 0 && (
                              <div className="mt-3 pt-3 border-t border-border space-y-2">
                                <Badge variant="secondary" className="text-xs">
                                  <Headphones className="h-3 w-3 mr-1" />
                                  {stepData.interviews.length} Interview(s)
                                </Badge>
                              </div>
                            )}
                          </div>

                          <div>
                            <Badge className={getStatusColor(stepData.step.status)}>
                              {stepData.step.status}
                            </Badge>
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              ) : (
                <div className="flex items-center justify-center p-12 bg-muted/30 rounded-lg border border-dashed border-border">
                  <div className="text-center">
                    <AlertCircle className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                    <p className="text-sm text-muted-foreground">No steps data available</p>
                  </div>
                </div>
              )}
            </TabsContent>

            {/* Tab 5: Interview Details */}
            <TabsContent value="interview" className="flex-1 overflow-y-auto space-y-4 mt-6">
              {playlist.length > 0 && (
                <Card className="mb-4">
                  <CardHeader>
                    <CardTitle className="text-base flex items-center gap-2">
                      <Headphones className="h-4 w-4" />
                      Interview Audio
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <InterviewAudioPlayer
                      playlist={playlist}
                      candidateName={candidate.candidateName}
                    />
                  </CardContent>
                </Card>
              )}

              <Card>
                <CardHeader>
                  <CardTitle className="text-base flex items-center gap-2">
                    <FileText className="h-4 w-4" />
                    Interview Transcripts
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {transcriptError && stepsData.length === 0 && (
                    <div className="py-4 text-sm text-muted-foreground text-center">
                      {transcriptError}
                    </div>
                  )}

                  {stepsData.length === 0 && !transcriptError && (
                    <button
                      onClick={loadTranscripts}
                      className="w-full py-2 text-sm text-primary hover:underline"
                    >
                      Load Transcripts
                    </button>
                  )}

                  {stepsData.length > 0 && (
                    <Tabs defaultValue={stepsData[0]?.stepId} className="w-full">
                      <div className="relative mb-4 bg-gray-200/50 rounded-lg p-2 overflow-hidden">
                        <div
                          ref={scrollRef}
                          className="overflow-x-auto scroll-smooth [&::-webkit-scrollbar]:hidden flex gap-2"
                          style={{
                            scrollbarWidth: "none",
                            msOverflowStyle: "none",
                          }}
                        >
                          <TabsList className="gap-2 bg-transparent border-0 p-0 h-auto">
                            {stepsData.map((step, index) => (
                              <TabsTrigger
                                key={step.stepId}
                                value={step.stepId}
                                className="shrink-0 px-4 py-2 text-xs rounded-lg font-medium data-[state=active]:bg-primary data-[state=active]:text-primary-foreground"
                              >
                                <span className="mr-2">Step {step.stepNumber}</span>
                                <span className="text-xs opacity-70">{step.stepName}</span>
                              </TabsTrigger>
                            ))}
                          </TabsList>
                        </div>
                      </div>

                      {stepsData.map((step) => (
                        <TabsContent
                          key={step.stepId}
                          value={step.stepId}
                          className="space-y-4"
                        >
                          {step.interviews.map((interview, idx) => (
                            <div key={interview.interviewId} className="space-y-3">
                              {step.interviews.length > 1 && (
                                <p className="text-sm font-semibold text-muted-foreground pb-2 border-b">
                                  Interview {idx + 1}
                                </p>
                              )}

                              {interview.loading && (
                                <div className="text-center py-8">
                                  <Loader2 className="h-6 w-6 animate-spin text-primary mx-auto" />
                                  <p className="mt-2 text-xs text-muted-foreground">
                                    Loading transcript...
                                  </p>
                                </div>
                              )}

                              {interview.error && (
                                <div className="py-4 text-sm text-destructive text-center">
                                  {interview.error}
                                </div>
                              )}

                              {!interview.loading &&
                                !interview.error &&
                                interview.transcript.length === 0 && (
                                  <div className="py-6 text-center text-muted-foreground text-sm">
                                    No transcript available.
                                  </div>
                                )}

                              {!interview.loading &&
                                !interview.error &&
                                interview.transcript.length > 0 && (
                                  <div className="space-y-4 py-2">
                                    {interview.transcript.map((entry: any, entryIdx: number) => {
                                      const isAgent =
                                        (entry.role || "").toLowerCase() === "agent";
                                      const speaker = isAgent ? "AI Interviewer" : "Candidate";
                                      const text =
                                        entry.text ||
                                        entry.content ||
                                        entry.message ||
                                        entry.transcript ||
                                        "";

                                      return (
                                        <div
                                          key={entryIdx}
                                          className={`flex gap-3 ${
                                            isAgent ? "flex-row" : "flex-row-reverse"
                                          }`}
                                        >
                                          {/* Avatar/Icon */}
                                          <div
                                            className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center ${
                                              isAgent
                                                ? "bg-primary/10 text-primary"
                                                : "bg-blue-500/10 text-blue-600"
                                            }`}
                                          >
                                            {isAgent ? (
                                              <Bot className="h-4 w-4" />
                                            ) : (
                                              <User className="h-4 w-4" />
                                            )}
                                          </div>

                                          {/* Message Content */}
                                          <div
                                            className={`flex-1 min-w-0 ${
                                              isAgent ? "" : "flex justify-end"
                                            }`}
                                          >
                                            <div
                                              className={`inline-block max-w-[85%] rounded-2xl px-4 py-3 shadow-sm ${
                                                isAgent
                                                  ? "bg-primary/5 border border-primary/20 text-foreground rounded-tl-sm"
                                                  : "bg-blue-500/5 border border-blue-500/20 text-foreground rounded-tr-sm"
                                              }`}
                                            >
                                              <div className="text-xs font-semibold mb-1.5 opacity-70">
                                                {speaker}
                                              </div>
                                              <p className="text-sm leading-relaxed whitespace-pre-wrap break-words">
                                                {text || "[No content]"}
                                              </p>
                                            </div>
                                          </div>
                                        </div>
                                      );
                                    })}
                                  </div>
                                )}

                              {!interview.loading &&
                                !interview.error &&
                                interview.transcript.length > 0 && (
                                  <button
                                    onClick={() =>
                                      loadTranscript(step.stepId, interview.interviewId)
                                    }
                                    className="w-full mt-2 py-1 text-xs text-muted-foreground hover:text-foreground"
                                  >
                                    Reload
                                  </button>
                                )}
                            </div>
                          ))}
                        </TabsContent>
                      ))}
                    </Tabs>
                  )}
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        )}
      </DialogContent>
    </Dialog>
  );
}
