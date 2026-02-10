using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

public sealed class BeginJobApplicationStepRequestDto
{
    [Required]
    [MaxLength(255)]
    public string StepName { get; set; } = string.Empty;

    public int? StepVersion { get; set; }

    [Required]
    public int StepNumber { get; set; }
}



