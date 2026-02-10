// Enterprise-grade API Client with Advanced Features
import {
  API_CONFIG,
  ApiError,
  ApiResponse,
  PaginatedApiResponse,
} from "./config";
// Removed interceptor - using unified auth directly
import { CacheStrategy, RequestPriority } from "./types";
import { CacheManager, CacheStrategies } from "./utils/cache";
import { RequestDeduplicator } from "./utils/deduplication";
import { MetricsCollector, consoleLogger } from "./utils/metrics";
import {
  DEFAULT_CIRCUIT_BREAKER_CONFIG,
  DEFAULT_RETRY_CONFIG,
  RetryManager,
} from "./utils/retry";

export interface EnhancedRequestOptions {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  headers?: Record<string, string>;
  body?: any;
  requireAuth?: boolean;
  cacheStrategy?: CacheStrategy;
  cacheTTL?: number;
  priority?: RequestPriority;
  timeout?: number;
  retryConfig?: any;
  enableDeduplication?: boolean;
  enableMetrics?: boolean;
  /**
   * Response type for the request
   * 'json' - Parse as JSON (default)
   * 'blob' - Return as Blob (for file downloads)
   * 'text' - Return as text
   * 'arrayBuffer' - Return as ArrayBuffer
   */
  responseType?: "json" | "blob" | "text" | "arrayBuffer";
  /**
   * Disable retry and circuit breaker for this request
   * Useful for operations where you want immediate failure (e.g., login, validation)
   * @default false
   */
  disableRetry?: boolean;
}

class ApiClient {
  private baseURL: string;
  private activeRequests = new Set<string>();

  constructor(baseURL: string) {
    this.baseURL = baseURL;

    // Initialize metrics logger if enabled
    if (API_CONFIG.enableLogging) {
      MetricsCollector.setLogger(consoleLogger);
    }
  }

  // Enterprise-grade request method with advanced features
  async request<T = any, Paginated = false>(
    url: string,
    options: EnhancedRequestOptions = {},
    azureToken?: string, // Optional - if not provided, interceptor will add it
  ) {
    const {
      method = "GET",
      headers = {},
      body,
      requireAuth = true,
      cacheStrategy = "no-cache",
      cacheTTL = API_CONFIG.cacheTimeout,
      priority = "normal",
      timeout = API_CONFIG.timeout,
      retryConfig = DEFAULT_RETRY_CONFIG,
      enableDeduplication = true,
      enableMetrics = API_CONFIG.enableMetrics,
      disableRetry = false,
    } = options;

    // Generate cache and deduplication keys
    const cacheKey = CacheManager.generateKey(url, method, body);
    const dedupeKey = RequestDeduplicator.generateKey(url, method, body);
    const requestId = `${method}:${url}:${Date.now()}`;

    // Generate the actual request function
    const makeRequest = async (): Promise<
      Paginated extends true ? PaginatedApiResponse<T> : ApiResponse<T>
    > => {
      // Build full URL
      const fullUrl = url.startsWith("http") ? url : `${this.baseURL}${url}`;

      // Get response type from options
      const responseType = options.responseType;

      // Prepare initial request options with security headers
      const requestOptions: RequestInit = {
        method,
        headers: {
          "Content-Type": "application/json",
          "X-Request-ID": requestId,
          "X-Requested-With": "XMLHttpRequest",
          ...headers,
        },
      };

      // Add body for non-GET requests
      if (body && method !== "GET") {
        if (body instanceof FormData) {
          delete (requestOptions.headers as Record<string, string>)[
            "Content-Type"
          ];
          requestOptions.body = body;
        } else {
          requestOptions.body = JSON.stringify(body);
        }
      }

      // Add auth token if required
      if (requireAuth && azureToken) {
        (requestOptions.headers as Record<string, string>)["Authorization"] =
          `Bearer ${azureToken}`;
      }

      // Start metrics collection
      if (enableMetrics) {
        MetricsCollector.startRequest(requestId, url, method);
      }

      try {
        // Create timeout controller
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), timeout);

        const response = await fetch(fullUrl, {
          ...requestOptions,
          signal: controller.signal,
        });

        clearTimeout(timeoutId);

        // Parse response based on responseType or content-type
        const contentType = response.headers.get("content-type");
        let data: any;

        // If responseType is explicitly set, use it
        if (responseType === "blob") {
          data = await response.blob();
        } else if (responseType === "arrayBuffer") {
          data = await response.arrayBuffer();
        } else if (responseType === "text") {
          data = await response.text();
        } else if (contentType?.includes("application/json")) {
          data = await response.json();
        } else if (
          contentType?.includes("application/pdf") ||
          contentType?.includes("application/octet-stream") ||
          contentType?.startsWith("image/") ||
          contentType?.startsWith("video/") ||
          contentType?.startsWith("audio/")
        ) {
          // Handle binary content types as blob
          data = await response.blob();
        } else {
          data = await response.text();
        }

        // End metrics collection
        if (enableMetrics) {
          MetricsCollector.endRequest(requestId, response.status);
        }

        // Handle HTTP errors
        if (!response.ok) {
          // Map backend error fields to frontend ApiError
          // Backend sends: errorCode, validationErrors, details, field
          // Frontend expects: code, errors (for ApiError constructor)
          const error = new ApiError(
            response.status,
            data?.message || `HTTP ${response.status}`,
            data?.validationErrors ||
              data?.errors ||
              (data?.details ? [data.details] : undefined),
            data?.errorCode || data?.code,
          );

          // Attach additional backend fields for detailed error handling
          (error as any).details = data?.details;
          (error as any).field = data?.field;
          (error as any).timestamp = data?.timestamp;

          if (enableMetrics) {
            MetricsCollector.endRequest(
              requestId,
              response.status,
              error.message,
            );
          }

          throw error;
        }

        return data;
      } catch (error) {
        if (enableMetrics) {
          MetricsCollector.endRequest(
            requestId,
            undefined,
            (error as Error).message,
          );
        }

        if (error instanceof ApiError) {
          throw error;
        }

        // Handle network errors
        if (error instanceof TypeError && error.message.includes("fetch")) {
          throw new ApiError(0, "Network connection failed");
        }

        if (error instanceof Error && error.name === "AbortError") {
          throw new ApiError(408, "Request timeout");
        }

        throw error;
      }
    };

    // Apply caching strategy
    const cachedRequest = async (): Promise<
      Paginated extends true ? PaginatedApiResponse<T> : ApiResponse<T>
    > => {
      switch (cacheStrategy) {
        case "no-cache":
          return CacheStrategies.noCache(makeRequest);

        case "cache-first":
          return CacheStrategies.cacheFirst(cacheKey, makeRequest, cacheTTL);

        case "network-first":
          return CacheStrategies.networkFirst(cacheKey, makeRequest, cacheTTL);

        case "cache-only":
          return CacheStrategies.cacheOnly(cacheKey);

        default:
          return makeRequest();
      }
    };

    // Apply deduplication if enabled
    const finalRequest = enableDeduplication
      ? () => RequestDeduplicator.deduplicate(dedupeKey, cachedRequest)
      : cachedRequest;

    // Apply retry logic with circuit breaker (unless disabled)
    if (disableRetry) {
      // Skip retry and circuit breaker - execute immediately
      return finalRequest();
    }

    return RetryManager.executeWithRetry(
      finalRequest,
      retryConfig,
      DEFAULT_CIRCUIT_BREAKER_CONFIG,
      url,
    );
  }

  // Enhanced convenience methods with intelligent defaults
  async get<T = any>(
    url: string,
    options?: Omit<EnhancedRequestOptions, "method">,
    azureToken?: string,
  ): Promise<ApiResponse<T>> {
    return this.request<T>(
      url,
      {
        method: "GET",
        cacheStrategy: "cache-first",
        ...options,
      },
      azureToken,
    );
  }

  async post<T = any>(
    url: string,
    body?: any,
    options?: Omit<EnhancedRequestOptions, "method" | "body">,
    azureToken?: string,
  ): Promise<ApiResponse<T>> {
    return this.request<T>(
      url,
      {
        method: "POST",
        body,
        cacheStrategy: "no-cache", // POST requests should not be cached
        ...options,
      },
      azureToken,
    );
  }

  async put<T = any>(
    url: string,
    body?: any,
    options?: Omit<EnhancedRequestOptions, "method" | "body">,
    azureToken?: string,
  ): Promise<ApiResponse<T>> {
    return this.request<T>(
      url,
      {
        method: "PUT",
        body,
        cacheStrategy: "no-cache", // PUT requests should not be cached
        ...options,
      },
      azureToken,
    );
  }

  async patch<T = any>(
    url: string,
    body?: any,
    options?: Omit<EnhancedRequestOptions, "method" | "body">,
    azureToken?: string,
  ): Promise<ApiResponse<T>> {
    return this.request<T>(
      url,
      {
        method: "PATCH",
        body,
        cacheStrategy: "no-cache", // PATCH requests should not be cached
        ...options,
      },
      azureToken,
    );
  }

  async delete<T = any>(
    url: string,
    options?: Omit<EnhancedRequestOptions, "method">,
    azureToken?: string,
  ): Promise<ApiResponse<T>> {
    return this.request<T>(
      url,
      {
        method: "DELETE",
        cacheStrategy: "no-cache", // DELETE requests should not be cached
        ...options,
      },
      azureToken,
    );
  }

  // Enhanced file upload method
  async upload<T = any>(
    url: string,
    file: File,
    options?: Omit<EnhancedRequestOptions, "method" | "body">,
    azureToken?: string,
  ): Promise<ApiResponse<T>> {
    const formData = new FormData();
    formData.append("file", file);

    return this.request<T>(
      url,
      {
        method: "POST",
        body: formData,
        requireAuth: true,
        cacheStrategy: "no-cache",
        timeout: 60000, // Longer timeout for file uploads
        ...options,
      },
      azureToken,
    );
  }

  // Utility methods
  getCacheStats() {
    return CacheManager.getStats();
  }

  clearCache() {
    CacheManager.clear();
  }

  getMetrics() {
    return MetricsCollector.getPerformanceStats();
  }

  getActiveRequests() {
    return this.activeRequests.size;
  }
}

// Create singleton instance
export const apiClient = new ApiClient(API_CONFIG.baseURL);
export const saasApiClient = new ApiClient(API_CONFIG.saasURL);

// Export for convenience
export default apiClient;
