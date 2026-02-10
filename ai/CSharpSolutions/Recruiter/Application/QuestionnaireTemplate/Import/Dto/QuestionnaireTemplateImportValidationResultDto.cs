namespace Recruiter.Application.QuestionnaireTemplate.Import.Dto;

public sealed class QuestionnaireTemplateImportValidationResultDto
{
    public bool IsValid { get; init; }
    public QuestionnaireTemplateImportScope? Scope { get; init; }

    public string? TemplateName { get; init; }
    public string? TemplateType { get; init; }

    public bool TemplateExists { get; init; }
    public int? ExistingLatestVersion { get; init; }
    public bool ExistingLatestInUse { get; init; }

    public int TotalRows { get; init; }
    public int SectionsCount { get; init; }
    public int QuestionsCount { get; init; }
    public int OptionsCount { get; init; }

    public List<QuestionnaireTemplateImportValidationErrorDto> Errors { get; init; } = new();
}

