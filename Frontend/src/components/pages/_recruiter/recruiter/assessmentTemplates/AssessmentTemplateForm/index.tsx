"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
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
import { HelpCircle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from "@/components/ui/form";

import { useApi } from "@/hooks/useApi";
import {
  AssessmentTemplate,
  ASSESSMENT_TEMPLATE_TYPES,
  AssessmentTemplateType,
} from "@/types/assessmentTemplate";
import {
  assessmentTemplateFormSchema,
  AssessmentTemplateFormData,
} from "@/schemas/assessmentTemplate.schema";
import { AssessmentBuilder } from "@/components/reusable/assessmentBuilder/AssessmentBuilder";
import { TemplateCandidatePreview } from "@/components/reusable/assessmentBuilder/CandidatePreview";
import ImportFromExcelDialog from "@/components/pages/_recruiter/recruiter/assessmentTemplates/AssessmentTemplatesTable/ImportFromExcelDialog";

interface AssessmentTemplateFormProps {
  template?: AssessmentTemplate;
  mode: "create" | "edit";
}

const defaultTemplateType: AssessmentTemplateType = "Quiz";

function createInitialSection() {
  return {
    id: crypto.randomUUID(),
    order: 1,
    title: "Section 1",
    questions: [],
  };
}

export default function AssessmentTemplateForm({ template, mode }: AssessmentTemplateFormProps) {
  const router = useRouter();
  const api = useApi();
  const [loading, setLoading] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [publishNow, setPublishNow] = useState(false);
  const [shouldUpdateVersion, setShouldUpdateVersion] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);

  // When editing a published template, pre-check "Publish after save" so saving keeps it published.
  useEffect(() => {
    if (mode === "edit" && template != null) {
      setPublishNow(Boolean(template.isPublished));
    }
  }, [mode, template]);
  const [confirmTypeChangeOpen, setConfirmTypeChangeOpen] = useState(false);
  const [pendingTemplateType, setPendingTemplateType] = useState<AssessmentTemplateType | null>(null);

  function getApiSaveError(err: any): string {
    const data = err?.response?.data;
    if (!data) return err?.message || "Failed to save template";
    if (typeof data.message === "string" && data.message.trim()) return data.message.trim();
    if (typeof data.details === "string" && data.details.trim()) return data.details.trim();
    const validationErrors = data.validationErrors;
    if (Array.isArray(validationErrors) && validationErrors.length > 0) {
      const messages = validationErrors.filter((m): m is string => typeof m === "string");
      if (messages.length > 0) return messages.join(" ");
    }
    if (data.errors && typeof data.errors === "object") {
      const firstKey = Object.keys(data.errors)[0];
      const firstVal = firstKey ? (data.errors as Record<string, unknown>)[firstKey] : null;
      const firstMsg = Array.isArray(firstVal) ? firstVal[0] : firstVal;
      if (typeof firstMsg === "string" && firstMsg.trim()) return firstMsg.trim();
    }
    return err?.message || "Failed to save template";
  }

  function getFirstFormError(errors: Record<string, unknown>): string | null {
    for (const value of Object.values(errors)) {
      if (!value || typeof value !== "object") continue;
      const obj = value as Record<string, unknown>;
      if (typeof obj.message === "string" && obj.message.trim()) return obj.message.trim();
      const nested = getFirstFormError(obj as Record<string, unknown>);
      if (nested) return nested;
    }
    return null;
  }

  const onInvalid = (errors: Record<string, unknown>) => {
    const first = getFirstFormError(errors);
    setSaveError(first || "Please fix the errors in the form. Open each question to see required fields (e.g. question title, options).");
    toast.error(first || "Please fix the errors in the form.");
  };

  const defaultValues: AssessmentTemplateFormData = useMemo(() => {
    return {
      name: template?.name || "",
      version: template?.version ?? 1,
      templateType: (template?.templateType as any) || defaultTemplateType,
      title: template?.title ?? "",
      description: template?.description ?? "",
      timeLimitSeconds: template?.timeLimitSeconds ?? null,
      sections: template?.sections?.length ? (template.sections as any) : ([createInitialSection()] as any),
    };
  }, [template]);

  const form = useForm<AssessmentTemplateFormData>({
    resolver: zodResolver(assessmentTemplateFormSchema) as any,
    defaultValues,
  });

  const templateType = form.watch("templateType");
  const sections = (useWatch({ control: form.control, name: "sections" as any }) as any[] | undefined) ?? [];
  const questionCount = useMemo(() => {
    let count = 0;
    for (const s of sections) {
      count += Array.isArray(s?.questions) ? s.questions.length : 0;
    }
    return count;
  }, [sections]);
  const hasAnyQuestions = questionCount > 0;

  const clearAllQuestions = () => {
    const currentSections = (form.getValues("sections" as any) as any[] | undefined) ?? [];
    form.setValue(
      "sections" as any,
      currentSections.map((s) => ({ ...s, questions: [] })),
      { shouldDirty: true, shouldTouch: true },
    );
  };

  const onTemplateTypeChange = (nextType: AssessmentTemplateType, currentType: AssessmentTemplateType) => {
    if (nextType === currentType) return;

    if (hasAnyQuestions) {
      setPendingTemplateType(nextType);
      setConfirmTypeChangeOpen(true);
      return;
    }

    form.setValue("templateType" as any, nextType as any, { shouldDirty: true, shouldTouch: true });
  };

  const confirmTemplateTypeChange = () => {
    if (!pendingTemplateType) {
      setConfirmTypeChangeOpen(false);
      return;
    }
    clearAllQuestions();
    form.setValue("templateType" as any, pendingTemplateType as any, { shouldDirty: true, shouldTouch: true });
    setConfirmTypeChangeOpen(false);
    setPendingTemplateType(null);
  };

  const cancelTemplateTypeChange = () => {
    setConfirmTypeChangeOpen(false);
    setPendingTemplateType(null);
  };

  const onSubmit = async (data: AssessmentTemplateFormData) => {
    setSaveError(null);
    setLoading(true);
    try {
      if (mode === "create") {
        await api.post("/QuestionnaireTemplate", data);
        toast.success("Questionnaire template created");
      } else if (template) {
        const updateData = { ...data, shouldUpdateVersion };
        try {
          await api.put(`/QuestionnaireTemplate/${template.name}/${template.version}`, updateData);
          toast.success("Questionnaire template saved");
        } catch (error: any) {
          if (error?.response?.status === 400 && error?.response?.data?.message?.includes("Version update is required")) {
            toast.error("Template is in use. Please enable 'Create new version' to proceed.");
            return;
          }
          throw error;
        }
      }

      // Optional publish step (will be wired once backend exists)
      if (publishNow && data.name) {
        await api.post(`/QuestionnaireTemplate/${data.name}/${data.version}/publish`, {});
        toast.success("Template published");
      }

      router.push("/recruiter/assessmentTemplates");
    } catch (error: any) {
      const message = getApiSaveError(error);
      setSaveError(message);
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-8">
      <div className="max-w-5xl mx-auto space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => router.push("/recruiter/assessmentTemplates")}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Questionnaire Templates
          </Button>
        </div>

        <div className="space-y-2">
          <h1 className="text-3xl font-bold">
            {mode === "create" ? "Create Questionnaire Template" : "Edit Questionnaire Template"}
          </h1>
          <p className="text-muted-foreground">
            Build reusable templates for job steps. Template Type controls the allowed question types and scoring fields.
            If you change the Template Type after adding questions, the builder will clear those questions to keep the template consistent.
          </p>
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit, onInvalid)} className="space-y-6">
            {saveError && (
              <div
                role="alert"
                className="rounded-lg border border-destructive/50 bg-destructive/10 px-4 py-3 text-sm text-destructive"
              >
                <p className="font-medium">Save failed</p>
                <p className="mt-1">{saveError}</p>
              </div>
            )}
            <Card>
              <CardHeader>
                <CardTitle>Template Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Name</FormLabel>
                        <FormControl>
                          <Input 
                            placeholder="e.g. basic-math-quiz" 
                            {...field} 
                            disabled={mode === "edit"}
                            className={mode === "edit" ? "bg-muted cursor-not-allowed" : ""}
                          />
                        </FormControl>
                        <FormDescription>
                          {mode === "edit" 
                            ? "Template name cannot be changed after creation. Create a new template if you need a different name."
                            : "Stable identifier (used for versioning). Cannot be changed after creation."}
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="templateType"
                    render={({ field }) => (
                      <FormItem className="min-w-0">
                        <FormLabel>Template Type</FormLabel>
                        <Select
                          value={field.value}
                          onValueChange={(v) => onTemplateTypeChange(v as any, field.value as any)}
                        >
                          <FormControl>
                            <SelectTrigger className="h-9 w-full min-w-0">
                              <SelectValue placeholder="Select type" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {ASSESSMENT_TEMPLATE_TYPES.map((t) => (
                              <SelectItem key={t} value={t}>
                                {t}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormDescription>
                          Controls which question types and fields are allowed in the builder.
                          {hasAnyQuestions ? (
                            <span className="block mt-1">
                              <span className="font-medium">Warning:</span> Changing this will delete all questions ({questionCount}).
                            </span>
                          ) : (
                            <span className="block mt-1">
                              <span className="font-medium">Tip:</span> Choose <span className="font-medium">Form</span> if you want to
                              mix question types.
                            </span>
                          )}
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="title"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Title (optional)</FormLabel>
                      <FormControl>
                        <Input
                          placeholder="Shown to recruiters/candidates depending on UX"
                          value={(field.value ?? "") as any}
                          onChange={(e) => field.onChange(e.target.value)}
                          onBlur={field.onBlur}
                          name={field.name}
                          ref={field.ref}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Description (optional)</FormLabel>
                      <FormControl>
                        <Textarea
                          rows={4}
                          placeholder="Describe what this template is for..."
                          value={(field.value ?? "") as any}
                          onChange={(e) => field.onChange(e.target.value)}
                          onBlur={field.onBlur}
                          name={field.name}
                          ref={field.ref}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {mode === "edit" && (
                  <div className="flex items-center justify-between border rounded-lg p-3">
                    <div className="flex-1 space-y-1">
                      <div className="flex items-center gap-2">
                        <Label htmlFor="shouldUpdateVersion">Create new version</Label>
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <HelpCircle className="h-4 w-4 text-muted-foreground cursor-help" />
                            </TooltipTrigger>
                            <TooltipContent className="max-w-xs">
                              <p>
                                When enabled, creates a new template version instead of updating the existing one.
                                This is required if the template is in use (referenced by job posts or has candidate submissions).
                                All questions and options will be versioned to preserve historical data.
                              </p>
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      </div>
                      <div className="text-xs text-muted-foreground">
                        Required if template is in use. Preserves historical submissions.
                      </div>
                    </div>
                    <Switch
                      id="shouldUpdateVersion"
                      checked={shouldUpdateVersion}
                      onCheckedChange={(checked) => setShouldUpdateVersion(checked === true)}
                    />
                  </div>
                )}

                <div className="flex items-center justify-between border rounded-lg p-3">
                  <div>
                    <Label>Publish after save</Label>
                    <div className="text-xs text-muted-foreground">
                      Publishing will freeze the version (backend enforcement).
                    </div>
                  </div>
                  <Switch checked={publishNow} onCheckedChange={setPublishNow} />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Builder</CardTitle>
              </CardHeader>
              <CardContent>
                <AssessmentBuilder mode={templateType} form={form} isEditMode={mode === "edit"} />
              </CardContent>
            </Card>

            <div className="flex items-center justify-end gap-2">
              {mode === "edit" && (
                <ImportFromExcelDialog
                  onImported={() => {
                    toast.success("Import completed. Refresh the page to see the latest changes.");
                  }}
                  initialTemplateName={form.getValues("name")}
                  initialTemplateVersion={form.getValues("version")}
                  initialTemplateType={templateType as any}
                />
              )}
              <Button type="button" variant="outline" onClick={() => setPreviewOpen(true)}>
                Preview
              </Button>
              <Button type="button" variant="outline" onClick={() => router.push("/recruiter/assessmentTemplates")}>
                Cancel
              </Button>
              <Button type="submit" disabled={loading}>
                {loading ? "Saving..." : "Save Template"}
              </Button>
            </div>
          </form>
        </Form>
      </div>

      <AlertDialog open={confirmTypeChangeOpen} onOpenChange={setConfirmTypeChangeOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Change template type?</AlertDialogTitle>
            <AlertDialogDescription>
              Changing the Template Type to{" "}
              <span className="font-medium">{pendingTemplateType ?? ""}</span> will delete all existing questions ({questionCount}).
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={cancelTemplateTypeChange}>Cancel</AlertDialogCancel>
            <AlertDialogAction className="bg-red-600 hover:bg-red-700" onClick={confirmTemplateTypeChange}>
              Delete questions &amp; change type
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog open={previewOpen} onOpenChange={setPreviewOpen}>
        <DialogContent className="w-[calc(100vw-2rem)] sm:max-w-4xl lg:max-w-5xl xl:max-w-6xl max-h-[85vh] overflow-y-auto p-6">
          <DialogHeader>
            <DialogTitle>Candidate preview</DialogTitle>
            <DialogDescription>Read-only preview of how the candidate will see the questions.</DialogDescription>
          </DialogHeader>

          <TemplateCandidatePreview form={form} />

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => setPreviewOpen(false)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}


