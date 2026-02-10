import { Metadata } from "next";
import Link from "next/link";
import { Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import JobStepsTable from "@/components/pages/_recruiter/recruiter/jobSteps/JobStepsTable";

export const metadata: Metadata = {
  title: "Job Steps | Recruiter",
  description: "Manage reusable job step templates",
};

export default function JobStepsPage() {
  return (
    <div className="container py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Job Steps</h1>
          <p className="text-muted-foreground">
            Manage reusable job step templates for your recruitment process
          </p>
        </div>
        
        <Link href="/recruiter/jobSteps/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            New Job Step
          </Button>
        </Link>
      </div>

      <JobStepsTable />
    </div>
  );
}

