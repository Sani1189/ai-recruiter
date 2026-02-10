"use client";

import { Link } from "lucide-react";
import { TooltipProvider } from "@/components/ui/tooltip";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CopyButton } from "@/components/ui/copy-button";
import { DataTable } from "@/components/ui/data-table-v2/data-table";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

import { mockInterviews } from "@/dummy";
import { getInterviewLink } from "@/lib/job";
import { JobPost } from "@/types/v2/type.view";
import { columns, type JobPostCandidate } from "./columns";

export default function CandidatesTable({
  candidates,
  jobPost,
}: {
  candidates: JobPostCandidate[];
  jobPost: JobPost;
}) {
  const _candidates = candidates.map((candidate) => ({
    ...candidate,
    jobPostId: jobPost.id,
    jobPostName: jobPost.name,
    jobPostVersion: jobPost.version,
  }));

  const interviewSteps = (mockInterviews.find(
    (interview) => interview.jobPost.id === jobPost.id,
  )?.jobPost as any)?.interviewSteps;

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="flex items-center gap-x-3">
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <CopyButton
                  text={getInterviewLink(jobPost.name || jobPost.id, jobPost.version || 1)}
                  successMessage="Interview link copied to clipboard"
                >
                  <Link className="text-primary h-5 w-5" />
                </CopyButton>
              </TooltipTrigger>

              <TooltipContent>
                <span>Copy Interview Link</span>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>

          <span>{jobPost.jobTitle} Position - Candidates</span>
        </CardTitle>
      </CardHeader>

      <CardContent>
        <DataTable
          columns={columns}
          data={_candidates}
          metadata={{
            searchField: "candidateName",
            placeholder: "Search Candidate Name...",
            filters: interviewSteps
              ? [
                  {
                    columnId: "stage",
                    title: "Stages",
                    options: interviewSteps.map((step: string) => ({
                      value: step,
                      label: step,
                    })),
                  },
                ]
              : undefined,
          }}
        />
      </CardContent>
    </Card>
  );
}
