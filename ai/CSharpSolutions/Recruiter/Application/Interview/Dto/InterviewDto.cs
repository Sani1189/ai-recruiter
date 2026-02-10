using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Interview.Dto;

/// <summary>
/// DTO for Interview operations
/// </summary>
public class InterviewDto : BaseModelDto
{
    [Required]
    public Guid JobApplicationStepId { get; set; }
    
    public string? InterviewAudioUrl { get; set; }
    
    [Required]
    public string InterviewConfigurationName { get; set; } = string.Empty;
    
    [Required]
    public int InterviewConfigurationVersion { get; set; }
    
    public string? TranscriptUrl { get; set; }
    
    public List<string> InterviewQuestions { get; set; } = new List<string>();
    
    public DateTimeOffset? CompletedAt { get; set; }
    
    public long? Duration { get; set; }
}
