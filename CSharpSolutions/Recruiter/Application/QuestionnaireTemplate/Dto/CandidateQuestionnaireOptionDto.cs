using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.QuestionnaireTemplate.Dto;

/// <summary>
/// Candidate-safe option DTO (no answer key / scoring fields).
/// </summary>
public class CandidateQuestionnaireOptionDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Version { get; set; }

    public int Order { get; set; }

    public string? Label { get; set; }

    public string? MediaUrl { get; set; }
    public Guid? MediaFileId { get; set; }
}

