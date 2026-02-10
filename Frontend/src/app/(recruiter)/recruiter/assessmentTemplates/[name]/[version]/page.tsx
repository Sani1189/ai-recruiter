"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Edit } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { useApi } from "@/hooks/useApi";
import { AssessmentTemplate } from "@/types/assessmentTemplate";
import { formatDate } from "@/lib/utils";
import { toast } from "sonner";
import { TemplateCandidatePreviewStatic } from "@/components/reusable/assessmentBuilder/CandidatePreview";

export default function AssessmentTemplateDetailPage() {
  const params = useParams();
  const api = useApi();

  const [template, setTemplate] = useState<AssessmentTemplate | null>(null);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState("overview");

  const name = params.name as string;
  const version = Number.parseInt(params.version as string, 10);

  useEffect(() => {
    const fetchTemplate = async () => {
      try {
        const response = await api.get(`/QuestionnaireTemplate/${name}/${version}`);
        const data = response.data || response;
        if (data?.name) setTemplate(data);
      } catch (error) {
        toast.error("Failed to fetch questionnaire template: " + error);
      } finally {
        setLoading(false);
      }
    };

    if (name && Number.isFinite(version)) fetchTemplate();
    else setLoading(false);
  }, [name, version]);

  if (loading) return <div>Loading...</div>;
  if (!template) return <div>Template not found</div>;

  const sections = (template.sections ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
  const questionCount = sections.reduce((acc, s) => acc + (s.questions?.length ?? 0), 0);
  const requiredCount = sections.reduce((acc, s) => acc + (s.questions?.filter((q) => !!q.isRequired).length ?? 0), 0);
  const optionQuestionCount = sections.reduce(
    (acc, s) => acc + (s.questions?.filter((q) => (q.options?.length ?? 0) > 0).length ?? 0),
    0,
  );
  const optionCount = sections.reduce((acc, s) => acc + (s.questions?.reduce((a, q) => a + (q.options?.length ?? 0), 0) ?? 0), 0);

  const questionTypeCounts = (() => {
    const m = new Map<string, number>();
    for (const s of sections) {
      for (const q of s.questions ?? []) {
        const key = q.questionType ?? "Unknown";
        m.set(key, (m.get(key) ?? 0) + 1);
      }
    }
    return Array.from(m.entries()).sort((a, b) => b[1] - a[1]);
  })();

  const published = template.isPublished || String(template.status ?? "").toLowerCase() === "published";

  return (
    <div className="container py-8">
      <div className="max-w-5xl mx-auto space-y-6">
        <div>
          <Link
            href="/recruiter/assessmentTemplates"
            className={buttonVariants({ variant: "ghost" })}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Questionnaire Templates
          </Link>
        </div>

        <div className="flex items-start justify-between">
          <div className="space-y-2">
            <h1 className="text-3xl font-bold">{template.title?.trim() || template.name}</h1>
            {template.title?.trim() ? <div className="text-sm text-muted-foreground font-mono">{template.name}</div> : null}
            <div className="flex items-center gap-2">
              <Badge variant="secondary" className="font-mono">
                v{template.version}
              </Badge>
              <Badge variant="outline">{template.templateType}</Badge>
              {published ? (
                <Badge variant="default">Published</Badge>
              ) : (
                <Badge variant="secondary">Draft</Badge>
              )}
            </div>
          </div>

          <Link
            href={`/recruiter/assessmentTemplates/${template.name}/${template.version}/edit`}
            className={buttonVariants({ variant: "outline", size: "sm" })}
          >
            <Edit className="mr-2 h-4 w-4" />
            Edit Template
          </Link>
        </div>

        <Tabs value={tab} onValueChange={setTab}>
          <TabsList>
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="structure">Questions</TabsTrigger>
            <TabsTrigger value="candidate">Candidate preview</TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Overview</CardTitle>
              </CardHeader>
              <CardContent className="text-sm space-y-4">
                {template.description?.trim() ? (
                  <div>
                    <div className="font-medium">Description</div>
                    <div className="text-muted-foreground whitespace-pre-wrap">{template.description}</div>
                  </div>
                ) : null}

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  <div>
                    <div className="font-medium">Sections</div>
                    <div className="text-muted-foreground">{sections.length}</div>
                  </div>
                  <div>
                    <div className="font-medium">Questions</div>
                    <div className="text-muted-foreground">{questionCount}</div>
                  </div>
                  <div>
                    <div className="font-medium">Required</div>
                    <div className="text-muted-foreground">{requiredCount}</div>
                  </div>
                  <div>
                    <div className="font-medium">Options</div>
                    <div className="text-muted-foreground">
                      {optionCount} <span className="text-xs">({optionQuestionCount} question(s))</span>
                    </div>
                  </div>
                </div>

                {questionTypeCounts.length ? (
                  <div className="space-y-2">
                    <div className="font-medium">Question types</div>
                    <div className="flex flex-wrap gap-2">
                      {questionTypeCounts.map(([type, count]) => (
                        <Badge key={type} variant="secondary">
                          {type}: {count}
                        </Badge>
                      ))}
                    </div>
                  </div>
                ) : null}

                <Separator />

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  <div>
                    <div className="font-medium">Created</div>
                    <div className="text-muted-foreground">{template.createdAt ? formatDate(template.createdAt) : "-"}</div>
                  </div>
                  <div>
                    <div className="font-medium">Updated</div>
                    <div className="text-muted-foreground">{template.updatedAt ? formatDate(template.updatedAt) : "-"}</div>
                  </div>
                  <div>
                    <div className="font-medium">Status</div>
                    <div className="text-muted-foreground">{template.status ?? (published ? "Published" : "Draft")}</div>
                  </div>
                  <div>
                    <div className="font-medium">Published</div>
                    <div className="text-muted-foreground">
                      {template.publishedAt ? formatDate(template.publishedAt) : "-"}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="structure" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Sections &amp; Questions</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {sections.length ? (
                  sections.map((s) => {
                    const questions = (s.questions ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
                    return (
                      <div key={s.id} className="rounded-lg border p-4 space-y-3">
                        <div className="flex items-start justify-between gap-4">
                          <div>
                            <div className="text-sm font-semibold">
                              {s.title?.trim() || "Untitled section"}{" "}
                              <span className="text-muted-foreground font-normal">#{s.order}</span>
                            </div>
                            <div className="text-xs text-muted-foreground">{questions.length} question(s)</div>
                          </div>
                        </div>

                        <div className="space-y-3">
                          {questions.map((q) => {
                            const optionsCount = q.options?.length ?? 0;
                            return (
                              <div key={`${q.name}_${q.version}`} className="rounded-md border bg-muted/20 p-3">
                                <div className="flex flex-wrap items-center justify-between gap-2">
                                  <div className="min-w-0">
                                    <div className="text-sm font-medium">
                                      <span className="text-muted-foreground mr-2">Q{q.order}.</span>
                                      {q.promptText?.trim() || <span className="text-muted-foreground">No question</span>}
                                      {q.isRequired ? <span className="ml-1 text-destructive">*</span> : null}
                                    </div>
                                  </div>
                                  <div className="flex items-center gap-2">
                                    <Badge variant="outline">{q.questionType}</Badge>
                                    {optionsCount ? (
                                      <Badge variant="secondary">{optionsCount} option(s)</Badge>
                                    ) : (
                                      <Badge variant="secondary">Free text</Badge>
                                    )}
                                  </div>
                                </div>

                                {q.options?.length ? (
                                  <div className="mt-2 flex flex-wrap gap-2">
                                    {q.options
                                      .slice()
                                      .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
                                      .map((o) => (
                                        <div
                                          key={`${o.name}_${o.version}`}
                                          className="rounded-md border bg-background px-2 py-1 text-xs text-muted-foreground"
                                        >
                                          {o.label?.trim() || `Option ${o.order}`}
                                        </div>
                                      ))}
                                  </div>
                                ) : null}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })
                ) : (
                  <div className="text-sm text-muted-foreground">No sections yet.</div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="candidate" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Candidate preview</CardTitle>
              </CardHeader>
              <CardContent>
                <TemplateCandidatePreviewStatic template={template} />
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}




