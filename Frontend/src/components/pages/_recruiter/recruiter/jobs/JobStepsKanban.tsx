"use client";

import { AlertDialogAction } from "@/components/ui/alert-dialog"
import { AlertDialogCancel } from "@/components/ui/alert-dialog"
import { AlertDialogDescription } from "@/components/ui/alert-dialog"
import { AlertDialogTitle } from "@/components/ui/alert-dialog"
import { AlertDialogHeader } from "@/components/ui/alert-dialog"
import { AlertDialogContent } from "@/components/ui/alert-dialog"
import { AlertDialog } from "@/components/ui/alert-dialog"
import { useState, useEffect, useRef } from "react";
import { GripVertical, CheckCircle2, Clock, Users, Loader2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
  TooltipProvider,
} from "@/components/ui/tooltip";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/useApi";
import CandidateDetailsModal from "./CandidateDetailsModal";

interface JobStep {
  stepNumber: number;
  status: string;
  stepDetails: {
    name: string;
    version: number;
    stepType: string;
    isInterview: boolean;
    participant?: string;
  };
}

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
  completedSteps?: Array<{
    stepNumber: number;
    status: string;
    completedAt?: string;
  }>;
}

interface JobStepsKanbanProps {
  steps: JobStep[];
  candidates: JobStepCandidate[];
  jobPostName?: string;
  jobPostVersion?: number;
  onRefreshNeeded?: () => void;
}

interface DragState {
  candidateId: string;
  applicationId: string;
  fromStep: number;
  toStep: number;
}


export default function JobStepsKanban({ 
  steps, 
  candidates,
  jobPostName = "",
  jobPostVersion = 1,
  onRefreshNeeded,
}: JobStepsKanbanProps) {
  const [kanbanData, setKanbanData] = useState<Map<number, JobStepCandidate[]>>(new Map());
  const [selectedCandidate, setSelectedCandidate] = useState<JobStepCandidate | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [dragState, setDragState] = useState<DragState | null>(null);
  const [isPromoting, setIsPromoting] = useState(false);
  const [promoteError, setPromoteError] = useState<string | null>(null);
  const api = useApi();

  const handleCandidatePromoted = (updatedCandidate: JobStepCandidate) => {
    // Update the candidate in the local state
    setSelectedCandidate(updatedCandidate);
    
    // Update kanban data immediately to reflect the promotion
    const previousStep = (updatedCandidate.completedStepsCount || 0); // The step they were in
    const newStep = updatedCandidate.currentStep || (previousStep + 1); // The step they moved to
    
    setKanbanData((prevData) => {
      const newData = new Map(prevData);
      
      // Remove from previous step
      const previousStepCandidates = newData.get(previousStep) || [];
      newData.set(
        previousStep,
        previousStepCandidates.filter(c => c.applicationId !== updatedCandidate.applicationId)
      );
      
      // Add to new step
      const newStepCandidates = newData.get(newStep) || [];
      newData.set(newStep, [...newStepCandidates, updatedCandidate]);
      
      return newData;
    });
    
    // Trigger a refresh of the parent component to sync the table
    if (onRefreshNeeded) {
      onRefreshNeeded();
    }
  };

  const handlePromoteFromDragDrop = async () => {
    if (!dragState || !dragState.applicationId || !dragState.fromStep || !dragState.toStep) {
      return;
    }

    try {
      setIsPromoting(true);
      setPromoteError(null);

      const payload = {
        JobPostName: jobPostName,
        JobPostVersion: jobPostVersion,
        CurrentStep: dragState.fromStep,
        NextStep: dragState.toStep,
        MarkAsComplete: true,
      };

      const response = await api.put(
        `/JobApplication/${dragState.applicationId}/promote-step`,
        payload
      );

      if (response) {
        // Update kanban data immediately for real-time UI update
        setKanbanData((prevData) => {
          const newData = new Map(prevData);
          
          // Remove from source step
          const sourceStepCandidates = newData.get(dragState.fromStep) || [];
          newData.set(
            dragState.fromStep,
            sourceStepCandidates.filter(c => c.applicationId !== dragState.applicationId)
          );
          
          // Find the candidate to move
          const candidateToMove = sourceStepCandidates.find(c => c.applicationId === dragState.applicationId);
          if (candidateToMove) {
            // Add to destination step
            const destStepCandidates = newData.get(dragState.toStep) || [];
            const updatedCandidate = {
              ...candidateToMove,
              currentStep: dragState.toStep,
              completedStepsCount: (candidateToMove.completedStepsCount || 0) + 1,
            };
            newData.set(dragState.toStep, [...destStepCandidates, updatedCandidate]);
          }
          
          return newData;
        });

        // Call the refresh callback to sync the data with the parent component
        if (onRefreshNeeded) {
          onRefreshNeeded();
        }
        
        setDragState(null);
      }
    } catch (error: any) {
      const errorMsg = error?.response?.data?.message || error?.message || "Failed to promote candidate";
      setPromoteError(errorMsg);
    } finally {
      setIsPromoting(false);
    }
  };

  const handleCandidateDragEnd = (result: any, fromStepNumber: number) => {
    const { source, destination, draggableId } = result;

    // If dropped outside a valid destination, do nothing
    if (!destination) {
      return;
    }

    // If dropped in the same position, do nothing
    if (
      source.droppableId === destination.droppableId &&
      source.index === destination.index
    ) {
      return;
    }

    const toStepNumber = parseInt(destination.droppableId);

    // Only allow dragging to the next immediate step
    if (toStepNumber !== fromStepNumber + 1) {
      return;
    }

    // Find the candidate being dragged
    const stepCandidates = kanbanData.get(fromStepNumber) || [];
    const candidate = stepCandidates[source.index];

    if (!candidate) {
      return;
    }

    // Find the current step to check if it's a recruiter step
    const currentStep = steps.find(s => s.stepNumber === fromStepNumber);
    const isRecruiterStep = currentStep?.stepDetails?.participant && 
      currentStep.stepDetails.participant.toLowerCase() === "recruiter";

    if (!isRecruiterStep) {
      return;
    }

    // Show confirmation dialog
    setDragState({
      candidateId: candidate.candidateId,
      applicationId: candidate.applicationId,
      fromStep: fromStepNumber,
      toStep: toStepNumber,
    });
  };

  useEffect(() => {
    // Group candidates by their current step
    const grouped = new Map<number, JobStepCandidate[]>();

    // Initialize all step columns
    steps.forEach((step) => {
      grouped.set(step.stepNumber, []);
    });

    // Group candidates by their current step
    candidates.forEach((candidate) => {
      let currentStep = 1;

      // 1. If currentStep is explicitly provided, use it
      if (candidate.currentStep && candidate.currentStep > 0) {
        currentStep = candidate.currentStep;
      }
      // 2. If completedStepsCount is provided, next step = completed + 1
      else if (typeof candidate.completedStepsCount === 'number') {
        currentStep = candidate.completedStepsCount + 1;
      }
      // 3. If completedSteps array is provided, count completed and move to next
      else if (Array.isArray(candidate.completedSteps)) {
        const completedCount = candidate.completedSteps.filter(
          (step) => step.status && step.status.toLowerCase() === 'completed'
        ).length;
        currentStep = completedCount + 1;
      }
      // 4. Try to extract step number from status string (fallback)
      else if (candidate.status && candidate.status.includes("step")) {
        const match = candidate.status.match(/step[\s-]*(\d+)/i);
        if (match) {
          currentStep = parseInt(match[1]);
        }
      }
      // 5. If completed, place them in the last step
      else if (candidate.status && candidate.status.toLowerCase() === "completed") {
        currentStep = steps.length;
      }

      // Ensure current step is within valid range
      if (currentStep > steps.length) {
        currentStep = steps.length;
      }

      if (grouped.has(currentStep)) {
        const stepCandidates = grouped.get(currentStep) || [];
        stepCandidates.push(candidate);
        grouped.set(currentStep, stepCandidates);
      } else if (currentStep > 0 && currentStep <= steps.length) {
        grouped.set(currentStep, [candidate]);
      }
    });

    setKanbanData(grouped);
  }, [candidates, steps, refreshTrigger]);

  const getStepColor = (stepNumber: number, totalSteps: number) => {
    const percentage = (stepNumber / totalSteps) * 100;
    if (percentage <= 25) return "from-blue-500/10 to-blue-400/5";
    if (percentage <= 50) return "from-purple-500/10 to-purple-400/5";
    if (percentage <= 75) return "from-amber-500/10 to-amber-400/5";
    return "from-green-500/10 to-green-400/5";
  };

  const getStepIcon = (stepNumber: number, totalSteps: number) => {
    if (stepNumber === totalSteps) {
      return <CheckCircle2 className="h-5 w-5 text-green-600" />;
    }
    return <Clock className="h-5 w-5 text-blue-600" />;
  };

  return (
    <div className="w-full">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <div className="flex items-center gap-2">
              Job Steps Pipeline
              <Badge variant="secondary" className="ml-2">
                {candidates.length} Candidates
              </Badge>
            </div>
          </CardTitle>
        </CardHeader>

        <CardContent>
          <div className="overflow-x-auto">
            <TooltipProvider>
              <div className="flex gap-4 min-w-full pb-4">
                {steps.map((step, index) => {
                  const stepCandidates = kanbanData.get(step.stepNumber) || [];
                  const isLastStep = index === steps.length - 1;

                  return (
                    <div
                      key={step.stepNumber}
                      className="flex-shrink-0 w-80"
                    >
                      {/* Column Header */}
                      <div
                        className={`bg-gradient-to-b ${getStepColor(step.stepNumber, steps.length)} rounded-t-lg p-4 border border-b-0 border-border`}
                      >
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center gap-2">
                            {getStepIcon(step.stepNumber, steps.length)}
                            <div className="flex flex-col gap-0.5">
                              <h3 className="font-semibold text-sm">
                                Step {step.stepNumber}
                              </h3>
                              <p className="text-xs text-muted-foreground">
                                {step.stepDetails.name}
                              </p>
                            </div>
                          </div>
                          <Badge
                            variant="outline"
                            className="flex items-center gap-1"
                          >
                            <Users className="h-3 w-3" />
                            {stepCandidates.length}
                          </Badge>
                        </div>

                        <div className="text-xs text-muted-foreground space-y-1">
                          <p>Type: {step.stepDetails.stepType}</p>
                          <p>Version: v{step.stepDetails.version}</p>
                          {step.stepDetails.isInterview && (
                            <Badge variant="default" className="text-xs mt-2 w-fit">
                              Interview Step
                            </Badge>
                          )}
                        </div>
                      </div>

                      {/* Candidates Container */}
                      <div
                        onDragOver={(e) => {
                          e.preventDefault();
                          e.dataTransfer.dropEffect = "move";
                          if (index > 0) {
                            e.currentTarget.classList.add("bg-primary/5");
                          }
                        }}
                        onDragLeave={(e) => {
                          e.currentTarget.classList.remove("bg-primary/5");
                        }}
                        onDrop={(e) => {
                          e.preventDefault();
                          e.currentTarget.classList.remove("bg-primary/5");

                          try {
                            const data = JSON.parse(
                              e.dataTransfer.getData("candidate")
                            );
                            const fromStep = data.fromStep;
                            const toStep = step.stepNumber;

                            // Only allow dragging to the next immediate step
                            if (toStep !== fromStep + 1) {
                              return;
                            }

                            // Find the current step to check if it's a recruiter step
                            const currentStep = steps.find(s => s.stepNumber === fromStep);
                            const isRecruiterStep = currentStep?.stepDetails?.participant && 
                              currentStep.stepDetails.participant.toLowerCase() === "recruiter";

                            if (!isRecruiterStep) {
                              return;
                            }

                            // Show confirmation dialog
                            setDragState({
                              candidateId: data.candidateId,
                              applicationId: data.applicationId,
                              fromStep: fromStep,
                              toStep: toStep,
                            });
                          } catch (error) {
                            // Silently ignore drag data parsing errors
                          }
                        }}
                        className="bg-muted/30 border border-t-0 border-border rounded-b-lg p-3 min-h-96 space-y-2 transition-colors"
                      >
                        {stepCandidates.length === 0 ? (
                          <div className="h-full flex items-center justify-center text-center">
                            <div className="text-muted-foreground text-sm">
                              <p className="font-medium">No candidates</p>
                              <p className="text-xs">yet in this step</p>
                            </div>
                          </div>
                        ) : (
                          stepCandidates.map((candidate, candidateIndex) => {
                            const currentStep = steps.find(s => s.stepNumber === step.stepNumber);
                            const isRecruiterStep = currentStep?.stepDetails?.participant && 
                              currentStep.stepDetails.participant.toLowerCase() === "recruiter";
                            const canDrag = isRecruiterStep && step.stepNumber < steps.length;

                            return (
                              <Tooltip key={candidate.applicationId}>
                                <TooltipTrigger asChild>
                                  <div
                                    draggable={canDrag || undefined}
                                    onDragStart={(e) => {
                                      if (canDrag) {
                                        e.dataTransfer.effectAllowed = "move";
                                        e.dataTransfer.setData(
                                          "candidate",
                                          JSON.stringify({
                                            candidateId: candidate.candidateId,
                                            applicationId: candidate.applicationId,
                                            fromStep: step.stepNumber,
                                            index: candidateIndex,
                                          })
                                        );
                                      }
                                    }}
                                    onDragEnd={() => {}}
                                    onClick={() => {
                                      setSelectedCandidate(candidate);
                                      setIsModalOpen(true);
                                    }}
                                    className={`group bg-background border border-border rounded-lg p-3 hover:shadow-md transition-all ${
                                      canDrag ? "cursor-grab active:cursor-grabbing" : "cursor-pointer"
                                    } hover:border-primary/50 hover:bg-muted/50 ${
                                      canDrag ? "opacity-100" : "opacity-90"
                                    }`}
                                  >
                                    <div className="flex items-start gap-2">
                                      <GripVertical className="h-4 w-4 text-muted-foreground/50 mt-0.5 group-hover:text-primary/50 flex-shrink-0" />
                                      <div className="flex-1 min-w-0">
                                        <p className="font-medium text-sm truncate">
                                          {candidate.candidateName}
                                        </p>
                                        <p className="text-xs text-muted-foreground truncate">
                                          {candidate.candidateSerial}
                                        </p>
                                        <div className="flex items-center gap-2 mt-2">
                                          <Badge
                                            variant="outline"
                                            className="text-xs h-fit"
                                          >
                                            {candidate.status}
                                          </Badge>
                                          {isLastStep && (
                                            <CheckCircle2 className="h-3 w-3 text-green-600 flex-shrink-0" />
                                          )}
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </TooltipTrigger>
                                <TooltipContent side="right">
                                  <div className="text-sm space-y-1">
                                    <p>
                                      <span className="font-semibold">Name:</span> {candidate.candidateName}
                                    </p>
                                    <p className="break-words max-w-xs">
                                      <span className="font-semibold">Email:</span> {candidate.candidateEmail}
                                    </p>
                                    <p>
                                      <span className="font-semibold">Status:</span> {candidate.status}
                                    </p>
                                    <p>
                                      <span className="font-semibold">Applied:</span>{" "}
                                      {new Date(candidate.appliedAt).toLocaleDateString()}
                                    </p>
                                    {candidate.completedAt && (
                                      <p>
                                        <span className="font-semibold">Completed:</span>{" "}
                                        {new Date(candidate.completedAt).toLocaleDateString()}
                                      </p>
                                    )}
                                  </div>
                                </TooltipContent>
                              </Tooltip>
                            );
                          })
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </TooltipProvider>
          </div>
        </CardContent>
      </Card>

      {/* Drag-Drop Promotion Confirmation Dialog */}
      <Dialog
        open={dragState !== null}
        onOpenChange={(open) => {
          if (!open) {
            setDragState(null);
            setPromoteError(null);
          }
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Promote Candidate?</DialogTitle>
            <DialogDescription>
              Are you sure you want to promote{" "}
              <span className="font-medium text-foreground">
                {candidates.find(c => c.applicationId === dragState?.applicationId)?.candidateName}
              </span>{" "}
              from <span className="font-medium">Step {dragState?.fromStep}</span> to{" "}
              <span className="font-medium">Step {dragState?.toStep}</span>?
            </DialogDescription>
          </DialogHeader>
          {promoteError && (
            <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/20">
              <div className="text-sm text-red-700 dark:text-red-300">{promoteError}</div>
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setDragState(null);
                setPromoteError(null);
              }}
              disabled={isPromoting}
            >
              Cancel
            </Button>
            <Button
              onClick={handlePromoteFromDragDrop}
              disabled={isPromoting}
            >
              {isPromoting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin mr-2" />
                  Promoting...
                </>
              ) : (
                "Confirm"
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Candidate Details Modal */}
      <CandidateDetailsModal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        candidate={selectedCandidate}
        applicationId={selectedCandidate?.applicationId}
        allSteps={steps}
        jobPostName={jobPostName}
        jobPostVersion={jobPostVersion}
        onCandidatePromoted={handleCandidatePromoted}
      />
    </div>
  );
}
