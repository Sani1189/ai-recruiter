import type { Metadata } from "next"
import JobsBrowseTable from "@/components/pages/_candidate/jobs/JobsBrowseTable"

export const metadata: Metadata = {
  title: "Browse Jobs | Candidate",
  description: "Browse available job opportunities",
}

export default function CandidateJobsPage() {
  return (
    <div className="container py-8 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Browse Jobs</h1>
        <p className="text-muted-foreground">Explore available job opportunities matching your profile</p>
      </div>

      <JobsBrowseTable />
    </div>
  )
}
