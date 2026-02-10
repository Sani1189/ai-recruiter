using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents education details extracted from CV
/// </summary>
[Table("Educations")]
public class Education : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [MaxLength(200)]
    public string? Degree { get; set; }

    [MaxLength(200)]
    public string? Institution { get; set; }

    [MaxLength(200)]
    public string? FieldOfStudy { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
