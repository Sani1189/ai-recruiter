"use client";

import React from "react";

import { useState } from "react";
import { ChevronDown, Mail, Calendar, Zap, Eye } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { formatDate } from "@/lib/utils";
import CandidateDetailsModal from "./CandidateDetailsModal";

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

interface CandidatesTableViewProps {
  candidates: JobStepCandidate[];
  steps: JobStep[];
  jobPostName?: string;
  jobPostVersion?: number;
  onRefreshNeeded?: () => void;
}

export default function CandidatesTableView({
  candidates,
  steps,
  jobPostName = "",
  jobPostVersion = 1,
  onRefreshNeeded,
}: CandidatesTableViewProps) {
  const [selectedCandidate, setSelectedCandidate] = useState<JobStepCandidate | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [sortBy, setSortBy] = useState<"name" | "date" | "step">("date");
  const [expandedRow, setExpandedRow] = useState<string | null>(null);

  const handleCandidatePromoted = (updatedCandidate: JobStepCandidate) => {
    // Update selected candidate with new data
    setSelectedCandidate(updatedCandidate);
    
    // Trigger refresh of the parent component to sync kanban and table
    if (onRefreshNeeded) {
      onRefreshNeeded();
    }
  };

  // Sort candidates based on selected sort option
  const sortedCandidates = [...candidates].sort((a, b) => {
    if (sortBy === "name") {
      return a.candidateName.localeCompare(b.candidateName);
    } else if (sortBy === "step") {
      const aStep = a.currentStep ?? a.completedStepsCount ?? 0;
      const bStep = b.currentStep ?? b.completedStepsCount ?? 0;
      return bStep - aStep;
    } else {
      return new Date(b.appliedAt).getTime() - new Date(a.appliedAt).getTime();
    }
  });

  const handleViewDetails = (candidate: JobStepCandidate) => {
    setSelectedCandidate(candidate);
    setIsModalOpen(true);
  };

  const getStepName = (stepNumber?: number): string => {
    if (!stepNumber) return "Not Started";
    const step = steps.find((s) => s.stepNumber === stepNumber);
    return step?.stepDetails.name || `Step ${stepNumber}`;
  };

  const getStepColor = (stepNumber?: number): string => {
    if (!stepNumber) return "bg-gray-100 text-gray-800";
    
    const completed = (stepNumber - 1);
    const total = steps.length;
    
    if (completed === total) {
      return "bg-green-100 text-green-800";
    } else if (completed > total * 0.66) {
      return "bg-blue-100 text-blue-800";
    } else if (completed > total * 0.33) {
      return "bg-amber-100 text-amber-800";
    }
    return "bg-red-100 text-red-800";
  };

  const getProgressPercentage = (stepNumber?: number): number => {
    if (!stepNumber || !steps.length) return 0;
    return ((stepNumber - 1) / steps.length) * 100;
  };

  return (
    <div className="space-y-4">
      {/* Controls */}
      <div className="flex items-center justify-between">
        <div className="text-sm text-muted-foreground">
          Total: <span className="font-semibold text-foreground">{candidates.length}</span> candidates
        </div>
        
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" size="sm" className="gap-2 bg-transparent">
              Sort by: {sortBy === "name" ? "Name" : sortBy === "step" ? "Progress" : "Applied Date"}
              <ChevronDown className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => setSortBy("date")}>
              Applied Date
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => setSortBy("name")}>
              Name (A-Z)
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => setSortBy("step")}>
              Progress
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Table */}
      <Card className="border-0">
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/50 hover:bg-muted/50">
                  <TableHead className="w-[50px]"></TableHead>
                  <TableHead className="font-semibold">Candidate</TableHead>
                  <TableHead className="font-semibold">Serial</TableHead>
                  <TableHead className="font-semibold">Email</TableHead>
                  <TableHead className="text-center font-semibold">Progress</TableHead>
                  <TableHead className="text-center font-semibold">Current Step</TableHead>
                  <TableHead className="font-semibold">Applied</TableHead>
                  <TableHead className="w-[100px] text-right font-semibold">Action</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedCandidates.length > 0 ? (
                  sortedCandidates.map((candidate) => {
                    const stepNumber = candidate.currentStep ?? (candidate.completedStepsCount ?? 0) + 1;
                    const isExpanded = expandedRow === candidate.applicationId;
                    
                    return (
                      <React.Fragment key={candidate.applicationId}>
                        <TableRow
                          className="hover:bg-muted/50 cursor-pointer transition-colors"
                          onClick={() => setExpandedRow(isExpanded ? null : candidate.applicationId)}
                        >
                          <TableCell>
                            <ChevronDown
                              className={`h-4 w-4 text-muted-foreground transition-transform ${
                                isExpanded ? "rotate-180" : ""
                              }`}
                            />
                          </TableCell>
                          <TableCell>
                            <div className="font-medium">{candidate.candidateName}</div>
                          </TableCell>
                          <TableCell>
                            <Badge variant="secondary" className="font-mono text-xs">
                              {candidate.candidateSerial}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm text-muted-foreground flex items-center gap-1">
                              <Mail className="h-3.5 w-3.5" />
                              {candidate.candidateEmail}
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <div className="flex-1">
                                <div className="h-2 bg-muted rounded-full overflow-hidden">
                                  <div
                                    className="h-full bg-gradient-to-r from-blue-500 to-blue-600 transition-all duration-300"
                                    style={{ width: `${getProgressPercentage(stepNumber)}%` }}
                                  />
                                </div>
                              </div>
                              <span className="text-xs font-medium text-muted-foreground whitespace-nowrap">
                                {Math.round(getProgressPercentage(stepNumber))}%
                              </span>
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="text-center">
                              <Badge className={getStepColor(stepNumber)}>
                                {getStepName(stepNumber)}
                              </Badge>
                            </div>
                          </TableCell>
                          <TableCell>
                            <div className="text-sm text-muted-foreground flex items-center gap-1">
                              <Calendar className="h-3.5 w-3.5" />
                              {formatDate(candidate.appliedAt).split(",")[0]}
                            </div>
                          </TableCell>
                          <TableCell className="text-right">
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={(e) => {
                                e.stopPropagation();
                                handleViewDetails(candidate);
                              }}
                              className="gap-1"
                            >
                              <Eye className="h-4 w-4" />
                              View
                            </Button>
                          </TableCell>
                        </TableRow>

                        {/* Expanded Details Row */}
                        {isExpanded && (
                          <TableRow className="bg-muted/30 hover:bg-muted/30">
                            <TableCell colSpan={8}>
                              <div className="p-4 space-y-3">
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                  <div>
                                    <p className="text-xs font-semibold text-muted-foreground mb-1">
                                      Status
                                    </p>
                                    <Badge variant="secondary">{candidate.status}</Badge>
                                  </div>
                                  <div>
                                    <p className="text-xs font-semibold text-muted-foreground mb-1">
                                      Steps Completed
                                    </p>
                                    <p className="text-sm font-medium">
                                      {candidate.completedStepsCount ?? 0} / {steps.length}
                                    </p>
                                  </div>
                                  <div>
                                    <p className="text-xs font-semibold text-muted-foreground mb-1">
                                      Application ID
                                    </p>
                                    <p className="text-xs font-mono text-muted-foreground">
                                      {candidate.applicationId.substring(0, 8)}...
                                    </p>
                                  </div>
                                  <div>
                                    <p className="text-xs font-semibold text-muted-foreground mb-1">
                                      Applied Date
                                    </p>
                                    <p className="text-sm">
                                      {formatDate(candidate.appliedAt)}
                                    </p>
                                  </div>
                                </div>

                                {candidate.completedAt && (
                                  <div className="pt-2 border-t border-border">
                                    <p className="text-xs font-semibold text-muted-foreground mb-1">
                                      Completed At
                                    </p>
                                    <p className="text-sm">
                                      {formatDate(candidate.completedAt)}
                                    </p>
                                  </div>
                                )}
                              </div>
                            </TableCell>
                          </TableRow>
                        )}
                      </React.Fragment>
                    );
                  })
                ) : (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-8">
                      <div className="text-muted-foreground">
                        <Zap className="h-8 w-8 mx-auto mb-2 opacity-50" />
                        <p>No candidates yet</p>
                      </div>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

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
