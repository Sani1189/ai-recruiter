using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

// JobPostStepAssignment entity - junction table linking JobPost to JobPostSteps with ordering
[Table("JobPostStepAssignments")]
public class JobPostStepAssignment : BaseDbModel
{
    [MaxLength(255)]
    public string JobPostName { get; set; } = string.Empty;

    public int JobPostVersion { get; set; }

    [MaxLength(255)]
    public string StepName { get; set; } = string.Empty;

    // Nullable: null means "use latest version" (like InterviewConfiguration prompt versions)
    public int? StepVersion { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int StepNumber { get; set; } // Order of step within this job post

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // "pending", "in_progress", "completed", "skipped", "failed"

    // Navigation properties
    public virtual JobPost? JobPost { get; set; }
    public virtual JobPostStep? JobPostStep { get; set; }

    public Guid? TenantId { get; set; }

    // Composite key for Entity Framework
    public override bool Equals(object? obj)
    {
        if (obj is not JobPostStepAssignment other) return false;
        return JobPostName == other.JobPostName &&
               JobPostVersion == other.JobPostVersion &&
               StepName == other.StepName &&
               StepVersion == other.StepVersion;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(JobPostName, JobPostVersion, StepName, StepVersion);
    }
}
