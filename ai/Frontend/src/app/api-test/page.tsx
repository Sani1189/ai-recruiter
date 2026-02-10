"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import { Loader2, CheckCircle, XCircle, Clock, Server, Wifi, WifiOff, BarChart3, Send, Code, Copy } from "lucide-react";
import { healthService, type HealthResponse, apiClient } from "@/lib/api";
import { RetryManager } from "@/lib/api/utils/retry";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import ApiMonitor from "@/components/api/ApiMonitor";
import { env } from "@/lib/config/env";
import { toast } from "sonner";

interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

interface TestResult {
  success: boolean;
  response?: ApiResponse<HealthResponse>;
  error?: string;
  timestamp: Date;
  duration: number;
}

interface CustomTestResult {
  success: boolean;
  response?: any;
  error?: string;
  timestamp: Date;
  duration: number;
  method: string;
  endpoint: string;
  status?: number;
}

export default function ApiTestPage() {
  const [testResults, setTestResults] = useState<TestResult[]>([]);
  const [isRunningTest, setIsRunningTest] = useState(false);

  // Custom API Tester state
  const [customEndpoint, setCustomEndpoint] = useState("/health");
  const [customMethod, setCustomMethod] = useState("GET");
  const [customBody, setCustomBody] = useState("");
  const [customResults, setCustomResults] = useState<CustomTestResult[]>([]);
  const [isRunningCustomTest, setIsRunningCustomTest] = useState(false);
  const [useAuth, setUseAuth] = useState(false);

  // Unified auth for bearer tokens
  const { getAccessToken, user, userType } = useUnifiedAuth();

  let testStartTime = 0;

  const runHealthTest = async () => {
    setIsRunningTest(true);
    testStartTime = Date.now();
    
    try {
      const response = await healthService.getHealth();
      const result: TestResult = {
        success: true,
        response: response,
        timestamp: new Date(),
        duration: Date.now() - testStartTime,
      };
      setTestResults(prev => [result, ...prev]);
    } catch (error: any) {
      const result: TestResult = {
        success: false,
        error: error.message || 'Unknown error occurred',
        timestamp: new Date(),
        duration: Date.now() - testStartTime,
      };
      setTestResults(prev => [result, ...prev]);
    } finally {
      setIsRunningTest(false);
    }
  };

  const clearResults = () => {
    setTestResults([]);
  };

  const resetCircuitBreaker = () => {
    // Reset circuit breaker by clearing the internal state
    RetryManager.resetAllCircuitBreakers();
    toast.success("Circuit breaker reset successfully. Try running the test again.");
  };

  const runCustomTest = async () => {
    setIsRunningCustomTest(true);
    const startTime = Date.now();
    
    try {
      let response: any;
      
      // Parse body for non-GET requests
      const body = customMethod !== "GET" && customBody ? JSON.parse(customBody) : undefined;
      
      // Get access token if authentication is enabled
      let accessToken: string | null = null;
      if (useAuth) {
        try {
          if (!user) {
            throw new Error("No user logged in. Please sign in first.");
          }
          
          accessToken = await getAccessToken();
          
          if (!accessToken) {
            throw new Error("Failed to get access token. Please ensure you are logged in.");
          }
          
          console.log('Access token obtained successfully:', {
            tokenLength: accessToken.length,
            userType: userType,
            email: user.email
          });
        } catch (tokenError: any) {
          throw new Error(`Authentication failed: ${tokenError.message}`);
        }
      }
      
      // Make the API call based on method
      switch (customMethod) {
        case "GET":
          response = await apiClient.get(customEndpoint, { 
            requireAuth: useAuth,
            headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined
          });
          break;
        case "POST":
          response = await apiClient.post(customEndpoint, body, { 
            requireAuth: useAuth,
            headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined
          });
          break;
        case "PUT":
          response = await apiClient.put(customEndpoint, body, { 
            requireAuth: useAuth,
            headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined
          });
          break;
        case "PATCH":
          response = await apiClient.patch(customEndpoint, body, { 
            requireAuth: useAuth,
            headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined
          });
          break;
        case "DELETE":
          response = await apiClient.delete(customEndpoint, { 
            requireAuth: useAuth,
            headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined
          });
          break;
        default:
          throw new Error(`Unsupported method: ${customMethod}`);
      }

      const result: CustomTestResult = {
        success: true,
        response: response,
        timestamp: new Date(),
        duration: Date.now() - startTime,
        method: customMethod,
        endpoint: customEndpoint,
        status: 200
      };
      
      setCustomResults(prev => [result, ...prev]);
      toast.success("API test completed successfully");
      
    } catch (error: any) {
      const result: CustomTestResult = {
        success: false,
        error: error.message || "Unknown error occurred",
        timestamp: new Date(),
        duration: Date.now() - startTime,
        method: customMethod,
        endpoint: customEndpoint,
        status: error.status || 0
      };
      
      setCustomResults(prev => [result, ...prev]);
      toast.error(`API test failed: ${error.message}`);
    } finally {
      setIsRunningCustomTest(false);
    }
  };

  const clearCustomResults = () => {
    setCustomResults([]);
  };

  const copyResponse = (response: any) => {
    navigator.clipboard.writeText(JSON.stringify(response, null, 2));
    toast.success("Response copied to clipboard");
  };

  const getStatusIcon = (success: boolean) => {
    if (success) {
      return <CheckCircle className="h-5 w-5 text-green-500" />;
    }
    return <XCircle className="h-5 w-5 text-red-500" />;
  };

  const getStatusBadge = (success: boolean) => {
    if (success) {
      return <Badge variant="default" className="bg-green-500">Success</Badge>;
    }
    return <Badge variant="destructive">Failed</Badge>;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary/5 via-background to-secondary/5 py-12">
      <div className="container px-4 mx-auto max-w-4xl">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">API Health Test</h1>
          <p className="text-muted-foreground">
            Test the connection between frontend and backend API
          </p>
        </div>

        {/* Test Controls */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Server className="h-5 w-5" />
              Backend Health Check
            </CardTitle>
            <CardDescription>
              Test the /health endpoint on your C# backend server
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center gap-4">
              <Button 
                onClick={runHealthTest} 
                disabled={isRunningTest}
                className="flex items-center gap-2"
              >
                {isRunningTest ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Testing...
                  </>
                ) : (
                  <>
                    <Wifi className="h-4 w-4" />
                    Run Health Test
                  </>
                )}
              </Button>
              
              {testResults.length > 0 && (
                <Button 
                  variant="outline" 
                  onClick={clearResults}
                  className="flex items-center gap-2"
                >
                  Clear Results
                </Button>
              )}
              
              <Button 
                variant="outline" 
                onClick={resetCircuitBreaker}
                className="flex items-center gap-2"
              >
                <WifiOff className="h-4 w-4" />
                Reset Circuit Breaker
              </Button>
            </div>

            {/* API Configuration Info */}
            <div className="bg-muted/50 p-4 rounded-lg">
              <h4 className="font-medium mb-2">API Configuration</h4>
              <div className="text-sm text-muted-foreground space-y-1">
                <div><strong>Base URL:</strong> {env.apiBaseUrl}</div>
                <div><strong>Endpoint:</strong> /health</div>
                <div><strong>Full URL:</strong> {`${env.apiBaseUrl}/health`}</div>
                <div><strong>Circuit Breaker Status:</strong> {
                  (() => {
                    const status = RetryManager.getCircuitBreakerStatus('default');
                    if (!status) return 'CLOSED (No failures)';
                    return `${status.state} (${status.failures} failures)`;
                  })()
                }</div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Test Results */}
        {testResults.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Test Results ({testResults.length})
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {testResults.map((result, index) => (
                <div key={index} className="border rounded-lg p-4">
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center gap-2">
                      {getStatusIcon(result.success)}
                      <span className="font-medium">
                        Test #{testResults.length - index}
                      </span>
                      {getStatusBadge(result.success)}
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {result.timestamp.toLocaleTimeString()} ({result.duration}ms)
                    </div>
                  </div>

                  {result.success && result.response ? (
                    <div className="space-y-2">
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <strong>Status:</strong> {result.response.data.status}
                        </div>
                        <div>
                          <strong>Version:</strong> {result.response.data.version}
                        </div>
                      </div>
                      <div className="text-sm">
                        <strong>Server Time:</strong> {new Date(result.response.data.timestamp).toLocaleString()}
                      </div>
                    </div>
                  ) : (
                    <div className="text-sm text-red-600">
                      <strong>Error:</strong> {result.error}
                    </div>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        )}

        {/* Custom API Tester */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Code className="h-5 w-5" />
              Custom API Endpoint Tester
            </CardTitle>
            <CardDescription>
              Test any API endpoint with different HTTP methods and request bodies
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="endpoint">Endpoint</Label>
                <Input
                  id="endpoint"
                  value={customEndpoint}
                  onChange={(e) => setCustomEndpoint(e.target.value)}
                  placeholder="/health"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="method">HTTP Method</Label>
                <Select value={customMethod} onValueChange={setCustomMethod}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="GET">GET</SelectItem>
                    <SelectItem value="POST">POST</SelectItem>
                    <SelectItem value="PUT">PUT</SelectItem>
                    <SelectItem value="PATCH">PATCH</SelectItem>
                    <SelectItem value="DELETE">DELETE</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Authentication Toggle */}
            <div className="flex items-center justify-between p-4 bg-muted/50 rounded-lg">
              <div className="flex items-center space-x-2">
                <Switch
                  id="use-auth"
                  checked={useAuth}
                  onCheckedChange={setUseAuth}
                />
                <Label htmlFor="use-auth" className="text-sm font-medium">
                  Include Bearer Token
                </Label>
                {useAuth && (
                  <Badge variant="secondary" className="ml-2">
                    Authenticated
                  </Badge>
                )}
              </div>
              
              {/* User Type Indicator */}
              {useAuth && user && (
                <div className="flex items-center gap-2">
                  <span className="text-xs text-muted-foreground">Token from:</span>
                  <Badge variant={userType === 'recruiter' ? "default" : "outline"} className="text-xs">
                    {userType === 'recruiter' ? "🔐 Recruiter Admin" : "👤 Candidate User"}
                  </Badge>
                </div>
              )}
            </div>

            {customMethod !== "GET" && (
              <div className="space-y-2">
                <Label htmlFor="body">Request Body (JSON)</Label>
                <Textarea
                  id="body"
                  value={customBody}
                  onChange={(e) => setCustomBody(e.target.value)}
                  placeholder='{"key": "value"}'
                  rows={4}
                />
              </div>
            )}

            <div className="flex items-center gap-4">
              <Button 
                onClick={runCustomTest} 
                disabled={isRunningCustomTest}
                className="flex items-center gap-2"
              >
                {isRunningCustomTest ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Testing...
                  </>
                ) : (
                  <>
                    <Send className="h-4 w-4" />
                    Test Endpoint
                  </>
                )}
              </Button>
              
              {customResults.length > 0 && (
                <Button 
                  variant="outline" 
                  onClick={clearCustomResults}
                  className="flex items-center gap-2"
                >
                  Clear Results
                </Button>
              )}
            </div>

            {/* API Configuration Info */}
            <div className="bg-muted/50 p-4 rounded-lg">
              <h4 className="font-medium mb-2">Request Details</h4>
              <div className="text-sm text-muted-foreground space-y-1">
                <div><strong>Full URL:</strong> {env.apiBaseUrl}{customEndpoint}</div>
                <div><strong>Method:</strong> {customMethod}</div>
                <div><strong>Authentication:</strong> {useAuth ? "Bearer Token (Azure B2C)" : "Disabled"}</div>
                {useAuth && (
                  <div>
                    <strong>Token Source:</strong> {
                      userType === 'recruiter' ? 'Recruiter Admin Token' : 
                      userType === 'candidate' ? 'Candidate User Token' : 
                      'Auto-detected Token'
                    }
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Custom Test Results */}
        {customResults.length > 0 && (
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Custom Test Results ({customResults.length})
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {customResults.map((result, index) => (
                <div key={index} className="border rounded-lg p-4">
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center gap-2">
                      {getStatusIcon(result.success)}
                      <span className="font-medium">
                        {result.method} {result.endpoint}
                      </span>
                      {getStatusBadge(result.success)}
                      {result.status && (
                        <Badge variant="outline">
                          {result.status}
                        </Badge>
                      )}
                      {useAuth && (
                        <Badge variant="secondary" className="text-xs">
                          🔐 Auth
                        </Badge>
                      )}
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {result.timestamp.toLocaleTimeString()} ({result.duration}ms)
                    </div>
                  </div>

                  {result.success && result.response ? (
                    <div className="space-y-2">
                      <div className="flex items-center justify-between">
                        <h4 className="font-medium">Response</h4>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => copyResponse(result.response)}
                          className="flex items-center gap-1"
                        >
                          <Copy className="h-3 w-3" />
                          Copy
                        </Button>
                      </div>
                      <pre className="bg-muted p-3 rounded text-xs overflow-x-auto max-h-60">
                        {JSON.stringify(result.response, null, 2)}
                      </pre>
                    </div>
                  ) : (
                    <div className="text-sm text-red-600">
                      <strong>Error:</strong> {result.error}
                    </div>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        )}

        {/* API Performance Monitor */}
        <Card className="mt-6">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <BarChart3 className="h-5 w-5" />
              API Performance Monitor
            </CardTitle>
            <CardDescription>
              Real-time monitoring of API performance, caching, and error rates
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ApiMonitor />
          </CardContent>
        </Card>

        {/* Instructions */}
        <Card className="mt-6">
          <CardHeader>
            <CardTitle>Instructions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            <p>1. Make sure your C# backend is running on port 8000</p>
            <p>2. Click "Run Health Test" to test the connection</p>
            <p>3. Check the results above to see if the API is responding</p>
            <p>4. Monitor API performance metrics in real-time above</p>
            <p>5. If tests fail, verify your backend is running and accessible</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
