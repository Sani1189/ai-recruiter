using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Job application linking a candidate to a job post.
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
public class JobApplication : BaseDbModel
{
    [Required]
    [MaxLength(255)]
    public string JobPostName { get; set; } = string.Empty;

    [Required]
    public int JobPostVersion { get; set; }

    [Required]
    public Guid CandidateId { get; set; }

    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? TenantId { get; set; }

    // Navigation properties
    [VersionedNavigation(typeof(JobApplicationStep), "JobApplicationId")]
    public virtual ICollection<JobApplicationStep> Steps { get; set; } = new List<JobApplicationStep>();

    public virtual JobPost? JobPost { get; set; }
    public virtual Candidate? Candidate { get; set; }
}
