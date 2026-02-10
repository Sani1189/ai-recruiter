"use client";

import { useEffect, useMemo, useState } from "react";
import { UseFormReturn, useWatch } from "react-hook-form";

import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Checkbox } from "@/components/ui/checkbox";
import { useApi } from "@/hooks/useApi";
import { AssessmentTemplateFormData } from "@/schemas/assessmentTemplate.schema";
import { AssessmentQuestion, AssessmentTemplate } from "@/types/assessmentTemplate";

function optionText(opt: any, index: number) {
  const t = (opt?.label ?? "").toString().trim();
  return t || `Option ${index + 1}`;
}

function useDownloadUrlCache(fileIdsToResolve: string[]) {
  const api = useApi();
  const [downloadUrlByFileId, setDownloadUrlByFileId] = useState<Record<string, string>>({});

  useEffect(() => {
    let cancelled = false;
    const run = async () => {
      const missing = fileIdsToResolve.filter((id) => !downloadUrlByFileId[id]);
      if (missing.length === 0) return;
      try {
        const results = await Promise.all(
          missing.map(async (id) => {
            const res = await api.get(`/File/${id}/download-url`);
            const data = (res as any)?.data ?? res;
            const url = data?.downloadUrl as string | undefined;
            return { id, url };
          }),
        );
        if (cancelled) return;
        setDownloadUrlByFileId((prev) => {
          const next = { ...prev };
          for (const r of results) {
            if (r?.id && r?.url) next[r.id] = r.url;
          }
          return next;
        });
      } catch {
        // ignore: if policy blocks download-url, fallback to blob url
      }
    };
    void run();
    return () => {
      cancelled = true;
    };
  }, [api, fileIdsToResolve, downloadUrlByFileId]);

  return downloadUrlByFileId;
}

export function QuestionCandidatePreview({
  form,
  qPath,
}: {
  form: UseFormReturn<AssessmentTemplateFormData, any, AssessmentTemplateFormData>;
  qPath: string;
}) {
  const questionType = (useWatch({ control: form.control, name: `${qPath}.questionType` as any }) as string | undefined) ?? "";
  const isRequired = !!useWatch({ control: form.control, name: `${qPath}.isRequired` as any });
  const promptText = ((useWatch({ control: form.control, name: `${qPath}.promptText` as any }) as string | null | undefined) ?? "").trim();
  const mediaUrl = (useWatch({ control: form.control, name: `${qPath}.mediaUrl` as any }) as string | null | undefined) ?? null;
  const mediaFileId = (useWatch({ control: form.control, name: `${qPath}.mediaFileId` as any }) as string | null | undefined) ?? null;
  const options = (useWatch({ control: form.control, name: `${qPath}.options` as any }) as any[] | undefined) ?? [];

  const needsOptions = useMemo(() => {
    return (
      questionType === "SingleChoice" ||
      questionType === "MultiChoice" ||
      questionType === "Radio" ||
      questionType === "Checkbox" ||
      questionType === "Likert" ||
      questionType === "Dropdown"
    );
  }, [questionType]);

  const isQuizSingle = questionType === "SingleChoice";
  const isQuizMulti = questionType === "MultiChoice";

  /**
   * Images are stored as stable blob URLs + File ids.
   * Blob URLs may not be publicly readable, so for preview we generate a short-lived SAS download URL.
   *
   * IMPORTANT: avoid hooks-in-loops; we fetch all needed fileIds in one effect and cache them.
   */
  const fileIdsToResolve = useMemo(() => {
    const ids = new Set<string>();
    if (mediaFileId) ids.add(mediaFileId);
    for (const o of options) {
      const id = o?.mediaFileId as string | null | undefined;
      if (id) ids.add(id);
    }
    return Array.from(ids);
  }, [mediaFileId, options]);

  const downloadUrlByFileId = useDownloadUrlCache(fileIdsToResolve);

  const questionImgSrc = (mediaFileId && downloadUrlByFileId[mediaFileId]) || mediaUrl;

  return (
    <div className="rounded-lg border bg-background p-4 space-y-4">
      <div className="space-y-1">
        <div className="text-sm font-medium">
          {promptText || <span className="text-muted-foreground">No question</span>}
          {isRequired && <span className="ml-1 text-destructive">*</span>}
        </div>
      </div>

      {questionImgSrc && (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={questionImgSrc} alt="question" className="max-h-56 w-auto rounded-md border object-contain" />
      )}

      {/* Render as a real form. Disable interaction only (pointer-events), not via `disabled` (which changes visuals). */}
      <div className="pointer-events-none select-none">
        {!needsOptions && (
          <div className="space-y-2">
            {questionType === "Textarea" ? (
              <Textarea rows={4} placeholder="Candidate answer..." />
            ) : (
              <Input placeholder="Candidate answer..." />
            )}
          </div>
        )}

        {needsOptions && (
          <div className="space-y-2">
            {(isQuizSingle || isQuizMulti) ? (
              <div className="flex flex-wrap gap-2">
                {options.map((o, i) => {
                  const imgSrc = (o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl || null;
                  return (
                    <div
                      key={o?.id ?? i}
                      className="flex items-center gap-2 rounded-md border bg-background px-3 py-2 text-sm"
                    >
                      <span
                        className={
                          isQuizSingle
                            ? "h-4 w-4 shrink-0 rounded-full border"
                            : "h-4 w-4 shrink-0 rounded-[4px] border"
                        }
                      />
                      {imgSrc ? (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img src={imgSrc} alt="" className="h-8 w-8 rounded-md border object-cover" />
                      ) : null}
                      <span className="min-w-0">{optionText(o, i)}</span>
                    </div>
                  );
                })}
              </div>
            ) : questionType === "Dropdown" ? (
              <select className="h-9 w-full rounded-md border bg-background px-3 text-sm">
                <option value="">Select an option...</option>
                {options.map((o, i) => (
                  <option key={o?.id ?? i} value={optionText(o, i)}>
                    {optionText(o, i)}
                  </option>
                ))}
              </select>
            ) : questionType === "Checkbox" ? (
              <div className="space-y-2">
                {options.map((o, i) => (
                  <label key={o?.id ?? i} className="flex items-center gap-2 text-sm">
                    <Checkbox />
                    {(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl}
                        alt=""
                        className="h-10 w-10 rounded-md border object-cover"
                      />
                    ) : null}
                    <span className="min-w-0">{optionText(o, i)}</span>
                  </label>
                ))}
              </div>
            ) : (
              <RadioGroup className="gap-2">
                {options.map((o, i) => (
                  <label key={o?.id ?? i} className="flex items-center gap-2 text-sm">
                    <RadioGroupItem value={String(i)} />
                    {(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl}
                        alt=""
                        className="h-10 w-10 rounded-md border object-cover"
                      />
                    ) : null}
                    <span className="min-w-0">{optionText(o, i)}</span>
                  </label>
                ))}
              </RadioGroup>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export function QuestionCandidatePreviewStatic({ question }: { question: AssessmentQuestion }) {
  const questionType = (question?.questionType ?? "").toString();
  const isRequired = !!question?.isRequired;
  const promptText = (question?.promptText ?? "").toString().trim();
  const mediaUrl = (question?.mediaUrl ?? null) as string | null;
  const mediaFileId = (question?.mediaFileId ?? null) as string | null;
  const options = (question?.options ?? []) as any[];

  const needsOptions = useMemo(() => {
    return (
      questionType === "SingleChoice" ||
      questionType === "MultiChoice" ||
      questionType === "Radio" ||
      questionType === "Checkbox" ||
      questionType === "Likert" ||
      questionType === "Dropdown"
    );
  }, [questionType]);

  const isQuizSingle = questionType === "SingleChoice";
  const isQuizMulti = questionType === "MultiChoice";

  const fileIdsToResolve = useMemo(() => {
    const ids = new Set<string>();
    if (mediaFileId) ids.add(mediaFileId);
    for (const o of options) {
      const id = o?.mediaFileId as string | null | undefined;
      if (id) ids.add(id);
    }
    return Array.from(ids);
  }, [mediaFileId, options]);

  const downloadUrlByFileId = useDownloadUrlCache(fileIdsToResolve);
  const questionImgSrc = (mediaFileId && downloadUrlByFileId[mediaFileId]) || mediaUrl;

  return (
    <div className="rounded-lg border bg-background p-4 space-y-4">
      <div className="space-y-1">
        <div className="text-sm font-medium">
          {promptText || <span className="text-muted-foreground">No question</span>}
          {isRequired && <span className="ml-1 text-destructive">*</span>}
        </div>
      </div>

      {questionImgSrc && (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={questionImgSrc} alt="question" className="max-h-56 w-auto rounded-md border object-contain" />
      )}

      <div className="pointer-events-none select-none">
        {!needsOptions && (
          <div className="space-y-2">
            {questionType === "Textarea" ? (
              <Textarea rows={4} placeholder="Candidate answer..." />
            ) : (
              <Input placeholder="Candidate answer..." />
            )}
          </div>
        )}

        {needsOptions && (
          <div className="space-y-2">
            {isQuizSingle || isQuizMulti ? (
              <div className="flex flex-wrap gap-2">
                {options.map((o, i) => {
                  const imgSrc = (o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl || null;
                  return (
                    <div
                      key={o?.id ?? i}
                      className="flex items-center gap-2 rounded-md border bg-background px-3 py-2 text-sm"
                    >
                      <span
                        className={
                          isQuizSingle ? "h-4 w-4 shrink-0 rounded-full border" : "h-4 w-4 shrink-0 rounded-[4px] border"
                        }
                      />
                      {imgSrc ? (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img src={imgSrc} alt="" className="h-8 w-8 rounded-md border object-cover" />
                      ) : null}
                      <span className="min-w-0">{optionText(o, i)}</span>
                    </div>
                  );
                })}
              </div>
            ) : questionType === "Dropdown" ? (
              <select className="h-9 w-full rounded-md border bg-background px-3 text-sm">
                <option value="">Select an option...</option>
                {options.map((o, i) => (
                  <option key={o?.id ?? i} value={o?.value ?? optionText(o, i)}>
                    {optionText(o, i)}
                  </option>
                ))}
              </select>
            ) : questionType === "Checkbox" ? (
              <div className="space-y-2">
                {options.map((o, i) => (
                  <label key={o?.id ?? i} className="flex items-center gap-2 text-sm">
                    <Checkbox />
                    {(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl}
                        alt=""
                        className="h-10 w-10 rounded-md border object-cover"
                      />
                    ) : null}
                    <span className="min-w-0">{optionText(o, i)}</span>
                  </label>
                ))}
              </div>
            ) : (
              <RadioGroup className="gap-2">
                {options.map((o, i) => (
                  <label key={o?.id ?? i} className="flex items-center gap-2 text-sm">
                    <RadioGroupItem value={String(i)} />
                    {(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={(o?.mediaFileId && downloadUrlByFileId[o.mediaFileId]) || o?.mediaUrl}
                        alt=""
                        className="h-10 w-10 rounded-md border object-cover"
                      />
                    ) : null}
                    <span className="min-w-0">{optionText(o, i)}</span>
                  </label>
                ))}
              </RadioGroup>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export function TemplateCandidatePreview({
  form,
}: {
  form: UseFormReturn<AssessmentTemplateFormData, any, AssessmentTemplateFormData>;
}) {
  const sections = (useWatch({ control: form.control, name: "sections" as any }) as any[] | undefined) ?? [];

  return (
    <div className="space-y-6">
      {sections.map((s, sIndex) => {
        const title = (s?.title ?? "").toString().trim();
        const questions = (s?.questions ?? []) as any[];
        return (
          <div key={s?.id ?? sIndex} className="space-y-3">
            <div className="flex items-center justify-between">
              <div>
                <div className="text-sm font-semibold">{title || `Section ${sIndex + 1}`}</div>
                <div className="text-xs text-muted-foreground">{questions.length} question(s)</div>
              </div>
            </div>
            <div className="space-y-3">
              {questions.map((q, qIndex) => (
                <div key={q?.name ?? qIndex} className="space-y-2">
                  <Label className="text-xs text-muted-foreground">Q{qIndex + 1}</Label>
                  <QuestionCandidatePreview form={form} qPath={`sections.${sIndex}.questions.${qIndex}`} />
                </div>
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}

export function TemplateCandidatePreviewStatic({ template }: { template: AssessmentTemplate }) {
  const sections = (template?.sections ?? [])
    .slice()
    .sort((a, b) => (a.order ?? 0) - (b.order ?? 0));

  return (
    <div className="space-y-6">
      {sections.map((s, sIndex) => {
        const title = (s?.title ?? "").toString().trim();
        const questions = (s?.questions ?? [])
          .slice()
          .sort((a, b) => (a.order ?? 0) - (b.order ?? 0));

        return (
          <div key={s?.id ?? sIndex} className="space-y-3">
            <div>
              <div className="text-sm font-semibold">{title || `Section ${sIndex + 1}`}</div>
              <div className="text-xs text-muted-foreground">{questions.length} question(s)</div>
            </div>

            <div className="space-y-3">
              {questions.map((q, qIndex) => (
                <div key={q?.name ?? qIndex} className="space-y-2">
                  <Label className="text-xs text-muted-foreground">Q{qIndex + 1}</Label>
                  <QuestionCandidatePreviewStatic question={q} />
                </div>
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}


