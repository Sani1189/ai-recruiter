"use client";

import { DropdownMenuTrigger } from "@radix-ui/react-dropdown-menu";
import { Table } from "@tanstack/react-table";
import { Settings2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuLabel,
  DropdownMenuPortal,
  DropdownMenuSeparator,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
} from "@/components/ui/dropdown-menu";

export function DataTableViewOptions<TData>({
  table,
}: {
  table: Table<TData>;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="ml-auto hidden h-8 lg:flex"
        >
          <Settings2 />
          View
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" className="w-[150px]">
        <DropdownMenuLabel>Toggle columns</DropdownMenuLabel>
        <DropdownMenuSeparator />
        {table
          .getAllColumns()
          .filter(
            (column) =>
              typeof column.accessorFn !== "undefined" && column.getCanHide(),
          )
          .map((column) => {
            const isGroup = column.columns.length > 0; // Check if the column is a group depending on its columns length
            // If the column is a group, we use DropdownMenuGroup, otherwise DropdownMenuCheckboxItem
            const Slot = isGroup ? DropdownMenuGroup : DropdownMenuCheckboxItem;

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

            const columnName =
              (column.columnDef.meta as any)?.name || column.id;

            return (
              <Slot key={column.id} className="capitalize" {...checkboxProps}>
                {isGroup ? (
                  <DropdownMenuSub>
                    <DropdownMenuSubTrigger className="pl-8">
                      {columnName}
                    </DropdownMenuSubTrigger>

                    <DropdownMenuPortal>
                      <DropdownMenuSubContent>
                        {column.columns.map((subColumn) => (
                          <DropdownMenuCheckboxItem
                            key={subColumn.id}
                            className="capitalize"
                            checked={subColumn.getIsVisible()}
                            onCheckedChange={(value) =>
                              subColumn.toggleVisibility(!!value)
                            }
                          >
                            {(subColumn.columnDef.meta as any)?.name ||
                              subColumn.id}
                          </DropdownMenuCheckboxItem>
                        ))}
                      </DropdownMenuSubContent>
                    </DropdownMenuPortal>
                  </DropdownMenuSub>
                ) : (
                  columnName
                )}
              </Slot>
            );
          })}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
