"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Download, Headphones } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

import InterviewAudioPlayer from "@/components/InterviewAudioPlayer";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
  TooltipProvider,
} from "@/components/ui/tooltip";

import { useApi } from "@/hooks/useApi";
import { interviewAudioService } from "@/lib/services/interviewAudioService";
import { downloadBlob, downloadFromSasUrl } from "@/lib/fileUtils";

export interface JobPostCandidate {
  applicationId: string;
  candidateId: string;
  candidateSerial: string;
  candidateName: string;
  candidateEmail: string;
  appliedAt: string;
  completedAt?: string;
  status: string;
  jobPostName?: string;
  jobPostVersion?: number;
  candidateCvFilePath?: string;
  currentStep?: number;
  completedStepsCount?: number;
}

function CandidateRowActions({ 
  candidateId, 
  candidateName, 
  jobPostName, 
  jobPostVersion,
  candidateCvFilePath
}: {
  candidateId: string;
  candidateName: string;
  jobPostName: string;
  jobPostVersion: number;
  candidateCvFilePath?: string;
}) {
  const [audioData, setAudioData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [downloadingResume, setDownloadingResume] = useState(false);
  const api = useApi();

  const loadAudio = async () => {
    if (audioData) return;
    
    setLoading(true);
    try {
      const data = await interviewAudioService.getJobApplicationWithInterviews(
        api,
        jobPostName,
        jobPostVersion,
        candidateId
      );
      setAudioData(data);
    } catch (error) {
      console.error('Failed to load audio:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadCv = async () => {
    if (!candidateCvFilePath || downloadingResume) return;

    setDownloadingResume(true);
    try {
      // Get SAS URL from backend
      const response = await api.get(`/userprofile/candidate/${candidateId}/resume`);
      
      // Response format: { downloadUrl: string, expiresInMinutes: number }
      // apiClient returns data directly, not wrapped in .data
      const downloadUrl = (response as any).downloadUrl || (response as any).data?.downloadUrl;
      
      if (!downloadUrl) {
        console.error('Download response:', response);
        throw new Error('Download URL not received from server');
      }
      
      // Download directly from Azure using SAS URL
      await downloadFromSasUrl(downloadUrl, `${candidateName}_Resume.pdf`);
    } catch (error) {
      console.error('Failed to download resume:', error);
    } finally {
      setDownloadingResume(false);
    }
  };

  const playlist = audioData 
    ? interviewAudioService.createPlaylist(audioData, candidateName)
    : [];

  return (
    <div className="flex gap-2">
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <Button 
              variant="ghost" 
              size="sm"
              onClick={handleDownloadCv}
              disabled={!candidateCvFilePath || downloadingResume}
              isLoading={downloadingResume}
            >
              <Download className="h-4 w-4" />
            </Button>
          </TooltipTrigger>
          <TooltipContent>
            {candidateCvFilePath ? 'Download resume' : 'Resume not available'}
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>

      <Dialog>
        <DialogTrigger asChild>
          <Button 
            variant="ghost" 
            size="sm"
            onClick={loadAudio}
            disabled={loading}
          >
            <Headphones className="h-4 w-4" />
          </Button>
        </DialogTrigger>
        
        <DialogContent className="sm:max-w-4xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Interview Audio</DialogTitle>
          </DialogHeader>

          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
              <p className="mt-2 text-muted-foreground">Loading...</p>
            </div>
          ) : (
            <InterviewAudioPlayer 
              playlist={playlist}
              candidateName={candidateName}
            />
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}

export const columns: ColumnDef<JobPostCandidate>[] = [
  {
    id: "select",
    header: ({ table }) => (
      <Checkbox
        checked={
          table.getIsAllPageRowsSelected() ||
          (table.getIsSomePageRowsSelected() && "indeterminate")
        }
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
        aria-label="Select all"
        className="translate-y-[2px]"
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(value) => row.toggleSelected(!!value)}
        aria-label="Select row"
        className="translate-y-[2px]"
      />
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: "candidateSerial",
    id: "candidateNo",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Candidate No" />
    ),
    cell: ({ row }) => {
      return (
        <Link
          href={`/recruiter/candidates/${row.original.candidateId}`}
          className="font-mono hover:underline text-sm text-muted-foreground"
        >
          {row.getValue("candidateNo")}
        </Link>
      );
    },
  },
  {
    accessorKey: "candidateName",
    id: "candidateName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Name" />
    ),
    cell: ({ row }) => {
      return (
        <Link
          href={`/recruiter/candidates/${row.original.candidateId}`}
          className="hover:underline"
        >
          {row.getValue("candidateName")}
        </Link>
      );
    },
  },
  {
    accessorKey: "candidateEmail",
    id: "candidateEmail",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Email" />
    ),
    cell: ({ row }) => {
      return (
        <Link
          href={`/recruiter/candidates/${row.original.candidateId}`}
          className="hover:underline"
        >
          {row.getValue("candidateEmail")}
        </Link>
      );
    },
  },
  {
    accessorKey: "status",
    id: "status",
    header: "Status",
    cell: ({ row }) => {
      return <span className="capitalize">{row.getValue("status")}</span>;
    },
  },
  {
    accessorKey: "appliedAt",
    id: "appliedAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Applied At" />
    ),
    cell: ({ row }) => {
      const date = new Date(row.getValue("appliedAt"));
      return <span>{date.toLocaleDateString()}</span>;
    },
  },
  {
    accessorKey: "actions",
    header: "Actions",
    cell: ({ row }) => {
      const candidate = row.original;
      
      return (
        <CandidateRowActions 
          candidateId={candidate.candidateId}
          candidateName={candidate.candidateName}
          jobPostName={candidate.jobPostName || ""}
          jobPostVersion={candidate.jobPostVersion || 1}
          candidateCvFilePath={candidate.candidateCvFilePath}
        />
      );
    },
  },
];
