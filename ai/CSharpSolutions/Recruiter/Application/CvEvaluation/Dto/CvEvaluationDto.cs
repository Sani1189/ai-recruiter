using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.CvEvaluation.Dto;

/// <summary>
/// DTO for CvEvaluation operations
/// </summary>
public class CvEvaluationDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    public string PromptCategory { get; set; } = string.Empty;

    [Required]
    public int PromptVersion { get; set; }

    [Required]
    public Guid FileId { get; set; }

    [Required]
    public string ModelUsed { get; set; } = string.Empty;

    [Required]
    public string ResponseJson { get; set; } = string.Empty;
}
