"use client";

import { useState } from "react";
import { Search, Filter, X } from "lucide-react";
import { toast } from "sonner";

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
import { Checkbox } from "@/components/ui/checkbox";

import { useApi } from "@/hooks/useApi";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";
import { PaginatedResponse } from "@/lib/api/config";
import { AssessmentTemplate, ASSESSMENT_TEMPLATE_TYPES } from "@/types/assessmentTemplate";

import { createColumns } from "./columns";
import ImportFromExcelDialog from "./ImportFromExcelDialog";

export default function AssessmentTemplatesTable() {
  const api = useApi();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const TEMPLATE_STATUSES = ["Draft", "Published", "Archived"] as const;
  const [formFilters, setFormFilters] = useState({
    searchTerm: "",
    templateType: "",
    status: "",
    showDeletedOnly: false,
  });

  const fetchTemplates = async (
    params: ServerTableParams,
  ): Promise<PaginatedResponse<AssessmentTemplate>> => {
    const queryParams = new URLSearchParams();
    queryParams.set("PageNumber", params.page.toString());
    queryParams.set("PageSize", params.pageSize.toString());

    if (params.sortBy) {
      queryParams.set("SortBy", params.sortBy);
      queryParams.set("SortDescending", params.sortOrder === "desc" ? "true" : "false");
    }

    if (params.filters?.searchTerm) queryParams.set("SearchTerm", params.filters.searchTerm);
    if (params.filters?.templateType) queryParams.set("TemplateType", params.filters.templateType);
    if (params.filters?.status) queryParams.set("Status", params.filters.status);
    if (params.filters?.showDeletedOnly) {
      queryParams.set("IncludeDeleted", "true");
      queryParams.set("OnlyDeleted", "true");
    }

    const response = await api.get(`/QuestionnaireTemplate/filtered?${queryParams.toString()}`);
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

  const { data, isLoading, tableProps, onFilterChange, fetchData, params } = useServerTable(fetchTemplates, {
    initialPageSize: 10,
    initialFilters: { searchTerm: "", templateType: "", status: "", showDeletedOnly: false },
    initialSortBy: "updatedAt",
    initialSortOrder: "desc",
  });

  const handleSearch = () => onFilterChange(formFilters);
  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") handleSearch();
  };

  const handleClearFilters = () => {
    setFormFilters({ searchTerm: "", templateType: "", status: "", showDeletedOnly: false });
    onFilterChange({ searchTerm: "", templateType: "", status: "", showDeletedOnly: false });
  };

  const handleDuplicate = async (
    t: AssessmentTemplate,
    newName: string,
    includeQuestions: boolean
  ): Promise<AssessmentTemplate> => {
    const response = await api.post(`/QuestionnaireTemplate/${t.name}/${t.version}/duplicate`, {
      newName: newName.trim(),
      includeQuestions,
      includeTitle: true,
      includeDescription: true,
    });
    const duplicated = response.data || response;
    fetchData();
    return duplicated;
  };

  const columns = createColumns({
    onDelete: async (t) => {
      const response = await api.delete(`/QuestionnaireTemplate/${t.name}/${t.version}`);
      const data = (response as any)?.data ?? response;
      const mode = String(data?.mode ?? "").toLowerCase();
      if (mode === "archived") {
        toast.success(`Template "${t.name}" v${t.version} archived (in use)`);
      } else {
        toast.success(`Template "${t.name}" v${t.version} deleted`);
      }
      fetchData();
    },
    onRestore: async (t) => {
      try {
        await api.post(`/QuestionnaireTemplate/${encodeURIComponent(t.name)}/${t.version}/restore`);
        toast.success(`Template "${t.name}" v${t.version} restored`);
        fetchData();
      } catch (err: any) {
        const message =
          err?.response?.data?.message ||
          err?.response?.data?.detail ||
          err?.message ||
          "Failed to restore template";
        toast.error(message);
      }
    },
    onDuplicate: handleDuplicate,
  });

  return (
    <Card className="shadow-card">
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle>Questionnaire Templates</CardTitle>
        <ImportFromExcelDialog onImported={fetchData} />
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Filter Toggle */}
        <div className="flex items-center justify-end">
          <Button variant="outline" size="sm" onClick={() => setIsFiltersOpen(!isFiltersOpen)}>
            <Filter className="mr-2 h-4 w-4" />
            {isFiltersOpen ? "Hide Filters" : "Show Filters"}
          </Button>
        </div>

        {/* Collapsible Filters */}
        {isFiltersOpen && (
          <div className="p-4 border rounded-lg bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-5 gap-2 items-end">
              <div className="space-y-2 w-full">
                <Input
                  id="search"
                  placeholder="Search by name, title..."
                  value={formFilters.searchTerm}
                  onChange={(e) => setFormFilters((p) => ({ ...p, searchTerm: e.target.value }))}
                  onKeyPress={handleKeyPress}
                  className="h-9"
                />
              </div>

              <div className="space-y-2">
                <Select
                  value={formFilters.templateType}
                  onValueChange={(value) => setFormFilters((p) => ({ ...p, templateType: value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All template types" />
                  </SelectTrigger>
                  <SelectContent>
                    {ASSESSMENT_TEMPLATE_TYPES.map((t) => (
                      <SelectItem key={t} value={t}>
                        {t}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Select
                  value={formFilters.status}
                  onValueChange={(value) => setFormFilters((p) => ({ ...p, status: value }))}
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All statuses" />
                  </SelectTrigger>
                  <SelectContent>
                    {TEMPLATE_STATUSES.map((s) => (
                      <SelectItem key={s} value={s}>
                        {s}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="flex items-center gap-2 h-9">
                <Checkbox
                  id="showDeletedOnly"
                  checked={formFilters.showDeletedOnly}
                  onCheckedChange={(checked) =>
                    setFormFilters((p) => ({ ...p, showDeletedOnly: checked === true }))
                  }
                />
                <label
                  htmlFor="showDeletedOnly"
                  className="text-sm text-muted-foreground cursor-pointer select-none"
                >
                  Show deleted
                </label>
              </div>

              <div className="flex gap-2">
                <Button onClick={handleSearch} disabled={isLoading} className="h-9 flex-1">
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
              <div className="text-muted-foreground">Loading templates...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            data={data}
            metadata={{
              searchField: "name",
              placeholder: "Search templates...",
              defaultHidden: ["updatedAt", "updatedBy"],
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


