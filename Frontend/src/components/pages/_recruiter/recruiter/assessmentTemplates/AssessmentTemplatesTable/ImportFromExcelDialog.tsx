"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { Upload } from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { useApi } from "@/hooks/useApi";
import { ASSESSMENT_TEMPLATE_TYPES, AssessmentTemplate } from "@/types/assessmentTemplate";
import {
  QuestionnaireTemplateImportExecuteResult,
  QuestionnaireTemplateImportScope,
  QuestionnaireTemplateImportValidationResult,
} from "@/types/questionnaireTemplateImport";

const SCOPES: { value: QuestionnaireTemplateImportScope; label: string; hint: string }[] = [
  {
    value: "CreateTemplate",
    label: "Create new template",
    hint: "Creates a new template v1 using the uploaded file contents.",
  },
  {
    value: "AppendToTemplate",
    label: "Append to existing template",
    hint: "Adds/updates sections, questions, and options on the latest version (versions if in use).",
  },
  {
    value: "AppendToSection",
    label: "Append questions into a section",
    hint: "Adds/updates questions/options only for one section order (versions if in use).",
  },
];

export default function ImportFromExcelDialog({
  onImported,
  initialTemplateName,
  initialTemplateVersion,
  initialTemplateType,
}: {
  onImported: () => void;
  initialTemplateName?: string;
  initialTemplateVersion?: number;
  initialTemplateType?: (typeof ASSESSMENT_TEMPLATE_TYPES)[number];
}) {
  const api = useApi();
  const [open, setOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const didInitFromEditContext = useRef(false);

  const [file, setFile] = useState<File | null>(null);
  const [scope, setScope] = useState<QuestionnaireTemplateImportScope>("CreateTemplate");
  const [templateName, setTemplateName] = useState("");
  const [templateTitle, setTemplateTitle] = useState("");
  const [templateDescription, setTemplateDescription] = useState("");
  const [templateVersion, setTemplateVersion] = useState<number | null>(null);
  const [templateType, setTemplateType] = useState<(typeof ASSESSMENT_TEMPLATE_TYPES)[number]>("Quiz");
  const [targetSectionOrder, setTargetSectionOrder] = useState<string>("");

  const [templateNames, setTemplateNames] = useState<string[]>([]);
  const [availableVersions, setAvailableVersions] = useState<number[]>([]);
  const [availableSections, setAvailableSections] = useState<{ order: number; title?: string | null }[]>([]);

  const [validation, setValidation] = useState<QuestionnaireTemplateImportValidationResult | null>(null);
  const [validationError, setValidationError] = useState<string | null>(null);

  const clearValidation = () => {
    setValidation(null);
    setValidationError(null);
  };

  const canValidate = useMemo(() => {
    if (!file) return false;
    if (!templateName.trim()) return false;
    if (scope === "CreateTemplate" && !templateType) return false;
    if (scope !== "CreateTemplate" && !templateVersion) return false;
    if (scope === "AppendToSection" && !targetSectionOrder.trim()) return false;
    return true;
  }, [file, scope, templateName, templateType, targetSectionOrder, templateVersion]);

  const isLockedToTemplate = Boolean(initialTemplateName && initialTemplateVersion);
  const allowedScopes = useMemo(() => {
    if (!isLockedToTemplate) return SCOPES;
    return SCOPES.filter((s) => s.value !== "CreateTemplate");
  }, [isLockedToTemplate]);

  useEffect(() => {
    if (!open) {
      didInitFromEditContext.current = false;
      return;
    }

    // If opened from edit form, preselect template + version once per open.
    // Important: do NOT keep forcing the scope, otherwise user can't choose AppendToSection.
    if (initialTemplateName && initialTemplateVersion && !didInitFromEditContext.current) {
      setTemplateName(initialTemplateName);
      setTemplateVersion(initialTemplateVersion);
      if (initialTemplateType) setTemplateType(initialTemplateType);
      setScope((prev) => (prev === "CreateTemplate" ? "AppendToTemplate" : prev));
      clearValidation();
      didInitFromEditContext.current = true;
    }

    // Load template names for dropdowns (append scopes).
    const loadNames = async () => {
      try {
        const query = new URLSearchParams({
          PageNumber: "1",
          PageSize: "2000",
          SortBy: "updatedAt",
          SortDescending: "true",
        });
        const response = await api.get(`/QuestionnaireTemplate/filtered?${query.toString()}`);
        const data = response.data || response;
        const items: AssessmentTemplate[] = data.items || [];
        const names = Array.from(new Set(items.map((t) => t.name).filter(Boolean))).sort((a, b) =>
          a.localeCompare(b),
        );
        setTemplateNames(names);
      } catch {
        // non-blocking
      }
    };

    loadNames();
  }, [open, api.get, initialTemplateName, initialTemplateVersion]);

  useEffect(() => {
    if (!open) return;
    if (scope === "CreateTemplate") return;
    if (!templateName.trim()) return;

    const loadVersions = async () => {
      try {
        const response = await api.get(`/QuestionnaireTemplate/${encodeURIComponent(templateName.trim())}/versions`);
        const versions: AssessmentTemplate[] = response.data || response;
        const v = (versions || []).map((x) => x.version).filter((n) => typeof n === "number") as number[];
        const ordered = Array.from(new Set(v)).sort((a, b) => b - a);
        setAvailableVersions(ordered);
        if (ordered.length > 0) {
          setTemplateVersion((prev) => prev ?? ordered[0]);
        }
      } catch {
        setAvailableVersions([]);
      }
    };

    // If locked (edit form), versions are fixed by the route.
    if (isLockedToTemplate) return;
    loadVersions();
  }, [open, scope, templateName, api, isLockedToTemplate]);

  useEffect(() => {
    if (!open) return;
    if (scope !== "AppendToSection") {
      setAvailableSections((prev) => (prev.length > 0 ? [] : prev));
      return;
    }
    if (!templateName.trim() || !templateVersion) return;

    const loadSections = async () => {
      try {
        const response = await api.get(
          `/QuestionnaireTemplate/${encodeURIComponent(templateName.trim())}/${templateVersion}`,
        );
        const t: AssessmentTemplate = response.data || response;
        const sections = Array.isArray((t as any)?.sections) ? ((t as any).sections as any[]) : [];
        const mapped = sections
          .map((s) => ({ order: s.order as number, title: s.title as string | null }))
          .filter((s) => typeof s.order === "number")
          .sort((a, b) => a.order - b.order);
        setAvailableSections(mapped);
        if (mapped.length > 0) {
          setTargetSectionOrder((prev) => prev || String(mapped[0].order));
        }

        // Prefer server truth for type when appending.
        if (t?.templateType) setTemplateType(t.templateType as any);
      } catch {
        setAvailableSections([]);
      }
    };

    loadSections();
  }, [open, scope, templateName, templateVersion, api]);

  const buildFormData = () => {
    const fd = new FormData();
    if (file) fd.append("file", file);

    // Overrides (so users don't need to edit these columns in Excel)
    fd.append("Scope", scope);
    fd.append("TemplateName", templateName.trim());
    fd.append("TemplateType", templateType);
    if (templateTitle.trim()) fd.append("title", templateTitle.trim());
    if (templateDescription.trim()) fd.append("description", templateDescription.trim());
    if (scope !== "CreateTemplate" && templateVersion) fd.append("TemplateVersion", String(templateVersion));
    if (scope === "AppendToSection") fd.append("TargetSectionOrder", targetSectionOrder.trim());

    return fd;
  };

  const getErrorMessage = (err: any): string | null => {
    const data = err?.response?.data;

    if (typeof data === "string" && data.trim()) return data.trim();
    if (typeof data?.message === "string" && data.message.trim()) return data.message.trim();
    if (typeof data?.detail === "string" && data.detail.trim()) return data.detail.trim();
    if (typeof data?.title === "string" && data.title.trim()) return data.title.trim();

    // ASP.NET validation problem details: { errors: { Field: [msg] } }
    const errors = data?.errors;
    if (errors && typeof errors === "object") {
      const firstKey = Object.keys(errors)[0];
      const firstVal = firstKey ? (errors as any)[firstKey] : null;
      const firstMsg = Array.isArray(firstVal) ? firstVal[0] : firstVal;
      if (typeof firstMsg === "string" && firstMsg.trim()) return firstMsg.trim();
    }

    if (typeof err?.message === "string" && err.message.trim()) return err.message.trim();
    return null;
  };

  const handleValidate = async () => {
    if (!canValidate) return;
    setSubmitting(true);
    setValidationError(null);
    try {
      const response = await api.post("/QuestionnaireTemplate/import/validate", buildFormData(), {
        headers: { "Content-Type": "multipart/form-data" },
      });
      const result = (response.data || response) as QuestionnaireTemplateImportValidationResult;
      setValidation(result);

      const blocking = (result.errors || []).filter((e) => String(e.severity).toLowerCase() === "error");
      if (blocking.length > 0) toast.error(`Found ${blocking.length} blocking error(s)`);
      else toast.success("File looks good");
    } catch (err: any) {
      const msg = getErrorMessage(err) || "Failed to validate import file";
      setValidationError(msg);
      setValidation(null);
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleExecute = async () => {
    if (!validation?.isValid) return;
    setSubmitting(true);
    try {
      const response = await api.post("/QuestionnaireTemplate/import/execute", buildFormData(), {
        headers: { "Content-Type": "multipart/form-data" },
      });
      const result = (response.data || response) as QuestionnaireTemplateImportExecuteResult;
      toast.success(`Imported: ${result.templateName} v${result.templateVersion}`);
      const warnings = (result.messages || []).filter((m) => String(m.severity).toLowerCase() === "warning");
      if (warnings.length > 0) toast.message(`Import completed with ${warnings.length} warning(s)`);
      onImported();
      setOpen(false);
    } catch (err: any) {
      toast.error(getErrorMessage(err) || "Failed to import file");
    } finally {
      setSubmitting(false);
    }
  };

  const reset = () => {
    setFile(null);
    clearValidation();
    setSubmitting(false);
    setScope("CreateTemplate");
    setTemplateName("");
    setTemplateTitle("");
    setTemplateDescription("");
    setTemplateVersion(null);
    setTemplateType("Quiz");
    setTargetSectionOrder("");
    setAvailableVersions([]);
    setAvailableSections([]);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (!next) reset();
      }}
    >
      <DialogTrigger asChild>
        <Button variant="outline">
          <Upload className="mr-2 h-4 w-4" />
          Import from Excel
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Import questionnaire from Excel</DialogTitle>
          <DialogDescription>
            Upload the Excel template you downloaded. We will validate and show a preview summary before importing.
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-1 gap-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label>Import scope</Label>
              <Select
                value={scope}
                onValueChange={(v) => {
                  const next = v as QuestionnaireTemplateImportScope;
                  if (isLockedToTemplate && next === "CreateTemplate") return;
                  setScope(next);
                  clearValidation();
                  if (next === "CreateTemplate") {
                    if (!isLockedToTemplate) {
                      setTemplateVersion(null);
                      setAvailableVersions([]);
                      setAvailableSections([]);
                      setTargetSectionOrder("");
                    }
                  }
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select scope" />
                </SelectTrigger>
                <SelectContent>
                  {allowedScopes.map((s) => (
                    <SelectItem key={s.value} value={s.value}>
                      {s.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <p className="text-xs text-muted-foreground">
                {SCOPES.find((s) => s.value === scope)?.hint}
              </p>
            </div>

            <div className="space-y-2">
              {scope === "CreateTemplate" ? (
                <>
                  <Label htmlFor="templateName">Template name</Label>
                  <Input
                    id="templateName"
                    placeholder="e.g. basic-math-quiz"
                    value={templateName}
                    onChange={(e) => {
                      setTemplateName(e.target.value);
                      clearValidation();
                    }}
                    autoComplete="off"
                    disabled={isLockedToTemplate}
                  />
                  <p className="text-xs text-muted-foreground">
                    Name is immutable after creation. Use a new name for a new template.
                  </p>
                </>
              ) : (
                <>
                  <Label>Template</Label>
                  {isLockedToTemplate ? (
                    <Input value={templateName} disabled className="bg-muted" />
                  ) : (
                    <Select
                      value={templateName}
                      onValueChange={(v) => {
                        setTemplateName(v);
                        setTemplateVersion(null);
                        setAvailableVersions([]);
                        setAvailableSections([]);
                        setTargetSectionOrder("");
                        clearValidation();
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select template name" />
                      </SelectTrigger>
                      <SelectContent>
                        {templateNames.map((n) => (
                          <SelectItem key={n} value={n}>
                            {n}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                </>
              )}
            </div>
          </div>

          {scope === "CreateTemplate" && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div className="space-y-2">
                <Label htmlFor="templateTitle">Template title</Label>
                <Input
                  id="templateTitle"
                  placeholder="e.g. Basic Math Quiz"
                  value={templateTitle}
                  onChange={(e) => {
                    setTemplateTitle(e.target.value);
                    clearValidation();
                  }}
                  autoComplete="off"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="templateDescription">Description</Label>
                <Input
                  id="templateDescription"
                  placeholder="Optional short description"
                  value={templateDescription}
                  onChange={(e) => {
                    setTemplateDescription(e.target.value);
                    clearValidation();
                  }}
                  autoComplete="off"
                />
              </div>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div className="space-y-2">
              <Label>Template type</Label>
              <Select
                value={templateType}
                onValueChange={(v) => setTemplateType(v as any)}
                disabled={scope !== "CreateTemplate"}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {ASSESSMENT_TEMPLATE_TYPES.map((t) => (
                    <SelectItem key={t} value={t}>
                      {t}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {scope !== "CreateTemplate" && (
                <p className="text-xs text-muted-foreground">
                  For append imports, the existing template type is used. (We still send the value for validation.)
                </p>
              )}
            </div>

            <div className="space-y-2">
              {scope === "CreateTemplate" ? (
                <>
                  <Label className="text-muted-foreground">Target section</Label>
                  <Input disabled placeholder="Not applicable" />
                </>
              ) : scope === "AppendToTemplate" ? (
                <>
                  <Label>Template version</Label>
                  {isLockedToTemplate ? (
                    <Input value={templateVersion ? `v${templateVersion}` : ""} disabled className="bg-muted" />
                  ) : (
                    <Select
                      value={templateVersion ? String(templateVersion) : ""}
                      onValueChange={(v) => {
                        const num = Number(v);
                        setTemplateVersion(Number.isFinite(num) ? num : null);
                        setAvailableSections([]);
                        setTargetSectionOrder("");
                        clearValidation();
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select version" />
                      </SelectTrigger>
                      <SelectContent>
                        {availableVersions.map((v) => (
                          <SelectItem key={v} value={String(v)}>
                            v{v}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                </>
              ) : (
                <>
                  <Label>Template version</Label>
                  {isLockedToTemplate ? (
                    <Input value={templateVersion ? `v${templateVersion}` : ""} disabled className="bg-muted" />
                  ) : (
                    <Select
                      value={templateVersion ? String(templateVersion) : ""}
                      onValueChange={(v) => {
                        const num = Number(v);
                        setTemplateVersion(Number.isFinite(num) ? num : null);
                        setAvailableSections([]);
                        setTargetSectionOrder("");
                        clearValidation();
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select version" />
                      </SelectTrigger>
                      <SelectContent>
                        {availableVersions.map((v) => (
                          <SelectItem key={v} value={String(v)}>
                            v{v}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}

                  <div className="pt-2 space-y-2">
                    <Label>Section</Label>
                    <Select
                      value={targetSectionOrder}
                      onValueChange={(v) => {
                        setTargetSectionOrder(v);
                        clearValidation();
                      }}
                      disabled={availableSections.length === 0}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select section" />
                      </SelectTrigger>
                      <SelectContent>
                        {availableSections.map((s) => (
                          <SelectItem key={s.order} value={String(s.order)}>
                            {`#${s.order}${s.title ? ` — ${s.title}` : ""}`}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </>
              )}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="file">Excel file (.xls or .xlsx)</Label>
            <Input
              id="file"
              type="file"
              accept=".xls,.xlsx,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,text/xml,application/xml"
              onChange={(e) => {
                const f = e.target.files?.[0] ?? null;
                setFile(f);
                clearValidation();
              }}
            />
            <p className="text-xs text-muted-foreground">
              Tip: Use the “Download Excel Template” button and fill in section/question/option rows.
            </p>
          </div>

          {(validation || validationError) && (
            <div className="rounded-lg border p-3 space-y-2">
              {validationError && (
                <div className="rounded-md bg-destructive/10 border border-destructive/20 p-3 text-sm text-destructive">
                  <div className="font-medium mb-1">Validation failed</div>
                  <div>{validationError}</div>
                </div>
              )}
              {validation && (
                <>
              <div className="flex flex-wrap gap-3 text-sm">
                <div>
                  <span className="text-muted-foreground">Valid:</span>{" "}
                  <span className="font-medium">{validation.isValid ? "Yes" : "No"}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Sections:</span>{" "}
                  <span className="font-mono">{validation.sectionsCount}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Questions:</span>{" "}
                  <span className="font-mono">{validation.questionsCount}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Options:</span>{" "}
                  <span className="font-mono">{validation.optionsCount}</span>
                </div>
                <div>
                  <span className="text-muted-foreground">Existing:</span>{" "}
                  <span className="font-medium">
                    {validation.templateExists
                      ? `Yes (latest v${validation.existingLatestVersion ?? "?"}${validation.existingLatestInUse ? ", in use" : ""})`
                      : "No"}
                  </span>
                </div>
              </div>

              {(validation.errors?.length ?? 0) > 0 && (
                <div className="max-h-56 overflow-auto text-sm">
                  <div className="font-medium mb-2">Issues</div>
                  <ul className="space-y-1">
                    {validation.errors.slice(0, 50).map((e, i) => (
                      <li key={i} className={String(e.severity).toLowerCase() === "error" ? "text-destructive" : ""}>
                        <span className="font-mono">Row {e.rowNumber}</span>
                        {e.column ? <span className="text-muted-foreground"> ({e.column})</span> : null}
                        : {e.message}
                      </li>
                    ))}
                  </ul>
                  {(validation.errors.length ?? 0) > 50 && (
                    <div className="text-xs text-muted-foreground mt-2">
                      Showing first 50 issues.
                    </div>
                  )}
                </div>
              )}
                </>
              )}
            </div>
          )}
        </div>

        <DialogFooter className="gap-2">
          <Button variant="outline" onClick={() => setOpen(false)} disabled={submitting}>
            Close
          </Button>
          <Button variant="secondary" onClick={handleValidate} disabled={!canValidate || submitting}>
            {submitting ? "Working..." : "Validate"}
          </Button>
          <Button onClick={handleExecute} disabled={!validation?.isValid || submitting}>
            Import
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

