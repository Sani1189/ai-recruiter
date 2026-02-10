// Performance Metrics and Monitoring

interface RequestMetrics {
  requestId: string;
  url: string;
  method: string;
  startTime: number;
  endTime?: number;
  duration?: number;
  status?: number;
  error?: string;
  retryCount: number;
}

interface ApiLogger {
  log: (level: 'info' | 'warn' | 'error', message: string, data?: any) => void;
}

export class MetricsCollector {
  private static metrics = new Map<string, RequestMetrics>();
  private static logger: ApiLogger | null = null;

  static setLogger(logger: ApiLogger): void {
    this.logger = logger;
  }

  static startRequest(requestId: string, url: string, method: string): RequestMetrics {
    const metrics: RequestMetrics = {
      requestId,
      url,
      method,
      startTime: Date.now(),
      retryCount: 0
    };
    
    this.metrics.set(requestId, metrics);
    return metrics;
  }

  static endRequest(requestId: string, status?: number, error?: string): void {
    const metrics = this.metrics.get(requestId);
    if (!metrics) return;

    metrics.endTime = Date.now();
    metrics.duration = metrics.endTime - metrics.startTime;
    metrics.status = status;
    metrics.error = error;

    // Log metrics if logger is available
    if (this.logger) {
      const level = status && status >= 400 ? 'warn' : 'info';
      // this.logger.log(level, 'API Request Completed', {
      //   url: metrics.url,
      //   method: metrics.method,
      //   duration: metrics.duration,
      //   status: metrics.status,
      //   retryCount: metrics.retryCount,
      //   error: metrics.error
      // });
    }

    // Keep metrics for a short time for debugging
    setTimeout(() => {
      this.metrics.delete(requestId);
    }, 60000); // 1 minute
  }

  static incrementRetry(requestId: string): void {
    const metrics = this.metrics.get(requestId);
    if (metrics) {
      metrics.retryCount++;
    }
  }

  static getMetrics(requestId: string): RequestMetrics | undefined {
    return this.metrics.get(requestId);
  }

  static getAllMetrics(): RequestMetrics[] {
    return Array.from(this.metrics.values());
  }

  static clearMetrics(): void {
    this.metrics.clear();
  }

  static getPerformanceStats(): {
    totalRequests: number;
    averageResponseTime: number;
    errorRate: number;
    retryRate: number;
  } {
    const allMetrics = this.getAllMetrics();
    const completedMetrics = allMetrics.filter(m => m.endTime);
    
    if (completedMetrics.length === 0) {
      return {
        totalRequests: 0,
        averageResponseTime: 0,
        errorRate: 0,
        retryRate: 0
      };
    }

    const totalRequests = completedMetrics.length;
    const averageResponseTime = completedMetrics.reduce((sum, m) => sum + (m.duration || 0), 0) / totalRequests;
    const errorRate = completedMetrics.filter(m => m.status && m.status >= 400).length / totalRequests;
    const retryRate = completedMetrics.filter(m => m.retryCount > 0).length / totalRequests;

    return {
      totalRequests,
      averageResponseTime,
      errorRate,
      retryRate
    };
  }
}

// Console logger implementation
export const consoleLogger: ApiLogger = {
  log: (level: 'info' | 'warn' | 'error', message: string, data?: any) => {
    const timestamp = new Date().toISOString();
    const logMessage = `[${timestamp}] [API-${level.toUpperCase()}] ${message}`;
    
    switch (level) {
      case 'error':
        console.error(logMessage, data);
        break;
      case 'warn':
        console.warn(logMessage, data);
        break;
      default:
        console.log(logMessage, data);
    }
  }
};
