using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

/// <summary>
/// Application step progress tracking.
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
public class JobApplicationStep : BaseDbModel
{
    [Required]
    public Guid JobApplicationId { get; set; }

    [Required]
    [MaxLength(255)]
    public string JobPostStepName { get; set; } = string.Empty;

    [Required]
    public int JobPostStepVersion { get; set; }

    [Required]
    [MaxLength(50)]
    public JobApplicationStepStatusEnum Status { get; set; } = JobApplicationStepStatusEnum.Pending;

    [Required]
    public int StepNumber { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? Data { get; set; } // JSON blob for step outputs/artifacts

    public Guid? TenantId { get; set; }

    // Navigation properties
    public virtual JobApplication? JobApplication { get; set; }
    public virtual JobPostStep? JobPostStep { get; set; }

    // 1:1 relationship with Interview
    public virtual Interview? Interview { get; set; }

    // 1:1 relationship with QuestionnaireSubmission (when this step is a questionnaire step)
    public virtual QuestionnaireCandidateSubmission? QuestionnaireCandidateSubmission { get; set; }

    // 1:Many relationship with JobApplicationStepFiles
    public virtual ICollection<JobApplicationStepFiles> JobApplicationStepFiles { get; set; } = new List<JobApplicationStepFiles>();
}
