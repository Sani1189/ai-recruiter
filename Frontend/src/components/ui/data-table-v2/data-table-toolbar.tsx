"use client";

import { Table } from "@tanstack/react-table";
import { X } from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { DataTableViewOptions } from "./data-table-view-options";
import { Metadata } from "./data-table";
import { DataTableFacetedFilter } from "./data-table-faceted-filter";

interface DataTableToolbarProps<TData> {
  table: Table<TData>;
}

const DEBOUNCE_DELAY = 500;

function getInitialSearchValue<TData>(
  table: Table<TData>,
  metaData: Metadata<TData>,
  isManualFiltering: boolean
): string {
  if (isManualFiltering && metaData.currentSearchValue !== undefined) {
    return metaData.currentSearchValue;
  }
  const column = table.getColumn(metaData.searchField?.toString() || "");
  return (column?.getFilterValue() as string) ?? "";
}

export function DataTableToolbar<TData>({ table }: DataTableToolbarProps<TData>) {
  const metaData = table.options.meta as Metadata<TData>;
  const isManualFiltering = table.options.manualFiltering === true;
  const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);
  
  const columnFilters = table.getState().columnFilters;
  const hasSearchValue = isManualFiltering && (metaData.currentSearchValue?.length ?? 0) > 0;
  const isFiltered = columnFilters.length > 0 || hasSearchValue;

  const [searchValue, setSearchValue] = useState(() =>
    getInitialSearchValue(table, metaData, isManualFiltering)
  );

  useEffect(() => {
    if (isManualFiltering) {
      const newValue = metaData.currentSearchValue ?? "";
      setSearchValue((prev) => (newValue !== prev ? newValue : prev));
    }
  }, [isManualFiltering, metaData.currentSearchValue]);

  const handleSearchChange = useCallback(
    (value: string) => {
      setSearchValue(value);

      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }

      if (isManualFiltering && metaData.onSearchChange) {
        debounceTimerRef.current = setTimeout(() => {
          metaData.onSearchChange?.(value);
        }, DEBOUNCE_DELAY);
      } else {
        const column = table.getColumn(metaData.searchField?.toString() || "");
        column?.setFilterValue(value);
      }
    },
    [isManualFiltering, metaData.onSearchChange, metaData.searchField, table]
  );

  useEffect(() => {
    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, []);

  const handleResetFilters = useCallback(() => {
    setSearchValue("");
    table.resetColumnFilters();
    if (isManualFiltering && metaData.onSearchChange) {
      metaData.onSearchChange("");
    }
  }, [isManualFiltering, metaData.onSearchChange, table]);

  return (
    <div className="flex items-center justify-between">
      <div className="flex flex-1 items-center gap-2">
        {metaData.searchField && (
          <Input
            placeholder={metaData.placeholder}
            value={searchValue}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="h-8 w-[150px] lg:w-[250px]"
          />
        )}

        {metaData.filters?.map((filter, index) => {
          const column = table.getColumn(filter.columnId.toString());
          return column ? (
            <DataTableFacetedFilter
              key={index}
              column={column}
              title={filter.title}
              options={filter.options}
            />
          ) : null;
        })}

        {isFiltered && (
          <Button variant="ghost" size="sm" onClick={handleResetFilters}>
            Reset
            <X />
          </Button>
        )}
      </div>

      <DataTableViewOptions table={table} />
    </div>
  );
}
