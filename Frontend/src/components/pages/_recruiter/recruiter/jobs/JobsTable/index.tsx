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

import { useApi } from "@/hooks/useApi";
import { PaginatedResponse } from "@/lib/api/config";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";

import { createColumns, Job, PublishJobDialog } from "./columns";

// Experience levels matching backend
const EXPERIENCE_LEVELS = ["Entry", "Mid", "Senior", "Lead", "Executive"];

// Job types matching backend
const JOB_TYPES = ["FullTime", "PartTime", "Contract", "Internship"];

interface JobsTableProps {
  initialData?: Job[];
}

export default function JobsTable({ initialData = [] }: JobsTableProps) {
  const api = useApi();
  const [publishJob, setPublishJob] = useState<Job | null>(null);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [formFilters, setFormFilters] = useState({
    searchTerm: '',
    experienceLevel: '',
    jobType: '',
    policeReportRequired: '',
  });
  
  // Create API fetch function for useServerTable
  const fetchJobs = async (params: ServerTableParams): Promise<PaginatedResponse<Job>> => {
    const queryParams = new URLSearchParams();
    
    // Add pagination
    queryParams.set('Page', params.page.toString());
    queryParams.set('PageSize', params.pageSize.toString());
    
    // Add sorting
    if (params.sortBy) {
      queryParams.set('SortBy', params.sortBy);
      queryParams.set('SortDescending', params.sortOrder === 'desc' ? 'true' : 'false');
    }
    
    // Add filters
    if (params.filters?.searchTerm) queryParams.set('SearchTerm', params.filters.searchTerm);
    if (params.filters?.experienceLevel) queryParams.set('ExperienceLevel', params.filters.experienceLevel);
    if (params.filters?.jobType) queryParams.set('JobType', params.filters.jobType);
    if (params.filters?.policeReportRequired) queryParams.set('PoliceReportRequired', params.filters.policeReportRequired);
    
    const response = await api.get(`/job/filtered/with-candidate-counts?${queryParams.toString()}`);
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
  } = useServerTable(fetchJobs, {
    initialPageSize: 10,
    initialFilters: { 
      searchTerm: '', 
      experienceLevel: '', 
      jobType: '',
      policeReportRequired: '',
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
      experienceLevel: '',
      jobType: '',
      policeReportRequired: '',
    });
    
    // Reset server filters
    handleFilterChange({
      searchTerm: '',
      experienceLevel: '',
      jobType: '',
      policeReportRequired: '',
    });
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  const handleDelete = async (job: Job) => {
    try {
      await api.delete(`/job/${job.name}/${job.version}`);
      
      // Success - refresh data
      fetchData();
      
      // Optional: Add toast notification for success
      // toast.success('Job deleted successfully');
    } catch (error: any) {
      // Handle specific error cases
      if (error?.status === 404) {
        console.error('Job not found');
        // Optional: toast.error('Job not found');
      } else if (error?.status === 400) {
        console.error('Failed to delete job:', error.message);
        // Optional: toast.error(error.message || 'Failed to delete job');
      } else {
        console.error('Failed to delete job:', error);
        // Optional: toast.error('An unexpected error occurred');
      }
      
      // Still refresh to sync with server state
      fetchData();
    }
  };

  const handleDuplicate = async (job: Job, newName: string, newJobTitle: string) => {
    const response = await api.post(`/job/${job.name}/${job.version}/duplicate`, {
      newName,
      newJobTitle,
    });

    // apiClient returns data directly, not wrapped in .data (but some callers still check .data)
    const duplicated = (response as any)?.data ?? response;

    // Keep table in sync (even if caller navigates away)
    fetchData();

    return duplicated as Job;
  };

  const handlePublish = async (job: Job, countryExposureCountryCodes: string[]) => {
    const raw = await api.get(`/job/${job.name}/${job.version}`);
    const full = (raw as any)?.data ?? raw;
    const steps = (full.assignedSteps ?? []).map(
      (s: { stepNumber: number; stepDetails: { name: string; version: number } }) => ({
        stepNumber: s.stepNumber,
        existingStepName: s.stepDetails?.name,
        existingStepVersion: s.stepDetails?.version,
      })
    );
    const payload = {
      name: full.name,
      version: full.version,
      jobTitle: full.jobTitle,
      jobType: full.jobType,
      experienceLevel: full.experienceLevel,
      jobDescription: full.jobDescription,
      maxAmountOfCandidatesRestriction: full.maxAmountOfCandidatesRestriction,
      minimumRequirements: full.minimumRequirements ?? [],
      policeReportRequired: full.policeReportRequired ?? false,
      status: "Published",
      originCountryCode: full.originCountryCode ?? null,
      countryExposureCountryCodes,
      steps,
      shouldUpdateVersion: false,
    };
    await api.put(`/job/${job.name}/${job.version}`, payload);
    fetchData();
  };

  const columns = createColumns({
    onDelete: handleDelete,
    onDuplicate: handleDuplicate,
    onOpenPublish: (job) => setPublishJob(job),
  });

  return (
    <>
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle>Job Postings</CardTitle>
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
            <div className="grid grid-cols-1 md:grid-cols-5 gap-2 items-end">
              {/* Search Input */}
              <div className="space-y-2 w-full">
                <Input
                  id="search"
                  placeholder="Search by name, title, description..."
                  value={formFilters.searchTerm}
                  onChange={(e) => setFormFilters(prev => ({ ...prev, searchTerm: e.target.value }))}
                  onKeyPress={handleKeyPress}
                  className="h-9"
                />
              </div>

              {/* Experience Level Filter */}
              <div className="space-y-2">
                <Select
                  value={formFilters.experienceLevel || undefined}
                  onValueChange={(value) => setFormFilters(prev => ({ ...prev, experienceLevel: value === 'all' ? '' : value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All levels" />
                  </SelectTrigger>
                  <SelectContent>
                    {EXPERIENCE_LEVELS.map((level) => (
                      <SelectItem key={level} value={level}>
                        {level}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              
              {/* Job Type Filter */}
              <div className="space-y-2">
                <Select
                  value={formFilters.jobType || undefined}
                  onValueChange={(value) => setFormFilters(prev => ({ ...prev, jobType: value === 'all' ? '' : value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All types" />
                  </SelectTrigger>
                  <SelectContent>
                    {JOB_TYPES.map((type) => (
                      <SelectItem key={type} value={type}>
                        {type}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Police Report Filter */}
              <div className="space-y-2">
                <Select
                  value={formFilters.policeReportRequired || undefined}
                  onValueChange={(value) => setFormFilters(prev => ({ ...prev, policeReportRequired: value === 'all' ? '' : value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="Police report" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="true">Required</SelectItem>
                    <SelectItem value="false">Not Required</SelectItem>
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
              <div className="text-muted-foreground">Loading jobs...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "jobTitle",
              placeholder: "Search jobs...",
              defaultHidden: ["name"],
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
    {publishJob && (
      <PublishJobDialog
        job={publishJob}
        open={!!publishJob}
        onOpenChange={(open) => !open && setPublishJob(null)}
        onPublish={handlePublish}
      />
    )}
    </>
  );
}

