namespace Recruiter.Application.QuestionnaireTemplate.Import.Dto;

public sealed class QuestionnaireTemplateImportRequestDto
{
    public QuestionnaireTemplateImportScope? Scope { get; init; }
    public string? TemplateName { get; init; }
    public int? TemplateVersion { get; init; }
    public string? TemplateType { get; init; }
    public int? TargetSectionOrder { get; init; }

    // Optional template metadata overrides (applied for CreateTemplate only)
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int? TimeLimitSeconds { get; init; }
}

