/**
 * useServerTable Hook
 * 
 * A flexible hook for server-side pagination, filtering, and sorting
 * that works with your existing DataTable components.
 * 
 * Features:
 * - Server-side pagination
 * - Server-side filtering
 * - Server-side sorting
 * - Automatic data fetching when parameters change
 * - Integration with TanStack Table
 * - Works with DataTable-v2 component
 */

import { useState, useEffect, useCallback } from 'react';
import { PaginatedResponse } from '@/lib/api/config';

export interface ServerTableParams {
  page: number;
  pageSize: number;
  filters?: Record<string, any>;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface ServerTablePagination {
  pageIndex: number;  // 0-based for TanStack Table
  pageSize: number;
  pageCount: number;
  rowCount: number;
}

export interface ServerTableOptions {
  initialPageSize?: number;
  initialFilters?: Record<string, any>;
  initialSortBy?: string;
  initialSortOrder?: 'asc' | 'desc';
  autoFetch?: boolean;
  onSuccess?: (data: any) => void;
  onError?: (error: Error) => void;
}

/**
 * Hook for server-side table operations with pagination, filtering, and sorting
 * 
 * @param fetchFunction Function that fetches data from the server
 * @param options Configuration options
 * @returns Table state and control functions
 */
export function useServerTable<T>(
  fetchFunction: (params: ServerTableParams) => Promise<PaginatedResponse<T>>,
  options: ServerTableOptions = {}
) {
  // Extract options with defaults
  const {
    initialPageSize = 25,
    initialFilters = {},
    initialSortBy,
    initialSortOrder = 'asc',
    autoFetch = true,
    onSuccess,
    onError,
  } = options;

  // Default parameters
  const defaultParams: ServerTableParams = {
    page: 1,
    pageSize: initialPageSize,
    filters: initialFilters,
    sortBy: initialSortBy,
    sortOrder: initialSortOrder,
  };

  // State
  const [data, setData] = useState<T[]>([]);
  const [params, setParams] = useState<ServerTableParams>(defaultParams);
  const [pagination, setPagination] = useState<ServerTablePagination>({
    pageIndex: 0,
    pageSize: defaultParams.pageSize,
    pageCount: 0,
    rowCount: 0
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetch data from the server
   * 
   * @param newParams Optional new parameters to merge with existing parameters
   * @returns Promise with the response
   */
  const fetchData = useCallback(async (newParams?: Partial<ServerTableParams>) => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Merge new params with existing params
      const mergedParams = newParams 
        ? { ...params, ...newParams }
        : params;
      
      // Update params state
      setParams(mergedParams);
      
      // Call the fetch function
      const response = await fetchFunction(mergedParams);
      
      // Update data and pagination
      setData(response.data);
      setPagination({
        pageIndex: response.pagination.page - 1, // Convert to 0-based for TanStack Table
        pageSize: response.pagination.pageSize,
        pageCount: response.pagination.totalPages,
        rowCount: response.pagination.totalItems
      });
      
      onSuccess?.(response);
      return response;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch data';
      setError(message);
      onError?.(err as Error);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, [fetchFunction, params, onSuccess, onError]);

  // Initial fetch
  useEffect(() => {
    if (autoFetch) {
      fetchData();
    }
  }, []); // Empty dependency array means this runs once on mount

  /**
   * Handle page change
   * 
   * @param pageIndex 0-based page index
   */
  const onPageChange = useCallback((pageIndex: number) => {
    fetchData({ page: pageIndex + 1 }); // Convert from 0-based to 1-based
  }, [fetchData]);

  /**
   * Handle page size change
   * 
   * @param pageSize New page size
   */
  const onPageSizeChange = useCallback((pageSize: number) => {
    fetchData({ page: 1, pageSize }); // Reset to first page when changing page size
  }, [fetchData]);

  /**
   * Handle filter change
   * 
   * @param filters New filters
   */
  const onFilterChange = useCallback((filters: Record<string, any>) => {
    fetchData({ page: 1, filters }); // Reset to first page when changing filters
  }, [fetchData]);

  /**
   * Handle sort change
   * 
   * @param sortBy Column to sort by
   * @param sortOrder Sort direction
   */
  const onSortChange = useCallback((sortBy: string, sortOrder: 'asc' | 'desc') => {
    fetchData({ sortBy, sortOrder });
  }, [fetchData]);

  /**
   * Reset all parameters to defaults
   */
  const resetParams = useCallback(() => {
    fetchData(defaultParams);
  }, [fetchData, defaultParams]);

  /**
   * Props for TanStack Table integration
   */
  const tableProps = {
    data,
    pageCount: pagination.pageCount,
    manualPagination: true,
    manualFiltering: true,
    manualSorting: true,
    state: {
      pagination: {
        pageIndex: pagination.pageIndex,
        pageSize: pagination.pageSize,
      },
      sorting: params.sortBy ? [{ id: params.sortBy, desc: params.sortOrder === 'desc' }] : [],
    },
    onPaginationChange: (updater: any) => {
      const newPagination = typeof updater === 'function'
        ? updater({ pageIndex: pagination.pageIndex, pageSize: pagination.pageSize })
        : updater;
        
      if (newPagination.pageIndex !== pagination.pageIndex) {
        onPageChange(newPagination.pageIndex);
      }
      
      if (newPagination.pageSize !== pagination.pageSize) {
        onPageSizeChange(newPagination.pageSize);
      }
    },
    onSortingChange: (updater: any) => {
      const currentSorting = params.sortBy ? [{ id: params.sortBy, desc: params.sortOrder === 'desc' }] : [];
      const newSorting = typeof updater === 'function' ? updater(currentSorting) : updater;
      
      if (newSorting.length > 0) {
        onSortChange(newSorting[0].id, newSorting[0].desc ? 'desc' : 'asc');
      }
    },
    getRowCount: () => pagination.rowCount,
    getPageCount: () => pagination.pageCount,
  };

  return {
    // Data
    data,
    isLoading,
    error,
    params,
    pagination,
    
    // Actions
    fetchData,
    onPageChange,
    onPageSizeChange,
    onFilterChange,
    onSortChange,
    resetParams,
    
    // TanStack Table integration
    tableProps
  };
}
