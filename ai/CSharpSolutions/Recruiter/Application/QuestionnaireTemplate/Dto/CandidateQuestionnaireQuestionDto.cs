using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.QuestionnaireTemplate.Dto;

/// <summary>
/// Candidate-safe question DTO (no answer key / scoring fields).
/// </summary>
public class CandidateQuestionnaireQuestionDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Version { get; set; }

    public int Order { get; set; }

    public string QuestionType { get; set; } = "Text";
    public bool IsRequired { get; set; }

    public string? PromptText { get; set; }

    public string? MediaUrl { get; set; }
    public Guid? MediaFileId { get; set; }

    public List<CandidateQuestionnaireOptionDto> Options { get; set; } = new();
}

