// Request Deduplication to prevent duplicate API calls

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

interface PendingRequest {
  promise: Promise<any>;
  timestamp: number;
}

export class RequestDeduplicator {
  private static pendingRequests = new Map<string, PendingRequest>();
  private static readonly REQUEST_TIMEOUT = 30000; // 30 seconds

  static async deduplicate<T>(
    key: string,
    requestFn: () => Promise<T>
  ): Promise<T> {
    // Clean up expired requests
    this.cleanupExpiredRequests();

    // Check if request is already pending
    const existingRequest = this.pendingRequests.get(key);
    if (existingRequest) {
      return existingRequest.promise;
    }

    // Create new request
    const promise = requestFn().finally(() => {
      // Clean up after request completes
      this.pendingRequests.delete(key);
    });

    // Store the pending request
    this.pendingRequests.set(key, {
      promise,
      timestamp: Date.now()
    });

    return promise;
  }

  static generateKey(url: string, method: HttpMethod, body?: any): string {
    const bodyHash = body ? this.hashString(JSON.stringify(body)) : '';
    return `${method}:${url}:${bodyHash}`;
  }

  static clear(): void {
    this.pendingRequests.clear();
  }

  static getPendingCount(): number {
    return this.pendingRequests.size;
  }

  static getPendingKeys(): string[] {
    return Array.from(this.pendingRequests.keys());
  }

  private static cleanupExpiredRequests(): void {
    const now = Date.now();
    for (const [key, request] of this.pendingRequests.entries()) {
      if (now - request.timestamp > this.REQUEST_TIMEOUT) {
        this.pendingRequests.delete(key);
      }
    }
  }

  private static hashString(str: string): string {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return hash.toString();
  }
}
