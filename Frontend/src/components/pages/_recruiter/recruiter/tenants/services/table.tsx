"use client";

import { toast } from "sonner";

import { Card, CardContent } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { TEMP } from "@/constants/temp";
import { useApi } from "@/hooks/useApi";
import { ServerTableParams, useServerTable } from "@/hooks/useServerTable";
import { PaginatedResponse } from "@/lib/api";
import { TenantServices } from "@/types/service";
import { createColumns } from "./column";

export default function ServicesTable() {
  const api = useApi(true);

  // Create API fetch function for useServerTable
  const fetchServices = async (
    params: ServerTableParams,
  ): Promise<PaginatedResponse<TenantServices>> => {
    const defaultPagination = {
      page: 1,
      pageSize: 10,
      totalItems: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false,
    };

    const response = await api.getList<TenantServices>(
      `/TenantServices/${TEMP.tenantId}`,
      {
        page: params.page,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        search: params.filters?.searchTerm || "",
      },
    );

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
    useServerTable(fetchServices, {
      initialPageSize: 10,
      initialFilters: {
        searchTerm: "",
      },
      initialSortBy: "createdAt",
      initialSortOrder: "desc",
    });

  const handleDelete = async (service: TenantServices) => {
    try {
      await api.delete(`/tenantservices/${service.tenantServiceId}`);

      // Optional: Add toast notification for success
      toast.success("Service deleted successfully");
    } catch (error: any) {
      // Handle specific error cases
      if (error?.status === 404) {
        console.error("Service not found");
        toast.error("Service not found");
      } else if (error?.status === 400) {
        console.error("Failed to delete service:", error.message);
        toast.error(error.message || "Failed to delete service");
      } else {
        console.error("Failed to delete service:", error);
        toast.error("An unexpected error occurred");
      }
    } finally {
      // refresh to sync with server state
      fetchData();
    }
  };

  const handleStatusToggle = async (service: TenantServices) => {
    try {
      await api.put(`/tenantservices/${service.tenantServiceId}/status`, {
        statusCode: service.statusCode === "ACT" ? "INA" : "ACT",
      });

      // Success - refresh data
      fetchData();

      // Optional: Add toast notification for success
      toast.success("Service status toggled successfully");
    } catch (error: any) {
      // Handle specific error cases
      if (error?.status === 404) {
        console.error("Service not found");
        toast.error("Service not found");
      } else if (error?.status === 400) {
        console.error("Failed to toggle status:", error.message);
        toast.error(error.message || "Failed to toggle status");
      } else {
        console.error("Failed to toggle status:", error);
        toast.error("An unexpected error occurred");
      }

      // Still refresh to sync with server state
      fetchData();
    }
  };

  const columns = createColumns({
    onDelete: handleDelete,
    onStatusToggle: handleStatusToggle,
  });

  return (
    <Card className="shadow-card">
      <CardContent className="space-y-4">
        <div className="relative">
          {/* Data Table */}
          {isLoading && (
            <div className="bg-background/50 absolute inset-0 z-10 flex items-center justify-center rounded-md backdrop-blur-sm">
              <div className="text-muted-foreground">Loading services...</div>
            </div>
          )}

          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "serviceName",
              placeholder: "Search services...",
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
