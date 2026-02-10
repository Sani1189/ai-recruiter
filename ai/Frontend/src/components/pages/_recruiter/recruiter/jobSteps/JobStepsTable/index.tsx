"use client";

import { useState } from "react";
import { Search, Filter, X } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { JobPostStep, STEP_TYPES } from "@/types/jobPostStep";
import { useApi } from "@/hooks/useApi";
import { PaginatedResponse } from "@/lib/api/config";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";

import { createColumns } from "./columns";
import { toast } from "sonner";

interface JobStepsTableProps {
  initialData?: JobPostStep[];
}

export default function JobStepsTable({ initialData = [] }: JobStepsTableProps) {
  const api = useApi();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [formFilters, setFormFilters] = useState({
    searchTerm: '',
    stepType: '',
  });
  
  const fetchJobSteps = async (params: ServerTableParams): Promise<PaginatedResponse<JobPostStep>> => {
    const queryParams = new URLSearchParams();
    
    queryParams.set('Page', params.page.toString());
    queryParams.set('PageSize', params.pageSize.toString());
    
    if (params.sortBy) {
      queryParams.set('SortBy', params.sortBy);
      queryParams.set('SortDescending', params.sortOrder === 'desc' ? 'true' : 'false');
    }
    
    if (params.filters?.searchTerm) queryParams.set('SearchTerm', params.filters.searchTerm);
    if (params.filters?.stepType) queryParams.set('StepType', params.filters.stepType);
    
    const response = await api.get(`/JobStep/filtered?${queryParams.toString()}`);
    const data = response.data || response;
    
    return {
      success: true,
      data: data.items || [],
      pagination: {
        page: data.pageNumber || 1,
        pageSize: data.pageSize || 10,
        totalItems: data.totalCount || 0,
        totalPages: data.totalPages || 1,
        hasNext: data.hasNextPage || false,
        hasPrevious: data.hasPreviousPage || false,
      },
    };
  };

  const {
    data,
    isLoading,
    tableProps,
    onFilterChange,
    fetchData,
    params,
  } = useServerTable(fetchJobSteps, {
    initialPageSize: 10,
    initialFilters: { 
      searchTerm: '', 
      stepType: '',
    },
    initialSortBy: 'createdAt',
    initialSortOrder: 'desc',
  });

  const handleFilterChange = (newFilters: any) => {
    onFilterChange(newFilters);
  };

  const handleSearch = () => {
    handleFilterChange(formFilters);
  };

  const handleClearFilters = () => {
    setFormFilters({
      searchTerm: '',
      stepType: '',
    });
    
    handleFilterChange({
      searchTerm: '',
      stepType: '',
    });
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const handleDelete = async (step: JobPostStep) => {
    try {
      const response = await api.delete(`/JobStep/${step.name}/${step.version}`);
      
      // Success - refresh data
      fetchData();
      
      toast.success('Prompt deleted successfully');
    } catch (error: any) {
      // Handle specific error cases
      if (error?.status === 404) {
        toast.error('Prompt not found');
      } else if (error?.status === 400) {
        toast.error(error.message || 'Failed to delete prompt');
      } else {
        toast.error('An unexpected error occurred');
      }
      
      // Still refresh to sync with server state
      fetchData();
    }
  };

  const handleDuplicate = async (step: JobPostStep, newName: string, newDisplayTitle?: string) => {
    const response = await api.post(`/JobStep/${step.name}/${step.version}/duplicate`, {
      newName,
      newDisplayTitle: newDisplayTitle?.trim() ? newDisplayTitle.trim() : null,
    });

    const duplicated = (response as any)?.data ?? response;
    fetchData();
    return duplicated as JobPostStep;
  };

  const columns = createColumns({ onDelete: handleDelete, onDuplicate: handleDuplicate });

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle>Job Steps</CardTitle>
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="flex items-center justify-end">
          <Button 
            variant="outline" 
            size="sm"
            onClick={() => setIsFiltersOpen(!isFiltersOpen)}
          >
            <Filter className="mr-2 h-4 w-4" />
            {isFiltersOpen ? 'Hide Filters' : 'Show Filters'}
          </Button>
        </div>

        {isFiltersOpen && (
          <div className="p-4 border rounded-lg bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-2 items-end">
              <div className="space-y-2 w-full">
                <Input
                  id="search"
                  placeholder="Search by name, step type..."
                  value={formFilters.searchTerm}
                  onChange={(e) => setFormFilters(prev => ({ ...prev, searchTerm: e.target.value }))}
                  onKeyPress={handleKeyPress}
                  className="h-9"
                />
              </div>

              <div className="space-y-2">
                <Select
                  value={formFilters.stepType}
                  onValueChange={(value) => setFormFilters(prev => ({ ...prev, stepType: value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All step types" />
                  </SelectTrigger>
                  <SelectContent>
                    {STEP_TYPES.map((type) => (
                      <SelectItem key={type} value={type}>
                        {type}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="flex gap-2">
                <Button 
                  onClick={handleSearch} 
                  disabled={isLoading}
                  className="h-9 flex-1"
                >
                  <Search className="mr-2 h-4 w-4" />
                  Search
                </Button>
                <Button 
                  variant="outline" 
                  onClick={handleClearFilters}
                  disabled={isLoading}
                  className="h-9"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        )}

        <div className="relative">
          {isLoading && (
            <div className="absolute inset-0 bg-background/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-md">
              <div className="text-muted-foreground">Loading job steps...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "name",
              placeholder: "Search job steps...",
              defaultHidden: ["createdBy", "prompt"],
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

