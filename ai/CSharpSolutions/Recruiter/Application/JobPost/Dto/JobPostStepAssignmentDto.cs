using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobPost.Dto;

public class JobPostStepAssignmentDto : BaseModelDto
{
    [Required]
    [MaxLength(255)]
    public string JobPostName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int JobPostVersion { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int StepNumber { get; set; } // Order of step within this job post

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // "pending", "in_progress", "completed", "skipped", "failed"

    // Step identification - StepVersion null means "use latest version dynamically"
    [MaxLength(255)]
    public string? StepName { get; set; }

    // Nullable: null means "use latest version" (not pinned to specific version)
    public int? StepVersion { get; set; }

    // Complete step details - contains all step information (name, version, type, etc.)
    // Used for response/read operations
    public JobPostStepDto? StepDetails { get; set; }
}
