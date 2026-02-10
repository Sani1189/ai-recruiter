"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";

import { useApi } from "@/hooks/useApi";
import { AssessmentTemplate } from "@/types/assessmentTemplate";
import AssessmentTemplateForm from "@/components/pages/_recruiter/recruiter/assessmentTemplates/AssessmentTemplateForm";

export default function EditAssessmentTemplatePage() {
  const params = useParams();
  const api = useApi();

  const [template, setTemplate] = useState<AssessmentTemplate | null>(null);
  const [loading, setLoading] = useState(true);

  const name = params.name as string;
  const version = Number.parseInt(params.version as string, 10);

  useEffect(() => {
    const fetchTemplate = async () => {
      try {
        const response = await api.get(`/QuestionnaireTemplate/${name}/${version}`);
        const data = response.data || response;
        if (data?.name) setTemplate(data);
      } catch (error) {
        console.error("Failed to fetch questionnaire template:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchTemplate();
  }, [name, version]);

  if (loading) return <div>Loading...</div>;
  if (!template) return <div>Template not found</div>;

  return <AssessmentTemplateForm template={template} mode="edit" />;
}


