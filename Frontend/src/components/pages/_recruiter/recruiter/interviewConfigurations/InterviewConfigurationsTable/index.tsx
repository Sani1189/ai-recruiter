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

import { 
  InterviewConfiguration, 
  INTERVIEW_MODALITIES 
} from "@/types/interviewConfiguration";
import { useApi } from "@/hooks/useApi";
import { ApiResponse, PaginatedResponse } from "@/lib/api/config";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";

import { createColumns } from "./columns";
import { toast } from "sonner";

interface InterviewConfigurationsTableProps {
  initialData?: InterviewConfiguration[];
}

export default function InterviewConfigurationsTable({ initialData = [] }: InterviewConfigurationsTableProps) {
  const api = useApi();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [formFilters, setFormFilters] = useState({
    searchTerm: '',
    modality: '',
    active: '',
  });
  
  // Create API fetch function for useServerTable
  const fetchConfigurations = async (params: ServerTableParams): Promise<PaginatedResponse<InterviewConfiguration>> => {
    const queryParams = new URLSearchParams();
    
    // Add pagination
    queryParams.set('PageNumber', params.page.toString());
    queryParams.set('PageSize', params.pageSize.toString());
    
    // Add sorting
    if (params.sortBy) {
      queryParams.set('SortBy', params.sortBy);
      queryParams.set('SortDescending', params.sortOrder === 'desc' ? 'true' : 'false');
    }
    
    // Add filters
    if (params.filters?.searchTerm) queryParams.set('SearchTerm', params.filters.searchTerm);
    if (params.filters?.modality) queryParams.set('Modality', params.filters.modality);
    if (params.filters?.active) queryParams.set('Active', params.filters.active);
    
    const response = await api.get(`/InterviewConfiguration/filtered?${queryParams.toString()}`);
    const data = response.data || response;
    
    // Transform API response to match PaginatedResponse format
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

  // Use server table hook for pagination, filtering, and sorting
  const {
    data,
    isLoading,
    tableProps,
    onFilterChange,
    onSortChange,
    fetchData,
    params,
  } = useServerTable(fetchConfigurations, {
    initialPageSize: 10,
    initialFilters: { 
      searchTerm: '', 
      modality: '', 
      active: '' 
    },
    initialSortBy: 'CreatedAt',
    initialSortOrder: 'desc',
  });

  // Filter handling functions
  const handleFilterChange = (newFilters: any) => {
    onFilterChange(newFilters);
  };

  const handleSearch = () => {
    handleFilterChange(formFilters);
  };

  const handleClearFilters = () => {
    // Reset form filters
    setFormFilters({
      searchTerm: '',
      modality: '',
      active: '',
    });
    
    // Reset server filters
    handleFilterChange({
      searchTerm: '',
      modality: '',
      active: '',
    });
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const handleDelete = async (configuration: InterviewConfiguration) => {
    try {
      const response = await api.delete(`/InterviewConfiguration/${configuration.name}/${configuration.version}`);
      
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

  const handleDuplicate = async (configuration: InterviewConfiguration, newName: string) => {
    const response = await api.post(`/InterviewConfiguration/${configuration.name}/${configuration.version}/duplicate`, {
      newName,
    });

    const duplicated = (response as any)?.data ?? response;
    fetchData();
    return duplicated as InterviewConfiguration;
  };

  // Create columns with delete + duplicate handlers
  const columns = createColumns({ onDelete: handleDelete, onDuplicate: handleDuplicate });

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle>Interview Configurations</CardTitle>
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Filter Toggle */}
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

        {/* Collapsible Filters */}
        {isFiltersOpen && (
          <div className="p-4 border rounded-lg bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-2 items-end">
              {/* Search Input */}
              <div className="space-y-2 w-full">
                <Input
                  id="search"
                  placeholder="Search by name, modality..."
                  value={formFilters.searchTerm}
                  onChange={(e) => setFormFilters(prev => ({ ...prev, searchTerm: e.target.value }))}
                  onKeyPress={handleKeyPress}
                  className="h-9"
                />
              </div>

              {/* Modality Filter */}
              <div className="space-y-2">
                <Select
                  value={formFilters.modality}
                  onValueChange={(value) => setFormFilters(prev => ({ ...prev, modality: value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All modalities" />
                  </SelectTrigger>
                  <SelectContent>
                    {INTERVIEW_MODALITIES.map((modality) => (
                      <SelectItem key={modality} value={modality}>
                        {modality.charAt(0).toUpperCase() + modality.slice(1)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              
              {/* Active Status Filter */}
              <div className="space-y-2">
                <Select
                  value={formFilters.active}
                  onValueChange={(value) => setFormFilters(prev => ({ ...prev, active: value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All statuses" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="true">Active</SelectItem>
                    <SelectItem value="false">Inactive</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Action Buttons */}
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

        {/* Data Table */}
        <div className="relative">
          {isLoading && (
            <div className="absolute inset-0 bg-background/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-md">
              <div className="text-muted-foreground">Loading interview configurations...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "name",
              placeholder: "Search configurations...",
              defaultHidden: ["instructionPromptName", "personalityPromptName", "questionsPromptName", "createdBy", "updatedAt", "updatedBy"],
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
