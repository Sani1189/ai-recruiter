using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

public class PromoteStepDto
{
    [Required]
    [MaxLength(255)]
    public string JobPostName { get; set; } = string.Empty;

    [Required]
    public int JobPostVersion { get; set; }

    [Required]
    public int CurrentStep { get; set; }

    [Required]
    public int NextStep { get; set; }

    [Required]
    public bool MarkAsComplete { get; set; } = true;
}
