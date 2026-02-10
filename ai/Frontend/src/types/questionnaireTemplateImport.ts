export type QuestionnaireTemplateImportScope =
  | "CreateTemplate"
  | "AppendToTemplate"
  | "AppendToSection";

export interface QuestionnaireTemplateImportValidationError {
  rowNumber: number;
  column?: string | null;
  message: string;
  severity: "Error" | "Warning" | string;
}

export interface QuestionnaireTemplateImportValidationResult {
  isValid: boolean;
  scope?: QuestionnaireTemplateImportScope | null;
  templateName?: string | null;
  templateType?: string | null;

  templateExists: boolean;
  existingLatestVersion?: number | null;
  existingLatestInUse: boolean;

  totalRows: number;
  sectionsCount: number;
  questionsCount: number;
  optionsCount: number;

  errors: QuestionnaireTemplateImportValidationError[];
}

export interface QuestionnaireTemplateImportExecuteResult {
  templateName: string;
  templateVersion: number;
  templateType: string;
  scope: QuestionnaireTemplateImportScope;
  createdNewTemplate: boolean;
  createdNewVersion: boolean;
  sectionsCount: number;
  questionsCount: number;
  optionsCount: number;
  messages?: QuestionnaireTemplateImportValidationError[];
}

