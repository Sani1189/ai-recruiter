"use client";

import { ColumnDef } from "@tanstack/react-table";
import Link from "next/link";

import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import { CandidateView } from "@/types/v2/type.view";

type CandidateRow = CandidateView & { fixedScores?: Record<string, number> };

const fixedCategories = [
  { key: "backendDevelopment", title: "Backend Development" },
  { key: "frontendDevelopment", title: "Frontend Development" },
  { key: "cloudDevelopment", title: "Cloud Development" },
  { key: "english", title: "English" },
  { key: "general", title: "General" },
  { key: "other", title: "Other" },
];

export const columns: ColumnDef<CandidateRow>[] = [
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
    accessorKey: "userProfile.name",
    id: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Name" />
    ),
    cell: ({ row }) => {
      return (
        <Link
          href={`/recruiter/candidates/${row.original.id}`}
          className="hover:underline"
        >
          {row.getValue("name")}
        </Link>
      );
    },
  },
  {
    accessorKey: "userProfile.email",
    id: "email",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Email" />
    ),
    cell: ({ row }) => {
      return <span>{row.getValue("email")}</span>;
    },
  },
  {
    accessorKey: "userProfile.phoneNumber",
    id: "phone number",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Phone Number" />
    ),
    cell: ({ row }) => {
      return <span>{row.getValue("phone number")}</span>;
    },
  },
  {
    accessorKey: "userProfile.age",
    id: "age",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Age" />
    ),
    cell: ({ row }) => {
      return <span className="px-4">{row.getValue("age")}</span>;
    },
  },
  {
    accessorKey: "score",
    id: "score",
    header: "Score",
    columns: [
      {
        accessorKey: "score.avg",
        id: "average",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} title="Average" />
        ),
        cell: ({ row }) => {
          return <span className="px-4">{row.getValue("average")}</span>;
        },
      },
      ...fixedCategories.map((fc) => ({
        id: fc.key,
        header: ({ column }: any) => (
          <DataTableColumnHeader column={column} title={fc.title} />
        ),
        cell: ({ row }: any) => {
          const val = row.original.fixedScores?.[fc.key] ?? 0;
          return <span className="px-4">{val}</span>;
        },
      })),
    ],
  },
  {
    accessorKey: "userProfile.nationality",
    id: "nationality",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Nationality" />
    ),
    cell: ({ row }) => {
      return <span>{row.getValue("nationality")}</span>;
    },
  },
];
