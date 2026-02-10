"use client"

import { useEffect, useMemo, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { ArrowLeft } from "lucide-react"
import { Button } from "@/components/ui/button"

import InterviewHeader from "@/components/pages/_candidate/interview/main-interview-area/InterviewHeader"
import InterviewStepper from "@/components/pages/_candidate/interview/InterviewStepper"
import JobDescription from "@/components/pages/_candidate/interview/sidebar/JobDescription"
import Tips from "@/components/pages/_candidate/interview/sidebar/Tips"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useApi } from "@/hooks/useApi"
import { hasErrorCode } from "@/lib/api/errorHandler"
import { useAuthStore } from "@/stores/useAuthStore"

type JobPostStepDetails = {
  name: string
  version: number
  stepType: string
  isInterview: boolean
  participant?: "Candidate" | "Recruiter"
  showStepForCandidate?: boolean
  displayTitle?: string | null
  displayContent?: string | null
  showSpinner?: boolean
  interviewConfigurationName?: string
  interviewConfigurationVersion?: number
}

type JobPostStep = {
  stepNumber: number
  stepName?: string
  stepVersion?: number
  stepDetails?: JobPostStepDetails
}

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
  assignedSteps?: JobPostStep[]
}

export default function PublicInterviewPage() {
  const params = useParams()
  const router = useRouter()
  const api = useApi()
  const { user, userType } = useAuthStore()

  const name = params.name as string
  const version = Number.parseInt(params.version as string)

  const [jobPost, setJobPost] = useState<JobPost | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [errorCode, setErrorCode] = useState<string | null>(null)

  useEffect(() => {
    let isActive = true

    const fetchJobPost = async () => {
      if (!name || !version) return
      try {
        setLoading(true)
        const response = await api.get(`/public/job/${name}/${version}`, {
          requireAuth: false,
        })
        if (!isActive) return
        setJobPost(response.data || response)
        setError(null)
      } catch (err: any) {
        if (!isActive) return
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
        if (isActive) {
          setLoading(false)
        }
      }
    }

    fetchJobPost()
    return () => {
      isActive = false
    }
  }, [name, version])

  // Require authentication before allowing resume upload or interviews.
  const ensureCandidateAuth = () => {
    if (!user || userType !== "candidate") {
      router.push(`/sign-in?redirect=/interview/${name}/${version}`)
      return false
    }
    return true
  }

  const jobPostForUi = useMemo(() => {
    if (!jobPost) return null
    return {
      ...jobPost,
      id: `${jobPost.name}-v${jobPost.version}`,
      duration: 30,
    }
  }, [jobPost])

  if (loading) {
    return (
      <div className="container min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4" />
          <p className="text-muted-foreground">Loading job details...</p>
        </div>
      </div>
    )
  }

  if (error || !jobPostForUi) {
    const isNotAvailable = errorCode === "JOB_NOT_AVAILABLE"
    return (
      <div className="container min-h-screen flex items-center justify-center">
        <Card className="max-w-md w-full">
          <CardHeader>
            <CardTitle className="text-destructive">
              {isNotAvailable ? "Job Not Available" : "Job Not Found"}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">{error || "This job posting could not be found."}</p>
            <Button onClick={() => router.push(isNotAvailable ? "/jobs" : `/job-post/${name}/${version}`)} variant="outline" className="w-full">
              <ArrowLeft className="mr-2 h-4 w-4" />
              {isNotAvailable ? "Back to Jobs" : "Back to Job Details"}
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container grid gap-8 py-8 lg:grid-cols-3">
      <div className="lg:col-span-3">
        <Button onClick={() => router.push(`/job-post/${name}/${version}`)} variant="ghost" size="sm" className="mb-4">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Job Details
        </Button>
      </div>

      <div className="space-y-6 lg:col-span-2">
        <InterviewHeader
          jobTitle={jobPostForUi.jobTitle}
          jobType={jobPostForUi.jobType}
          experienceLevel={jobPostForUi.experienceLevel}
          tone={undefined}
          focusArea={undefined}
        />

        <InterviewStepper jobPost={jobPostForUi} requireCandidateAuth={ensureCandidateAuth} />
      </div>

      <div className="space-y-6">
        <JobDescription jobDescription={jobPostForUi.jobDescription} />

        {jobPostForUi.minimumRequirements && jobPostForUi.minimumRequirements.length > 0 && (
          <Card className="shadow-card">
            <CardHeader>
              <CardTitle className="text-lg">Minimum Requirements</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {jobPostForUi.minimumRequirements.map((req, index) => (
                <p key={index} className="text-muted-foreground text-sm">
                  • {req}
                </p>
              ))}
            </CardContent>
          </Card>
        )}

        <Tips />
      </div>
    </div>
  )
}
