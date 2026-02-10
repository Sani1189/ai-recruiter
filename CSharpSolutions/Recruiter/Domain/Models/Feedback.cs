using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents feedback for an interview.
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
[Table("Feedbacks")]
public class Feedback : BaseDbModel
{
    [Required]
    public string Detailed { get; set; } = string.Empty;

    [Required]
    public string Summary { get; set; } = string.Empty;

    public List<string> Strengths { get; set; } = new List<string>();

    public List<string> Weaknesses { get; set; } = new List<string>();

    [Required]
    public Guid InterviewId { get; set; }

    // 1:1 relationship with Interview
    public virtual Interview? Interview { get; set; }

    public Guid? TenantId { get; set; }
}
