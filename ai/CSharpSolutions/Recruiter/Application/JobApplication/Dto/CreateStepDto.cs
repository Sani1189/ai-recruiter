using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

public class CreateStepDto
{
    [Required]
    [MaxLength(255)]
    public string JobPostStepName { get; set; } = string.Empty;

    [Required]
    public int JobPostStepVersion { get; set; }

    [Required]
    public int StepNumber { get; set; }

    public string? Data { get; set; }
}


