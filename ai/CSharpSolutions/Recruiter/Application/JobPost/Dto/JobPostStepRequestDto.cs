using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobPost.Dto;

public class JobPostStepRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int StepNumber { get; set; } // Order within job post

    // Option 1: Use existing step template
    [MaxLength(255)]
    public string? ExistingStepName { get; set; }

    // version can be null if UseLatestVersion is true
    [Range(1, int.MaxValue)]
    public int? ExistingStepVersion { get; set; }

    public bool UseLatestVersion { get; set; } = true;

    // Option 2: Create new step template (optional - if ExistingStepName is null)
    public JobPostStepDto? NewStep { get; set; }

    // Validation: Must have either existing step reference OR new step data
    // When using existing step, ExistingStepName is required but ExistingStepVersion is optional (null = use latest)
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ExistingStepName) || NewStep != null;
    }
}
