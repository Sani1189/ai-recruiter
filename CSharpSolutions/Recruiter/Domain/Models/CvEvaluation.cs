using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents a CV parsing and evaluation event
/// </summary>
[Table("CvEvaluations")]
public class CvEvaluation : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    [MaxLength(255)]
    public string PromptCategory { get; set; } = string.Empty;

    [Required]
    public int PromptVersion { get; set; }

    [Required]
    public Guid FileId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ModelUsed { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ResponseJson { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
    public virtual File? File { get; set; }
    public virtual List<Scoring> Scorings { get; set; } = new List<Scoring>();
}
