using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents projects and research extracted from CV
/// </summary>
[Table("ProjectsResearch")]
public class ProjectResearch : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Role { get; set; }

    [MaxLength(500)]
    public string? TechnologiesUsed { get; set; }

    [MaxLength(500)]
    public string? Link { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
