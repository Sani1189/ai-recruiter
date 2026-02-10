import { Metadata } from "next";
import Link from "next/link";
import { Download, Plus } from "lucide-react";

import AssessmentTemplatesTable from "@/components/pages/_recruiter/recruiter/assessmentTemplates/AssessmentTemplatesTable";
import { buttonVariants } from "@/components/ui/button";

export const metadata: Metadata = {
  title: "Questionnaire Templates | Recruiter Portal",
  description: "Manage reusable questionnaire templates (quiz, personality, forms)",
};

export default function AssessmentTemplatesPage() {
  return (
    <div className="container space-y-6 py-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Questionnaire Templates</h1>
          <p className="text-muted-foreground">
            Build reusable question templates for job steps (quiz, personality, forms)
          </p>
        </div>

        <div className="flex items-center gap-2">
          <a
            href="/questionnaire-import-template.xls"
            className={buttonVariants({ variant: "outline" })}
            download
          >
            <Download className="mr-2 h-4 w-4" />
            Download Excel Template
          </a>

          <Link
            href="/recruiter/assessmentTemplates/new"
            className={buttonVariants({ variant: "default" })}
          >
            <Plus className="mr-2 h-4 w-4" />
            New Template
          </Link>
        </div>
      </div>

      <AssessmentTemplatesTable />
    </div>
  );
}


