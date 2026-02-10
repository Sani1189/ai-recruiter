using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobPost.Dto;

public class AssignStepRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int StepNumber { get; set; } // Order of step within this job post

    [MaxLength(50)]
    public string? Status { get; set; } = "pending"; // "pending", "in_progress", "completed", "skipped", "failed"

    // Complete step details - contains all step information (name, version, type, etc.)
    public JobPostStepDto? StepDetails { get; set; }
}
