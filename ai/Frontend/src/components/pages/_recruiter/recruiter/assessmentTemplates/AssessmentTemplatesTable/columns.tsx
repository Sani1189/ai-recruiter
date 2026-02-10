"use client";

import Link from "next/link";
import { ColumnDef } from "@tanstack/react-table";
import { Eye, Pencil, MoreVertical, Trash2, Copy } from "lucide-react";
import { useState, useMemo } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { AssessmentTemplate } from "@/types/assessmentTemplate";
import { formatDate } from "@/lib/utils";

function DuplicateTemplateDialog({
  template,
  onDuplicate,
  open: controlledOpen,
  onOpenChange: controlledOnOpenChange,
  hideTrigger = false,
}: {
  template: AssessmentTemplate;
  onDuplicate: (template: AssessmentTemplate, newName: string, includeQuestions: boolean) => Promise<AssessmentTemplate>;
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
    return { suggestedName: `${template.name}-copy` };
  }, [template.name]);

  const [newName, setNewName] = useState("");
  const [includeQuestions, setIncludeQuestions] = useState(true);

  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    if (next) {
      setNewName(defaults.suggestedName);
      setIncludeQuestions(true);
    }
  };

  const canSubmit = newName.trim().length > 0;

  const handleDuplicate = async () => {
    if (!canSubmit || submitting) return;
    setSubmitting(true);
    try {
      const duplicated = await onDuplicate(template, newName.trim(), includeQuestions);
      toast.success(`Duplicated "${template.name}" → "${(duplicated as any)?.name ?? newName.trim()}"`);
      setOpen(false);

      const targetName = (duplicated as any)?.name ?? newName.trim();
      const targetVersion = (duplicated as any)?.version ?? 1;
      router.push(`/recruiter/assessmentTemplates/${encodeURIComponent(targetName)}/${targetVersion}`);
    } catch (err: any) {
      const message =
        err?.message ||
        err?.errors?.[0] ||
        err?.data?.error ||
        "Failed to duplicate template";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      {!hideTrigger && (
        <DialogTrigger asChild>
          <Button variant="ghost" size="sm" title="Duplicate Template">
            <Copy className="h-4 w-4" />
          </Button>
        </DialogTrigger>
      )}

      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Duplicate Template</DialogTitle>
          <DialogDescription>
            Create a new template with a different name. The new template will start at version 1.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="newName">New Template Name</Label>
            <Input
              id="newName"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="e.g. basic-math-quiz-copy"
              autoComplete="off"
            />
            <p className="text-sm text-muted-foreground">
              Current template: <span className="font-mono">{template.name}</span>
            </p>
          </div>

          <div className="flex items-center space-x-2">
            <Checkbox
              id="includeQuestions"
              checked={includeQuestions}
              onCheckedChange={(checked) => setIncludeQuestions(checked === true)}
            />
            <Label htmlFor="includeQuestions" className="text-sm font-normal cursor-pointer">
              Include questions and options
            </Label>
          </div>
        </div>

        <DialogFooter className="gap-2">
          <Button variant="outline" onClick={() => setOpen(false)} disabled={submitting}>
            Cancel
          </Button>
          <Button onClick={handleDuplicate} disabled={!canSubmit || submitting}>
            {submitting ? "Duplicating..." : "Duplicate"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}


export function createColumns({
  onDelete,
  onRestore,
  onDuplicate,
}: {
  onDelete: (t: AssessmentTemplate) => Promise<void>;
  onRestore?: (t: AssessmentTemplate) => Promise<void>;
  onDuplicate?: (t: AssessmentTemplate, newName: string, includeQuestions: boolean) => Promise<AssessmentTemplate>;
}): ColumnDef<AssessmentTemplate>[] {
  const getQuestionsCount = (t: AssessmentTemplate): number | null => {
    if (typeof (t as any)?.questionsCount === "number") return (t as any).questionsCount;
    const sections = Array.isArray((t as any)?.sections) ? (t as any).sections : [];
    if (sections.length === 0) return null;
    return sections.reduce(
      (acc: number, s: any) => acc + (Array.isArray(s?.questions) ? s.questions.length : 0),
      0,
    );
  };

  return [
    {
      accessorKey: "name",
      header: "Name",
      cell: ({ row }) => {
        const t = row.original;
        return (
          <Link
            href={`/recruiter/assessmentTemplates/${encodeURIComponent(t.name)}/${t.version}`}
            className="font-medium hover:underline underline-offset-4"
          >
            {t.name}
          </Link>
        );
      },
    },
    {
      accessorKey: "title",
      header: "Title",
      cell: ({ row }) => {
        const t = row.original;
        if (!t.title) return "-";
        return (
          <Link
            href={`/recruiter/assessmentTemplates/${encodeURIComponent(t.name)}/${t.version}`}
            className="hover:underline underline-offset-4"
          >
            {t.title}
          </Link>
        );
      },
    },
    {
      accessorKey: "templateType",
      header: "Type",
      cell: ({ row }) => (
        <Badge variant="outline">{row.original.templateType}</Badge>
      ),
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => {
        const status = row.original.status;
        const variant =
          status === "Published"
            ? "default"
            : status === "Draft"
            ? "secondary"
            : "outline";
        return <Badge variant={variant}>{status}</Badge>;
      },
    },
    {
      accessorKey: "version",
      header: "Version",
      cell: ({ row }) => `v${row.original.version}`,
    },
    {
      id: "questionsCount",
      header: "Questions",
      cell: ({ row }) => {
        const count = getQuestionsCount(row.original);
        return <span className="font-mono">{count ?? "-"}</span>;
      },
    },
    {
      accessorKey: "createdAt",
      header: "Created At",
      cell: ({ row }) => (row.original.createdAt ? formatDate(row.original.createdAt) : "-"),
    },
    {
      accessorKey: "updatedBy",
      header: "Updated By",
      cell: ({ row }) => (row.original.updatedBy?.trim() ? row.original.updatedBy : "-"),
    },
    {
      accessorKey: "updatedAt",
      header: "Updated At",
      cell: ({ row }) => (row.original.updatedAt ? formatDate(row.original.updatedAt) : "-"),
    },
    {
      id: "actions",
      cell: ({ row }) => {
        return <ActionsCell row={row} onDelete={onDelete} onRestore={onRestore} onDuplicate={onDuplicate} />;
      },
    },
  ];
}

function ActionsCell({
  row,
  onDelete,
  onRestore,
  onDuplicate,
}: {
  row: { original: AssessmentTemplate };
  onDelete: (t: AssessmentTemplate) => Promise<void>;
  onRestore?: (t: AssessmentTemplate) => Promise<void>;
  onDuplicate?: (t: AssessmentTemplate, newName: string, includeQuestions: boolean) => Promise<AssessmentTemplate>;
}) {
  const t = row.original;
  const [duplicateOpen, setDuplicateOpen] = useState(false);

  return (
    <>
      {!t.isDeleted && onDuplicate && (
        <DuplicateTemplateDialog
          template={t}
          onDuplicate={onDuplicate}
          open={duplicateOpen}
          onOpenChange={setDuplicateOpen}
          hideTrigger
        />
      )}

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="icon">
            <MoreVertical className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          {t.isDeleted ? (
            <ConfirmDialog
              trigger={
                <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                  Restore
                </DropdownMenuItem>
              }
              title="Restore template?"
              description={`This will restore "${t.name}" v${t.version} and make it visible in lists again.`}
              confirmText="Restore"
              showToast={false}
              payload={t}
              onConfirm={async (data) => {
                if (data && onRestore) {
                  await onRestore(data);
                }
              }}
            />
          ) : (
            <>
              <DropdownMenuItem asChild>
                <Link href={`/recruiter/assessmentTemplates/${t.name}/${t.version}`} className="flex items-center">
                  <Eye className="mr-2 h-4 w-4" />
                  View
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href={`/recruiter/assessmentTemplates/${t.name}/${t.version}/edit`} className="flex items-center">
                  <Pencil className="mr-2 h-4 w-4" />
                  Edit
                </Link>
              </DropdownMenuItem>
              {onDuplicate && (
                <DropdownMenuItem onSelect={() => setDuplicateOpen(true)}>
                  <Copy className="mr-2 h-4 w-4" />
                  Duplicate
                </DropdownMenuItem>
              )}
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
                  title="Delete template?"
                  description={`This will delete "${t.name}" v${t.version}. If it’s referenced by submissions, the backend may soft-delete it instead to preserve history.`}
                  confirmText="Delete"
                  showToast={false}
                  payload={t}
                  onConfirm={async (data) => {
                    if (data) await onDelete(data);
                  }}
                />
              </DropdownMenuItem>
            </>
          )}
        </DropdownMenuContent>
      </DropdownMenu>
    </>
  );
}
