namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class QuestionnaireOptionDto : VersionedEntityDto
{
    public int Order { get; set; }

    public string? Label { get; set; }

    public string? MediaUrl { get; set; }
    public Guid? MediaFileId { get; set; }

    // Quiz-only
    public bool? IsCorrect { get; set; }
    public decimal? Score { get; set; }

    // Generic
    public decimal? Weight { get; set; }

    // Likert-only
    public decimal? Wa { get; set; }
}


