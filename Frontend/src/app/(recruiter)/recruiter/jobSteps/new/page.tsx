import { Metadata } from "next";
import JobStepForm from "@/components/pages/_recruiter/recruiter/jobSteps/JobStepForm";

export const metadata: Metadata = {
  title: "Create Job Step | Recruiter",
  description: "Create a new job step template",
};

export default function NewJobStepPage() {
  return <JobStepForm mode="create" />;
}

