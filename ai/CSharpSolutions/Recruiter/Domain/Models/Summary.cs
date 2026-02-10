using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents summaries extracted from CV
/// </summary>
[Table("Summaries")]
public class Summary : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    public string? Text { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
