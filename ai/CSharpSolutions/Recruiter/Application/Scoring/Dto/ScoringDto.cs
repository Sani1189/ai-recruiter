using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Scoring.Dto;

/// <summary>
/// DTO for Scoring operations
/// </summary>
public class ScoringDto : BaseModelDto
{
    [Required]
    public Guid CvEvaluationId { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string FixedCategory { get; set; } = string.Empty;

    public int? Score { get; set; }

    public int? Years { get; set; }

    public string? Level { get; set; }
}
