"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Pen, Trash2, Eye, CopyPlus, MoreVertical } from "lucide-react";
import Link from "next/link";
import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { formatDate } from "@/lib/utils";
import { JobPostStep } from "@/types/jobPostStep";

interface ColumnsProps {
  onDelete?: (step: JobPostStep) => void;
  onDuplicate?: (step: JobPostStep, newName: string, newDisplayTitle?: string) => Promise<JobPostStep> | JobPostStep;
}

function DuplicateJobPostStepDialog({
  step,
  onDuplicate,
  open: controlledOpen,
  onOpenChange: controlledOnOpenChange,
  hideTrigger = false,
}: {
  step: JobPostStep;
  onDuplicate: (step: JobPostStep, newName: string, newDisplayTitle?: string) => Promise<JobPostStep> | JobPostStep;
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
  hideTrigger?: boolean;
}) {
  const router = useRouter();
  const [uncontrolledOpen, setUncontrolledOpen] = useState(false);
  const open = controlledOpen ?? uncontrolledOpen;
  const setOpen = controlledOnOpenChange ?? setUncontrolledOpen;
  const [submitting, setSubmitting] = useState(false);

  const defaults = useMemo(() => {
    const suggestedName = `${step.name}-copy`;
    const suggestedDisplayTitle = step.displayTitle ? `${step.displayTitle} (Copy)` : "";
    return { suggestedName, suggestedDisplayTitle };
  }, [step.name, step.displayTitle]);

  const [newName, setNewName] = useState("");
  const [newDisplayTitle, setNewDisplayTitle] = useState("");

  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    if (next) {
      setNewName(defaults.suggestedName);
      setNewDisplayTitle(defaults.suggestedDisplayTitle);
    }
  };

  const canSubmit = newName.trim().length > 0;

  const handleDuplicate = async () => {
    if (!canSubmit || submitting) return;

    setSubmitting(true);
    try {
      const duplicated = await onDuplicate(step, newName.trim(), newDisplayTitle);
      toast.success(`Duplicated "${step.name}" → "${(duplicated as any)?.name ?? newName.trim()}"`);
      setOpen(false);

      const targetName = (duplicated as any)?.name ?? newName.trim();
      const targetVersion = (duplicated as any)?.version ?? 1;
      router.push(`/recruiter/jobSteps/${encodeURIComponent(targetName)}/${targetVersion}`);
    } catch (err: any) {
      const message =
        err?.message ||
        err?.errors?.[0] ||
        err?.data?.error ||
        "Failed to duplicate job step";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      {!hideTrigger && (
        <DialogTrigger asChild>
          <Button variant="ghost" size="sm" title="Duplicate Step (new v1)">
            <CopyPlus className="h-4 w-4" />
          </Button>
        </DialogTrigger>
      )}

      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Duplicate Job Step</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="newName">New Step Name</Label>
            <Input
              id="newName"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="e.g. resume-upload-v1-copy"
              autoComplete="off"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="newDisplayTitle">New Display Title (optional)</Label>
            <Input
              id="newDisplayTitle"
              value={newDisplayTitle}
              onChange={(e) => setNewDisplayTitle(e.target.value)}
              placeholder="Shown to candidates (if applicable)"
              autoComplete="off"
            />
          </div>

          <p className="text-sm text-muted-foreground">
            This creates a <span className="font-medium">fresh new Step v1</span> and copies the same prompt/interview config references.
          </p>
        </div>

        <DialogFooter className="gap-2">
          <Button variant="outline" onClick={() => setOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button onClick={handleDuplicate} disabled={!canSubmit} isLoading={submitting}>
            Duplicate
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function JobStepActionsMenu({
  step,
  onDelete,
  onDuplicate,
}: {
  step: JobPostStep;
  onDelete?: (step: JobPostStep) => void;
  onDuplicate?: (step: JobPostStep, newName: string, newDisplayTitle?: string) => Promise<JobPostStep> | JobPostStep;
}) {
  const [duplicateOpen, setDuplicateOpen] = useState(false);
  const stepPath = `/recruiter/jobSteps/${encodeURIComponent(step.name)}/${step.version}`;

  return (
    <>
      {onDuplicate && (
        <DuplicateJobPostStepDialog
          step={step}
          onDuplicate={onDuplicate}
          open={duplicateOpen}
          onOpenChange={setDuplicateOpen}
          hideTrigger
        />
      )}

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="icon" aria-label="Open actions">
            <MoreVertical className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>

        <DropdownMenuContent align="end">
          <DropdownMenuItem asChild>
            <Link href={stepPath} className="flex items-center">
              <Eye className="mr-2 h-4 w-4" />
              View
            </Link>
          </DropdownMenuItem>

          <DropdownMenuItem asChild>
            <Link href={`${stepPath}/edit`} className="flex items-center">
              <Pen className="mr-2 h-4 w-4" />
              Edit
            </Link>
          </DropdownMenuItem>

          {onDuplicate && (
            <DropdownMenuItem onSelect={() => setDuplicateOpen(true)}>
              <CopyPlus className="mr-2 h-4 w-4" />
              Duplicate
            </DropdownMenuItem>
          )}

          {onDelete && (
            <DropdownMenuItem
              className="text-destructive focus:text-destructive"
              onSelect={(e) => e.preventDefault()}
            >
              <ConfirmDialog
                trigger={
                  <div className="flex w-full items-center">
                    <Trash2 className="mr-2 h-4 w-4" />
                    Delete
                  </div>
                }
                title="Delete Job Step?"
                description={`Are you sure you want to delete "${step.name}" version ${step.version}? This action cannot be undone.`}
                confirmText="Delete"
                successMessage={`Job step "${step.name}" v${step.version} deleted successfully`}
                errorMessage="Failed to delete job step"
                payload={step}
                onConfirm={async (data) => {
                  if (data) onDelete(data);
                }}
              />
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  );
}

export const createColumns = ({ onDelete, onDuplicate }: ColumnsProps = {}): ColumnDef<JobPostStep>[] => [
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
    accessorKey: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Name" />
    ),
    cell: ({ row }) => {
      const name = row.getValue("name") as string;
      return (
        <div className="font-medium hover:underline">
          <Link href={`/recruiter/jobSteps/${name}/${row.getValue("version") as number}`}>
            {name}
          </Link>
        </div>
      );
    },
  },
  {
    accessorKey: "version",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Version" />
    ),
    cell: ({ row }) => {
      const version = row.getValue("version") as number;
      return (
        <Badge variant="secondary" className="font-mono">
          v{version}
        </Badge>
      );
    },
  },
  {
    accessorKey: "stepType",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Step Type" />
    ),
    cell: ({ row }) => {
      const stepType = row.getValue("stepType") as string;
      return (
        <Badge variant="outline" className="capitalize">
          {stepType}
        </Badge>
      );
    },
    filterFn: (row, id, value) => {
      return value.includes(row.getValue(id));
    },
  },
  {
    accessorKey: "interviewConfigurationName",
    header: "Interview Config",
    cell: ({ row }) => {
      const configName = row.getValue("interviewConfigurationName") as string | null;
      const configVersion = row.original.interviewConfigurationVersion;
      
      if (!configName) {
        return <span className="text-muted-foreground text-sm">-</span>;
      }
      
      return (
        <div className="max-w-[150px]">
          <div className="font-medium text-sm truncate" title={configName}>
            {configName}
          </div>
          {configVersion && (
            <div className="text-xs text-muted-foreground">
              v{configVersion}
            </div>
          )}
        </div>
      );
    },
  },
  {
    id: "prompt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Prompt" />
    ),
    accessorFn: (row) => row.promptName ?? "",
    cell: ({ row }) => {
      const promptName = row.original.promptName;
      const promptVersion = row.original.promptVersion;

      if (!promptName) {
        return <span className="text-muted-foreground text-sm">-</span>;
      }

      return (
        <div className="max-w-[180px]">
          <Link
            href={
              promptVersion
                ? `/recruiter/prompts/${promptName}/${promptVersion}`
                : `/recruiter/prompts/${promptName}/latest`
            }
            target="_blank"
            rel="noopener noreferrer"
            className="font-medium text-sm truncate hover:underline block"
            title={promptName}
          >
            {promptName}
          </Link>
          <div className="text-xs text-muted-foreground">
            {promptVersion ? `v${promptVersion}` : "Latest"}
          </div>
        </div>
      );
    },
  },
  {
    accessorKey: "participant",
    header: "Participant",
    cell: ({ row }) => {
      const participant = row.getValue("participant") as string;
      return (
        <Badge variant="secondary" className="text-xs">
          {participant || "-"}
        </Badge>
      );
    },
  },
  {
    accessorKey: "createdAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Created" />
    ),
    cell: ({ row }) => {
      const date = row.getValue("createdAt") as string;
      return (
        <div className="text-sm text-muted-foreground">
          {date ? formatDate(date) : '-'}
        </div>
      );
    },
  },
  {
    accessorKey: "createdBy",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Created By" />
    ),
    cell: ({ row }) => {
      const createdBy = row.getValue("createdBy") as string;
      return (
        <div className="text-sm text-muted-foreground">
          {createdBy || "-"}
        </div>
      );
    },
  },
  {
    id: "actions",
    header: "Actions",
    cell: ({ row }) => {
      const step = row.original;

      return <JobStepActionsMenu step={step} onDelete={onDelete} onDuplicate={onDuplicate} />;
    },
  },
];

export const columns = createColumns();

