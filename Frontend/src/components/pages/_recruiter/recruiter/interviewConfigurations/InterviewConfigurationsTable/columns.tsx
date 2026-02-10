"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Pen, Trash2, Eye, CopyPlus } from "lucide-react";
import Link from "next/link";
import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";

import { formatDate } from "@/lib/utils";
import { InterviewConfiguration } from "@/types/interviewConfiguration";
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

interface ColumnsProps {
  onDelete?: (configuration: InterviewConfiguration) => void;
  onDuplicate?: (configuration: InterviewConfiguration, newName: string) => Promise<InterviewConfiguration> | InterviewConfiguration;
}

function DuplicateInterviewConfigurationDialog({
  configuration,
  onDuplicate,
}: {
  configuration: InterviewConfiguration;
  onDuplicate: (configuration: InterviewConfiguration, newName: string) => Promise<InterviewConfiguration> | InterviewConfiguration;
}) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const defaults = useMemo(() => {
    const suggestedName = `${configuration.name}-copy`;
    return { suggestedName };
  }, [configuration.name]);

  const [newName, setNewName] = useState("");

  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    if (next) {
      setNewName(defaults.suggestedName);
    }
  };

  const canSubmit = newName.trim().length > 0;

  const handleDuplicate = async () => {
    if (!canSubmit || submitting) return;

    setSubmitting(true);
    try {
      const duplicated = await onDuplicate(configuration, newName.trim());
      toast.success(`Duplicated "${configuration.name}" → "${(duplicated as any)?.name ?? newName.trim()}"`);
      setOpen(false);

      const targetName = (duplicated as any)?.name ?? newName.trim();
      const targetVersion = (duplicated as any)?.version ?? 1;
      router.push(`/recruiter/interviewConfigurations/${encodeURIComponent(targetName)}/${targetVersion}`);
    } catch (err: any) {
      const message =
        err?.message ||
        err?.errors?.[0] ||
        err?.data?.error ||
        "Failed to duplicate interview configuration";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="sm" title="Duplicate (new v1)">
          <CopyPlus className="h-4 w-4" />
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Duplicate Interview Configuration</DialogTitle>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="newName">New Configuration Name</Label>
          <Input
            id="newName"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="e.g. voice-default-v1-copy"
            autoComplete="off"
          />
          <p className="text-sm text-muted-foreground">
            This creates a <span className="font-medium">fresh new Configuration v1</span> and copies the same prompt references.
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

export const createColumns = ({ onDelete, onDuplicate }: ColumnsProps = {}): ColumnDef<InterviewConfiguration>[] => [
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
          <Link href={`/recruiter/interviewConfigurations/${name}/${row.getValue("version") as number}`}>{name}</Link>
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
    accessorKey: "modality",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Modality" />
    ),
    cell: ({ row }) => {
      const modality = row.getValue("modality") as string;
      return (
        <Badge variant="outline" className="capitalize">
          {modality}
        </Badge>
      );
    },
    filterFn: (row, id, value) => {
      return value.includes(row.getValue(id));
    },
  },
  {
    accessorKey: "active",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" />
    ),
    cell: ({ row }) => {
      const active = row.getValue("active") as boolean;
      return (
        <Badge variant={active ? "default" : "secondary"}>
          {active ? "Active" : "Inactive"}
        </Badge>
      );
    },
    filterFn: (row, id, value) => {
      return value.includes(row.getValue(id));
    },
  },
  {
    accessorKey: "instructionPromptName",
    header: "Instruction Prompt",
    cell: ({ row }) => {
      const instructionPromptName = row.getValue("instructionPromptName") as string;
      const instructionPromptVersion = row.getValue("instructionPromptVersion") as number | undefined;
      return (
        <div className="max-w-[150px]">
          <div className="font-medium text-sm truncate" title={instructionPromptName}>
            {instructionPromptName}
          </div>
          {instructionPromptVersion && (
            <div className="text-xs text-muted-foreground">
              v{instructionPromptVersion}
            </div>
          )}
        </div>
      );
    },
  },
  {
    accessorKey: "personalityPromptName",
    header: "Personality Prompt",
    cell: ({ row }) => {
      const personalityPromptName = row.getValue("personalityPromptName") as string;
      const personalityPromptVersion = row.getValue("personalityPromptVersion") as number | undefined;
      return (
        <div className="max-w-[150px]">
          <div className="font-medium text-sm truncate" title={personalityPromptName}>
            {personalityPromptName}
          </div>
          {personalityPromptVersion && (
            <div className="text-xs text-muted-foreground">
              v{personalityPromptVersion}
            </div>
          )}
        </div>
      );
    },
  },
  {
    accessorKey: "questionsPromptName",
    header: "Questions Prompt",
    cell: ({ row }) => {
      const questionsPromptName = row.getValue("questionsPromptName") as string;
      const questionsPromptVersion = row.getValue("questionsPromptVersion") as number | undefined;
      return (
        <div className="max-w-[150px]">
          <div className="font-medium text-sm truncate" title={questionsPromptName}>
            {questionsPromptName}
          </div>
          {questionsPromptVersion && (
            <div className="text-xs text-muted-foreground">
              v{questionsPromptVersion}
            </div>
          )}
        </div>
      );
    },
  },
  {
    accessorKey: "duration",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Duration" />
    ),
    cell: ({ row }) => {
      const duration = row.getValue("duration") as number | undefined;
      return (
        <div className="text-sm text-muted-foreground">
          {duration ? `${duration} min` : "-"}
        </div>
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
          {formatDate(date)}
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
          {createdBy}
        </div>
      );
    },
  },
  {
    accessorKey: "updatedAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Updated" />
    ),
    cell: ({ row }) => {
      const date = row.getValue("updatedAt") as string;
      return (
        <div className="text-sm text-muted-foreground">
          {formatDate(date)}
        </div>
      );
    },
  },
  {
    accessorKey: "updatedBy",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Updated By" />
    ),
    cell: ({ row }) => {
      const updatedBy = row.getValue("updatedBy") as string;
      return (
        <div className="text-sm text-muted-foreground">
          {updatedBy}
        </div>
      );
    },
  },
  {
    id: "actions",
    header: "Actions",
    cell: ({ row }) => {
      const configuration = row.original;
      
      return (
        <div className="flex items-center gap-2">
          <Link
            href={`/recruiter/interviewConfigurations/${configuration.name}/${configuration.version}`}
            className={buttonVariants({ variant: "ghost", size: "sm" })}
          >
            <Eye className="h-4 w-4" />
          </Link>
          
          <Link
            href={`/recruiter/interviewConfigurations/${configuration.name}/${configuration.version}/edit`}
            className={buttonVariants({ variant: "ghost", size: "sm" })}
          >
            <Pen className="h-4 w-4" />
          </Link>
          
           <ConfirmDialog
            trigger={
              <Button
                variant="ghost"
                size="sm"
                className="text-destructive hover:text-destructive"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            }
            title="Delete Interview Configuration?"
            description={`Are you sure you want to delete "${configuration.name}" version ${configuration.version}? This action cannot be undone.`}
            confirmText="Delete"
            successMessage={`Interview Configuration "${configuration.name}" v${configuration.version} deleted successfully`}
            errorMessage="Failed to delete Interview Configuration"
            payload={configuration}
            onConfirm={async (data) => {
              if (data) {
                onDelete?.(data);
              }
            }}
          />

          {onDuplicate && (
            <DuplicateInterviewConfigurationDialog configuration={configuration} onDuplicate={onDuplicate} />
          )}
        </div>
      );
    },
  },
];

// Default export for backward compatibility
export const columns = createColumns();
