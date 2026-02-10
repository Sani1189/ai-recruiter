using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobApplication.Dto;

/// <summary>
/// DTO for JobApplicationStep operations
/// </summary>
public class JobApplicationStepDto : BaseModelDto
{
    [Required]
    public Guid JobApplicationId { get; set; }
    
    [Required]
    public string JobPostStepName { get; set; } = string.Empty;
    
    [Required]
    public int JobPostStepVersion { get; set; }
    
    [Required]
    public string Status { get; set; } = "pending";
    
    [Required]
    public int StepNumber { get; set; }
    
    public DateTimeOffset? StartedAt { get; set; }
    
    public DateTimeOffset? CompletedAt { get; set; }
    
    public string? Data { get; set; }
}
