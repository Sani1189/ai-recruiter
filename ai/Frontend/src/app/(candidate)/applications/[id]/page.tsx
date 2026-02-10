"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { useApi } from "@/hooks/useApi";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, Calendar, Clock, CheckCircle, Circle, Play } from "lucide-react";
import Link from "next/link";

type ApplicationDetail = {
  id: string;
  jobPostName: string;
  jobPostVersion: number;
  createdAt?: string;
  updatedAt?: string;
  completedAt?: string | null;
  candidateId?: string;
  createdBy?: string;
  updatedBy?: string;
};

type ApplicationStep = {
  id: string;
  jobApplicationId: string;
  jobPostStepName: string;
  jobPostStepVersion: number;
  status: string;
  stepNumber: number;
  startedAt?: string | null;
  completedAt?: string | null;
  data?: string | null;
  createdAt?: string;
  updatedAt?: string;
};

export default function CandidateApplicationDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const id = params?.id as string;
  const api = useApi();
  const [data, setData] = useState<ApplicationDetail | null>(null);
  const [steps, setSteps] = useState<ApplicationStep[]>([]);
  const [loading, setLoading] = useState(true);
  const [stepsLoading, setStepsLoading] = useState(false);

  useEffect(() => {
    if (!id) return;
    const load = async () => {
      setLoading(true);
      try {
        // Load application details
        const res = await api.get(`/candidate/job-application/my-application/${id}`);
        setData(((res as any)?.data ?? res) as ApplicationDetail);
        
        // Load application steps
        setStepsLoading(true);
        try {
          const stepsRes = await api.get(`/candidate/job-application/steps/${id}`);
          setSteps(((stepsRes as any)?.data ?? stepsRes) as ApplicationStep[]);
        } catch (stepsError) {
          console.error('Failed to load application steps:', stepsError);
          setSteps([]);
        } finally {
          setStepsLoading(false);
        }
      } catch (error) {
        console.error('Failed to load application:', error);
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, [id]); // Removed 'api' from dependency array to prevent infinite loops

  if (loading) {
    return (
      <div className="container py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
            <p className="text-muted-foreground">Loading application details...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="container py-8">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Application Not Found</h1>
          <p className="text-muted-foreground mb-6">The application you're looking for doesn't exist or you don't have access to it.</p>
          <Button onClick={() => router.back()}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Go Back
          </Button>
        </div>
      </div>
    );
  }

  const completedSteps = steps.filter(step => step.completedAt).length;
  const totalSteps = steps.length;
  const progressPercentage = totalSteps > 0 ? Math.round((completedSteps / totalSteps) * 100) : 0;

  return (
    <div className="container py-8 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" size="sm" onClick={() => router.back()}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Button>
          <div>
            <h2 className="text-3xl font-bold">Application Details</h2>
            <Link href={`/job-post/${data.jobPostName}/${data.jobPostVersion}`} className="text-muted-foreground font-medium text-lg hover:underline">
              <h3 className="text-lg font-medium">{data.jobPostName}</h3>
            </Link>
          </div>
        </div>
        <Badge variant={data.completedAt ? "default" : "secondary"} className="text-sm">
          {data.completedAt ? "Completed" : "In Progress"}
        </Badge>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Application Overview */}
        <div className="lg:col-span-2 space-y-6">
          <Card className="shadow-card">
            <CardHeader>
              <CardTitle className="flex items-center">
                <Calendar className="mr-2 h-5 w-5" />
                Application Overview
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <div className="text-sm text-muted-foreground">Applied At</div>
                  <div className="font-medium">{data.createdAt ? new Date(data.createdAt).toLocaleString() : "-"}</div>
                </div>
                <div>
                  <div className="text-sm text-muted-foreground">Last Updated</div>
                  <div className="font-medium">{data.updatedAt ? new Date(data.updatedAt).toLocaleString() : "-"}</div>
                </div>
                
                <div className="md:col-span-2">
                  <div className="text-sm text-muted-foreground mb-2">Progress</div>
                  <div className="flex items-center space-x-3">
                    <div className="flex-1 bg-gray-200 rounded-full h-3">
                      <div 
                        className="bg-blue-600 h-3 rounded-full transition-all duration-300" 
                        style={{ width: `${progressPercentage}%` }}
                      ></div>
                    </div>
                    <span className="text-sm font-medium text-muted-foreground whitespace-nowrap">
                      {progressPercentage}% ({completedSteps}/{totalSteps} steps)
                    </span>
                  </div>
                </div>
              </div>

              {/* Application Steps */}
              <div className="pt-4 border-t">
                <div className="flex items-center mb-4">
                  <Clock className="mr-2 h-5 w-5" />
                  <h3 className="text-lg font-semibold">Application Steps ({completedSteps}/{totalSteps} completed)</h3>
                </div>
                
                {stepsLoading ? (
                  <div className="flex items-center justify-center py-8">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                    <span className="ml-2 text-muted-foreground">Loading steps...</span>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {steps.map((step, index) => (
                      <div key={step.id} className="flex items-center space-x-3 p-3 rounded-lg border">
                        <div className="flex-shrink-0">
                          {step.completedAt ? (
                            <CheckCircle className="h-5 w-5 text-green-600" />
                          ) : step.startedAt ? (
                            <Play className="h-5 w-5 text-blue-600" />
                          ) : (
                            <Circle className="h-5 w-5 text-gray-400" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center justify-between">
                            <div className="font-medium">{step.jobPostStepName} (v{step.jobPostStepVersion})</div>
                            <div className="flex items-center space-x-2">
                              <Badge variant="outline">Step {index + 1}</Badge>
                              <Badge variant={step.completedAt ? "default" : step.startedAt ? "secondary" : "outline"}>
                                {step.status}
                              </Badge>
                            </div>
                          </div>
                          <div className="mt-1 grid grid-cols-1 md:grid-cols-3 text-sm text-muted-foreground">
                            <div>Started: {step.startedAt ? new Date(step.startedAt).toLocaleString() : "Not started"}</div>
                            <div>Completed: {step.completedAt ? new Date(step.completedAt).toLocaleString() : "Not completed"}</div>
                            <div>Updated: {step.updatedAt ? new Date(step.updatedAt).toLocaleDateString() : "-"}</div>
                          </div>
                          {step.data && (
                            <div className="mt-2 text-sm">
                              <div className="text-muted-foreground">Data submitted:</div>
                              <div className="bg-gray-50 p-2 rounded text-xs font-mono break-all">
                                {step.data.length > 100 ? `${step.data.substring(0, 100)}...` : step.data}
                              </div>
                            </div>
                          )}
                        </div>
                      </div>
                    ))}
                    {steps.length === 0 && (
                      <div className="text-center py-8 text-muted-foreground">
                        No steps assigned to this application yet.
                      </div>
                    )}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>


        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          <Card className="shadow-card">
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <Button className="w-full" variant="outline">
                Contact Recruiter
              </Button>
              <Button 
                className="w-full" 
                variant="outline"
                onClick={() => router.push(`/job-post/${data.jobPostName}/${data.jobPostVersion}`)}
              >
                View Job Posting
              </Button>
            </CardContent>
          </Card>

          <Card className="shadow-card">
            <CardHeader>
              <CardTitle>Application Timeline</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="flex items-center space-x-3">
                  <div className="w-2 h-2 bg-green-600 rounded-full"></div>
                  <div className="text-sm">
                    <div className="font-medium">Application Submitted</div>
                    <div className="text-muted-foreground">{data.createdAt ? new Date(data.createdAt).toLocaleDateString() : "-"}</div>
                  </div>
                </div>
                {data.updatedAt && (
                  <div className="flex items-center space-x-3">
                    <div className="w-2 h-2 bg-blue-600 rounded-full"></div>
                    <div className="text-sm">
                      <div className="font-medium">Last Updated</div>
                      <div className="text-muted-foreground">{new Date(data.updatedAt).toLocaleDateString()}</div>
                    </div>
                  </div>
                )}
                {data.completedAt && (
                  <div className="flex items-center space-x-3">
                    <div className="w-2 h-2 bg-green-600 rounded-full"></div>
                    <div className="text-sm">
                      <div className="font-medium">Application Completed</div>
                      <div className="text-muted-foreground">{new Date(data.completedAt).toLocaleDateString()}</div>
                    </div>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}


