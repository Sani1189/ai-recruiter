"use client";

import { ColumnDef } from "@tanstack/react-table";
import {
  Copy,
  CopyPlus,
  Pen,
  Trash2,
  Users,
  MoreVertical,
  Eye,
  Send,
} from "lucide-react";
import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { CopyButton } from "@/components/ui/copy-button";
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
import { MultiSelect } from "@/components/ui/multi-select";

import { useCountries } from "@/hooks/useCountries";
import { getInterviewLink } from "@/lib/job";
import { formatDate } from "@/lib/utils";

// Type definition for Job from API (will align with JobPostDto)
export interface Job {
  name: string;
  version: number;
  jobTitle: string;
  jobType: string;
  experienceLevel: string;
  jobDescription: string;
  policeReportRequired?: boolean;
  maxAmountOfCandidatesRestriction?: number;
  minimumRequirements?: string[];
  candidateCount?: number;
  status?: string;
  createdAt: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

interface ColumnsProps {
  onDelete?: (job: Job) => void;
  onDuplicate?: (
    job: Job,
    newName: string,
    newJobTitle: string,
  ) => Promise<Job> | Job;
  /** Opens the publish dialog for the given job. Dialog is rendered once at table level. */
  onOpenPublish?: (job: Job) => void;
}

function DuplicateJobPostDialog({
  job,
  onDuplicate,
  open: controlledOpen,
  onOpenChange: controlledOnOpenChange,
  hideTrigger = false,
}: {
  job: Job;
  onDuplicate: (
    job: Job,
    newName: string,
    newJobTitle: string,
  ) => Promise<Job> | Job;
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
    const suggestedName = `${job.name}-copy`;
    const suggestedTitle = `${job.jobTitle} (Copy)`;
    return { suggestedName, suggestedTitle };
  }, [job.name, job.jobTitle]);

  const [newName, setNewName] = useState("");
  const [newJobTitle, setNewJobTitle] = useState("");

  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    if (next) {
      setNewName(defaults.suggestedName);
      setNewJobTitle(defaults.suggestedTitle);
    }
  };

  const canSubmit = newName.trim().length > 0 && newJobTitle.trim().length > 0;

  const handleDuplicate = async () => {
    if (!canSubmit || submitting) return;

    setSubmitting(true);
    try {
      const duplicated = await onDuplicate(
        job,
        newName.trim(),
        newJobTitle.trim(),
      );
      toast.success(
        `Duplicated "${job.jobTitle}" → "${(duplicated as any)?.jobTitle ?? newJobTitle}"`,
      );
      setOpen(false);

      const targetName = (duplicated as any)?.name ?? newName.trim();
      const targetVersion = (duplicated as any)?.version ?? 1;
      router.push(
        `/recruiter/jobs/${encodeURIComponent(targetName)}/${targetVersion}`,
      );
    } catch (err: any) {
      const message =
        err?.message ||
        err?.errors?.[0] ||
        err?.data?.error ||
        "Failed to duplicate job post";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      {!hideTrigger && (
        <DialogTrigger asChild>
          <Button variant="ghost" size="icon" title="Duplicate Job (new v1)">
            <CopyPlus className="h-4 w-4" />
          </Button>
        </DialogTrigger>
      )}

      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Duplicate Job Post</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="newName">New Job Post Name</Label>
            <Input
              id="newName"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="e.g. software-engineer-nyc-v1"
              autoComplete="off"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="newJobTitle">New Job Title</Label>
            <Input
              id="newJobTitle"
              value={newJobTitle}
              onChange={(e) => setNewJobTitle(e.target.value)}
              placeholder="e.g. Software Engineer (NYC)"
              autoComplete="off"
            />
          </div>

          <p className="text-muted-foreground text-sm">
            This creates a{" "}
            <span className="font-medium">fresh new JobPost v1</span> and
            reassigns the same step template references.
          </p>
        </div>

        <DialogFooter className="gap-2">
          <Button
            variant="outline"
            onClick={() => setOpen(false)}
            disabled={submitting}
          >
            Cancel
          </Button>
          <Button
            onClick={handleDuplicate}
            disabled={!canSubmit}
            isLoading={submitting}
          >
            Duplicate
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function JobActionsMenu({
  job,
  onDelete,
  onDuplicate,
  onOpenPublish,
}: {
  job: Job;
  onDelete?: (job: Job) => void;
  onDuplicate?: (
    job: Job,
    newName: string,
    newJobTitle: string,
  ) => Promise<Job> | Job;
  onOpenPublish?: (job: Job) => void;
}) {
  const [duplicateOpen, setDuplicateOpen] = useState(false);

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            title="Actions"
            className="h-8 w-8"
          >
            <MoreVertical className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem asChild>
            <Link href={`/recruiter/jobs/${job.name}/${job.version}`}>
              <Eye className="mr-2 h-4 w-4" />
              View
            </Link>
          </DropdownMenuItem>
          <DropdownMenuItem asChild>
            <Link href={`/recruiter/jobs/${job.name}/${job.version}/edit`}>
              <Pen className="mr-2 h-4 w-4" />
              Edit
            </Link>
          </DropdownMenuItem>
          {job.status === "Draft" && onOpenPublish && (
            <DropdownMenuItem onSelect={() => onOpenPublish(job)}>
              <Send className="mr-2 h-4 w-4" />
              Publish
            </DropdownMenuItem>
          )}
          {onDuplicate && (
            <>
              <DropdownMenuItem onSelect={() => setDuplicateOpen(true)}>
                <CopyPlus className="mr-2 h-4 w-4" />
                Duplicate
              </DropdownMenuItem>
              <DuplicateJobPostDialog
                job={job}
                onDuplicate={onDuplicate}
                open={duplicateOpen}
                onOpenChange={setDuplicateOpen}
                hideTrigger
              />
            </>
          )}
          <DropdownMenuItem asChild>
            <CopyButton
              text={getInterviewLink(job.name, job.version)}
              variant="ghost"
              size="sm"
              successMessage="Job link copied to clipboard"
              title="Copy Job Link"
              className="w-full justify-start"
            >
              <Copy className="mr-2 h-4 w-4" />
              Copy link
            </CopyButton>
          </DropdownMenuItem>
          {onDelete && (
            <ConfirmDialog
              trigger={
                <DropdownMenuItem
                  onSelect={(e) => e.preventDefault()}
                  className="text-destructive focus:text-destructive"
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete
                </DropdownMenuItem>
              }
              title="Delete Job?"
              description={`Are you sure you want to delete "${job.jobTitle}" (${job.name} v${job.version})? This action cannot be undone.`}
              confirmText="Delete"
              successMessage={`Job "${job.jobTitle}" deleted successfully`}
              errorMessage="Failed to delete Job"
              payload={job}
              onConfirm={async (data) => data && onDelete(data)}
            />
          )}
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  );
}

/** Renders a single publish dialog; mount once at table level to avoid N useCountries() calls. */
export function PublishJobDialog({
  job,
  onPublish,
  open,
  onOpenChange,
}: {
  job: Job;
  onPublish: (job: Job, countryExposureCountryCodes: string[]) => Promise<void>;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const { countries, loading: countriesLoading } = useCountries();
  const [selectedCodes, setSelectedCodes] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open) setSelectedCodes([]);
  }, [open]);

  const options = useMemo(
    () => countries.map((c) => ({ label: c.name, value: c.countryCode })),
    [countries],
  );

  const handlePublish = async () => {
    setSubmitting(true);
    try {
      await onPublish(job, selectedCodes);
      toast.success(`Job "${job.jobTitle}" published`);
      onOpenChange(false);
    } catch (err: any) {
      const msg =
        err?.message ||
        err?.errors?.[0] ||
        err?.data?.error ||
        "Failed to publish job";
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Publish job</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <p className="text-muted-foreground text-sm">
            Select countries for job exposure. The job will be published and
            visible in these countries.
          </p>
          <div className="space-y-2">
            <Label>Country exposure</Label>
            <MultiSelect
              key={`${job.name}-${job.version}-${open}`}
              options={options}
              defaultValue={selectedCodes}
              onValueChange={setSelectedCodes}
              placeholder="Select countries..."
              disabled={countriesLoading}
              modalPopover={true}
            />
          </div>
        </div>
        <DialogFooter className="gap-2">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={submitting}
          >
            Cancel
          </Button>
          <Button
            onClick={handlePublish}
            disabled={submitting || selectedCodes.length === 0}
            isLoading={submitting}
          >
            Publish
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export const createColumns = ({
  onDelete,
  onDuplicate,
  onOpenPublish,
}: ColumnsProps = {}): ColumnDef<Job>[] => [
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
    accessorKey: "jobTitle",
    id: "jobTitle",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job Title" />
    ),
    cell: ({ row }) => {
      return (
        <Link
          href={`/recruiter/jobs/${row.original.name}/${row.original.version}`}
          className="hover:underline"
        >
          {row.getValue("jobTitle")}
        </Link>
      );
    },
  },
  {
    accessorKey: "name",
    id: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Name" />
    ),
    cell: ({ row }) => {
      return <span className="font-mono text-sm">{row.getValue("name")}</span>;
    },
  },
  {
    accessorKey: "version",
    id: "version",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Version" />
    ),
    cell: ({ row }) => {
      return <Badge variant="outline">v{row.getValue("version")}</Badge>;
    },
  },
  {
    accessorKey: "status",
    id: "status",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" />
    ),
    cell: ({ row }) => {
      const status = (row.getValue("status") as string) ?? "Draft";
      const variant =
        status === "Published"
          ? "default"
          : status === "Archived"
            ? "secondary"
            : "outline";
      return (
        <Badge variant={variant} className="capitalize">
          {status}
        </Badge>
      );
    },
  },
  {
    accessorKey: "jobType",
    id: "jobType",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job Type" />
    ),
    cell: ({ row }) => {
      return <Badge variant="secondary">{row.getValue("jobType")}</Badge>;
    },
  },
  {
    accessorKey: "experienceLevel",
    id: "experienceLevel",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Experience Level" />
    ),
    cell: ({ row }) => {
      return <Badge variant="outline">{row.getValue("experienceLevel")}</Badge>;
    },
  },
  {
    accessorKey: "candidateCount",
    id: "candidateCount",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Candidates" />
    ),
    cell: ({ row }) => {
      const count = (row.getValue("candidateCount") as number) || 0;
      return (
        <div className="flex items-center gap-1 px-4">
          <Users className="text-muted-foreground h-4 w-4" />
          <span>{String(count)}</span>
        </div>
      );
    },
  },
  {
    accessorKey: "createdAt",
    id: "createdAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Created At" />
    ),
    cell: ({ row }) => {
      return (
        <span className="px-4">{formatDate(row.getValue("createdAt"))}</span>
      );
    },
  },
  {
    id: "actions",
    header: () => <span className="px-3">Actions</span>,
    cell: ({ row }) => {
      const job = row.original;
      return (
        <JobActionsMenu
          job={job}
          onDelete={onDelete}
          onDuplicate={onDuplicate}
          onOpenPublish={onOpenPublish}
        />
      );
    },
    enableSorting: false,
    enableHiding: false,
  },
];
