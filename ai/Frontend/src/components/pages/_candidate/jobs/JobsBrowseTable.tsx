"use client"

import type React from "react"
import { useRouter } from "next/navigation"
import { useState } from "react"
import { Search, Filter, X, Briefcase, Loader2, CheckCircle2 } from "lucide-react"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useApi } from "@/hooks/useApi"
import type { PaginatedResponse } from "@/lib/api/config"
import { useServerTable, type ServerTableParams } from "@/hooks/useServerTable"
import { useJobApplicationFlow } from "@/hooks/useJobApplicationFlow"

// Experience levels matching backend
const EXPERIENCE_LEVELS = ["Entry", "Mid", "Senior", "Lead", "Executive"]

// Job types matching backend
const JOB_TYPES = ["FullTime", "PartTime", "Contract", "Internship"]

interface Job {
  name: string
  version: number
  jobTitle: string
  experienceLevel: string
  jobType: string
  jobDescription: string
  policeReportRequired: boolean
  createdAt: string
}

interface JobWithApplicationStatus extends Job {
  hasApplied?: boolean
}

export default function JobsBrowseTable() {
  const api = useApi()
  const router = useRouter()
  const jobAppFlow = useJobApplicationFlow()
  const [isFiltersOpen, setIsFiltersOpen] = useState(false)
  const [applyingJobId, setApplyingJobId] = useState<string | null>(null)
  const [appliedJobs, setAppliedJobs] = useState<Set<string>>(new Set())
  const [formFilters, setFormFilters] = useState({
    searchTerm: "",
    experienceLevel: "",
    jobType: "",
    policeReportRequired: "",
  })

  const fetchJobs = async (params: ServerTableParams): Promise<PaginatedResponse<JobWithApplicationStatus>> => {
    const queryParams = new URLSearchParams()

    queryParams.set("Page", params.page.toString())
    queryParams.set("PageSize", params.pageSize.toString())

    if (params.sortBy) {
      queryParams.set("SortBy", params.sortBy)
      queryParams.set("SortDescending", params.sortOrder === "desc" ? "true" : "false")
    }

    if (params.filters?.searchTerm) queryParams.set("SearchTerm", params.filters.searchTerm)
    if (params.filters?.experienceLevel) queryParams.set("ExperienceLevel", params.filters.experienceLevel)
    if (params.filters?.jobType) queryParams.set("JobType", params.filters.jobType)
    if (params.filters?.policeReportRequired)
      queryParams.set("PoliceReportRequired", params.filters.policeReportRequired)

    const response = await api.get(`/Job/filtered?${queryParams.toString()}`)
    const data = response.data || response

    const jobsWithStatus = await Promise.all(
      (data.items || []).map(async (job: Job) => {
        try {
          const progressResponse = await jobAppFlow.getApplicationProgress(job.name, job.version)
          const progressData = progressResponse.data || progressResponse
          const hasApplied = !!progressData?.jobApplication?.id

          if (hasApplied) {
            setAppliedJobs((prev) => new Set([...prev, `${job.name}-${job.version}`]))
          }

          return {
            ...job,
            hasApplied,
          }
        } catch {
          return { ...job, hasApplied: false }
        }
      }),
    )

    return {
      success: true,
      data: jobsWithStatus,
      pagination: {
        page: data.pageNumber || 1,
        pageSize: data.pageSize || 10,
        totalItems: data.totalCount || 0,
        totalPages: data.totalPages || 1,
        hasNext: (data.pageNumber || 1) < (data.totalPages || 1),
        hasPrevious: (data.pageNumber || 1) > 1,
      },
    }
  }

  const { data, isLoading, tableProps, onFilterChange } = useServerTable(fetchJobs, {
    initialPageSize: 10,
    initialFilters: {
      searchTerm: "",
      experienceLevel: "",
      jobType: "",
      policeReportRequired: "",
    },
    initialSortBy: "createdAt",
    initialSortOrder: "desc",
  })

  const handleFilterChange = (newFilters: any) => {
    onFilterChange(newFilters)
  }

  const handleSearch = () => {
    handleFilterChange(formFilters)
  }

  const handleClearFilters = () => {
    setFormFilters({
      searchTerm: "",
      experienceLevel: "",
      jobType: "",
      policeReportRequired: "",
    })

    handleFilterChange({
      searchTerm: "",
      experienceLevel: "",
      jobType: "",
      policeReportRequired: "",
    })
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch()
    }
  }

  const handleViewDetails = (jobName: string) => {
    router.push(`/jobs/${encodeURIComponent(jobName)}`)
  }

  const handleApplyClick = async (e: React.MouseEvent, job: JobWithApplicationStatus) => {
    e.stopPropagation()

    if (job.hasApplied) {
      router.push(`/applications?jobId=${encodeURIComponent(job.name)}`)
      return
    }

    try {
      setApplyingJobId(`${job.name}-${job.version}`)
      toast.loading(`Applying to ${job.jobTitle}...`, { id: "apply-browse" })

      const response = await jobAppFlow.applyForJob(job.name, job.version)

      if (!response.success && (response as any).status >= 400) {
        throw new Error((response as any).message || "Failed to apply for job")
      }

      toast.dismiss("apply-browse")
      toast.success("Application submitted!", {
        description: `You've successfully applied for ${job.jobTitle}`,
      })

      setAppliedJobs((prev) => new Set([...prev, `${job.name}-${job.version}`]))
      job.hasApplied = true

      // Redirect after brief delay
      setTimeout(() => {
        router.push(`/applications?jobId=${encodeURIComponent(job.name)}`)
      }, 1500)
    } catch (err) {
      console.error("Error applying for job:", err)
      toast.dismiss("apply-browse")
      const errorMessage = err instanceof Error ? err.message : "Failed to apply for this job. Please try again."
      toast.error(errorMessage)
    } finally {
      setApplyingJobId(null)
    }
  }

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle>Available Positions</CardTitle>
      </CardHeader>

      <CardContent className="space-y-4">
        <div className="flex items-center justify-end">
          <Button variant="outline" size="sm" onClick={() => setIsFiltersOpen(!isFiltersOpen)}>
            <Filter className="mr-2 h-4 w-4" />
            {isFiltersOpen ? "Hide Filters" : "Show Filters"}
          </Button>
        </div>

        {isFiltersOpen && (
          <div className="p-4 border rounded-lg bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-5 gap-2 items-end">
              <div className="space-y-2 w-full">
                <Input
                  id="search"
                  placeholder="Search by title, company, description..."
                  value={formFilters.searchTerm}
                  onChange={(e) => setFormFilters((prev) => ({ ...prev, searchTerm: e.target.value }))}
                  onKeyPress={handleKeyPress}
                  className="h-9"
                />
              </div>

              <div className="space-y-2">
                <Select
                  value={formFilters.experienceLevel || undefined}
                  onValueChange={(value) =>
                    setFormFilters((prev) => ({ ...prev, experienceLevel: value === "all" ? "" : value }))
                  }
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All levels" />
                  </SelectTrigger>
                  <SelectContent>
                    {EXPERIENCE_LEVELS.map((level) => (
                      <SelectItem key={level} value={level}>
                        {level}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Select
                  value={formFilters.jobType || undefined}
                  onValueChange={(value) =>
                    setFormFilters((prev) => ({ ...prev, jobType: value === "all" ? "" : value }))
                  }
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="All types" />
                  </SelectTrigger>
                  <SelectContent>
                    {JOB_TYPES.map((type) => (
                      <SelectItem key={type} value={type}>
                        {type}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Select
                  value={formFilters.policeReportRequired || undefined}
                  onValueChange={(value) =>
                    setFormFilters((prev) => ({ ...prev, policeReportRequired: value === "all" ? "" : value }))
                  }
                >
                  <SelectTrigger className="h-9 w-full">
                    <SelectValue placeholder="Police report" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="true">Required</SelectItem>
                    <SelectItem value="false">Not Required</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="flex gap-2">
                <Button onClick={handleSearch} disabled={isLoading} className="h-9 flex-1">
                  <Search className="mr-2 h-4 w-4" />
                  Search
                </Button>
                <Button
                  variant="outline"
                  onClick={handleClearFilters}
                  disabled={isLoading}
                  className="h-9 bg-transparent"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        )}

        {isLoading ? (
          <div className="grid gap-4">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="rounded-lg border border-border bg-card p-6 animate-pulse">
                <div className="space-y-3">
                  <div className="h-6 bg-muted rounded w-3/4" />
                  <div className="h-4 bg-muted rounded w-full" />
                  <div className="h-4 bg-muted rounded w-2/3" />
                  <div className="flex gap-2 pt-2">
                    <div className="h-8 bg-muted rounded w-24" />
                    <div className="h-8 bg-muted rounded w-24" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : data.length === 0 ? (
          <div className="flex items-center justify-center py-12">
            <div className="text-center">
              <Briefcase className="h-12 w-12 mx-auto text-muted-foreground/50 mb-4" />
              <p className="text-muted-foreground">No jobs found matching your criteria</p>
            </div>
          </div>
        ) : (
          <div className="grid gap-4">
            {data.map((job) => {
              const jobKey = `${job.name}-${job.version}`
              const isApplying = applyingJobId === jobKey
              const hasApplied = job.hasApplied || appliedJobs.has(jobKey)

              return (
                <div
                  key={jobKey}
                  onClick={() => handleViewDetails(job.name)}
                  className="group rounded-lg border border-border bg-card p-6 transition-all duration-200 hover:shadow-lg hover:border-primary/50 dark:hover:border-primary/40 cursor-pointer hover:-translate-y-1"
                >
                  <div className="space-y-3">
                    {/* Job header */}
                    <div className="flex items-start justify-between gap-4">
                      <div className="flex-1">
                        <h3 className="text-lg font-semibold text-foreground group-hover:text-primary transition-colors">
                          {job.jobTitle}
                        </h3>
                      </div>
                      <div className="text-right flex flex-col gap-2">
                        <span
                          className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${
                            job.jobType === "FullTime"
                              ? "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300"
                              : job.jobType === "PartTime"
                                ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300"
                                : job.jobType === "Contract"
                                  ? "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300"
                                  : "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300"
                          }`}
                        >
                          {job.jobType}
                        </span>
                        {hasApplied && (
                          <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300 text-xs font-medium">
                            <CheckCircle2 className="h-3 w-3" />
                            Applied
                          </span>
                        )}
                      </div>
                    </div>

                    {/* Job description */}
                    <p className="text-sm text-foreground/80 line-clamp-2">{job.jobDescription}</p>

                    {/* Job details */}
                    <div className="flex flex-wrap gap-4 pt-2">
                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <Briefcase className="h-4 w-4" />
                        <span>{job.experienceLevel} Level</span>
                      </div>
                      {job.policeReportRequired && (
                        <div className="text-sm text-yellow-600 dark:text-yellow-500 font-medium">
                          Police Report Required
                        </div>
                      )}
                    </div>

                    <div className="pt-3 flex gap-2">
                      <Button
                        variant="default"
                        size="sm"
                        onClick={(e) => {
                          e.stopPropagation()
                          handleViewDetails(job.name)
                        }}
                      >
                        View Details
                      </Button>
                      <Button
                        variant={hasApplied ? "outline" : "default"}
                        size="sm"
                        disabled={isApplying}
                        onClick={(e) => handleApplyClick(e, job)}
                      >
                        {isApplying ? (
                          <>
                            <Loader2 className="h-4 w-4 mr-1 animate-spin" />
                            Applying...
                          </>
                        ) : hasApplied ? (
                          <>
                            <CheckCircle2 className="h-4 w-4 mr-1" />
                            Applied
                          </>
                        ) : (
                          "Apply Now"
                        )}
                      </Button>
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </CardContent>
    </Card>
  )
}
