using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobPost.Dto;

public sealed class DuplicateJobPostStepRequestDto
{
    [Required]
    [MaxLength(255)]
    public string NewName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? NewDisplayTitle { get; set; }
}


