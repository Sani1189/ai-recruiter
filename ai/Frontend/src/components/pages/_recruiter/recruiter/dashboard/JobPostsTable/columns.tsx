"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Copy, Pen, Trash2, Users } from "lucide-react";
import Link from "next/link";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { CopyButton } from "@/components/ui/copy-button";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";

import { getInterviewLink } from "@/lib/job";
import { formatDate } from "@/lib/utils";
import { JobPost } from "@/types/v2/type.view";

export const columns: ColumnDef<JobPost>[] = [
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
    id: "job title",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job Title" />
    ),
    cell: ({ row }) => {
      return (
        <Link
          href={`/recruiter/jobs/${row.original.id}`}
          className="hover:underline"
        >
          {row.getValue("job title")}
        </Link>
      );
    },
  },
  {
    accessorKey: "jobType",
    id: "job type",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job Type" />
    ),
    cell: ({ row }) => {
      return <Badge variant="secondary">{row.getValue("job type")}</Badge>;
    },
  },
  {
    accessorKey: "experienceLevel",
    id: "experience level",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Experience Level" />
    ),
    cell: ({ row }) => {
      return (
        <Badge variant="outline">{row.getValue("experience level")}</Badge>
      );
    },
  },
  {
    accessorKey: "duration",
    id: "duration",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Duration" />
    ),
    cell: ({ row }) => {
      return <span className="px-4">{row.getValue("duration")} min</span>;
    },
  },
  {
    accessorKey: "candidatesCount",
    id: "candidates count",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Candidates" />
    ),
    cell: ({ row }) => {
      return (
        <div className="flex items-center gap-1 px-4">
          <Users className="text-muted-foreground h-4 w-4" />
          {row.getValue("candidates count")}
        </div>
      );
    },
  },
  {
    accessorKey: "createdAt",
    id: "created at",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Created At" />
    ),
    cell: ({ row }) => {
      return (
        <span className="px-4">{formatDate(row.getValue("created at"))}</span>
      );
    },
  },
  {
    accessorKey: "actions",
    header: () => <span className="px-3">Actions</span>,
    cell: ({ row }) => {
      return (
        <div className="flex items-center justify-start gap-2">
          <Link
            href={`/recruiter/jobs/edit/${row.original.id}`}
            className={buttonVariants({ variant: "ghost", size: "icon" })}
          >
            <Pen className="h-4 w-4" />
          </Link>

          <Button
            variant="ghost"
            size="icon"
            // onClick={() => handleDeleteInterview(interview.id)}
          >
            <Trash2 className="text-destructive h-4 w-4" />
          </Button>

          <CopyButton
            text={getInterviewLink(row.original.name ?? row.original.id, row.original.version ?? 1)}
            variant="ghost"
            size="icon"
            successMessage="Interview link copied to clipboard"
          >
            <Copy className="h-4 w-4" />
          </CopyButton>
        </div>
      );
    },
  },
];
