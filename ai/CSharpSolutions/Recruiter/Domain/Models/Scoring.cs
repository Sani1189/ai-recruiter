using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents scoring data extracted from CV evaluation
/// </summary>
[Table("Scorings")]
public class Scoring : BaseDbModel
{
    [Required]
    public Guid CvEvaluationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)] public string? FixedCategory { get; set; } = string.Empty;

    public int? Score { get; set; }

    public int? Years { get; set; }

    [MaxLength(50)]
    public string? Level { get; set; }

    // Navigation properties
    public virtual CvEvaluation? CvEvaluation { get; set; }
}
