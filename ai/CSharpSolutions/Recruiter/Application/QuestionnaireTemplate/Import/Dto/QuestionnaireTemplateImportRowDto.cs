namespace Recruiter.Application.QuestionnaireTemplate.Import.Dto;

internal sealed class QuestionnaireTemplateImportRowDto
{
    public int RowNumber { get; init; }

    public string? Scope { get; set; }
    public string? TemplateName { get; set; }
    public string? TemplateType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public string? TargetSectionOrder { get; set; }

    public string? SectionOrder { get; set; }
    public string? SectionTitle { get; set; }

    public string? QuestionOrder { get; set; }
    public string? QuestionType { get; set; }
    public string? IsRequired { get; set; }
    public string? QuestionTitle { get; set; }

    public string? TraitKey { get; set; }
    public string? Ws { get; set; }

    public string? OptionOrder { get; set; }
    public string? OptionLabel { get; set; }
    public string? IsCorrect { get; set; }
    public string? Score { get; set; }
    public string? Wa { get; set; }
}

