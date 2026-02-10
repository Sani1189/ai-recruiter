namespace Recruiter.Domain.Enums;

/// <summary>
/// Centralized string constants for questionnaire/assessment-related API/domain values.
/// We intentionally store these values as strings in the DB/API for stability.
/// Keeping them here prevents scattering hard-coded strings across the codebase.
/// </summary>
public static class QuestionnaireConstants
{
    public static class StepTypes
    {
        // Canonical step type name going forward.
        public const string Questionnaire = "Questionnaire";
        // Legacy/compat value used in older data payloads.
        public const string AssessmentLegacy = "Assessment";
        // Legacy/compat value used in older data payloads.
        public const string MultipleChoiceLegacy = "Multiple Choice";
    }

    public static class TemplateTypes
    {
        public const string Form = "Form";
        public const string Quiz = "Quiz";
        public const string Personality = "Personality";
    }

    public static class QuestionTypes
    {
        public const string Text = "Text";
        public const string Textarea = "Textarea";
        public const string SingleChoice = "SingleChoice";
        public const string MultiChoice = "MultiChoice";
        public const string Likert = "Likert";
        public const string Radio = "Radio";
        public const string Checkbox = "Checkbox";
        public const string Dropdown = "Dropdown";
    }

    public static class SubmissionStatuses
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string AutoScored = "AutoScored";
        public const string Reviewed = "Reviewed";
    }
}

