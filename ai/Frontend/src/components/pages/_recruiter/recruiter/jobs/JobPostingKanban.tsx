"use client";

import { useState, useEffect } from "react";
import {
  GripVertical,
  Plus,
  Trash2,
  Edit2,
  AlertCircle,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { useApi } from "@/hooks/useApi";
import { toast } from "sonner";
import Link from "next/link";

interface KanbanColumn {
  id: string;
  columnName: string;
  sequence: number;
  isVisible: boolean;
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
}

interface JobPostingKanbanProps {
  recruiterId: string;
  jobs: JobPost[];
  columns: KanbanColumn[];
  onJobMoved?: (jobName: string, jobVersion: number, columnId: string) => void;
  onColumnsChanged?: () => void;
}

export default function JobPostingKanban({
  recruiterId,
  jobs,
  columns,
  onJobMoved,
  onColumnsChanged,
}: JobPostingKanbanProps) {
  const api = useApi();
  const [draggedJob, setDraggedJob] = useState<JobPost | null>(null);
  const [draggedFrom, setDraggedFrom] = useState<string | null>(null);
  const [isAddColumnOpen, setIsAddColumnOpen] = useState(false);
  const [newColumnName, setNewColumnName] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [columnToDelete, setColumnToDelete] = useState<KanbanColumn | null>(null);
  const [editingColumn, setEditingColumn] = useState<KanbanColumn | null>(null);

  const getJobsByColumn = (columnId: string) => {
    return jobs.filter((job) => job.currentBoardColumnId === columnId);
  };

  const handleDragStart = (
    e: React.DragEvent,
    job: JobPost,
    columnId: string
  ) => {
    setDraggedJob(job);
    setDraggedFrom(columnId);
    e.dataTransfer.effectAllowed = "move";
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  };

  const handleDrop = async (e: React.DragEvent, toColumnId: string) => {
    e.preventDefault();
    if (!draggedJob || draggedFrom === toColumnId) {
      setDraggedJob(null);
      setDraggedFrom(null);
      return;
    }

    try {
      setIsLoading(true);
      await api.put(
        `/job/${draggedJob.name}/${draggedJob.version}/move-to-column/${toColumnId}`,
        {}
      );

      if (onJobMoved) {
        onJobMoved(draggedJob.name, draggedJob.version, toColumnId);
      }

      toast.success(`${draggedJob.jobTitle} moved successfully`);
    } catch (error) {
      toast.error("Failed to move job posting");
      console.error("Error moving job:", error);
    } finally {
      setIsLoading(false);
      setDraggedJob(null);
      setDraggedFrom(null);
    }
  };

  const handleAddColumn = async () => {
    if (!newColumnName.trim()) {
      toast.error("Column name cannot be empty");
      return;
    }

    try {
      setIsLoading(true);
      await api.post(`/KanbanBoardColumn/recruiter/${recruiterId}`, {
        recruiterId,
        columnName: newColumnName,
        sequence: columns.length + 1,
        isVisible: true,
      });

      toast.success("Column created successfully");

      setNewColumnName("");
      setIsAddColumnOpen(false);

      if (onColumnsChanged) {
        onColumnsChanged();
      }
    } catch (error) {
      toast.error("Failed to create column");
      console.error("Error creating column:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteColumn = async (column: KanbanColumn) => {
    try {
      setIsLoading(true);
      await api.delete(`/KanbanBoardColumn/${column.id}`);

      toast.success("Column deleted successfully");

      if (onColumnsChanged) {
        onColumnsChanged();
      }
    } catch (error) {
      toast.error("Failed to delete column");
      console.error("Error deleting column:", error);
    } finally {
      setIsLoading(false);
      setColumnToDelete(null);
    }
  };

  const handleUpdateColumn = async () => {
    if (!editingColumn) return;

    try {
      setIsLoading(true);
      await api.put(`/KanbanBoardColumn/${editingColumn.id}`, {
        columnName: editingColumn.columnName,
        isVisible: editingColumn.isVisible,
      });

      toast.success("Column updated successfully");

      if (onColumnsChanged) {
        onColumnsChanged();
      }

      setEditingColumn(null);
    } catch (error) {
      toast.error("Failed to update column");
      console.error("Error updating column:", error);
    } finally {
      setIsLoading(false);
    }
  };

  if (columns.length === 0) {
    return (
      <div className="space-y-4">
        <Card className="border-dashed border-2">
          <CardContent className="pt-8">
            <div className="flex flex-col items-center justify-center text-center py-12">
              <AlertCircle className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-semibold mb-2">No Kanban Columns</h3>
              <p className="text-muted-foreground mb-6 max-w-sm">
                Create your first column to organize job postings on the kanban board.
              </p>
              <Button onClick={() => setIsAddColumnOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Create First Column
              </Button>
            </div>
          </CardContent>
        </Card>

        <Dialog open={isAddColumnOpen} onOpenChange={setIsAddColumnOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create New Column</DialogTitle>
              <DialogDescription>
                Give your first kanban column a name to start organizing job postings.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4">
              <Input
                placeholder="e.g., Pipeline, Draft, Active, Closed"
                value={newColumnName}
                onChange={(e) => setNewColumnName(e.target.value)}
                disabled={isLoading}
              />
            </div>
            <DialogFooter>
              <Button
                variant="outline"
                onClick={() => setIsAddColumnOpen(false)}
                disabled={isLoading}
              >
                Cancel
              </Button>
              <Button onClick={handleAddColumn} disabled={isLoading}>
                Create Column
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Add Column Button */}
      <div className="flex justify-end">
        <Button
          variant="outline"
          onClick={() => setIsAddColumnOpen(true)}
          disabled={isLoading}
        >
          <Plus className="mr-2 h-4 w-4" />
          Add Column
        </Button>
      </div>

      {/* Kanban Board */}
      <div className="grid grid-cols-1 gap-4 auto-cols-max overflow-x-auto pb-4" 
           style={{ gridAutoFlow: "column" }}>
        {columns
          .filter((col) => col.isVisible)
          .sort((a, b) => a.sequence - b.sequence)
          .map((column) => (
            <div
              key={column.id}
              className="flex flex-col bg-muted/50 rounded-lg p-4 min-w-80 max-h-[600px] overflow-y-auto"
            >
              {/* Column Header */}
              <div className="flex items-center justify-between mb-4 pb-3 border-b">
                <div>
                  <h3 className="font-semibold text-sm">{column.columnName}</h3>
                  <p className="text-xs text-muted-foreground">
                    {getJobsByColumn(column.id).length} jobs
                  </p>
                </div>
                <div className="flex items-center gap-1">
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setEditingColumn(column)}
                    disabled={isLoading}
                  >
                    <Edit2 className="h-4 w-4" />
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setColumnToDelete(column)}
                    disabled={isLoading}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>

              {/* Drop Zone */}
              <div
                onDragOver={handleDragOver}
                onDrop={(e) => handleDrop(e, column.id)}
                className="flex-1 space-y-2 min-h-96"
              >
                {getJobsByColumn(column.id).length === 0 && (
                  <div className="text-center py-8 text-muted-foreground text-sm">
                    No jobs in this column
                  </div>
                )}

                {getJobsByColumn(column.id).map((job) => (
                  <div
                    key={`${job.name}-${job.version}`}
                    draggable
                    onDragStart={(e) => handleDragStart(e, job, column.id)}
                    className="bg-white dark:bg-slate-900 rounded-lg p-3 cursor-move hover:shadow-md transition-shadow border border-border"
                  >
                    <div className="flex items-start gap-2">
                      <GripVertical className="h-4 w-4 text-muted-foreground mt-1 flex-shrink-0" />
                      <div className="flex-1 min-w-0">
                        <Link
                          href={`/recruiter/jobs/${job.name}/${job.version}`}
                        >
                          <h4 className="font-medium text-sm hover:text-primary truncate">
                            {job.jobTitle}
                          </h4>
                        </Link>
                        <div className="flex items-center gap-2 mt-1 flex-wrap">
                          <Badge
                            variant={
                              job.status === "published"
                                ? "default"
                                : "secondary"
                            }
                            className="text-xs"
                          >
                            {job.status || "draft"}
                          </Badge>
                          {job.industry && (
                            <span className="text-xs text-muted-foreground">
                              {job.industry}
                            </span>
                          )}
                          {job.candidateCount !== undefined && (
                            <span className="text-xs text-muted-foreground">
                              {job.candidateCount} candidates
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
      </div>

      {/* Add Column Dialog */}
      <Dialog open={isAddColumnOpen} onOpenChange={setIsAddColumnOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create New Column</DialogTitle>
            <DialogDescription>
              Add a new column to your kanban board to organize job postings.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <Input
              placeholder="e.g., Pipeline, Draft, Active, Closed"
              value={newColumnName}
              onChange={(e) => setNewColumnName(e.target.value)}
              disabled={isLoading}
            />
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setIsAddColumnOpen(false)}
              disabled={isLoading}
            >
              Cancel
            </Button>
            <Button onClick={handleAddColumn} disabled={isLoading}>
              Create Column
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit Column Dialog */}
      <Dialog open={!!editingColumn} onOpenChange={() => setEditingColumn(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Column</DialogTitle>
            <DialogDescription>
              Update the column name and visibility settings.
            </DialogDescription>
          </DialogHeader>
          {editingColumn && (
            <div className="space-y-4">
              <Input
                placeholder="Column name"
                value={editingColumn.columnName}
                onChange={(e) =>
                  setEditingColumn({
                    ...editingColumn,
                    columnName: e.target.value,
                  })
                }
                disabled={isLoading}
              />
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="visible"
                  checked={editingColumn.isVisible}
                  onChange={(e) =>
                    setEditingColumn({
                      ...editingColumn,
                      isVisible: e.target.checked,
                    })
                  }
                  disabled={isLoading}
                />
                <label htmlFor="visible" className="text-sm">
                  Visible on board
                </label>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setEditingColumn(null)}
              disabled={isLoading}
            >
              Cancel
            </Button>
            <Button onClick={handleUpdateColumn} disabled={isLoading}>
              Update Column
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Column Alert Dialog */}
      <AlertDialog open={!!columnToDelete} onOpenChange={() => setColumnToDelete(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Column?</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete the "{columnToDelete?.columnName}"
              column? Jobs in this column will not be deleted.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogCancel disabled={isLoading}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={() => columnToDelete && handleDeleteColumn(columnToDelete)}
            disabled={isLoading}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            Delete
          </AlertDialogAction>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
