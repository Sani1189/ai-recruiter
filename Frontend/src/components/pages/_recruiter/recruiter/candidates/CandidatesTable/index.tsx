"use client";

import { Users } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { CandidateView } from "@/types/v2/type.view";
import { columns } from "./columns";
import { useApi } from "@/hooks/useApi";
import { PaginatedResponse } from "@/lib/api/config";
import { useServerTable, ServerTableParams } from "@/hooks/useServerTable";

const normalizeFixedKey = (label: string) =>
  label
    .toLowerCase()
    .split(" ")
    .filter(Boolean)
    .map((w, i) => (i === 0 ? w : w[0].toUpperCase() + w.slice(1)))
    .join("");

export default function CandidatesTable() {
  const api = useApi();

  // Create API fetch function for useServerTable
  const fetchCandidates = async (params: ServerTableParams): Promise<PaginatedResponse<CandidateView>> => {
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
    
    const response = await api.get(`/candidate/filtered?${queryParams.toString()}`);
    const data = response.data || response;
    
    // Transform API data to match CandidateView format
    const transformedCandidates: CandidateView[] = (data.items || []).map((candidate: any) => {
      const scorings: any[] = candidate.scorings || [];

      const grouped = new Map<string, { total: number; count: number }>();
      const numericScores: number[] = [];
      scorings.forEach((s) => {
        const val = typeof s.score === "number" ? s.score : null;
        if (val != null) numericScores.push(val);
        const label = (s.fixedCategory || s.category || "Other").toString().trim();
        if (val == null) return;
        const current = grouped.get(label) || { total: 0, count: 0 };
        current.total += val;
        current.count += 1;
        grouped.set(label, current);
      });

      const fixedScores: Record<string, number> = {};
      Array.from(grouped.entries()).forEach(([label, agg]) => {
        fixedScores[normalizeFixedKey(label)] = Math.round(agg.total / agg.count);
      });

      const avg =
        numericScores.length > 0
          ? Math.round(numericScores.reduce((a, b) => a + b, 0) / numericScores.length)
          : 0;

      return {
        id: candidate.id,
        userProfile: candidate.userProfile || {
          id: candidate.userId,
          name: "Unknown",
          email: "",
          phoneNumber: null,
          age: null,
          nationality: null,
          resumeUrl: null,
        },
        score: {
          avg,
          english: 0,
          technical: 0,
          communication: 0,
          problemSolving: 0,
        },
        // attach aggregated fixed scores for table rendering
        fixedScores,
      } as CandidateView & { fixedScores: Record<string, number> };
    });
    
    // Transform API response to match PaginatedResponse format
    return {
      success: true,
      data: transformedCandidates,
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
    params,
  } = useServerTable(fetchCandidates, {
    initialPageSize: 10,
    initialFilters: { searchTerm: '' },
    initialSortBy: 'createdAt',
    initialSortOrder: 'desc',
  });

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="flex items-center">
          <Users className="text-brand mr-2 h-5 w-5" />
          All Candidates
        </CardTitle>
      </CardHeader>

      <CardContent>
        <div className="relative">
          {isLoading && (
            <div className="absolute inset-0 bg-background/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-md">
              <div className="text-muted-foreground">Loading candidates...</div>
            </div>
          )}
          <DataTable
            columns={columns}
            {...tableProps}
            metadata={{
              searchField: "name",
              placeholder: "Search by Candidate Name",
              defaultHidden: [
                "backendDevelopment",
                "frontendDevelopment",
                "cloudDevelopment",
                "general",
                "other",
              ],
              currentSearchValue: params.filters?.searchTerm || "",
              onSearchChange: (searchValue) => {
                onFilterChange({ ...params.filters, searchTerm: searchValue });
              },
            }}
          />
        </div>
      </CardContent>
    </Card>
  );
}
