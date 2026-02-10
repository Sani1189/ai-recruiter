using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

/// <summary>
/// DTO for updating job application status
/// </summary>
public class UpdateApplicationStatusDto
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
