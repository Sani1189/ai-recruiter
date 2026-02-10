"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import JobPostingsPipelineView from "@/components/pages/_recruiter/recruiter/jobs/JobPostingsPipelineView";
import { useAuthStore } from "@/stores/useAuthStore";

export default function JobsPage() {
  const { user } = useAuthStore();
  const [isLoading, setIsLoading] = useState(!user);

  useEffect(() => {
    if (user?.id) {
      setIsLoading(false);
    }
  }, [user?.id]);

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

      {!isLoading && user?.id && (
        <JobPostingsPipelineView recruiterId={user.id} />
      )}

      {isLoading && (
        <div className="text-center py-8 text-muted-foreground">
          Loading...
        </div>
      )}
    </div>
  );
}

