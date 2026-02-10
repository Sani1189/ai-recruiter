namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class QuestionnaireQuestionHistoryDetailsDto
{
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public int Order { get; set; }
    public string QuestionType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public string? PromptText { get; set; }

    public List<QuestionnaireOptionDto> Options { get; set; } = new();
}

