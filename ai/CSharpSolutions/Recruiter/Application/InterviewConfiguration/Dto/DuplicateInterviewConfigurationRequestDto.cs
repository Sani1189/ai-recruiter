using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.InterviewConfiguration.Dto;

public sealed class DuplicateInterviewConfigurationRequestDto
{
    [Required]
    [MaxLength(255)]
    public string NewName { get; set; } = string.Empty;
}












