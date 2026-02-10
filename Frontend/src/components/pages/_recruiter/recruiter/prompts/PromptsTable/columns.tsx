"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Pen, Trash2, Eye, Tag, CopyPlus } from "lucide-react";
import Link from "next/link";
import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
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

import { formatDate } from "@/lib/utils";
import { Prompt } from "@/types/prompt";
import { ConfirmDialog } from "@/components/ConfirmDialog";

interface ColumnsProps {
  onDelete?: (prompt: Prompt) => void;
  onDuplicate?: (prompt: Prompt, newName: string) => Promise<Prompt> | Prompt;
}

const PROTECTED_PROMPT_NAMES = new Set<string>([
  "CVExtractionSystemInstructions",
  "CVExtractionScoringInstructions"
]);

function DuplicatePromptDialog({
  prompt,
  onDuplicate,
}: {
  prompt: Prompt;
  onDuplicate: (prompt: Prompt, newName: string) => Promise<Prompt> | Prompt;
}) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const defaults = useMemo(() => {
    return { suggestedName: `${prompt.name}-copy` };
  }, [prompt.name]);

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
      const duplicated = await onDuplicate(prompt, newName.trim());
      toast.success(`Duplicated "${prompt.name}" → "${(duplicated as any)?.name ?? newName.trim()}"`);
      setOpen(false);

      const targetName = (duplicated as any)?.name ?? newName.trim();
      const targetVersion = (duplicated as any)?.version ?? 1;
      router.push(`/recruiter/prompts/${encodeURIComponent(targetName)}/${targetVersion}`);
    } catch (err: any) {
      const message =
        err?.message ||
        err?.errors?.[0] ||
        err?.data?.error ||
        "Failed to duplicate prompt";
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
          <DialogTitle>Duplicate Prompt</DialogTitle>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="newName">New Prompt Name</Label>
          <Input
            id="newName"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="e.g. rubric-default-v1-copy"
            autoComplete="off"
          />
          <p className="text-sm text-muted-foreground">
            This creates a <span className="font-medium">fresh new Prompt v1</span> and copies category/content/locale/tags.
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

export const createColumns = ({ onDelete, onDuplicate }: ColumnsProps = {}): ColumnDef<Prompt>[] => [
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
        <div className="font-medium">
          {name}
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
    accessorKey: "category",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Category" />
    ),
    cell: ({ row }) => {
      const category = row.getValue("category") as string;
      return (
        <Badge variant="outline" className="capitalize">
          {category}
        </Badge>
      );
    },
    filterFn: (row, id, value) => {
      return value.includes(row.getValue(id));
    },
  },
  {
    accessorKey: "locale",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Locale" />
    ),
    cell: ({ row }) => {
      const locale = row.getValue("locale") as string;
      return locale ? (
        <Badge variant="secondary" className="text-xs">
          {locale}
        </Badge>
      ) : (
        <span className="text-muted-foreground text-sm">-</span>
      );
    },
  },
  {
    accessorKey: "tags",
    header: "Tags",
    cell: ({ row }) => {
      const tags = row.getValue("tags") as string[];
      return (
        <div className="flex flex-wrap gap-1 max-w-[150px]">
          {tags.length > 0 ? (
            tags.slice(0, 2).map((tag, index) => (
              <Badge key={index} variant="outline" className="text-xs truncate">
                {tag.length > 12 ? `${tag.substring(0, 12)}...` : tag}
              </Badge>
            ))
          ) : (
            <span className="text-muted-foreground text-sm">-</span>
          )}
          {tags.length > 2 && (
            <Badge variant="outline" className="text-xs">
              +{tags.length - 2}
            </Badge>
          )}
        </div>
      );
    },
  },
  {
    accessorKey: "content",
    header: "Content",
    cell: ({ row }) => {
      const content = row.getValue("content") as string;
      const truncated = content.length > 50 
        ? `${content.substring(0, 50)}...` 
        : content;
      
      return (
        <div className="max-w-[200px]">
          <p 
            className="text-sm text-muted-foreground line-clamp-1 cursor-help" 
            title={content}
          >
            {truncated}
          </p>
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
    header: "Created By",
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
      const prompt = row.original;
      const isProtected = PROTECTED_PROMPT_NAMES.has(prompt.name);
      
      return (
        <div className="flex items-center gap-2">
          <Link
            href={`/recruiter/prompts/${prompt.name}/${prompt.version}`}
            className={buttonVariants({ variant: "ghost", size: "sm" })}
          >
            <Eye className="h-4 w-4" />
          </Link>
          
          <Link
            href={`/recruiter/prompts/${prompt.name}/${prompt.version}/edit`}
            className={buttonVariants({ variant: "ghost", size: "sm" })}
          >
            <Pen className="h-4 w-4" />
          </Link>

          {onDuplicate && !isProtected && (
            <DuplicatePromptDialog prompt={prompt} onDuplicate={onDuplicate} />
          )}

          {onDuplicate && isProtected && (
            <Tooltip>
              <TooltipTrigger asChild>
                <span className="inline-flex">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-muted-foreground cursor-not-allowed pointer-events-none"
                    disabled
                    title="Duplicate disabled"
                  >
                    <CopyPlus className="h-4 w-4" />
                  </Button>
                </span>
              </TooltipTrigger>
              <TooltipContent sideOffset={6}>
                This prompt is required by the CV extraction pipeline and cannot be duplicated.
              </TooltipContent>
            </Tooltip>
          )}
          
          {isProtected ? (
            <Tooltip>
              <TooltipTrigger asChild>
                <span className="inline-flex">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-muted-foreground cursor-not-allowed pointer-events-none"
                    disabled
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </span>
              </TooltipTrigger>
              <TooltipContent sideOffset={6}>
                This prompt is required by the CV extraction pipeline and cannot be deleted.
              </TooltipContent>
            </Tooltip>
          ) : (
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
              title="Delete Prompt?"
              description={`Are you sure you want to delete "${prompt.name}" version ${prompt.version}? This action cannot be undone.`}
              confirmText="Delete"
              successMessage={`Prompt "${prompt.name}" v${prompt.version} deleted successfully`}
              errorMessage="Failed to delete Prompt"
              payload={prompt}
              onConfirm={async (data) => {
                if (data) {
                  onDelete?.(data);
                }
              }}
            />
          )}
        </div>
      );
    },
  },
];

// Default export for backward compatibility
export const columns = createColumns();
