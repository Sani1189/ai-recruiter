"use client";

import { useCallback } from "react";

import {
  apiClient,
  EnhancedRequestOptions,
  saasApiClient,
} from "@/lib/api/client";
import { ApiListParams } from "@/lib/api/types";
import { useUnifiedAuth } from "./useUnifiedAuth";

export function useApi(forSaaS: boolean = false) {
  const { getAccessToken } = useUnifiedAuth();

  const request = useCallback(
    async <T = any, Paginated = false>(
      url: string,
      options: EnhancedRequestOptions = {},
    ) => {
      const { requireAuth = true, ...restOptions } = options;

      // Get token if authentication is required
      let token: string | null = null;
      if (requireAuth) {
        try {
          token = await getAccessToken();
          if (!token) {
            throw new Error("No access token available. Please log in.");
          }
        } catch (error) {
          throw new Error(
            `Authentication failed: ${error instanceof Error ? error.message : "Unknown error"}`,
          );
        }
      }

      if (forSaaS) {
        return saasApiClient.request<T, Paginated>(
          url,
          restOptions,
          token || undefined,
        );
      }

      // Use the existing apiClient with the token
      return apiClient.request<T, Paginated>(
        url,
        restOptions,
        token || undefined,
      );
    },
    [getAccessToken, forSaaS],
  );

  // Convenience methods that use the existing apiClient methods
  const get = useCallback(
    <T = any>(url: string, options?: Omit<EnhancedRequestOptions, "method">) =>
      request<T>(url, { ...options, method: "GET" }),
    [request],
  );

  const post = useCallback(
    <T = any>(
      url: string,
      body?: any,
      options?: Omit<EnhancedRequestOptions, "method" | "body">,
    ) => request<T>(url, { ...options, method: "POST", body }),
    [request],
  );

  const put = useCallback(
    <T = any>(
      url: string,
      body?: any,
      options?: Omit<EnhancedRequestOptions, "method" | "body">,
    ) => request<T>(url, { ...options, method: "PUT", body }),
    [request],
  );

  const patch = useCallback(
    <T = any>(
      url: string,
      body?: any,
      options?: Omit<EnhancedRequestOptions, "method" | "body">,
    ) => request<T>(url, { ...options, method: "PATCH", body }),
    [request],
  );

  const del = useCallback(
    <T = any>(url: string, options?: Omit<EnhancedRequestOptions, "method">) =>
      request<T>(url, { ...options, method: "DELETE" }),
    [request],
  );

  // Enhanced methods for pagination and filtering
  const getList = useCallback(
    <T = any>(
      url: string,
      params: ApiListParams,
      options?: Omit<EnhancedRequestOptions, "method">,
    ) => {
      const queryParams = new URLSearchParams();

      // Add pagination
      queryParams.set("Page", params.page.toString());
      queryParams.set("PageSize", params.pageSize.toString());

      // Add search
      if (params.search) {
        queryParams.set("SearchTerm", params.search);
      }

      // Add filters
      if (params.filters) {
        Object.entries(params.filters).forEach(([key, value]) => {
          if (value !== null && value !== undefined && value !== "") {
            queryParams.set(key, value.toString());
          }
        });
      }

      // Add sorting
      if (params.sortBy) {
        queryParams.set("SortBy", params.sortBy);
        queryParams.set(
          "SortDescending",
          params.sortOrder === "desc" ? "true" : "false",
        );
      }

      const fullUrl = `${url}?${queryParams.toString()}`;
      return request<T, true>(fullUrl, { ...options, method: "GET" });
    },
    [request],
  );

  // Helper to build query string from params
  const buildQueryString = useCallback((params: Partial<ApiListParams>) => {
    const queryParams = new URLSearchParams();

    if (params.page) queryParams.set("page", params.page.toString());
    if (params.pageSize)
      queryParams.set("pageSize", params.pageSize.toString());
    if (params.search) queryParams.set("search", params.search);
    if (params.sortBy) {
      queryParams.set("sortBy", params.sortBy);
      queryParams.set("sortOrder", params.sortOrder || "asc");
    }

    if (params.filters) {
      Object.entries(params.filters).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== "") {
          queryParams.set(key, value.toString());
        }
      });
    }

    return queryParams.toString();
  }, []);

  return {
    request,
    get,
    post,
    put,
    patch,
    delete: del,
    getList,
    buildQueryString,
  };
}
