import { Metadata } from "next";
import Link from "next/link";
import { Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import JobsTable from "@/components/pages/_recruiter/recruiter/jobs/JobsTable/index";

export const metadata: Metadata = {
  title: "Jobs | Recruiter",
  description: "Manage job postings and openings",
};

export default function JobsPage() {
  return (
    <div className="container py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Jobs</h1>
          <p className="text-muted-foreground">
            Manage job postings and openings for your recruitment process
          </p>
        </div>
        
        <Link href="/recruiter/jobs/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            New Job
          </Button>
        </Link>
      </div>

      <JobsTable />
    </div>
  );
}

