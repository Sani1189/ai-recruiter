import { ArrowLeft } from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import JobDetailsDisplay from "@/components/pages/_candidate/jobs/JobDetailsDisplay"

interface JobDetailsPageProps {
  params: Promise<{
    jobId: string
  }>
}

export const metadata = {
  title: "Job Details",
  description: "View job posting details and apply",
}

export default async function JobDetailsPage({ params }: JobDetailsPageProps) {
  const { jobId } = await params

  return (
    <div className="min-h-screen bg-background py-8">
      <div className="container max-w-4xl">
        {/* Back button */}
        <div className="mb-6">
          <Link href="/jobs">
            <Button variant="ghost" size="sm" className="gap-2">
              <ArrowLeft className="h-4 w-4" />
              Back to Jobs
            </Button>
          </Link>
        </div>

        {/* Job details */}
        <JobDetailsDisplay jobId={decodeURIComponent(jobId)} />
      </div>
    </div>
  )
}
