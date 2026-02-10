namespace Recruiter.Application.QuestionnaireTemplate.Import.Dto;

public sealed class QuestionnaireTemplateImportExecuteResultDto
{
    public string TemplateName { get; init; } = string.Empty;
    public int TemplateVersion { get; init; }
    public string TemplateType { get; init; } = string.Empty;
    public QuestionnaireTemplateImportScope Scope { get; init; }

    public bool CreatedNewTemplate { get; init; }
    public bool CreatedNewVersion { get; init; }

    public int SectionsCount { get; init; }
    public int QuestionsCount { get; init; }
    public int OptionsCount { get; init; }

    public List<QuestionnaireTemplateImportValidationErrorDto> Messages { get; init; } = new();
}

