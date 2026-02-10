using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents skills extracted from CV
/// </summary>
[Table("Skills")]
public class Skill : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Required]
    [MaxLength(200)]
    public string SkillName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Proficiency { get; set; }

    public int? YearsExperience { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
