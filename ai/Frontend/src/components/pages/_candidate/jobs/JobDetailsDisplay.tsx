"use client"

import type React from "react"
import { useEffect, useState, useRef } from "react"
import { Briefcase, GraduationCap, Clock, AlertCircle, CheckCircle2, Users, Loader2 } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useApi } from "@/hooks/useApi"
import { useRouter } from "next/navigation"
import { toast } from "sonner"
import { useJobApplicationFlow } from "@/hooks/useJobApplicationFlow"

interface JobDetails {
  name: string
  version: number
  jobTitle: string
  experienceLevel: string
  jobType: string
  jobDescription: string
  minimumRequirements: string[]
  policeReportRequired: boolean
  maxAmountOfCandidatesRestriction: number
  createdAt: string
}

interface JobDetailsDisplayProps {
  jobId: string
}

/* ------------------------------ Skeleton ------------------------------ */

const SkeletonLoader = () => (
  <div className="max-w-5xl mx-auto px-4 animate-pulse">
    <Card className="rounded-2xl">
      <CardContent className="p-8 space-y-6">
        <div className="h-10 bg-muted rounded w-2/3" />
        <div className="flex gap-2">
          <div className="h-7 w-24 bg-muted rounded-full" />
          <div className="h-7 w-32 bg-muted rounded-full" />
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 py-6 border-y">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="h-12 bg-muted rounded-lg" />
          ))}
        </div>
        <div className="space-y-3">
          <div className="h-4 bg-muted rounded w-full" />
          <div className="h-4 bg-muted rounded w-full" />
          <div className="h-4 bg-muted rounded w-3/4" />
        </div>
      </CardContent>
    </Card>
  </div>
)

/* ------------------------------ Component ------------------------------ */

export default function JobDetailsDisplay({ jobId }: JobDetailsDisplayProps) {
  const api = useApi()
  const router = useRouter()
  const [job, setJob] = useState<JobDetails | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isApplying, setIsApplying] = useState(false)
  const [hasApplied, setHasApplied] = useState(false)
  const fetchedRef = useRef(false)
  const jobAppFlow = useJobApplicationFlow()

  useEffect(() => {
    if (fetchedRef.current) return
    fetchedRef.current = true

    const fetchJobDetails = async () => {
      try {
        setIsLoading(true)
        const response = await api.get(`/Job/filtered?SearchTerm=${encodeURIComponent(jobId)}`)
        const data = response.data || response

        if (data?.items?.length) {
          setJob(data.items[0])
          setError(null)

          const progressResponse = await jobAppFlow.getApplicationProgress(data.items[0].name, data.items[0].version)
          const progressData = progressResponse.data || progressResponse
          if (progressData?.jobApplication?.id) {
            setHasApplied(true)
          }
        } else {
          setError("Job not found")
        }
      } catch (err) {
        setError("Failed to load job details")
        console.error("Error fetching job details:", err)
      } finally {
        setIsLoading(false)
      }
    }

    fetchJobDetails()
  }, [jobId, api, jobAppFlow])

  const handleApplyNow = async () => {
    if (!job) return

    try {
      setIsApplying(true)
      toast.loading("Processing your application...", { id: "apply-job" })

      console.log("Applying for job:", job.name, job.version)
      const applyResponse = await jobAppFlow.applyForJob(job.name, job.version)

      if (!applyResponse.success && (applyResponse as any).status >= 400) {
        throw new Error((applyResponse as any).message || "Failed to apply for job")
      }

      toast.dismiss("apply-job")
      toast.success("Successfully applied for the job!", {
        description: "You can now proceed with the application steps.",
      })

      setHasApplied(true)

      setTimeout(() => {
        router.push(`/applications?jobId=${encodeURIComponent(job.name)}`)
      }, 1500)
    } catch (err) {
      console.error("Error applying for job:", err)
      toast.dismiss("apply-job")
      const errorMessage = err instanceof Error ? err.message : "Failed to apply for the job. Please try again."
      toast.error(errorMessage)
    } finally {
      setIsApplying(false)
    }
  }

  const getJobTypeColor = (type: string) => {
    switch (type) {
      case "FullTime":
        return "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300"
      case "PartTime":
        return "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300"
      case "Contract":
        return "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300"
      case "Internship":
        return "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300"
      default:
        return "bg-muted text-muted-foreground"
    }
  }

  if (isLoading) return <SkeletonLoader />

  if (!job || error) {
    return (
      <div className="py-20 text-center space-y-4">
        <AlertCircle className="mx-auto h-12 w-12 text-destructive" />
        <h2 className="text-xl font-semibold">Job Not Found</h2>
        <p className="text-muted-foreground">{error}</p>
        <Button onClick={() => router.push("/jobs")}>Back to Jobs</Button>
      </div>
    )
  }

  return (
    <div className="max-w-5xl mx-auto px-4">
      <Card className="border border-border/60 shadow-xl rounded-2xl overflow-hidden">
        <CardContent className="p-6 md:p-10 space-y-10">
          {/* Header */}
          <div className="space-y-4">
            <h1 className="text-3xl md:text-4xl font-bold tracking-tight">{job.jobTitle}</h1>

            <div className="flex flex-wrap gap-2">
              <span className={`px-3 py-1 rounded-full text-sm font-medium ${getJobTypeColor(job.jobType)}`}>
                {job.jobType}
              </span>

              <span className="flex items-center gap-1 px-3 py-1 rounded-full bg-muted text-sm">
                <GraduationCap className="h-4 w-4" />
                {job.experienceLevel} Level
              </span>

              {hasApplied && (
                <span className="flex items-center gap-1 px-3 py-1 rounded-full bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300 text-sm">
                  <CheckCircle2 className="h-4 w-4" />
                  Applied
                </span>
              )}
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 py-6 border-y">
            <Stat icon={Briefcase} label="Job Type" value={job.jobType} />
            <Stat icon={GraduationCap} label="Experience" value={job.experienceLevel} />
            <Stat icon={Clock} label="Posted" value={new Date(job.createdAt).toLocaleDateString()} />
            <Stat icon={Users} label="Max Applicants" value={job.maxAmountOfCandidatesRestriction} />
          </div>

          {/* Description */}
          <Section title="Job Description">
            <p className="leading-relaxed text-muted-foreground whitespace-pre-wrap">{job.jobDescription}</p>
          </Section>

          {/* Requirements */}
          <Section title="Minimum Requirements">
            <ul className="space-y-3">
              {job.minimumRequirements.map((req, i) => (
                <li key={i} className="flex gap-3">
                  <CheckCircle2 className="h-5 w-5 text-primary mt-0.5" />
                  <span>{req}</span>
                </li>
              ))}
            </ul>
          </Section>

          {/* Police Report */}
          {job.policeReportRequired && (
            <div className="rounded-xl border border-yellow-500/30 bg-yellow-50/50 dark:bg-yellow-900/10 p-4 flex gap-3">
              <AlertCircle className="h-5 w-5 text-yellow-600 mt-1" />
              <div>
                <p className="font-semibold">Police Report Required</p>
                <p className="text-sm text-muted-foreground">Background verification is mandatory for this role.</p>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      <div className="sticky bottom-0 mt-6 bg-background/90 backdrop-blur border-t px-4 py-4 rounded-xl shadow-lg flex gap-3">
        <Button onClick={handleApplyNow} disabled={isApplying || hasApplied} className="flex-1 h-12 text-base">
          {isApplying ? (
            <>
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              Applying...
            </>
          ) : hasApplied ? (
            <>
              <CheckCircle2 className="h-4 w-4 mr-2" />
              Already Applied
            </>
          ) : (
            "Apply Now"
          )}
        </Button>

        <Button variant="outline" onClick={() => router.push("/jobs")} className="h-12">
          Back
        </Button>
      </div>
    </div>
  )
}

/* ------------------------------ Helpers ------------------------------ */

function Section({
  title,
  children,
}: {
  title: string
  children: React.ReactNode
}) {
  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold">{title}</h2>
      {children}
    </div>
  )
}

function Stat({
  icon: Icon,
  label,
  value,
}: {
  icon: any
  label: string
  value: string | number
}) {
  return (
    <div className="flex items-center gap-3">
      <Icon className="h-5 w-5 text-primary" />
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="font-medium">{value}</p>
      </div>
    </div>
  )
}
