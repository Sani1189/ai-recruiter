"use client";

import Link from "next/link";
import { ColumnDef } from "@tanstack/react-table";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

type MyApplication = {
  id: string;
  jobPostName: string;
  jobPostVersion: number;
  createdAt?: string;
  updatedAt?: string;
  completedAt?: string | null;
  status?: string;
  progress?: number;
  assignedSteps?: Array<{
    id: string;
    status: string;
    completedAt?: string | null;
  }>;
};

export const createColumns = (): ColumnDef<MyApplication>[] => [
  {
    accessorKey: "jobPostName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job" />
    ),
    cell: ({ row }) => (
      <Link href={`/applications/${row.original.id}`} className="font-medium hover:underline">
        {row.original.jobPostName}
      </Link>
    ),
    enableSorting: true,
  },
  {
    accessorKey: "completedAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" />
    ),
    cell: ({ row }) => (
      <Badge variant={row.original.completedAt ? "default" : "secondary"}>
        {row.original.completedAt ? "Completed" : "In Progress"}
      </Badge>
    ),
    enableSorting: true,
  },
  {
    accessorKey: "createdAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Applied" />
    ),
    cell: ({ row }) => (
      <span>{row.original.createdAt ? new Date(row.original.createdAt).toLocaleDateString() : "-"}</span>
    ),
    enableSorting: true,
  },
  {
    accessorKey: "updatedAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Last Updated" />
    ),
    cell: ({ row }) => (
      <span>{row.original.updatedAt ? new Date(row.original.updatedAt).toLocaleDateString() : "-"}</span>
    ),
    enableSorting: true,
  },
  {
    id: "actions",
    header: () => <span></span>,
    cell: ({ row }) => (
      <div className="text-right">
        <Link href={`/applications/${row.original.id}`}>
          <Button size="sm" variant="outline">View</Button>
        </Link>
      </div>
    ),
  },
];


