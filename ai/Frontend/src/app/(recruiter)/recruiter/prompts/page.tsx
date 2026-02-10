// removed client component
import { Metadata } from "next";
import Link from "next/link";
import { Plus } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import PromptsTable from "@/components/pages/_recruiter/recruiter/prompts/PromptsTable";

export const metadata: Metadata = {
  title: "Prompts | Recruiter Portal",
  description: "Manage reusable prompt templates for your recruitment process",
};

export default function PromptsPage() {
  return (
    <div className="container space-y-6 py-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Prompts</h1>
          <p className="text-muted-foreground">
            Manage prompt templates for your recruitment process
          </p>
        </div>
        <Link
          href="/recruiter/prompts/new"
          className={buttonVariants({ variant: "default" })}
        >
          <Plus className="mr-2 h-4 w-4" />
          New Prompt
        </Link>
      </div>

      {/* Prompts Table */}
      <PromptsTable />
    </div>
  );
}
