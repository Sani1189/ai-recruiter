"use client";

import { useState } from "react";
import { LayoutGrid, Table as TableIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import JobStepsKanban from "./JobStepsKanban";
import CandidatesTableView from "./CandidatesTableView";

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
}

interface JobStepsPipelineViewProps {
  steps: JobStep[];
  candidates: JobStepCandidate[];
  jobPostName?: string;
  jobPostVersion?: number;
  onRefreshNeeded?: () => void;
}

export default function JobStepsPipelineView({
  steps,
  candidates,
  jobPostName = "",
  jobPostVersion = 1,
  onRefreshNeeded,
}: JobStepsPipelineViewProps) {
  const [viewMode, setViewMode] = useState<"kanban" | "table">("kanban");

  return (
    <div className="space-y-4">
      {/* View Toggle Controls */}
      <Card className="border-0 bg-gradient-to-r from-background to-muted/50">
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">Pipeline Overview</CardTitle>
            
            {/* Toggle Buttons */}
            <div className="flex items-center gap-2 bg-muted p-1 rounded-lg">
              <Button
                size="sm"
                variant={viewMode === "kanban" ? "default" : "ghost"}
                onClick={() => setViewMode("kanban")}
                className="gap-1.5"
              >
                <LayoutGrid className="h-4 w-4" />
                <span className="hidden sm:inline">Kanban</span>
              </Button>
              <Button
                size="sm"
                variant={viewMode === "table" ? "default" : "ghost"}
                onClick={() => setViewMode("table")}
                className="gap-1.5"
              >
                <TableIcon className="h-4 w-4" />
                <span className="hidden sm:inline">Table</span>
              </Button>
            </div>
          </div>
        </CardHeader>
      </Card>

      {/* Content Area */}
      <div className="min-h-screen">
        {viewMode === "kanban" ? (
          <JobStepsKanban
            steps={steps}
            candidates={candidates}
            jobPostName={jobPostName}
            jobPostVersion={jobPostVersion}
            onRefreshNeeded={onRefreshNeeded}
          />
        ) : (
          <CandidatesTableView
            steps={steps}
            candidates={candidates}
            jobPostName={jobPostName}
            jobPostVersion={jobPostVersion}
            onRefreshNeeded={onRefreshNeeded}
          />
        )}
      </div>
    </div>
  );
}
