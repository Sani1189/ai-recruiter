import { Metadata } from "next";
import AssessmentTemplateForm from "@/components/pages/_recruiter/recruiter/assessmentTemplates/AssessmentTemplateForm";

export const metadata: Metadata = {
  title: "Create Questionnaire Template | Recruiter",
  description: "Create a new questionnaire template",
};

export default function NewAssessmentTemplatePage() {
  return <AssessmentTemplateForm mode="create" />;
}


