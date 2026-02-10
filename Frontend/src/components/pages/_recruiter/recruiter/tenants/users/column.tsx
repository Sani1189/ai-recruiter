"use client";

import { ColumnDef } from "@tanstack/react-table";
import { MoreVertical, Pen, Trash2 } from "lucide-react";
import Link from "next/link";

import { ConfirmDialog } from "@/components/ConfirmDialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-v2/data-table-column-header";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { formatDate } from "@/lib/utils";
import { UserSchema } from "@/schemas/tenants/user.schema";

// Type definition for User from API
export interface User extends UserSchema {
  userId: string;
  createdAt: string;
}

interface ColumnsProps {
  onDelete?: (user: User) => void;
}

export const createColumns = ({
  onDelete,
}: ColumnsProps = {}): ColumnDef<User>[] => [
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
    accessorKey: "firstName",
    id: "firstName",
    meta: {
      name: "First Name",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="First Name" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("firstName")}</span>;
    },
  },
  {
    accessorKey: "lastName",
    id: "lastName",
    meta: {
      name: "Last Name",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Last Name" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("lastName")}</span>;
    },
  },
  {
    accessorKey: "email",
    id: "email",
    meta: {
      name: "Email",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Email" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("email")}</span>;
    },
  },
  {
    accessorKey: "roleName",
    id: "roleName",
    meta: {
      name: "Role",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Role" />
    ),
    cell: ({ row }) => {
      return (
        <Badge variant="outline" className="mx-2 capitalize">
          {row.getValue("roleName")}
        </Badge>
      );
    },
  },
  {
    accessorKey: "statusDesc",
    id: "statusDesc",
    meta: {
      name: "Status",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" />
    ),
    cell: ({ row }) => {
      return (
        <Badge variant="outline" className="mx-2 capitalize">
          {row.getValue("statusDesc")}
        </Badge>
      );
    },
  },
  {
    accessorKey: "createdAt",
    id: "createdAt",
    meta: {
      name: "Created At",
    },
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
    meta: {
      name: "Actions",
    },
    header: () => <span className="px-3">Actions</span>,
    cell: ({ row }) => {
      const user = row.original;

      return (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" aria-label="Open actions">
              <MoreVertical className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>

          <DropdownMenuContent align="end">
            <DropdownMenuItem asChild>
              <Link
                href={`/recruiter/tenants/users/edit/${user.userId}`}
                className="flex w-full items-center gap-3"
              >
                <Pen className="h-4 w-4" />
                <span>Edit</span>
              </Link>
            </DropdownMenuItem>

            {onDelete && (
              <DropdownMenuItem
                className="text-destructive focus:text-destructive"
                onSelect={(e) => e.preventDefault()}
              >
                <ConfirmDialog
                  trigger={
                    <div className="flex w-full items-center gap-3">
                      <Trash2 className="text-destructive h-4 w-4" />
                      <span>Delete</span>
                    </div>
                  }
                  title="Delete User?"
                  description={`Are you sure you want to delete "${user.firstName} ${user.lastName}"? This action cannot be undone.`}
                  confirmText="Delete"
                  successMessage={`User "${user.firstName} ${user.lastName}" deleted successfully`}
                  errorMessage="Failed to delete User"
                  payload={user}
                  onConfirm={async (data) => {
                    if (data) {
                      onDelete(data);
                    }
                  }}
                />
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      );
    },
    enableSorting: false,
    enableHiding: false,
  },
];
