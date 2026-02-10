using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Summary.Dto;

/// <summary>
/// DTO for Summary operations
/// </summary>
public class SummaryDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? Text { get; set; }
}
