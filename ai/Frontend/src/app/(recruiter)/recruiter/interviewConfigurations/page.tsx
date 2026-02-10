"use client";

import { useState } from "react";
import { Plus } from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import InterviewConfigurationsTable from "@/components/pages/_recruiter/recruiter/interviewConfigurations/InterviewConfigurationsTable";

export default function InterviewConfigurationsPage() {
  return (
    <div className="container py-8">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="space-y-1">
            <h1 className="text-3xl font-bold">Interview Configurations</h1>
            <p className="text-muted-foreground">
              Manage interview configuration templates for your recruitment process
            </p>
          </div>
          <Link href="/recruiter/interviewConfigurations/new">
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              New Configuration
            </Button>
          </Link>
        </div>

        {/* Table */}
        <InterviewConfigurationsTable />
      </div>
    </div>
  );
}
