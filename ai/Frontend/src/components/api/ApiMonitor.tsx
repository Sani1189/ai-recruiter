"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
// import { Progress } from "@/components/ui/progress"; // Component not available
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  Activity, 
  Database, 
  Clock, 
  AlertTriangle, 
  RefreshCw, 
  Trash2,
  TrendingUp,
  Server,
  Wifi
} from "lucide-react";
import { apiClient } from "@/lib/api";

interface ApiMetrics {
  totalRequests: number;
  averageResponseTime: number;
  errorRate: number;
  retryRate: number;
}

interface CacheStats {
  size: number;
  maxSize: number;
  hitRate: number;
}

export default function ApiMonitor() {
  const [metrics, setMetrics] = useState<ApiMetrics>({
    totalRequests: 0,
    averageResponseTime: 0,
    errorRate: 0,
    retryRate: 0
  });
  const [cacheStats, setCacheStats] = useState<CacheStats>({
    size: 0,
    maxSize: 1000,
    hitRate: 0
  });
  const [activeRequests, setActiveRequests] = useState(0);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const refreshMetrics = async () => {
    setIsRefreshing(true);
    try {
      const newMetrics = apiClient.getMetrics();
      const newCacheStats = apiClient.getCacheStats();
      const newActiveRequests = apiClient.getActiveRequests();

      setMetrics(newMetrics);
      setCacheStats(newCacheStats);
      setActiveRequests(newActiveRequests);
    } catch (error) {
      console.error('Failed to refresh metrics:', error);
    } finally {
      setIsRefreshing(false);
    }
  };

  useEffect(() => {
    refreshMetrics();
    const interval = setInterval(refreshMetrics, 5000); // Refresh every 5 seconds
    return () => clearInterval(interval);
  }, []);

  const getPerformanceColor = (value: number, thresholds: { good: number; warning: number }) => {
    if (value <= thresholds.good) return "text-green-600";
    if (value <= thresholds.warning) return "text-yellow-600";
    return "text-red-600";
  };

  const getErrorRateColor = (rate: number) => {
    if (rate === 0) return "text-green-600";
    if (rate <= 0.05) return "text-yellow-600";
    return "text-red-600";
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">API Performance Monitor</h2>
          <p className="text-muted-foreground">
            Real-time monitoring of API performance and health
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={refreshMetrics}
            disabled={isRefreshing}
          >
            <RefreshCw className={`h-4 w-4 mr-2 ${isRefreshing ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => {
              apiClient.clearCache();
              refreshMetrics();
            }}
          >
            <Trash2 className="h-4 w-4 mr-2" />
            Clear Cache
          </Button>
        </div>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="performance">Performance</TabsTrigger>
          <TabsTrigger value="cache">Cache</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Requests</CardTitle>
                <Activity className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{metrics.totalRequests}</div>
                <p className="text-xs text-muted-foreground">
                  All time requests
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Active Requests</CardTitle>
                <Wifi className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{activeRequests}</div>
                <p className="text-xs text-muted-foreground">
                  Currently in flight
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Avg Response Time</CardTitle>
                <Clock className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className={`text-2xl font-bold ${getPerformanceColor(metrics.averageResponseTime, { good: 500, warning: 1000 })}`}>
                  {metrics.averageResponseTime.toFixed(0)}ms
                </div>
                <p className="text-xs text-muted-foreground">
                  Average latency
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Error Rate</CardTitle>
                <AlertTriangle className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className={`text-2xl font-bold ${getErrorRateColor(metrics.errorRate)}`}>
                  {(metrics.errorRate * 100).toFixed(1)}%
                </div>
                <p className="text-xs text-muted-foreground">
                  Failed requests
                </p>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="performance" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <TrendingUp className="h-5 w-5" />
                  Response Time Distribution
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Average Response Time</span>
                    <span className={getPerformanceColor(metrics.averageResponseTime, { good: 500, warning: 1000 })}>
                      {metrics.averageResponseTime.toFixed(0)}ms
                    </span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${Math.min((metrics.averageResponseTime / 2000) * 100, 100)}%` }}
                    />
                  </div>
                </div>
                
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Retry Rate</span>
                    <span>{(metrics.retryRate * 100).toFixed(1)}%</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-yellow-600 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${metrics.retryRate * 100}%` }}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Server className="h-5 w-5" />
                  System Health
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">API Status</span>
                  <Badge 
                    variant={metrics.errorRate < 0.05 ? "default" : metrics.errorRate < 0.1 ? "secondary" : "destructive"}
                    className={metrics.errorRate < 0.05 ? "bg-green-500" : ""}
                  >
                    {metrics.errorRate < 0.05 ? "Healthy" : metrics.errorRate < 0.1 ? "Warning" : "Critical"}
                  </Badge>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Performance</span>
                  <Badge 
                    variant={metrics.averageResponseTime < 500 ? "default" : metrics.averageResponseTime < 1000 ? "secondary" : "destructive"}
                    className={metrics.averageResponseTime < 500 ? "bg-green-500" : ""}
                  >
                    {metrics.averageResponseTime < 500 ? "Excellent" : metrics.averageResponseTime < 1000 ? "Good" : "Slow"}
                  </Badge>
                </div>

                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Reliability</span>
                  <Badge 
                    variant={metrics.retryRate < 0.1 ? "default" : metrics.retryRate < 0.2 ? "secondary" : "destructive"}
                    className={metrics.retryRate < 0.1 ? "bg-green-500" : ""}
                  >
                    {metrics.retryRate < 0.1 ? "Stable" : metrics.retryRate < 0.2 ? "Unstable" : "Failing"}
                  </Badge>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="cache" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Database className="h-5 w-5" />
                Cache Statistics
              </CardTitle>
              <CardDescription>
                Intelligent request caching performance
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 md:grid-cols-3">
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Cache Size</span>
                    <span>{cacheStats.size} / {cacheStats.maxSize}</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-green-600 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${(cacheStats.size / cacheStats.maxSize) * 100}%` }}
                    />
                  </div>
                </div>
                
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Hit Rate</span>
                    <span>{(cacheStats.hitRate * 100).toFixed(1)}%</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${cacheStats.hitRate * 100}%` }}
                    />
                  </div>
                </div>
                
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Memory Usage</span>
                    <span>{((cacheStats.size / cacheStats.maxSize) * 100).toFixed(1)}%</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-purple-600 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${(cacheStats.size / cacheStats.maxSize) * 100}%` }}
                    />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
