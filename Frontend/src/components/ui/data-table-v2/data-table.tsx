"use client";

import {
  ColumnDef,
  ColumnFiltersState,
  flexRender,
  getCoreRowModel,
  getFacetedRowModel,
  getFacetedUniqueValues,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  SortingState,
  useReactTable,
  VisibilityState,
} from "@tanstack/react-table";
import * as React from "react";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { cn } from "@/lib/utils";
import { DataTablePagination } from "./data-table-pagination";
import { DataTableToolbar } from "./data-table-toolbar";

type OptionalKeyOf<T> = keyof T | (string & {});

export type Metadata<T> = {
  searchField: OptionalKeyOf<T>;
  placeholder: string;
  filters?: {
    columnId: OptionalKeyOf<T>;
    title: string;
    options: { value: string; label: string }[];
  }[];
  defaultHidden?: OptionalKeyOf<T>[];
  onSearchChange?: (searchValue: string) => void;
  currentSearchValue?: string;
};

interface DataTableProps<TData, TValue> {
  data: TData[];
  columns: ColumnDef<TData, TValue>[];
  metadata: Metadata<TData>;
  // Server-side pagination props
  pageCount?: number;
  manualPagination?: boolean;
  manualFiltering?: boolean;
  manualSorting?: boolean;
  state?: {
    pagination?: {
      pageIndex: number;
      pageSize: number;
    };
    sorting?: SortingState;
  };
  onPaginationChange?: (updater: any) => void;
  onSortingChange?: (updater: any) => void;
  getRowCount?: () => number;
  getPageCount?: () => number;
}

export function DataTable<TData, TValue>({
  columns,
  data,
  metadata,
  pageCount,
  manualPagination,
  manualFiltering,
  manualSorting,
  state: externalState,
  onPaginationChange,
  onSortingChange,
  getRowCount,
  getPageCount,
}: DataTableProps<TData, TValue>) {
  const [rowSelection, setRowSelection] = React.useState({});
  const [columnVisibility, setColumnVisibility] =
    React.useState<VisibilityState>(() => {
      // Set initial visibility of columns based on defaultHidden prop
      const hiddenColumns =
        metadata.defaultHidden?.reduce(
          (acc, column) => {
            acc[column as string] = false; // Hide the column
            return acc;
          },
          {} as Record<string, boolean>,
        ) || {};

      return hiddenColumns;
    });
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>(
    [],
  );
  const [sorting, setSorting] = React.useState<SortingState>([]);

  const table = useReactTable({
    data,
    columns,
    state: {
      sorting: externalState?.sorting || sorting,
      columnVisibility, // ✅ Keep local column visibility state
      rowSelection,
      columnFilters,
      // Only merge pagination from props
      ...(externalState?.pagination && {
        pagination: externalState.pagination,
      }),
    },
    initialState: {
      pagination: {
        pageSize: externalState?.pagination?.pageSize || 25,
      },
    },
    enableRowSelection: true,
    meta: metadata,
    onRowSelectionChange: setRowSelection,
    onSortingChange: onSortingChange || setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: (updater) => {
      setColumnVisibility(updater);
    },
    // Include server-side pagination props
    ...(pageCount && { pageCount }),
    ...(manualPagination && { manualPagination }),
    ...(manualFiltering && { manualFiltering }),
    ...(manualSorting && { manualSorting }),
    ...(onPaginationChange && { onPaginationChange }),
    ...(getRowCount && { getRowCount }),
    ...(getPageCount && { getPageCount }),
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFacetedRowModel: getFacetedRowModel(),
    getFacetedUniqueValues: getFacetedUniqueValues(),
  });

  return (
    <div className="flex flex-col gap-4">
      <DataTableToolbar table={table} />

      <div className="max-h-[500px] overflow-auto rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header, idx) => {
                  return (
                    <TableHead
                      key={header.id}
                      colSpan={header.colSpan}
                      className={cn("sticky top-0 z-20", {
                        "border-x": header.headerGroup.depth > 0,
                        "border-x text-center": header.colSpan > 1,
                      })}
                    >
                      {header.isPlaceholder
                        ? null
                        : flexRender(
                            header.column.columnDef.header,
                            header.getContext(),
                          )}
                    </TableHead>
                  );
                })}
              </TableRow>
            ))}
          </TableHeader>

          <TableBody>
            {table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={row.getIsSelected() && "selected"}
                >
                  {row.getVisibleCells().map((cell, index) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  No results.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      <DataTablePagination table={table} />
    </div>
  );
}
