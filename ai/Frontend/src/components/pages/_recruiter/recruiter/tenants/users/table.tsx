"use client";

import { toast } from "sonner";

import { Card, CardContent } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { TEMP } from "@/constants/temp";
import { useApi } from "@/hooks/useApi";
import { ServerTableParams, useServerTable } from "@/hooks/useServerTable";
import { PaginatedResponse } from "@/lib/api";
import { createColumns, User } from "./column";

export default function UsersTable() {
  const api = useApi(true);

  // Create API fetch function for useServerTable
  const fetchUsers = async (
    params: ServerTableParams,
  ): Promise<PaginatedResponse<User>> => {
    const defaultPagination = {
      page: 1,
      pageSize: 10,
      totalItems: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false,
    };

    const response = await api.getList<User>(`/TenantUsers/${TEMP.tenantId}`, {
      page: params.page,
      pageSize: params.pageSize,
      sortBy: params.sortBy,
      sortOrder: params.sortOrder,
      search: params.filters?.searchTerm || "",
    });
    if (!response.success) {
      return {
        success: false,
        data: [],
        pagination: defaultPagination,
      };
    }

    // Transform API response to match PaginatedResponse format
    return {
      success: true,
      data: response.data,
      pagination: {
        hasNext: response.pagination.hasNextPage || defaultPagination.hasNext,
        hasPrevious:
          response.pagination.hasPreviousPage || defaultPagination.hasPrevious,
        page: response.pagination.pageNumber || defaultPagination.page,
        pageSize: response.pagination.pageSize || defaultPagination.pageSize,
        totalItems:
          response.pagination.totalCount || defaultPagination.totalItems,
        totalPages:
          response.pagination.totalPages || defaultPagination.totalPages,
      },
    };
  };

  // Use server table hook for pagination, filtering, and sorting
  const { data, isLoading, tableProps, params, fetchData, onFilterChange } =
    useServerTable(fetchUsers, {
      initialPageSize: 10,
      initialFilters: {
        searchTerm: "",
      },
      initialSortBy: "createdAt",
      initialSortOrder: "desc",
    });

  const handleDelete = async (user: User) => {
    try {
      await api.delete(`/TenantUsers/${TEMP.tenantId}/${user.userId}`);

      // Success - refresh data
      fetchData();

      // Optional: Add toast notification for success
      toast.success("User deleted successfully");
    } catch (error: any) {
      // Handle specific error cases
      if (error?.status === 404) {
        console.error("User not found");
        toast.error("User not found");
      } else if (error?.status === 400) {
        console.error("Failed to delete user:", error.message);
        toast.error(error.message || "Failed to delete user");
      } else {
        console.error("Failed to delete user:", error);
        toast.error("An unexpected error occurred");
      }

      // Still refresh to sync with server state
      fetchData();
    }
  };

  const columns = createColumns({ onDelete: handleDelete });

  return (
    <Card className="shadow-card">
      <CardContent className="space-y-4">
        {/* Data Table */}
        <div className="relative">
          {isLoading && (
            <div className="bg-background/50 absolute inset-0 z-10 flex items-center justify-center rounded-md backdrop-blur-sm">
              <div className="text-muted-foreground">Loading users...</div>
            </div>
          )}

          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "firstName",
              placeholder: "Search users...",
              currentSearchValue: params.filters?.searchTerm || "",
              onSearchChange: (searchValue) => {
                onFilterChange({ ...params.filters, searchTerm: searchValue });
              },
            }}
            // Pass only specific server-side pagination props
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
