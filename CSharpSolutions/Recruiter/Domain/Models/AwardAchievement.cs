using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents awards and achievements extracted from CV
/// </summary>
[Table("AwardsAchievements")]
public class AwardAchievement : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Issuer { get; set; }

    public int? Year { get; set; }

    public string? Description { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
