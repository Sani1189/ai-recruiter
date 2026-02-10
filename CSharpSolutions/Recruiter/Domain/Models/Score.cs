using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents scoring data for an interview
/// </summary>
[Table("Scores")]
public class Score : BaseDbModel
{
    [Required]
    public double Average { get; set; }

    [Required]
    public double English { get; set; }

    [Required]
    public double Technical { get; set; }

    [Required]
    public double Communication { get; set; }

    [Required]
    public double ProblemSolving { get; set; }

    [Required]
    public Guid InterviewId { get; set; }

    // 1:1 relationship with Interview
    public virtual Interview? Interview { get; set; }

    public Guid? TenantId { get; set; }
}
