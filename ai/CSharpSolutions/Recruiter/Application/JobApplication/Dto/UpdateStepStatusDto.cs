using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

public class UpdateStepStatusDto
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}