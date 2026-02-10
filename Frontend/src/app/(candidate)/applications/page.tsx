"use client";

import ApplicationsTable from "@/components/pages/_candidate/applications/ApplicationsTable";

export default function CandidateApplicationsPage() {
  return (
    <div className="container py-8 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Applications</h1>
          <p className="text-muted-foreground">
            View and manage your job applications
          </p>
        </div>
      </div>

      {/* Applications Table */}
      <ApplicationsTable />
    </div>
  );
}


