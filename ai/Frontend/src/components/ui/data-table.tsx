"use client";

import {
  Column,
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getSortedRowModel,
  useReactTable,
} from "@tanstack/react-table";
import {
  ArrowDownWideNarrow,
  ArrowUpWideNarrow,
  ChevronDown,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuPortal,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";

interface DataTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  searchField: keyof TData | (string & {});
  placeholder?: string;
  defaultHidden?: (keyof TData | (string & {}))[];
}
export function DataTable<TData, TValue>({
  columns,
  data,
  searchField,
  placeholder = "Search...",
  defaultHidden,
}: DataTableProps<TData, TValue>) {
  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(), // core model
    getFilteredRowModel: getFilteredRowModel(), // model for filtering like search
    getSortedRowModel: getSortedRowModel(), // model for sorting
    initialState: {
      columnVisibility: {
        // Set initial visibility of columns based on defaultHidden prop
        ...defaultHidden?.reduce(
          (acc, column) => {
            acc[column as string] = false; // Hide the column
            return acc;
          },
          {} as Record<string, boolean>,
        ),
      },
    },
  });

  // Function to get the header content for a column
  // depending on whether it is a string or a react component
  const getHeaderContent = (column: Column<TData, unknown>) => {
    return typeof column.columnDef.header === "string"
      ? column.columnDef.header
      : column.id;
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center">
        <Input
          placeholder={placeholder}
          value={
            (table
              .getColumn(searchField as string)
              ?.getFilterValue() as string) ?? ""
          }
          onChange={(event) =>
            table
              .getColumn(searchField as string)
              ?.setFilterValue(event.target.value)
          }
          className="max-w-sm"
        />

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" className="ml-auto">
              Columns <ChevronDown />
            </Button>
          </DropdownMenuTrigger>

          <DropdownMenuContent align="end">
            {table
              .getAllColumns()
              .filter((column) => column.getCanHide())
              .filter((column) => column.id !== searchField)
              .map((column) => {
                const isGroup = column.columns.length > 0; // Check if the column is a group depending on its columns length
                // If the column is a group, we use DropdownMenuGroup, otherwise DropdownMenuCheckboxItem
                const Slot = isGroup
                  ? DropdownMenuGroup
                  : DropdownMenuCheckboxItem;

                // If the column is a group, we don't need to pass checked and onCheckedChange props
                // If the column is not a group, we pass these props to control visibility
                // of the column in the table
                const checkboxProps = isGroup
                  ? {}
                  : {
                      checked: column.getIsVisible(),
                      onCheckedChange: (value: boolean) =>
                        column.toggleVisibility(value),
                    };

                return (
                  <Slot
                    key={column.id}
                    className="capitalize"
                    {...checkboxProps}
                  >
                    {isGroup ? (
                      <DropdownMenuSub>
                        <DropdownMenuSubTrigger className="pl-8">
                          {getHeaderContent(column)}
                        </DropdownMenuSubTrigger>

                        <DropdownMenuPortal>
                          <DropdownMenuSubContent>
                            {column.columns.map((subColumn) => (
                              <DropdownMenuCheckboxItem
                                key={subColumn.id}
                                className="capitalize"
                                checked={subColumn.getIsVisible()}
                                onCheckedChange={(value) =>
                                  subColumn.toggleVisibility(value)
                                }
                              >
                                {getHeaderContent(subColumn)}
                              </DropdownMenuCheckboxItem>
                            ))}
                          </DropdownMenuSubContent>
                        </DropdownMenuPortal>
                      </DropdownMenuSub>
                    ) : (
                      getHeaderContent(column)
                    )}
                  </Slot>
                );
              })}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="overflow-hidden rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => {
                  return (
                    <TableHead
                      key={header.id}
                      colSpan={header.colSpan}
                      className={cn({
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
                  {row.getVisibleCells().map((cell) => (
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
    </div>
  );
}

/**
 * ColumnSortButton is a helper component for rendering a button
 * that toggles the sorting of a column in the data table.
 */
interface ColumnSortButtonProps<TData>
  extends React.ComponentPropsWithoutRef<typeof Button> {
  column: Column<TData, unknown>;
  children: React.ReactNode;
}
export function ColumnSortButton<TData>({
  column,
  children,
  ...props
}: ColumnSortButtonProps<TData>) {
  return (
    <Button
      variant="ghost"
      {...props}
      onClick={column.getToggleSortingHandler()}
    >
      {children}
      {{
        asc: <ArrowUpWideNarrow />,
        desc: <ArrowDownWideNarrow />,
      }[column.getIsSorted() as string] ?? null}
    </Button>
  );
}
