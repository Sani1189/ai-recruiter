using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobPost.Dto;

/// <summary>
/// DTO for job post candidate information
/// </summary>
public class JobPostCandidateDto
{
    public Guid ApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    
    /// <summary>
    /// Auto-generated candidate serial in format CA-YY-MM-XXXX
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string CandidateSerial { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string CandidateName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidateCvFilePath { get; set; }
    
    public DateTime AppliedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// The current step number the candidate is on (1-indexed).
    /// This is calculated as: number of completed steps + 1
    /// </summary>
    public int CurrentStep { get; set; } = 1;
    
    /// <summary>
    /// Number of steps the candidate has completed
    /// </summary>
    public int CompletedStepsCount { get; set; } = 0;
}
