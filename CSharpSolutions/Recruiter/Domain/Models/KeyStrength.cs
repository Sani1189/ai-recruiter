using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents key strengths extracted from CV
/// </summary>
[Table("KeyStrengths")]
public class KeyStrength : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    [MaxLength(200)]
    public string StrengthName { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
