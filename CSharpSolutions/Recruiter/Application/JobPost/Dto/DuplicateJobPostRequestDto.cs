using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobPost.Dto;

public sealed class DuplicateJobPostRequestDto
{
    [Required]
    [MaxLength(255)]
    public string NewName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NewJobTitle { get; set; } = string.Empty;
}


