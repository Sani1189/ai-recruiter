// Exponential Backoff Retry Logic with Circuit Breaker
import { ApiError } from '../config';
import { RetryConfig, CircuitBreakerConfig } from '../types';

export class RetryManager {
  private static circuitBreakerState: Map<string, {
    failures: number;
    lastFailureTime: number;
    state: 'CLOSED' | 'OPEN' | 'HALF_OPEN';
  }> = new Map();

  static async executeWithRetry<T>(
    operation: () => Promise<T>,
    retryConfig: RetryConfig,
    circuitBreakerConfig: CircuitBreakerConfig,
    operationKey: string = 'default'
  ): Promise<T> {
    let lastError: Error;
    
    for (let attempt = 0; attempt <= retryConfig.maxAttempts; attempt++) {
      try {
        // Check circuit breaker
        if (!this.isCircuitBreakerOpen(operationKey, circuitBreakerConfig)) {
          const result = await operation();
          this.onSuccess(operationKey);
          return result;
        } else {
          throw new Error('Circuit breaker is open');
        }
      } catch (error) {
        lastError = error as Error;
        
        // Check if error is retryable
        const isRetryable = retryConfig.retryCondition(error as ApiError);
        
        // Only count retryable errors toward circuit breaker failures
        // Client errors (400, 409, 422) should not open the circuit breaker
        if (isRetryable) {
          this.onFailure(operationKey, circuitBreakerConfig);
        } else {
          // For non-retryable errors (client errors), reset the failure count
          // since they indicate a problem with the request, not the server
          this.onSuccess(operationKey);
        }
        
        // Don't retry if it's the last attempt or error is not retryable
        if (attempt === retryConfig.maxAttempts || !isRetryable) {
          throw lastError;
        }
        
        // Calculate delay with exponential backoff
        const delay = Math.min(
          retryConfig.baseDelay * Math.pow(retryConfig.backoffFactor, attempt),
          retryConfig.maxDelay
        );
        
        // Add jitter to prevent thundering herd
        const jitter = Math.random() * 0.1 * delay;
        await this.delay(delay + jitter);
      }
    }
    
    throw lastError!;
  }

  private static isCircuitBreakerOpen(operationKey: string, config: CircuitBreakerConfig): boolean {
    const state = this.circuitBreakerState.get(operationKey);
    
    if (!state) {
      return false; // Closed by default
    }
    
    const now = Date.now();
    
    if (state.state === 'OPEN') {
      if (now - state.lastFailureTime > config.resetTimeout) {
        state.state = 'HALF_OPEN';
        return false; // Allow one request to test
      }
      return true; // Still open
    }
    
    return false; // Closed or half-open
  }

  private static onSuccess(operationKey: string): void {
    const state = this.circuitBreakerState.get(operationKey);
    if (state) {
      state.failures = 0;
      state.state = 'CLOSED';
    }
  }

  private static onFailure(operationKey: string, config: CircuitBreakerConfig): void {
    const state = this.circuitBreakerState.get(operationKey) || {
      failures: 0,
      lastFailureTime: 0,
      state: 'CLOSED' as const
    };
    
    state.failures++;
    state.lastFailureTime = Date.now();
    
    if (state.failures >= config.failureThreshold) {
      state.state = 'OPEN';
    }
    
    this.circuitBreakerState.set(operationKey, state);
  }

  private static delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  // Reset circuit breaker for a specific operation
  static resetCircuitBreaker(operationKey: string = 'default'): void {
    this.circuitBreakerState.delete(operationKey);
  }

  // Reset all circuit breakers
  static resetAllCircuitBreakers(): void {
    this.circuitBreakerState.clear();
  }

  // Get circuit breaker status
  static getCircuitBreakerStatus(operationKey: string = 'default'): {
    state: 'CLOSED' | 'OPEN' | 'HALF_OPEN';
    failures: number;
    lastFailureTime: number;
  } | null {
    const state = this.circuitBreakerState.get(operationKey);
    if (!state) return null;
    
    return {
      state: state.state,
      failures: state.failures,
      lastFailureTime: state.lastFailureTime
    };
  }
}

// Default retry configurations
export const DEFAULT_RETRY_CONFIG: RetryConfig = {
  maxAttempts: 2, // Reduced from 3 to 2 for better UX (total 3 attempts including first)
  baseDelay: 1000,
  maxDelay: 10000,
  backoffFactor: 2,
  retryCondition: (error: ApiError) => {
    // Only retry on transient errors that might succeed on retry:
    // - Network errors (status 0): Connection failed, might be temporary
    // - Server errors (5xx): Server issue, might recover
    // - Timeout (408): Request timeout, might succeed with retry
    // - Rate limiting (429): Too many requests, retry after delay
    //
    // DO NOT retry on client errors:
    // - 400 Bad Request: Invalid input, will always fail
    // - 401 Unauthorized: Authentication issue, need to re-login
    // - 403 Forbidden: Permission issue, will always fail
    // - 404 Not Found: Resource doesn't exist, will always fail
    // - 409 Conflict: Duplicate entry, will always fail
    // - 422 Unprocessable Entity: Validation failed, will always fail
    return error.status === 0 || 
           error.status >= 500 || 
           error.status === 408 || 
           error.status === 429;
  }
};

export const DEFAULT_CIRCUIT_BREAKER_CONFIG: CircuitBreakerConfig = {
  failureThreshold: 5,
  resetTimeout: 30000, // 30 seconds
  monitoringPeriod: 60000 // 1 minute
};
