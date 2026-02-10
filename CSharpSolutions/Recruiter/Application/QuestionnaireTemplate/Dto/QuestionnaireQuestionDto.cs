namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class QuestionnaireQuestionDto : VersionedEntityDto
{
    public int Order { get; set; }

    public string QuestionType { get; set; } = "Text";
    public bool IsRequired { get; set; }

    public string? PromptText { get; set; }

    public string? MediaUrl { get; set; }
    public Guid? MediaFileId { get; set; }

    // Personality-only (Likert)
    public decimal? Ws { get; set; }
    public string? TraitKey { get; set; }

    public List<QuestionnaireOptionDto> Options { get; set; } = new();
}


