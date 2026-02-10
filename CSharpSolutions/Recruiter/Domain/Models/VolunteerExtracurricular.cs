using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents volunteer and extracurricular activities extracted from CV
/// </summary>
[Table("VolunteerExtracurricular")]
public class VolunteerExtracurricular : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [MaxLength(200)]
    public string? Role { get; set; }

    [MaxLength(200)]
    public string? Organization { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
