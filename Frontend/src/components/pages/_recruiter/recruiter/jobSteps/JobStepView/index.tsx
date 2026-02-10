"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft, Edit, ExternalLink } from "lucide-react";
import Link from "next/link";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { useApi } from "@/hooks/useApi";
import { formatDate } from "@/lib/utils";
import { JobPostStep } from "@/types/jobPostStep";

interface JobStepViewProps {
  step: JobPostStep;
}

type AssessmentTemplateSummary = {
  name: string;
  version: number;
  templateType?: string | null;
  status?: string | null;
  sectionsCount: number;
  questionsCount: number;
};

export default function JobStepView({ step }: JobStepViewProps) {
  const router = useRouter();
  const api = useApi();
  const [assessmentTemplate, setAssessmentTemplate] = useState<AssessmentTemplateSummary | null>(null);
  const [assessmentLoading, setAssessmentLoading] = useState(false);
  // Back-compat: older step type names: "Multiple Choice" / "Assessment" -> "Questionnaire"
  const rawStepType: string = (step as any)?.stepType ?? "";
  const displayStepType =
    rawStepType === "Multiple Choice" || rawStepType === "Assessment" ? "Questionnaire" : rawStepType;

  useEffect(() => {
    const templateName = step.questionnaireTemplateName ?? step.assessmentTemplateName;
    if (!templateName) {
      setAssessmentTemplate(null);
      return;
    }

    const fetchTemplate = async () => {
      setAssessmentLoading(true);
      try {
        const version = step.questionnaireTemplateVersion ?? step.assessmentTemplateVersion;
        const endpoint = version
          ? `/QuestionnaireTemplate/${encodeURIComponent(templateName)}/${version}`
          : `/QuestionnaireTemplate/${encodeURIComponent(templateName)}/latest`;

        const response = await api.get(endpoint);
        const t = (response?.data ?? response) as any;

        const sections = Array.isArray(t?.sections) ? t.sections : [];
        const sectionsCount =
          typeof t?.sectionsCount === "number" ? t.sectionsCount : sections.length;
        const questionsCount =
          typeof t?.questionsCount === "number"
            ? t.questionsCount
            : sections.reduce(
                (acc: number, s: any) =>
                  acc + (Array.isArray(s?.questions) ? s.questions.length : 0),
                0,
              );

        setAssessmentTemplate({
          name: t?.name ?? templateName,
          version: t?.version ?? version ?? 0,
          templateType: t?.templateType ?? null,
          status: t?.status ?? null,
          sectionsCount,
          questionsCount,
        });
      } catch (error) {
        console.error("Failed to fetch questionnaire template:", error);
        setAssessmentTemplate(null);
      } finally {
        setAssessmentLoading(false);
      }
    };

    fetchTemplate();
  }, [step.questionnaireTemplateName, step.questionnaireTemplateVersion, step.assessmentTemplateName, step.assessmentTemplateVersion]);

  return (
    <div className="container py-8">
      <div className="max-w-4xl mx-auto space-y-6">
        <div>
          <Link
            href="/recruiter/jobSteps"
            className={buttonVariants({ variant: "ghost" })}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Job Steps
          </Link>
        </div>

        <div className="space-y-4">
          <div className="flex items-start justify-between">
            <div className="space-y-2">
              <h1 className="text-3xl font-bold">{step.name}</h1>
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="font-mono">
                  v{step.version}
                </Badge>
                <Badge variant="outline" className="capitalize">
                  {displayStepType}
                </Badge>
              </div>
            </div>
            
            <Link
              href={`/recruiter/jobSteps/${step.name}/${step.version}/edit`}
              className={buttonVariants({ variant: "outline", size: "sm" })}
            >
              <Edit className="mr-2 h-4 w-4" />
              Edit Job Step
            </Link>
          </div>
        </div>

        {/* Row 1 */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Basic Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="text-sm font-medium text-muted-foreground">Step Type</div>
                <Badge variant="outline" className="mt-1">
                  {displayStepType}
                </Badge>
              </div>

              <div>
                <div className="text-sm font-medium text-muted-foreground">Participant</div>
                <Badge variant="secondary" className="mt-1">
                  {step.participant}
                </Badge>
              </div>

              <div>
                <div className="text-sm font-medium text-muted-foreground">Show Step to Candidate</div>
                <Badge variant={step.showStepForCandidate ? "default" : "secondary"} className="mt-1">
                  {step.showStepForCandidate ? "Yes" : "No"}
                </Badge>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Candidate Display</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="text-sm font-medium text-muted-foreground">Display title</div>
                <div className="mt-1 font-medium">
                  {step.showStepForCandidate ? (step.displayTitle || "-") : "-"}
                </div>
              </div>

              <div>
                <div className="text-sm font-medium text-muted-foreground">Display content</div>
                <div className="mt-1 text-sm text-muted-foreground whitespace-pre-wrap">
                  {step.showStepForCandidate ? (step.displayContent || "-") : "-"}
                </div>
              </div>

              <div>
                <div className="text-sm font-medium text-muted-foreground">Show spinner</div>
                <Badge variant={step.showStepForCandidate && step.showSpinner ? "default" : "secondary"} className="mt-1">
                  {step.showStepForCandidate && step.showSpinner ? "Yes" : "No"}
                </Badge>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg">Interview Configuration</CardTitle>
                {step.interviewConfigurationName && (
                  <Link
                    href={
                      step.interviewConfigurationVersion
                        ? `/recruiter/interviewConfigurations/${step.interviewConfigurationName}/${step.interviewConfigurationVersion}`
                        : `/recruiter/interviewConfigurations/${step.interviewConfigurationName}/latest`
                    }
                    className={buttonVariants({ variant: "ghost", size: "sm" })}
                  >
                    <ExternalLink className="h-4 w-4 mr-2" />
                    View Details
                  </Link>
                )}
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {step.interviewConfigurationName ? (
                <>
                  <div>
                    <div className="text-sm font-medium text-muted-foreground">Configuration Name</div>
                    <div className="mt-1 font-medium">{step.interviewConfigurationName}</div>
                  </div>

                  <div>
                    <div className="text-sm font-medium text-muted-foreground">Configuration Version</div>
                    <Badge variant="secondary" className="mt-1 font-mono">
                      {step.interviewConfigurationVersion ? `v${step.interviewConfigurationVersion}` : 'Latest'}
                    </Badge>
                  </div>
                </>
              ) : (
                <div className="text-sm text-muted-foreground">
                  No interview configuration assigned
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Row 2 */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg">Prompt</CardTitle>
                {step.promptName && (
                  <Link
                    href={
                      step.promptVersion
                        ? `/recruiter/prompts/${step.promptName}/${step.promptVersion}`
                        : `/recruiter/prompts/${step.promptName}/latest`
                    }
                    target="_blank"
                    rel="noopener noreferrer"
                    className={buttonVariants({ variant: "ghost", size: "sm" })}
                  >
                    <ExternalLink className="h-4 w-4 mr-2" />
                    Open Prompt
                  </Link>
                )}
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {step.promptName ? (
                <>
                  <div>
                    <div className="text-sm font-medium text-muted-foreground">Prompt Name</div>
                    <Link
                      href={
                        step.promptVersion
                          ? `/recruiter/prompts/${step.promptName}/${step.promptVersion}`
                          : `/recruiter/prompts/${step.promptName}/latest`
                      }
                      target="_blank"
                      rel="noopener noreferrer"
                      className="mt-1 inline-flex items-center gap-2 font-medium hover:underline"
                    >
                      {step.promptName}
                      <ExternalLink className="h-4 w-4" />
                    </Link>
                  </div>

                  <div>
                    <div className="text-sm font-medium text-muted-foreground">Prompt Version</div>
                    <Badge variant="secondary" className="mt-1 font-mono">
                      {step.promptVersion ? `v${step.promptVersion}` : "Latest"}
                    </Badge>
                  </div>
                </>
              ) : (
                <div className="text-sm text-muted-foreground">No prompt assigned</div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg">Questionnaire Template</CardTitle>
                {assessmentTemplate?.name && assessmentTemplate.version > 0 && (
                  <Link
                    href={`/recruiter/assessmentTemplates/${encodeURIComponent(assessmentTemplate.name)}/${assessmentTemplate.version}`}
                    className={buttonVariants({ variant: "ghost", size: "sm" })}
                  >
                    <ExternalLink className="h-4 w-4 mr-2" />
                    View Details
                  </Link>
                )}
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {!(step.questionnaireTemplateName ?? step.assessmentTemplateName) ? (
                <div className="text-sm text-muted-foreground">No questionnaire template assigned</div>
              ) : assessmentLoading ? (
                <div className="text-sm text-muted-foreground">Loading questionnaire template...</div>
              ) : (
                <>
                  <div>
                    <div className="text-sm font-medium text-muted-foreground">Template Name</div>
                    <div className="mt-1 font-medium">
                      {step.questionnaireTemplateName ?? step.assessmentTemplateName}
                    </div>
                  </div>

                  <div>
                    <div className="text-sm font-medium text-muted-foreground">Template Version</div>
                    <Badge variant="secondary" className="mt-1 font-mono">
                      {(step.questionnaireTemplateVersion ?? step.assessmentTemplateVersion)
                        ? `v${step.questionnaireTemplateVersion ?? step.assessmentTemplateVersion}`
                        : "Latest"}
                    </Badge>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <div className="text-sm font-medium text-muted-foreground">Sections</div>
                      <div className="mt-1 font-medium">{assessmentTemplate?.sectionsCount ?? "-"}</div>
                    </div>
                    <div>
                      <div className="text-sm font-medium text-muted-foreground">Questions</div>
                      <div className="mt-1 font-medium">{assessmentTemplate?.questionsCount ?? "-"}</div>
                    </div>
                  </div>

                  {(assessmentTemplate?.templateType || assessmentTemplate?.status) && (
                    <div className="flex flex-wrap gap-2">
                      {assessmentTemplate?.templateType && (
                        <Badge variant="outline">{assessmentTemplate.templateType}</Badge>
                      )}
                      {assessmentTemplate?.status && (
                        <Badge variant="secondary">{assessmentTemplate.status}</Badge>
                      )}
                    </div>
                  )}
                </>
              )}
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Metadata</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <div className="text-sm font-medium text-muted-foreground">Created At</div>
              <div className="mt-1">{step.createdAt ? formatDate(step.createdAt) : '-'}</div>
            </div>

            <div>
              <div className="text-sm font-medium text-muted-foreground">Created By</div>
              <div className="mt-1">{step.createdBy || '-'}</div>
            </div>

            <div>
              <div className="text-sm font-medium text-muted-foreground">Updated At</div>
              <div className="mt-1">{step.updatedAt ? formatDate(step.updatedAt) : '-'}</div>
            </div>

            <div>
              <div className="text-sm font-medium text-muted-foreground">Updated By</div>
              <div className="mt-1">{step.updatedBy || '-'}</div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

