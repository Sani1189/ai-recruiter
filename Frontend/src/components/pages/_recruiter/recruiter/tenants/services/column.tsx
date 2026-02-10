"use client";

import { ColumnDef } from "@tanstack/react-table";
import { MoreVertical, Repeat2, Trash2 } from "lucide-react";

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
import { TenantServices } from "@/types/service";

interface ColumnsProps {
  onDelete?: (service: TenantServices) => void;
  onStatusToggle?: (service: TenantServices) => void;
}

export const createColumns = ({
  onDelete,
  onStatusToggle,
}: ColumnsProps = {}): ColumnDef<TenantServices>[] => [
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
    accessorKey: "serviceName",
    id: "serviceName",
    meta: {
      name: "Service Name",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Service Name" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("serviceName")}</span>;
    },
  },
  {
    accessorKey: "description",
    id: "description",
    meta: {
      name: "Description",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("description")}</span>;
    },
  },
  {
    accessorKey: "servicePlan",
    id: "servicePlan",
    meta: {
      name: "Service Plan",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Service Plan" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("servicePlan")}</span>;
    },
  },
  {
    accessorKey: "serviceType",
    id: "serviceType",
    meta: {
      name: "Service Type",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Service Type" />
    ),
    cell: ({ row }) => {
      return <span className="px-2">{row.getValue("serviceType")}</span>;
    },
  },
  {
    accessorKey: "serviceStartDate",
    id: "serviceStartDate",
    meta: {
      name: "Start Date",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Start Date" />
    ),
    cell: ({ row }) => {
      return (
        <span className="px-2">
          {formatDate(row.getValue("serviceStartDate"))}
        </span>
      );
    },
  },
  {
    accessorKey: "serviceEndDate",
    id: "serviceEndDate",
    meta: {
      name: "End Date",
    },
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="End Date" />
    ),
    cell: ({ row }) => {
      return (
        <span className="px-2">
          {formatDate(row.getValue("serviceEndDate"))}
        </span>
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
      const service = row.original;

      return (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" aria-label="Open actions">
              <MoreVertical className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>

          <DropdownMenuContent align="end">
            {onStatusToggle && (
              <DropdownMenuItem asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onStatusToggle(service)}
                >
                  <Repeat2 className="h-4 w-4" />
                  <span>Toggle Status</span>
                </Button>
              </DropdownMenuItem>
            )}

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
                  title="Delete Service?"
                  description={`Are you sure you want to delete "${service.serviceName}"? This action cannot be undone.`}
                  confirmText="Delete"
                  successMessage={`Service "${service.serviceName}" deleted successfully`}
                  errorMessage="Failed to delete Service"
                  payload={service}
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
