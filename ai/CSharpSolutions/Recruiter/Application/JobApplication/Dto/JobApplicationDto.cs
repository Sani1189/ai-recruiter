using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobApplication.Dto;

/// <summary>
/// DTO for JobApplication operations
/// </summary>
public class JobApplicationDto : BaseModelDto
{
    [Required]
    public string JobPostName { get; set; } = string.Empty;
    
    [Required]
    public int JobPostVersion { get; set; }
    
    public Guid? CandidateId { get; set; } // Will be set automatically from authenticated user
    
    public JobPostDto? JobPost { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
