"use client";

import { useState } from "react";
import { Search, Filter, X } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";
import { useApi } from "@/hooks/useApi";
import { createColumns } from "./columns";

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

export default function ApplicationsTable() {
  const api = useApi();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [formFilters, setFormFilters] = useState({
    searchTerm: "",
  });

  const fetchApplications = async (params: ServerTableParams) => {
    const query = new URLSearchParams();
    // Pagination
    query.set("PageNumber", String(params.page));
    query.set("PageSize", String(params.pageSize));
    // Sorting
    if (params.sortBy) {
      query.set("SortBy", params.sortBy);
      query.set("SortDescending", params.sortOrder === "desc" ? "true" : "false");
    }
    // Filters
    const filters = params.filters || {};
    if ((filters as any).searchTerm) query.set("SearchTerm", (filters as any).searchTerm);

    const res = await api.get(`/candidate/job-application/my-applications/filtered?${query.toString()}`);
    const data = (res as any)?.data ?? res;
    return {
      success: true,
      data: (data?.items ?? []) as MyApplication[],
      pagination: {
        page: data?.pageNumber ?? 1,
        pageSize: data?.pageSize ?? params.pageSize,
        totalItems: data?.totalCount ?? 0,
        totalPages: data?.totalPages ?? 1,
        hasNext: data?.hasNextPage ?? false,
        hasPrevious: data?.hasPreviousPage ?? false,
      },
    };
  };

  const {
    data,
    isLoading,
    tableProps,
    onFilterChange,
    onSortChange,
    fetchData,
    params,
  } = useServerTable(fetchApplications, {
    initialPageSize: 10,
    initialFilters: { searchTerm: "" },
    initialSortBy: "createdAt",
    initialSortOrder: "desc",
  });

  const handleSearch = () => onFilterChange(formFilters);
  const handleClearFilters = () => {
    setFormFilters({ searchTerm: "" });
    onFilterChange({ searchTerm: "" });
  };
  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") handleSearch();
  };

  const columns = createColumns();

  return (
    <Card className="shadow-card">
      <CardContent className="space-y-4">
        {/* Filter Toggle */}
        <div className="flex items-center justify-end">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setIsFiltersOpen(!isFiltersOpen)}
          >
            <Filter className="mr-2 h-4 w-4" />
            {isFiltersOpen ? "Hide Filters" : "Show Filters"}
          </Button>
        </div>

        {/* Collapsible Filters */}
        {isFiltersOpen && (
          <div className="p-4 border rounded-lg bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-2 items-end">
              {/* Search Input */}
              <div className="space-y-2 w-full md:col-span-3">
                <Input
                  id="search"
                  placeholder="Search by job name..."
                  value={formFilters.searchTerm}
                  onChange={(e) => setFormFilters(prev => ({ ...prev, searchTerm: e.target.value }))}
                  onKeyPress={handleKeyPress}
                  className="h-9"
                />
              </div>

              {/* Action Buttons */}
              <div className="flex gap-2">
                <Button onClick={handleSearch} disabled={isLoading} className="h-9 flex-1">
                  <Search className="mr-2 h-4 w-4" />
                  Search
                </Button>
                <Button variant="outline" onClick={handleClearFilters} disabled={isLoading} className="h-9">
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        )}

        <div className="relative">
          {isLoading && (
            <div className="absolute inset-0 bg-background/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-md">
              <div className="text-muted-foreground">Loading applications...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            data={data}
            metadata={{ 
              searchField: "jobPostName", 
              placeholder: "Search applications...",
              currentSearchValue: params.filters?.searchTerm || "",
              onSearchChange: (searchValue) => {
                onFilterChange({ ...params.filters, searchTerm: searchValue });
              },
            }}
          pageCount={tableProps.pageCount}
          manualPagination={tableProps.manualPagination}
          manualFiltering={tableProps.manualFiltering}
          manualSorting={tableProps.manualSorting}
          state={tableProps.state}
          onPaginationChange={tableProps.onPaginationChange}
          onSortingChange={tableProps.onSortingChange}
          getRowCount={tableProps.getRowCount}
          getPageCount={tableProps.getPageCount}
        />
        </div>
      </CardContent>
    </Card>
  );
}


