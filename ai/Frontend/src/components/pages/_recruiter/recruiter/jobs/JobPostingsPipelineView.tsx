"use client";

import { useState, useEffect } from "react";
import { LayoutGrid, Table as TableIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useApi } from "@/hooks/useApi";
import { toast } from "sonner";
import JobPostingKanban from "./JobPostingKanban";
import JobsTable from "./JobsTable/index";

interface KanbanColumn {
  id: string;
  columnName: string;
  sequence: number;
  isVisible: boolean;
  createdAt?: string;
  updatedAt?: string;
}

interface JobPost {
  id: string;
  name: string;
  version: number;
  jobTitle: string;
  status: string;
  currentBoardColumnId?: string;
  industry?: string;
  candidateCount?: number;
  jobType?: string;
  experienceLevel?: string;
}

interface JobPostingsPipelineViewProps {
  recruiterId: string;
  initialJobs?: JobPost[];
  initialColumns?: KanbanColumn[];
}

export default function JobPostingsPipelineView({
  recruiterId,
  initialJobs = [],
  initialColumns = [],
}: JobPostingsPipelineViewProps) {
  const api = useApi();
  const [viewMode, setViewMode] = useState<"kanban" | "table">("kanban");
  const [columns, setColumns] = useState<KanbanColumn[]>(initialColumns);
  const [jobs, setJobs] = useState<JobPost[]>(initialJobs);
  const [isLoading, setIsLoading] = useState(!initialColumns.length);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // Fetch columns on mount
  useEffect(() => {
    fetchColumns();
  }, [recruiterId]);

  const fetchColumns = async () => {
    try {
      setIsLoading(true);
      const response = await api.get(`/KanbanBoardColumn/recruiter/${recruiterId}`);
      const columnsData = response.data || response || [];
      setColumns(Array.isArray(columnsData) ? columnsData : []);

      // If no columns exist, create the default one
      if (!columnsData || columnsData.length === 0) {
        await createDefaultColumn();
      }
    } catch (error) {
      console.error("Error fetching columns:", error);
      setColumns([]);
    } finally {
      setIsLoading(false);
    }
  };

  const createDefaultColumn = async () => {
    try {
      const response = await api.post(`/KanbanBoardColumn/recruiter/${recruiterId}`, {
        recruiterId,
        columnName: "Pipeline",
        sequence: 1,
        isVisible: true,
      });

      const newColumn = response.data || response;
      setColumns([newColumn]);

      // Move all jobs to the first column
      if (jobs.length > 0) {
        for (const job of jobs) {
          try {
            await api.put(
              `/job/${job.name}/${job.version}/move-to-column/${newColumn.id}`,
              {}
            );
          } catch (err) {
            console.error(`Error moving job ${job.name}:`, err);
          }
        }
        setRefreshTrigger((prev) => prev + 1);
      }

      toast.success("Default column created and jobs assigned");
    } catch (error) {
      console.error("Error creating default column:", error);
      toast.error("Failed to create default column");
    }
  };

  const handleJobMoved = (jobName: string, jobVersion: number, columnId: string) => {
    setJobs((prevJobs) =>
      prevJobs.map((job) =>
        job.name === jobName && job.version === jobVersion
          ? { ...job, currentBoardColumnId: columnId }
          : job
      )
    );
  };

  const handleColumnsChanged = () => {
    fetchColumns();
    setRefreshTrigger((prev) => prev + 1);
  };

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
      <div>
        {viewMode === "kanban" ? (
          isLoading ? (
            <Card>
              <CardContent className="pt-8">
                <div className="text-center py-8 text-muted-foreground">
                  Loading kanban board...
                </div>
              </CardContent>
            </Card>
          ) : (
            <JobPostingKanban
              recruiterId={recruiterId}
              jobs={jobs}
              columns={columns}
              onJobMoved={handleJobMoved}
              onColumnsChanged={handleColumnsChanged}
            />
          )
        ) : (
          <JobsTable key={refreshTrigger} />
        )}
      </div>
    </div>
  );
}
