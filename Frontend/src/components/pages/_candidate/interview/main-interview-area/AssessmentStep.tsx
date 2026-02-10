"use client";

import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";
import { ChevronLeft, ChevronRight } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { useApi } from "@/hooks/useApi";
import { cn } from "@/lib/utils";

// Simple in-memory caches to avoid re-fetching the same template and download urls when navigating steps.
const templateCache = new Map<string, CandidateAssessmentTemplate>();
const templatePromiseCache = new Map<string, Promise<CandidateAssessmentTemplate>>();
const downloadUrlCache = new Map<string, string>();

type CandidateAssessmentOption = {
  name: string;
  version: number;
  order: number;
  label?: string | null;
  mediaUrl?: string | null;
  mediaFileId?: string | null;
};

type CandidateAssessmentQuestion = {
  name: string;
  version: number;
  order: number;
  questionType: string;
  isRequired: boolean;
  promptText?: string | null;
  mediaUrl?: string | null;
  mediaFileId?: string | null;
  options: CandidateAssessmentOption[];
};

type CandidateAssessmentSection = {
  id: string;
  order: number;
  title?: string | null;
  description?: string | null;
  questions: CandidateAssessmentQuestion[];
};

type CandidateAssessmentTemplate = {
  name: string;
  version: number;
  templateType: string;
  title?: string | null;
  description?: string | null;
  timeLimitSeconds?: number | null;
  sections: CandidateAssessmentSection[];
};

type AnswerState = {
  answerText?: string;
  selectedOptionIds?: string[];
};

const OPTION_TYPES = new Set(["SingleChoice", "MultiChoice", "Likert", "Radio", "Checkbox", "Dropdown"]);
const SINGLE_SELECT_TYPES = new Set(["SingleChoice", "Radio", "Dropdown", "Likert"]);

function typeNeedsOptions(t?: string | null) {
  return OPTION_TYPES.has((t ?? "").trim());
}

function isSingleSelect(t?: string | null) {
  return SINGLE_SELECT_TYPES.has((t ?? "").trim());
}

function optionText(opt: CandidateAssessmentOption, index: number) {
  const t = (opt?.label ?? "").toString().trim();
  return t || `Option ${index + 1}`;
}

export default function AssessmentStep({
  jobApplicationStepId,
  isCompleted,
  onSubmitted,
}: {
  jobApplicationStepId: string | null;
  isCompleted: boolean;
  onSubmitted: () => void;
}) {
  const { get, post } = useApi();
  const [template, setTemplate] = useState<CandidateAssessmentTemplate | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [answers, setAnswers] = useState<Record<string, AnswerState>>({});
  const [errorsByQuestionId, setErrorsByQuestionId] = useState<Record<string, string>>({});
  const [downloadUrlByFileId, setDownloadUrlByFileId] = useState<Record<string, string>>({});
  const [fetchedForStepId, setFetchedForStepId] = useState<string | null>(null);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);

  const questions = useMemo(() => {
    const sections = template?.sections ?? [];
    return sections
      .slice()
      .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
      .flatMap((s) => (s.questions ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0)));
  }, [template]);

  const questionsWithSection = useMemo(() => {
    const sections = (template?.sections ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
    const out: { question: CandidateAssessmentQuestion; sectionTitle: string | null }[] = [];
    for (const section of sections) {
      const sectionQuestions = (section.questions ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
      const title = (section.title ?? "").trim() || null;
      for (const q of sectionQuestions) {
        out.push({ question: q, sectionTitle: title });
      }
    }
    return out;
  }, [template]);

  // Reset form state when switching to a different step so we never show another step's answers.
  useEffect(() => {
    setAnswers({});
    setErrorsByQuestionId({});
    setCurrentQuestionIndex(0);
  }, [jobApplicationStepId]);

  const fileIdsToResolve = useMemo(() => {
    const ids = new Set<string>();
    for (const q of questions) {
      if (q.mediaFileId) ids.add(q.mediaFileId);
      for (const o of q.options ?? []) {
        if (o.mediaFileId) ids.add(o.mediaFileId);
      }
    }
    return Array.from(ids);
  }, [questions]);

  useEffect(() => {
    if (!jobApplicationStepId) return;
    if (fetchedForStepId === jobApplicationStepId) return;
    let cancelled = false;
    const run = async () => {
      try {
        setLoading(true);
        setLoadError(null);
        const cached = templateCache.get(jobApplicationStepId);
        if (cached) {
          setTemplate(cached);
          setFetchedForStepId(jobApplicationStepId);
          return;
        }

        const existingPromise = templatePromiseCache.get(jobApplicationStepId);
        const promise =
          existingPromise ??
          (async () => {
            const res = await get(`/candidate/questionnaire/steps/${jobApplicationStepId}/template`);
            const data = (res as any)?.data ?? res;
            return data as CandidateAssessmentTemplate;
          })();

        templatePromiseCache.set(jobApplicationStepId, promise);
        const data = await promise;
        if (cancelled) return;
        templateCache.set(jobApplicationStepId, data);
        setTemplate(data);
        setFetchedForStepId(jobApplicationStepId);
      } catch (e) {
        console.error(e);
        if (!cancelled) {
          setTemplate(null);
          setLoadError("Failed to load assessment questions. Please try again.");
        }
        toast.error("Failed to load assessment questions");
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void run();
    return () => {
      cancelled = true;
    };
  }, [get, jobApplicationStepId, fetchedForStepId]);

  useEffect(() => {
    if (fileIdsToResolve.length === 0) return;
    let cancelled = false;
    const run = async () => {
      const missing = fileIdsToResolve.filter((id) => !downloadUrlCache.has(id));
      if (missing.length === 0) {
        setDownloadUrlByFileId((prev) => {
          const next = { ...prev };
          let changed = false;
          for (const id of fileIdsToResolve) {
            const cached = downloadUrlCache.get(id);
            if (cached && !next[id]) {
              next[id] = cached;
              changed = true;
            }
          }
          return changed ? next : prev;
        });
        return;
      }
      try {
        setDownloadUrlByFileId((prev) => {
          const next = { ...prev };
          let changed = false;
          for (const id of fileIdsToResolve) {
            const cached = downloadUrlCache.get(id);
            if (cached && !next[id]) {
              next[id] = cached;
              changed = true;
            }
          }
          return changed ? next : prev;
        });

        const results = await Promise.all(
          missing.map(async (id) => {
            const res = await get(`/File/${id}/download-url`);
            const data = (res as any)?.data ?? res;
            return { id, url: data?.downloadUrl as string | undefined };
          })
        );
        if (cancelled) return;
        setDownloadUrlByFileId((prev) => {
          const next = { ...prev };
          for (const r of results) {
            if (r?.id && r?.url) {
              next[r.id] = r.url;
              downloadUrlCache.set(r.id, r.url);
            }
          }
          return next;
        });
      } catch {
        // ignore
      }
    };
    void run();
    return () => {
      cancelled = true;
    };
  }, [get, fileIdsToResolve]);

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    for (const q of questions) {
      if (!q.isRequired) continue;
      const a = answers[q.name] ?? {};
      if (typeNeedsOptions(q.questionType)) {
        const count = (a.selectedOptionIds ?? []).length;
        if (count < 1) nextErrors[q.name] = "This question is required.";
      } else {
        if (!a.answerText || !a.answerText.trim()) nextErrors[q.name] = "This question is required.";
      }
    }
    setErrorsByQuestionId(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const validateCurrentQuestion = (): boolean => {
    const item = questionsWithSection[currentQuestionIndex];
    if (!item) return true;
    const q = item.question;
    if (!q.isRequired) return true;
    const a = answers[q.name] ?? {};
    let valid = true;
    let err = "";
    if (typeNeedsOptions(q.questionType)) {
      const count = (a.selectedOptionIds ?? []).length;
      if (count < 1) {
        valid = false;
        err = "Please select an answer before continuing.";
      }
    } else {
      if (!a.answerText || !a.answerText.trim()) {
        valid = false;
        err = "Please enter your answer before continuing.";
      }
    }
    if (err) {
      setErrorsByQuestionId((prev) => ({ ...prev, [q.name]: err }));
    } else {
      setErrorsByQuestionId((prev) => {
        const next = { ...prev };
        delete next[q.name];
        return next;
      });
    }
    if (!valid) toast.error(err);
    return valid;
  };

  const goNext = () => {
    if (!validateCurrentQuestion()) return;
    setCurrentQuestionIndex((i) => Math.min(i + 1, questions.length - 1));
  };

  const goPrev = () => {
    setCurrentQuestionIndex((i) => Math.max(i - 1, 0));
  };

  const submit = async () => {
    if (!jobApplicationStepId) return;
    if (!validate()) {
      toast.error("Please answer all required questions");
      return;
    }

    try {
      setSubmitting(true);
      const payload = {
        answers: questions.map((q) => ({
          questionName: q.name,
          questionVersion: q.version,
          answerText: answers[q.name]?.answerText ?? null,
          selectedOptions: (answers[q.name]?.selectedOptionIds ?? [])
            .map((optName) => (q.options ?? []).find((o) => o.name === optName))
            .filter(Boolean)
            .map((o) => ({
              optionName: (o as CandidateAssessmentOption).name,
              optionVersion: (o as CandidateAssessmentOption).version,
            })),
        })),
      };
      await post(`/candidate/questionnaire/steps/${jobApplicationStepId}/submit`, payload);
      onSubmitted();
    } catch (e: any) {
      console.error(e);
      const message =
        e?.response?.data?.message ??
        e?.message ??
        "Failed to submit assessment";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  if (!jobApplicationStepId) {
    return (
      <Card className="rounded-xl border border-border bg-card shadow-sm">
        <CardContent className="py-10 text-center text-muted-foreground text-sm">
          Preparing questionnaire...
        </CardContent>
      </Card>
    );
  }

  if (loadError) {
    return (
      <Card className="rounded-xl border border-border bg-card shadow-sm">
        <CardHeader>
          <CardTitle className="text-base text-foreground">Questionnaire</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-destructive">{loadError}</p>
          <Button
            type="button"
            variant="outline"
            onClick={() => {
              setFetchedForStepId(null);
              setLoadError(null);
              if (jobApplicationStepId) {
                templatePromiseCache.delete(jobApplicationStepId);
                templateCache.delete(jobApplicationStepId);
              }
            }}
            className="rounded-lg border-border text-foreground"
          >
            Retry
          </Button>
        </CardContent>
      </Card>
    );
  }

  if (loading || !template) {
    return (
      <Card className="rounded-xl border border-border bg-card shadow-sm">
        <CardContent className="py-12 flex flex-col items-center justify-center gap-4">
          <div className="h-9 w-9 animate-spin rounded-full border-2 border-primary border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading questionnaire...</p>
        </CardContent>
      </Card>
    );
  }

  if (isCompleted) {
    return (
      <Card className="rounded-xl border border-border bg-card shadow-sm overflow-hidden">
        <CardContent className="py-12 flex flex-col items-center justify-center gap-4 text-center">
          <div className="rounded-full bg-emerald-500/15 dark:bg-emerald-500/20 p-4">
            <svg
              className="h-8 w-8 text-emerald-600 dark:text-emerald-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <div className="space-y-1">
            <h3 className="text-lg font-semibold text-foreground">Questionnaire submitted</h3>
            <p className="text-sm text-muted-foreground">
              Thank you for completing this step. You can continue to the next part of your application.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  const totalQuestions = questionsWithSection.length;
  const currentItem = questionsWithSection[currentQuestionIndex];
  const isFirst = currentQuestionIndex === 0;
  const isLast = currentQuestionIndex === totalQuestions - 1;

  if (totalQuestions === 0) {
    return (
      <Card className="rounded-xl border border-border bg-card shadow-sm">
        <CardContent className="py-10 text-center text-muted-foreground">
          No questions in this questionnaire.
        </CardContent>
      </Card>
    );
  }

  const q = currentItem.question;
  const qId = q.name;
  const a = answers[qId] ?? {};
  const err = errorsByQuestionId[qId] ?? "";
  const qImgSrc = (q.mediaFileId && downloadUrlByFileId[q.mediaFileId]) || q.mediaUrl || null;
  const opts = (q.options ?? []).slice().sort((x, y) => (x.order ?? 0) - (y.order ?? 0));

  return (
    <Card className="rounded-xl border border-border bg-card shadow-sm overflow-hidden">
      {/* Progress */}
      <div className="px-4 sm:px-6 pt-5 pb-2 border-b border-border bg-muted/30">
        <div className="flex items-center justify-between text-sm">
          <span className="font-medium text-foreground">
            Question {currentQuestionIndex + 1} of {totalQuestions}
          </span>
          <span className="text-muted-foreground tabular-nums">
            {Math.round(((currentQuestionIndex + 1) / totalQuestions) * 100)}%
          </span>
        </div>
        <div className="mt-2 h-1.5 w-full rounded-full bg-muted overflow-hidden">
          <div
            className="h-full rounded-full bg-primary transition-all duration-300 ease-out"
            style={{ width: `${((currentQuestionIndex + 1) / totalQuestions) * 100}%` }}
          />
        </div>
      </div>

      <CardContent className="px-4 sm:px-6 py-6 sm:py-8">
        {/* Section title when applicable */}
        {currentItem.sectionTitle && (
          <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground mb-3">
            {currentItem.sectionTitle}
          </p>
        )}

        {/* Question prompt */}
        <h3 className="text-lg sm:text-xl font-semibold text-foreground leading-snug mb-4">
          {q.promptText?.trim() || <span className="text-muted-foreground italic">No question text</span>}
          {q.isRequired ? <span className="text-destructive ml-0.5" aria-label="Required">*</span> : null}
        </h3>

        {qImgSrc ? (
          <div className="mb-5 rounded-lg border border-border overflow-hidden bg-muted/20">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={qImgSrc} alt="" className="max-h-64 w-full object-contain" />
          </div>
        ) : null}

        {/* Answer area */}
        <div className="space-y-4">
          {!typeNeedsOptions(q.questionType) ? (
            q.questionType === "Textarea" ? (
              <Textarea
                rows={5}
                value={a.answerText ?? ""}
                onChange={(e) =>
                  setAnswers((prev) => ({ ...prev, [qId]: { ...prev[qId], answerText: e.target.value } }))
                }
                placeholder="Type your answer here..."
                className={cn(
                  "rounded-lg border-border bg-background text-foreground placeholder:text-muted-foreground resize-none transition-colors",
                  err && "border-destructive focus-visible:ring-destructive"
                )}
              />
            ) : (
              <Input
                value={a.answerText ?? ""}
                onChange={(e) =>
                  setAnswers((prev) => ({ ...prev, [qId]: { ...prev[qId], answerText: e.target.value } }))
                }
                placeholder="Type your answer here..."
                className={cn(
                  "rounded-lg border-border bg-background text-foreground placeholder:text-muted-foreground h-11",
                  err && "border-destructive focus-visible:ring-destructive"
                )}
              />
            )
          ) : isSingleSelect(q.questionType) ? (
            <RadioGroup
              value={(a.selectedOptionIds ?? [])[0] ?? ""}
              onValueChange={(v) =>
                setAnswers((prev) => ({ ...prev, [qId]: { ...prev[qId], selectedOptionIds: v ? [v] : [] } }))
              }
              className="gap-3"
            >
              {opts.map((o, i) => {
                const imgSrc = (o.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o.mediaUrl || null;
                return (
                  <label
                    key={o.name ?? i}
                    className={cn(
                      "flex items-center gap-3 rounded-lg border border-border bg-card px-4 py-3 cursor-pointer transition-all",
                      "hover:border-primary/50 hover:bg-muted/40 focus-within:ring-2 focus-within:ring-primary/20",
                      (a.selectedOptionIds ?? []).includes(o.name) && "border-primary bg-primary/5"
                    )}
                  >
                    <RadioGroupItem value={o.name} className="flex-shrink-0" />
                    {imgSrc ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img src={imgSrc} alt="" className="h-12 w-12 rounded-md border border-border object-cover flex-shrink-0" />
                    ) : null}
                    <span className="text-sm font-medium text-foreground min-w-0">{optionText(o, i)}</span>
                  </label>
                );
              })}
            </RadioGroup>
          ) : (
            <div className="space-y-2">
              {opts.map((o, i) => {
                const selected = (a.selectedOptionIds ?? []).includes(o.name);
                const imgSrc = (o.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o.mediaUrl || null;
                return (
                  <label
                    key={o.name ?? i}
                    className={cn(
                      "flex items-center gap-3 rounded-lg border border-border bg-card px-4 py-3 cursor-pointer transition-all",
                      "hover:border-primary/50 hover:bg-muted/40 focus-within:ring-2 focus-within:ring-primary/20",
                      selected && "border-primary bg-primary/5"
                    )}
                  >
                    <Checkbox
                      checked={selected}
                      onCheckedChange={(checked) => {
                        setAnswers((prev) => {
                          const current = prev[qId]?.selectedOptionIds ?? [];
                          const next = checked
                            ? Array.from(new Set([...current, o.name]))
                            : current.filter((x) => x !== o.name);
                          return { ...prev, [qId]: { ...prev[qId], selectedOptionIds: next } };
                        });
                      }}
                      className="flex-shrink-0"
                    />
                    {imgSrc ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img src={imgSrc} alt="" className="h-12 w-12 rounded-md border border-border object-cover flex-shrink-0" />
                    ) : null}
                    <span className="text-sm font-medium text-foreground min-w-0">{optionText(o, i)}</span>
                  </label>
                );
              })}
            </div>
          )}

          {err ? (
            <p className="text-sm text-destructive font-medium" role="alert">
              {err}
            </p>
          ) : null}
        </div>
      </CardContent>

      {/* Navigation */}
      <div className="px-4 sm:px-6 py-4 border-t border-border bg-muted/20 flex items-center justify-between gap-4">
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={goPrev}
          disabled={isFirst}
          className="rounded-lg border-border text-foreground hover:bg-muted gap-1.5"
        >
          <ChevronLeft className="h-4 w-4" />
          Previous
        </Button>
        {isLast ? (
          <Button
            type="button"
            onClick={submit}
            disabled={submitting}
            className="rounded-lg bg-primary text-primary-foreground hover:bg-primary/90 gap-1.5"
          >
            {submitting ? "Submitting..." : "Submit"}
          </Button>
        ) : (
          <Button
            type="button"
            onClick={goNext}
            className="rounded-lg bg-primary text-primary-foreground hover:bg-primary/90 gap-1.5"
          >
            Next
            <ChevronRight className="h-4 w-4" />
          </Button>
        )}
      </div>
    </Card>
  );
}

