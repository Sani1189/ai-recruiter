"use client";

import { useEffect, useRef, useState } from "react";
import { useFieldArray, useWatch, UseFormReturn } from "react-hook-form";
import { ChevronDown, HelpCircle, Pencil, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import {
  AssessmentTemplateType,
  ASSESSMENT_QUESTION_TYPES,
  FORM_QUESTION_TYPES,
  PERSONALITY_QUESTION_TYPES,
  QUIZ_QUESTION_TYPES,
} from "@/types/assessmentTemplate";
import { AssessmentTemplateFormData } from "@/schemas/assessmentTemplate.schema";
import { MediaUploadField } from "./MediaUploadField";
import { QuestionCandidatePreview } from "./CandidatePreview";
import { slugify } from "@/lib/stringUtils";
import { useApi } from "@/hooks/useApi";

type BuilderMode = AssessmentTemplateType;

const allowedQuestionTypesByMode: Record<BuilderMode, readonly string[]> = {
  Quiz: QUIZ_QUESTION_TYPES,
  Personality: PERSONALITY_QUESTION_TYPES,
  Form: ASSESSMENT_QUESTION_TYPES,
};

const QUESTION_TYPE_LABELS: Record<string, string> = {
  Text: "Text",
  Textarea: "Long text",
  Radio: "Radio (single answer)",
  Checkbox: "Checkbox (multi answer)",
  Dropdown: "Dropdown (single answer)",
  SingleChoice: "Single choice (scored)",
  MultiChoice: "Multiple choice (scored)",
  Likert: "Likert (Ws/Wa scoring)",
};

function getQuestionTypeLabel(t: string) {
  return QUESTION_TYPE_LABELS[t] ?? t;
}

function typeNeedsOptions(t?: string | null) {
  return (
    t === "SingleChoice" ||
    t === "MultiChoice" ||
    t === "Likert" ||
    t === "Radio" ||
    t === "Checkbox" ||
    t === "Dropdown"
  );
}

function isQuizScoredType(t?: string | null) {
  return t === "SingleChoice" || t === "MultiChoice";
}

function isPersonalityLikertType(t?: string | null) {
  return t === "Likert";
}

type OptionSeed = {
  name: string;
  version: number;
  order: number;
  label: string;
  mediaUrl: string | null;
  mediaFileId: string | null;
  isCorrect: boolean;
  score: number | null;
  weight: number | null;
  wa: number | null;
};

function createOptionSeed(
  order: number,
  overrides?: Partial<OptionSeed>,
): OptionSeed {
  const label = overrides?.label || "";
  return {
    name: slugify(label) || `option_${order}`,
    version: 1,
    order,
    label: "",
    mediaUrl: null,
    mediaFileId: null,
    isCorrect: false,
    score: null,
    weight: null,
    wa: null,
    ...(overrides ?? {}),
  };
}

function seedOptionsForQuestionType(t?: string | null) {
  if (t === "Likert") {
    return [
      createOptionSeed(1, { label: "Strongly Agree", wa: 10 }),
      createOptionSeed(2, { label: "Agree", wa: 5 }),
      createOptionSeed(3, { label: "Neutral", wa: 0 }),
      createOptionSeed(4, { label: "Disagree", wa: -5 }),
      createOptionSeed(5, { label: "Strongly Disagree", wa: -10 }),
    ];
  }

  if (t === "SingleChoice" || t === "MultiChoice") {
    // Common quiz default: 4 choices, each worth 1 point by default (can be edited).
    return [
      createOptionSeed(1, { score: 1 }),
      createOptionSeed(2, { score: 1 }),
      createOptionSeed(3, { score: 1 }),
      createOptionSeed(4, { score: 1 }),
    ];
  }

  if (t === "Radio" || t === "Checkbox") {
    // Minimal seed for form choices.
    return [createOptionSeed(1), createOptionSeed(2)];
  }

  if (t === "Dropdown") {
    return [createOptionSeed(1), createOptionSeed(2)];
  }

  return [];
}

function defaultQuestionTypeForTemplate(templateType: BuilderMode) {
  if (templateType === "Quiz") return QUIZ_QUESTION_TYPES[0] as string;
  if (templateType === "Personality")
    return PERSONALITY_QUESTION_TYPES[0] as string;
  return FORM_QUESTION_TYPES[0] as string;
}

function allowedQuestionTypesForTemplate(
  templateType: BuilderMode,
  currentQuestionType?: string | null,
) {
  const allowed = allowedQuestionTypesByMode[templateType] as readonly string[];
  if (!currentQuestionType) return allowed;
  // Backward compat: if existing data contains question types not allowed for the current template type,
  // include them so the edit dialog can render the saved value and let the user fix it (or switch to Form).
  if (allowed.includes(currentQuestionType as any)) return allowed;
  return [...allowed, currentQuestionType];
}

export function AssessmentBuilder({
  mode,
  form,
  isEditMode = false,
}: {
  mode: BuilderMode;
  form: UseFormReturn<
    AssessmentTemplateFormData,
    any,
    AssessmentTemplateFormData
  >;
  isEditMode?: boolean;
}) {
  const sectionsArray = useFieldArray({
    control: form.control,
    name: "sections",
  });

  const addSection = () => {
    sectionsArray.append({
      id: crypto.randomUUID(),
      order: sectionsArray.fields.length + 1,
      title: `Section ${sectionsArray.fields.length + 1}`,
      questions: [],
    } as any);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <div className="font-medium">Sections</div>
          <div className="text-muted-foreground text-xs">
            Template type: <span className="font-mono">{mode}</span>
          </div>
        </div>
        <Button type="button" variant="outline" size="sm" onClick={addSection}>
          <Plus className="mr-2 h-4 w-4" />
          Add section
        </Button>
      </div>

      {sectionsArray.fields.length === 0 ? (
        <div className="text-muted-foreground text-sm">No sections yet.</div>
      ) : (
        <div className="space-y-6">
          {sectionsArray.fields.map((section, sectionIndex) => (
            <SectionEditor
              key={section.id}
              mode={mode}
              form={form}
              sectionIndex={sectionIndex}
              isEditMode={isEditMode}
              onRemove={() => sectionsArray.remove(sectionIndex)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function SectionEditor({
  mode,
  form,
  sectionIndex,
  isEditMode,
  onRemove,
}: {
  mode: BuilderMode;
  form: UseFormReturn<
    AssessmentTemplateFormData,
    any,
    AssessmentTemplateFormData
  >;
  sectionIndex: number;
  isEditMode: boolean;
  onRemove: () => void;
}) {
  const questionsArray = useFieldArray({
    control: form.control,
    name: `sections.${sectionIndex}.questions` as any,
  });

  const [addOpen, setAddOpen] = useState(false);
  const allowed = allowedQuestionTypesByMode[mode];
  const [draftType, setDraftType] = useState<string>(
    (defaultQuestionTypeForTemplate(mode) ?? "Text") as string,
  );
  const [draftPrompt, setDraftPrompt] = useState<string>("");

  const addQuestionFromDraft = () => {
    const qType = (draftType || allowed[0] || "Text") as any;
    const needsOptions = typeNeedsOptions(qType);
    const seedOptions = needsOptions ? seedOptionsForQuestionType(qType) : [];
    const order = questionsArray.fields.length + 1;

    questionsArray.append({
      name: slugify(draftPrompt) || `question_${order}`,
      version: 1,
      order,
      questionType: qType,
      isRequired: false,
      promptText: draftPrompt || "",
      mediaUrl: null,
      mediaFileId: null,
      ws: qType === "Likert" ? 1 : null,
      traitKey: qType === "Likert" ? "extrovert_introvert" : null,
      options: seedOptions,
    } as any);

    setDraftPrompt("");
    setDraftType((defaultQuestionTypeForTemplate(mode) ?? "Text") as string);
    setAddOpen(false);
  };

  return (
    <div className="bg-muted/20 space-y-4 rounded-lg border p-4">
      <div className="flex items-start justify-between gap-2">
        <div className="flex-1 space-y-2">
          <Label>Section title</Label>
          <Input
            {...form.register(`sections.${sectionIndex}.title` as const)}
            placeholder="Section title"
          />
        </div>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={onRemove}
          title="Remove section"
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex items-center justify-between">
        <div className="text-sm font-medium">Questions</div>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => setAddOpen(true)}
        >
          <Plus className="mr-2 h-4 w-4" />
          Add question
        </Button>
      </div>

      {questionsArray.fields.length === 0 ? (
        <div className="text-muted-foreground text-sm">No questions yet.</div>
      ) : (
        <div className="space-y-2">
          {questionsArray.fields.map((q, qIndex) => (
            <QuestionCard
              key={qIndex}
              mode={mode}
              form={form}
              sectionIndex={sectionIndex}
              questionIndex={qIndex}
              isEditMode={isEditMode}
              onRemove={() => questionsArray.remove(qIndex)}
            />
          ))}
        </div>
      )}

      <Dialog open={addOpen} onOpenChange={setAddOpen}>
        <DialogContent className="max-h-[85vh] w-[calc(100vw-2rem)] overflow-y-auto p-6 sm:max-w-3xl lg:max-w-4xl">
          <DialogHeader>
            <DialogTitle>Add question</DialogTitle>
            <DialogDescription>
              Choose a question type and write the prompt. You can edit details
              after creating.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Question type</Label>
              <Select value={draftType} onValueChange={(v) => setDraftType(v)}>
                <SelectTrigger className="h-9 w-full">
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {allowed.map((t) => (
                    <SelectItem key={t} value={t}>
                      {getQuestionTypeLabel(t)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Question</Label>
              <Textarea
                rows={4}
                value={draftPrompt}
                onChange={(e) => setDraftPrompt(e.target.value)}
                placeholder="Write the question..."
              />
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setAddOpen(false)}
            >
              Cancel
            </Button>
            <Button type="button" onClick={addQuestionFromDraft}>
              Create question
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function QuestionCard({
  mode,
  form,
  sectionIndex,
  questionIndex,
  isEditMode,
  onRemove,
}: {
  mode: BuilderMode;
  form: UseFormReturn<
    AssessmentTemplateFormData,
    any,
    AssessmentTemplateFormData
  >;
  sectionIndex: number;
  questionIndex: number;
  isEditMode: boolean;
  onRemove: () => void;
}) {
  const qPath = `sections.${sectionIndex}.questions.${questionIndex}` as const;
  const [open, setOpen] = useState(false);

  // IMPORTANT: useWatch ensures the card re-renders even if RHF mutates arrays in-place.
  const questionType =
    (useWatch({
      control: form.control,
      name: `${qPath}.questionType` as any,
    }) as string | undefined) ?? "";
  const promptText = (
    (useWatch({ control: form.control, name: `${qPath}.promptText` as any }) as
      | string
      | null
      | undefined) ?? ""
  ).trim();
  const isRequired = !!useWatch({
    control: form.control,
    name: `${qPath}.isRequired` as any,
  });
  const options =
    (useWatch({ control: form.control, name: `${qPath}.options` as any }) as
      | any[]
      | undefined) ?? [];

  const needsOptions = typeNeedsOptions(questionType);
  const correctCount =
    !isQuizScoredType(questionType) || !needsOptions
      ? 0
      : options.filter((o) => !!o?.isCorrect).length;

  const shortPrompt =
    promptText.length > 140 ? `${promptText.slice(0, 140)}…` : promptText;

  const handleDone = async () => {
    // Validate the question fields
    const fieldPaths = [
      `sections.${sectionIndex}.questions.${questionIndex}.questionType`,
      `sections.${sectionIndex}.questions.${questionIndex}.options`,
    ];

    const isValid = await form.trigger(fieldPaths as any);

    if (isValid) {
      setOpen(false);
    }
  };

  return (
    <div className="bg-background rounded-md border p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <div className="text-sm font-medium">
              Q{questionIndex + 1}.{" "}
              {shortPrompt || (
                <span className="text-muted-foreground">No prompt yet</span>
              )}
            </div>
            {isRequired && (
              <span className="bg-primary/10 text-primary rounded-full px-2 py-0.5 text-xs">
                Required
              </span>
            )}
          </div>
          <div className="text-muted-foreground mt-1 flex flex-wrap gap-x-3 gap-y-1 text-xs">
            <span>Type: {getQuestionTypeLabel(questionType || "-")}</span>
            {needsOptions && <span>Options: {options.length}</span>}
            {isQuizScoredType(questionType) && needsOptions && (
              <span>Correct: {correctCount}</span>
            )}
          </div>
        </div>

        <div className="flex items-center gap-1">
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => setOpen(true)}
          >
            <Pencil className="mr-2 h-4 w-4" />
            Edit
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            onClick={onRemove}
            title="Remove question"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-h-[85vh] w-[calc(100vw-2rem)] overflow-y-auto p-6 sm:max-w-4xl lg:max-w-5xl xl:max-w-6xl">
          <DialogHeader>
            <DialogTitle>Edit question</DialogTitle>
            <DialogDescription>
              Configure question details and options. This is recruiter-only
              authoring.
            </DialogDescription>
          </DialogHeader>
          <QuestionEditorBody
            mode={mode}
            form={form}
            sectionIndex={sectionIndex}
            questionIndex={questionIndex}
            isEditMode={isEditMode}
          />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleDone}>
              Done
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function QuestionEditorBody({
  mode,
  form,
  sectionIndex,
  questionIndex,
  isEditMode,
}: {
  mode: BuilderMode;
  form: UseFormReturn<
    AssessmentTemplateFormData,
    any,
    AssessmentTemplateFormData
  >;
  sectionIndex: number;
  questionIndex: number;
  isEditMode: boolean;
}) {
  const api = useApi();
  const qPath = `sections.${sectionIndex}.questions.${questionIndex}` as const;
  const optionArray = useFieldArray({
    control: form.control,
    name: `${qPath}.options` as any,
  });

  // Question version history + activation (edit mode only)
  const [historyOpen, setHistoryOpen] = useState(false);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [historySubmitting, setHistorySubmitting] = useState(false);
  const [history, setHistory] = useState<
    {
      version: number;
      createdAt: string;
      updatedAt: string;
      createdBy?: string | null;
      updatedBy?: string | null;
      isDeleted: boolean;
    }[]
  >([]);
  const [historyDetails, setHistoryDetails] = useState<any[]>([]);
  const [expandedVersion, setExpandedVersion] = useState<number | null>(null);

  const addOption = () => {
    const isQuiz = isQuizScoredType(questionType);
    const isLikert = isPersonalityLikertType(questionType);
    const order = optionArray.fields.length + 1;
    optionArray.append({
      name: `option_${order}`,
      version: 1,
      order,
      label: "",
      mediaUrl: null,
      mediaFileId: null,
      isCorrect: false,
      score: isQuiz ? 1 : null,
      weight: null,
      wa: isLikert ? 0 : null,
    } as any);
  };

  // Note: templates can mix question types; behaviors are driven by the question type, not the template type.

  // Use useWatch to ensure the editor re-renders reliably when these values change.
  const questionType =
    (useWatch({
      control: form.control,
      name: `${qPath}.questionType` as any,
    }) as string | undefined) ?? undefined;
  const isRequired = !!useWatch({
    control: form.control,
    name: `${qPath}.isRequired` as any,
  });
  const allowedQuestionTypes = allowedQuestionTypesForTemplate(
    mode,
    questionType,
  );
  // Backward compat: if older data used Radio/Checkbox in quiz, normalize to Single/Multi.
  useEffect(() => {
    if (mode !== "Quiz") return;
    if (questionType === "Radio") {
      form.setValue(`${qPath}.questionType` as any, "SingleChoice", {
        shouldDirty: true,
      });
    } else if (questionType === "Checkbox") {
      form.setValue(`${qPath}.questionType` as any, "MultiChoice", {
        shouldDirty: true,
      });
    }
  }, [mode, questionType]);

  const isSingleChoice =
    questionType === "SingleChoice" || questionType === "Radio";
  const needsOptions = typeNeedsOptions(questionType);
  const showCorrectAnswer = isQuizScoredType(questionType);
  const showWa = isPersonalityLikertType(questionType);

  // Auto-seed options when switching to an option-based question type.
  const seededRef = useRef(false);
  useEffect(() => {
    if (!needsOptions) {
      seededRef.current = false;
      return;
    }

    // Seed only if currently empty.
    if (optionArray.fields.length === 0 && !seededRef.current) {
      seededRef.current = true;
      optionArray.append(seedOptionsForQuestionType(questionType) as any);
    }
  }, [questionType]);

  // Watch options as a whole. Do NOT call hooks inside optionArray.fields.map (adding/removing options would break hook order).
  const watchedOptions =
    (useWatch({ control: form.control, name: `${qPath}.options` as any }) as
      | any[]
      | undefined) ?? [];

  // Apply defaults when switching question types so recruiters don't have to set score/wa/ws repeatedly.
  useEffect(() => {
    if (!questionType) return;

    if (questionType === "Likert") {
      const ws = form.getValues(`${qPath}.ws` as any);
      if (ws === null || ws === undefined) {
        form.setValue(`${qPath}.ws` as any, 1, { shouldDirty: true });
      }
      const traitKey =
        (form.getValues(`${qPath}.traitKey` as any) as
          | string
          | null
          | undefined) ?? null;
      if (!traitKey) {
        form.setValue(`${qPath}.traitKey` as any, "extrovert_introvert", {
          shouldDirty: true,
        });
      }
      for (let i = 0; i < optionArray.fields.length; i++) {
        const wa = form.getValues(`${qPath}.options.${i}.wa` as any);
        if (wa === null || wa === undefined) {
          form.setValue(`${qPath}.options.${i}.wa` as any, 0, {
            shouldDirty: true,
          });
        }
      }
    }

    if (questionType === "SingleChoice" || questionType === "MultiChoice") {
      for (let i = 0; i < optionArray.fields.length; i++) {
        const score = form.getValues(`${qPath}.options.${i}.score` as any);
        if (score === null || score === undefined) {
          form.setValue(`${qPath}.options.${i}.score` as any, 1, {
            shouldDirty: true,
          });
        }
      }
    }
  }, [questionType]);

  const setSingleCorrect = (selectedOptionId: string) => {
    for (let i = 0; i < optionArray.fields.length; i++) {
      const optId = optionArray.fields[i]?.id as any;
      form.setValue(
        `${qPath}.options.${i}.isCorrect` as any,
        optId === selectedOptionId,
        {
          shouldDirty: true,
          shouldTouch: true,
        },
      );
    }
  };

  const templateName = form.watch("name");
  const templateVersion = form.watch("version");
  const sectionOrder = form.watch(
    `sections.${sectionIndex}.order` as any,
  ) as number;
  const questionName = form.watch(`${qPath}.name` as any) as string;
  const activeQuestionVersion = form.watch(`${qPath}.version` as any) as number;

  useEffect(() => {
    if (!historyOpen || !questionName) {
      setHistory([]);
      setHistoryDetails([]);
      setExpandedVersion(null);
      return;
    }

    let cancelled = false;
    setHistoryLoading(true);
    api
      .get(
        `/QuestionnaireTemplate/questions/${encodeURIComponent(questionName)}/history`,
      )
      .then((res: any) => {
        if (cancelled) return;
        const data = res?.data ?? res;
        const list = Array.isArray(data) ? data : [];
        setHistoryDetails(list);
        setHistory(
          list.map((q: any) => ({
            version: q.version,
            createdAt: q.createdAt,
            updatedAt: q.updatedAt,
            createdBy: q.createdBy ?? null,
            updatedBy: q.updatedBy ?? null,
            isDeleted: !!q.isDeleted,
          })),
        );
      })
      .catch((err: any) => {
        if (cancelled) return;
        toast.error(err?.message || "Failed to load question history");
        setHistory([]);
        setHistoryDetails([]);
        setExpandedVersion(null);
      })
      .finally(() => {
        if (cancelled) return;
        setHistoryLoading(false);
      });

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [historyOpen, questionName]);

  const activateQuestionVersion = async (targetVersion: number) => {
    if (!templateName || !templateVersion || !sectionOrder || !questionName)
      return;
    if (historySubmitting) return;
    setHistorySubmitting(true);
    try {
      const res: any = await api.post(
        `/QuestionnaireTemplate/${encodeURIComponent(templateName)}/${templateVersion}/sections/${sectionOrder}/questions/${encodeURIComponent(
          questionName,
        )}/active/${targetVersion}`,
        {},
      );
      const updated = (res?.data ?? res) as any;
      form.reset(updated);
      toast.success(`Activated ${questionName} v${targetVersion}`);
      setHistoryOpen(false);
    } catch (err: any) {
      toast.error(
        err?.response?.data?.message ||
          err?.message ||
          "Failed to activate question version",
      );
    } finally {
      setHistorySubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 items-end gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <Label>Question type</Label>
          <Select
            value={(questionType as any) || ""}
            onValueChange={(v) =>
              form.setValue(`${qPath}.questionType` as any, v, {
                shouldDirty: true,
                shouldTouch: true,
              })
            }
          >
            <SelectTrigger className="h-9 w-full">
              <SelectValue placeholder="Select type" />
            </SelectTrigger>
            <SelectContent>
              {allowedQuestionTypes.map((t) => (
                <SelectItem key={t} value={t}>
                  {getQuestionTypeLabel(t)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label>Required</Label>
          <div className="flex h-9 items-center gap-2 pt-1">
            <Switch
              checked={isRequired}
              onCheckedChange={(v) =>
                form.setValue(`${qPath}.isRequired` as any, v, {
                  shouldDirty: true,
                  shouldTouch: true,
                })
              }
            />
            <span className="text-muted-foreground text-xs">
              Candidate must answer
            </span>
          </div>
        </div>

        {isPersonalityLikertType(questionType) && (
          <div className="grid grid-cols-1 gap-6 md:col-span-2 md:grid-cols-12">
            <div className="space-y-2 md:col-span-3">
              <Label>Ws *</Label>
              <Input
                className="h-9"
                type="number"
                step="0.1"
                {...form.register(`${qPath}.ws` as any, {
                  valueAsNumber: true,
                })}
              />
              {(() => {
                const error =
                  form.formState.errors?.sections?.[sectionIndex]?.questions?.[
                    questionIndex
                  ]?.ws;
                return error ? (
                  <p className="text-destructive text-xs">{error.message}</p>
                ) : null;
              })()}
            </div>
            <div className="space-y-2 md:col-span-9">
              <Label>Trait key *</Label>
              <Input
                className="h-9"
                {...form.register(`${qPath}.traitKey` as any)}
                placeholder="e.g. extrovert_introvert"
              />
              {(() => {
                const error =
                  form.formState.errors?.sections?.[sectionIndex]?.questions?.[
                    questionIndex
                  ]?.traitKey;
                return error ? (
                  <p className="text-destructive text-xs">{error.message}</p>
                ) : null;
              })()}
            </div>
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <Label>Question</Label>
          <Textarea
            rows={3}
            {...form.register(`${qPath}.promptText` as any)}
            placeholder="Write the question..."
          />
        </div>
        <div className="space-y-2">
          <MediaUploadField
            label="Question image (optional)"
            accept="image/*"
            form={form}
            urlPath={`${qPath}.mediaUrl` as any}
            fileIdPath={`${qPath}.mediaFileId` as any}
          />
        </div>
      </div>

      {isEditMode && (
        <div className="bg-muted/20 rounded-lg border p-4">
          <div className="flex items-start justify-between gap-4">
            <div className="space-y-1">
              <div className="text-sm font-medium">Question history</div>
              <p className="text-muted-foreground text-xs">
                If the template is in use, the backend automatically versions questions on change to preserve history.
              </p>
            </div>

            <div className="flex items-center gap-2">
              <Badge variant="outline">v{activeQuestionVersion}</Badge>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setHistoryOpen(true)}
              >
                History
              </Button>
            </div>
          </div>

          <Dialog open={historyOpen} onOpenChange={setHistoryOpen}>
            <DialogContent className="max-h-[90vh] w-[calc(100vw-2rem)] max-w-275 overflow-y-auto p-4">
              <DialogHeader>
                <DialogTitle>Question history</DialogTitle>
                <DialogDescription>
                  View previous versions and optionally activate one for this
                  template version (draft only).
                </DialogDescription>
              </DialogHeader>

              {historyLoading ? (
                <div className="text-muted-foreground py-6 text-sm">
                  Loading...
                </div>
              ) : history.length === 0 ? (
                <div className="text-muted-foreground py-6 text-sm">
                  No history found.
                </div>
              ) : (
                <div className="mt-2 max-h-[70vh] space-y-2 overflow-y-auto pr-1">
                  {history.map((h) => {
                    const isActive = h.version === activeQuestionVersion;
                    const detail = historyDetails.find(
                      (d: any) => d?.version === h.version,
                    );
                    const isExpanded = expandedVersion === h.version;
                    return (
                      <div key={h.version} className="rounded-md border">
                        <div
                          role="button"
                          tabIndex={0}
                          className="hover:bg-muted/20 flex w-full cursor-pointer items-center justify-between gap-3 px-3 py-2 text-left select-none"
                          onClick={() =>
                            setExpandedVersion((prev) =>
                              prev === h.version ? null : h.version,
                            )
                          }
                          onKeyDown={(e) => {
                            if (e.key === "Enter" || e.key === " ") {
                              e.preventDefault();
                              setExpandedVersion((prev) =>
                                prev === h.version ? null : h.version,
                              );
                            }
                          }}
                          aria-expanded={isExpanded}
                        >
                          <div className="flex items-center gap-2">
                            <Badge variant={isActive ? "default" : "outline"}>
                              v{h.version}
                            </Badge>
                            {isActive && (
                              <span className="text-muted-foreground text-xs">
                                Active
                              </span>
                            )}
                            {h.isDeleted && (
                              <Badge variant="destructive" className="text-xs">
                                Deleted
                              </Badge>
                            )}
                          </div>

                          <div className="flex items-center gap-2">
                            <ChevronDown
                              className={`text-muted-foreground h-4 w-4 transition-transform ${
                                isExpanded ? "rotate-180" : ""
                              }`}
                            />
                            <Button
                              type="button"
                              variant={isActive ? "secondary" : "default"}
                              size="sm"
                              className="h-7 px-2 text-xs"
                              disabled={
                                isActive || historySubmitting || h.isDeleted
                              }
                              onClick={(e) => {
                                e.preventDefault();
                                e.stopPropagation();
                                activateQuestionVersion(h.version);
                              }}
                            >
                              {isActive
                                ? "Active"
                                : historySubmitting
                                  ? "Working..."
                                  : "Make active"}
                            </Button>
                          </div>
                        </div>

                        <div
                          className={`grid transition-all duration-200 ease-out ${
                            isExpanded ? "grid-rows-[1fr]" : "grid-rows-[0fr]"
                          }`}
                        >
                          <div className="overflow-hidden">
                            <div className="space-y-2 border-t px-3 py-2">
                              <div className="text-sm">
                                <span className="text-muted-foreground">
                                  Question:{" "}
                                </span>
                                <span>{detail?.promptText || "-"}</span>
                              </div>

                              <div className="space-y-1.5">
                                <div className="text-sm font-medium">
                                  Options
                                </div>
                                {Array.isArray(detail?.options) &&
                                detail.options.length > 0 ? (
                                  <div className="space-y-0.5">
                                    {detail.options.map((o: any) => {
                                      const label = (o.label ?? "")
                                        .toString()
                                        .trim();
                                      return (
                                        <div
                                          key={`${o.name}_${o.version}_${o.order}`}
                                          className="bg-muted/10 flex items-center justify-between gap-2 rounded-md border px-2 py-1"
                                        >
                                          <div className="min-w-0 truncate text-[13px] leading-4">
                                            {label || "-"}
                                          </div>
                                          <div className="text-muted-foreground flex items-center gap-1 text-xs">
                                            {o.isCorrect === true && (
                                              <Badge variant="default">
                                                Correct
                                              </Badge>
                                            )}
                                            {typeof o.score === "number" && (
                                              <Badge variant="outline">
                                                Score {o.score}
                                              </Badge>
                                            )}
                                          </div>
                                        </div>
                                      );
                                    })}
                                  </div>
                                ) : (
                                  <div className="text-muted-foreground text-sm">
                                    No options
                                  </div>
                                )}
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </DialogContent>
          </Dialog>
        </div>
      )}

      {needsOptions && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div className="text-sm font-medium">Options</div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addOption}
            >
              <Plus className="mr-2 h-4 w-4" />
              Add option
            </Button>
          </div>

          <div className="space-y-3">
            {optionArray.fields.map((opt, optIndex) => {
              const optPath = `${qPath}.options.${optIndex}` as const;
              const optId = opt.id as string;
              const isCorrect = !!watchedOptions?.[optIndex]?.isCorrect;
              const showScore = isQuizScoredType(questionType);
              const showWeight = !isQuizScoredType(questionType) && !showWa;
              return (
                <div key={optId} className="bg-muted/10 rounded-lg border p-3">
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:gap-3">
                    {showCorrectAnswer && (
                      <div className="flex shrink-0 items-center gap-2 lg:w-35">
                        <Switch
                          checked={isCorrect}
                          onCheckedChange={(v) => {
                            if (isSingleChoice) {
                              setSingleCorrect(optId);
                            } else {
                              form.setValue(`${optPath}.isCorrect` as any, v, {
                                shouldDirty: true,
                                shouldTouch: true,
                              });
                            }
                          }}
                        />
                        <span className="text-muted-foreground text-xs">
                          Correct
                        </span>
                      </div>
                    )}

                    {showWa && (
                      <div className="w-full shrink-0 space-y-1 lg:w-40">
                        <Input
                          className="h-9"
                          type="number"
                          step="1"
                          {...form.register(`${optPath}.wa` as any, {
                            valueAsNumber: true,
                          })}
                          placeholder="Wa (-10..10) *"
                        />
                        {(() => {
                          const error =
                            form.formState.errors?.sections?.[sectionIndex]
                              ?.questions?.[questionIndex]?.options?.[optIndex]
                              ?.wa;
                          return error ? (
                            <p className="text-destructive text-xs">
                              {error.message}
                            </p>
                          ) : null;
                        })()}
                      </div>
                    )}

                    <div className="min-w-0 flex-1">
                      {(() => {
                        const reg = form.register(`${optPath}.label` as any);
                        const error =
                          form.formState.errors?.sections?.[sectionIndex]
                            ?.questions?.[questionIndex]?.options?.[optIndex]
                            ?.label;
                        return (
                          <div className="space-y-1">
                            <Input
                              className="h-9 w-full"
                              {...reg}
                              placeholder="Option label *"
                              onBlur={(e) => {
                                reg.onBlur(e);
                              }}
                            />
                            {error && (
                              <p className="text-destructive text-xs">
                                {error.message}
                              </p>
                            )}
                          </div>
                        );
                      })()}
                    </div>

                    {showScore && (
                      <div className="w-full shrink-0 lg:w-35">
                        <Input
                          className="h-9 w-full"
                          type="number"
                          step="0.1"
                          {...form.register(`${optPath}.score` as any, {
                            valueAsNumber: true,
                          })}
                          placeholder="Score (opt.)"
                        />
                      </div>
                    )}

                    {showWeight && (
                      <div className="w-full shrink-0 lg:w-40">
                        <Input
                          className="h-9 w-full"
                          type="number"
                          step="0.1"
                          {...form.register(`${optPath}.weight` as any, {
                            valueAsNumber: true,
                          })}
                          placeholder="Weight (opt.)"
                        />
                      </div>
                    )}

                    <div className="flex shrink-0 items-center justify-end gap-2 lg:w-24">
                      <MediaUploadField
                        label="Image"
                        accept="image/*"
                        compact
                        iconOnly
                        form={form}
                        urlPath={`${optPath}.mediaUrl` as any}
                        fileIdPath={`${optPath}.mediaFileId` as any}
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="shrink-0"
                        onClick={() => optionArray.remove(optIndex)}
                        title="Remove option"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
          {/* Versioning controls moved into the unified "Apply changes by" panel above */}
        </div>
      )}

      {/* Candidate preview (live) */}
      <div className="space-y-2 pt-2">
        <div className="text-sm font-medium">Candidate preview</div>
        <QuestionCandidatePreview form={form} qPath={qPath} />
      </div>
    </div>
  );
}
