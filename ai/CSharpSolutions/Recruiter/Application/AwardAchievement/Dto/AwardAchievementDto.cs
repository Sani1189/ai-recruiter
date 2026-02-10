using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.AwardAchievement.Dto;

/// <summary>
/// DTO for AwardAchievement operations
/// </summary>
public class AwardAchievementDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Issuer { get; set; }

    public int? Year { get; set; }

    public string? Description { get; set; }
}
