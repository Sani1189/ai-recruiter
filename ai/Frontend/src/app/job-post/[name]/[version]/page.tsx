"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { ArrowLeft, Briefcase, CheckCircle, ExternalLink, ClipboardCheck } from "lucide-react"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { useApi } from "@/hooks/useApi"
import { hasErrorCode } from "@/lib/api/errorHandler"
import { useAuthStore } from "@/stores/useAuthStore"
import { useJobApplicationFlow } from "@/hooks/useJobApplicationFlow"

interface JobPost {
  name: string
  version: number
  jobTitle: string
  jobType: string
  experienceLevel: string
  jobDescription: string
  policeReportRequired?: boolean
  maxAmountOfCandidatesRestriction?: number
  minimumRequirements?: string[]
  assignedSteps?: Array<{
    stepNumber: number
    stepName?: string
    stepVersion?: number
    stepDetails?: {
      name: string
      version: number
      stepType: string
      isInterview: boolean
      interviewConfigurationName?: string
      interviewConfigurationVersion?: number
    }
  }>
}

export default function JobPostDetailsPage() {
  const params = useParams()
  const router = useRouter()
  const api = useApi()
  const { user, userType } = useAuthStore()
  const { getApplicationStatus } = useJobApplicationFlow()

  const name = params.name as string
  const version = Number.parseInt(params.version as string)

  const [jobPost, setJobPost] = useState<JobPost | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [errorCode, setErrorCode] = useState<string | null>(null)
  const [hasApplied, setHasApplied] = useState(false)
  const [applicationId, setApplicationId] = useState<string | null>(null)
  const [checkingStatus, setCheckingStatus] = useState(false)

  // Fetch job post data
  useEffect(() => {
    async function fetchJobPost() {
      if (!name || !version) return

      try {
        setLoading(true)
        const response = await api.get(`/public/job/${name}/${version}`, {
          requireAuth: false,
        })
        setJobPost(response.data || response)
        setError(null)
      } catch (err: any) {
        console.error("Error fetching job post:", err)
        const code = err?.code ?? err?.errorCode ?? null
        setErrorCode(code)
        if (hasErrorCode(err, "JOB_NOT_AVAILABLE")) {
          setError("This job is not available at the moment.")
        } else {
          setError(err?.message || "Failed to load job post")
        }
        setJobPost(null)
      } finally {
        setLoading(false)
      }
    }

    fetchJobPost()
  }, [name, version])

  useEffect(() => {
    async function checkApplicationStatus() {
      if (!user || userType !== "candidate") {
        setHasApplied(false)
        return
      }

      try {
        setCheckingStatus(true)
        const status = await getApplicationStatus(name, version)
        setHasApplied(status.hasApplied)
        if (status.applicationId) {
          setApplicationId(status.applicationId)
        }
        console.log("Application status checked:", status)
      } catch (err) {
        console.error("Error checking application status:", err)
      } finally {
        setCheckingStatus(false)
      }
    }

    checkApplicationStatus()
  }, [user, userType, name, version, getApplicationStatus])

  const handleApplyNow = () => {
    if (!user || userType !== "candidate") {
      router.push(`/sign-in?redirect=/job-post/${name}/${version}`)
      return
    }
    router.push(`/interview/${name}/${version}`)
  }

  const handleViewApplication = () => {
    if (applicationId) {
      router.push(`/applications/${applicationId}`)
    }
  }

  if (loading) {
    return (
      <div className="container py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
            <p className="text-muted-foreground">Loading job details...</p>
          </div>
        </div>
      </div>
    )
  }

  if (error || !jobPost) {
    const isNotAvailable = errorCode === "JOB_NOT_AVAILABLE"
    return (
      <div className="container py-8">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">{isNotAvailable ? "Job Not Available" : "Job Not Found"}</h1>
          <p className="text-muted-foreground mb-6">{error || "This job posting could not be found."}</p>
          <Button onClick={() => router.push("/jobs")}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Jobs
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="container py-8 space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => router.back()}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back
          </Button>
          <div>
            <h1 className="text-3xl font-bold">{jobPost.jobTitle}</h1>
            <div className="flex flex-wrap items-center gap-2 mt-2">
              <Badge variant="outline" className="text-sm">
                v{jobPost.version}
              </Badge>
              <Badge variant="secondary" className="text-sm">
                {jobPost.jobType}
              </Badge>
              <Badge variant="secondary" className="text-sm">
                {jobPost.experienceLevel}
              </Badge>
              {hasApplied && (
                <Badge variant="default" className="text-sm bg-green-600">
                  <CheckCircle className="mr-1 h-3 w-3" />
                  Applied
                </Badge>
              )}
            </div>
          </div>
        </div>

        {hasApplied ? (
          <div className="flex gap-2 flex-wrap">
            <Button
              onClick={() => router.push(`/interview/${name}/${version}`)}
              size="lg"
              className="bg-blue-600 hover:bg-blue-700"
            >
              <ExternalLink className="mr-2 h-4 w-4" />
              Continue Interview
            </Button>
            {applicationId && (
              <Button onClick={handleViewApplication} size="lg" variant="outline">
                <ClipboardCheck className="mr-2 h-4 w-4" />
                View Application
              </Button>
            )}
          </div>
        ) : (
          <Button onClick={handleApplyNow} size="lg" disabled={checkingStatus}>
            <ExternalLink className="mr-2 h-4 w-4" />
            {checkingStatus ? "Checking..." : "Apply Now"}
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Job Description */}
          <Card className="shadow-card">
            <CardHeader>
              <CardTitle className="flex items-center">
                <Briefcase className="mr-2 h-5 w-5" />
                Job Description
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="prose max-w-none">
                <div className="whitespace-pre-wrap text-muted-foreground">{jobPost.jobDescription}</div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Job Details */}
          <Card className="shadow-card">
            <CardHeader>
              <CardTitle>Job Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="text-sm text-muted-foreground">Job Type</div>
                <div className="font-medium">{jobPost.jobType}</div>
              </div>
              <div>
                <div className="text-sm text-muted-foreground">Experience Level</div>
                <div className="font-medium">{jobPost.experienceLevel}</div>
              </div>
              {jobPost.policeReportRequired && (
                <div>
                  <div className="text-sm text-muted-foreground">Police Report</div>
                  <div className="font-medium text-orange-600">Required</div>
                </div>
              )}
              {jobPost.maxAmountOfCandidatesRestriction && (
                <div>
                  <div className="text-sm text-muted-foreground">Candidate Limit</div>
                  <div className="font-medium">{jobPost.maxAmountOfCandidatesRestriction} candidates</div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Minimum Requirements */}
          {jobPost.minimumRequirements && jobPost.minimumRequirements.length > 0 && (
            <Card className="shadow-card">
              <CardHeader>
                <CardTitle className="flex items-center">
                  <CheckCircle className="mr-2 h-5 w-5" />
                  Minimum Requirements
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {jobPost.minimumRequirements.map((req, index) => (
                  <div key={index} className="flex items-start space-x-2">
                    <CheckCircle className="h-4 w-4 text-green-600 mt-0.5 flex-shrink-0" />
                    <span className="text-sm text-muted-foreground">{req}</span>
                  </div>
                ))}
              </CardContent>
            </Card>
          )}

          {/* Quick Actions */}
          <Card className="shadow-card">
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {hasApplied ? (
                <>
                  <Button
                    onClick={() => router.push(`/interview/${name}/${version}`)}
                    className="w-full bg-blue-600 hover:bg-blue-700"
                  >
                    <ExternalLink className="mr-2 h-4 w-4" />
                    Continue Interview
                  </Button>
                  {applicationId && (
                    <Button onClick={handleViewApplication} variant="outline" className="w-full bg-transparent">
                      <ClipboardCheck className="mr-2 h-4 w-4" />
                      View Application
                    </Button>
                  )}
                </>
              ) : (
                <>
                  <Button onClick={handleApplyNow} className="w-full" disabled={checkingStatus}>
                    <ExternalLink className="mr-2 h-4 w-4" />
                    {checkingStatus ? "Checking..." : "Apply for this Job"}
                  </Button>
                  <Button variant="outline" className="w-full bg-transparent" onClick={() => router.push("/jobs")}>
                    <ArrowLeft className="mr-2 h-4 w-4" />
                    Back to Jobs
                  </Button>
                </>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
