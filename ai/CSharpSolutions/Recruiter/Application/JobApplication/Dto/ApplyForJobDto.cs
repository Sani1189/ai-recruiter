using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

/// <summary>
/// DTO for applying for a job - candidate ID is automatically set from authenticated user
/// </summary>
public class ApplyForJobDto
{
    [Required]
    [MaxLength(255)]
    public string JobPostName { get; set; } = string.Empty;
    
    [Required]
    public int JobPostVersion { get; set; }
}
