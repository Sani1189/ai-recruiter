"use client";

import { useState } from "react";
import { Search, Filter, X } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { Prompt, PROMPT_CATEGORIES, COMMON_LOCALES } from "@/types/prompt";
import { useApi } from "@/hooks/useApi";
import { ApiResponse, PaginatedResponse } from "@/lib/api/config";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";

import { createColumns } from "./columns";

// API fetch function for useServerTable (will be defined inside component)

interface PromptsTableProps {
  initialData?: Prompt[];
}

export default function PromptsTable({ initialData = [] }: PromptsTableProps) {
  const api = useApi();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [formFilters, setFormFilters] = useState({
    searchTerm: '',
    category: '',
    locale: '',
  });
  
  // Create API fetch function for useServerTable
  const fetchPrompts = async (params: ServerTableParams): Promise<PaginatedResponse<Prompt>> => {
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
    if (params.filters?.category) queryParams.set('Category', params.filters.category);
    if (params.filters?.locale) queryParams.set('Locale', params.filters.locale);
    
    
    const response = await api.get(`/Prompt/filtered?${queryParams.toString()}`);
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
  } = useServerTable(fetchPrompts, {
    initialPageSize: 10,
    initialFilters: { 
      searchTerm: '', 
      category: '', 
      locale: '' 
    },
    initialSortBy: 'createdAt',
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
      category: '',
      locale: '',
    });
    
    // Reset server filters
    handleFilterChange({
      searchTerm: '',
      category: '',
      locale: '',
    });
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const handleDelete = async (prompt: Prompt) => {
    try {
      await api.delete(`/Prompt/${prompt.name}/${prompt.version}`);
      
      // Success - refresh data
      fetchData();
      
      // Optional: Add toast notification for success
      // toast.success('Prompt deleted successfully');
    } catch (error: any) {
      // Handle specific error cases
      if (error?.status === 404) {
        console.error('Prompt not found');
        // Optional: toast.error('Prompt not found');
      } else if (error?.status === 400) {
        console.error('Failed to delete prompt:', error.message);
        // Optional: toast.error(error.message || 'Failed to delete prompt');
      } else {
        console.error('Failed to delete prompt:', error);
        // Optional: toast.error('An unexpected error occurred');
      }
      
      // Still refresh to sync with server state
      fetchData();
    }
  };

  const handleDuplicate = async (prompt: Prompt, newName: string) => {
    const response = await api.post(`/Prompt/${prompt.name}/${prompt.version}/duplicate`, { newName });
    const duplicated = (response as any)?.data ?? response;
    fetchData();
    return duplicated as Prompt;
  };

  // Create columns with delete handler
  const columns = createColumns({ onDelete: handleDelete, onDuplicate: handleDuplicate });

  // useServerTable handles initial data loading automatically

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle>Prompts</CardTitle>
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
                        placeholder="Search by name, content, or tags..."
                        value={formFilters.searchTerm}
                        onChange={(e) => setFormFilters(prev => ({ ...prev, searchTerm: e.target.value }))}
                        onKeyPress={handleKeyPress}
                        className="h-9"
                      />
                    </div>

                    {/* Category Filter */}
                    <div className="space-y-2">
                      <Select
                        value={formFilters.category}
                        onValueChange={(value) => setFormFilters(prev => ({ ...prev, category: value }))}
                      >
                        <SelectTrigger className="h-9 w-full">
                          <SelectValue placeholder="All categories" />
                        </SelectTrigger>
                        <SelectContent>
                          {PROMPT_CATEGORIES.map((category) => (
                            <SelectItem key={category} value={category}>
                              {category.charAt(0).toUpperCase() + category.slice(1)}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    
                    {/* Locale Filter */}
                    <div className="space-y-2">
                      <Select
                        value={formFilters.locale}
                        onValueChange={(value) => setFormFilters(prev => ({ ...prev, locale: value }))}
                      >
                        <SelectTrigger className="h-9 w-full">
                          <SelectValue placeholder="All locales" />
                        </SelectTrigger>
                        <SelectContent>
                          {COMMON_LOCALES.map((locale) => (
                            <SelectItem key={locale} value={locale}>
                              {locale}
                            </SelectItem>
                          ))}
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
              <div className="text-muted-foreground">Loading prompts...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "name",
              placeholder: "Search prompts...",
              defaultHidden: ["createdBy", "content"],
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
