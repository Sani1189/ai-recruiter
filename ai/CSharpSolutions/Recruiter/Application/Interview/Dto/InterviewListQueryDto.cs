using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Interview.Dto;

/// Interview list query parameters DTO for filtering and pagination
public class InterviewListQueryDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }
    
    public Guid? JobApplicationStepId { get; set; }
    
    [StringLength(100, ErrorMessage = "Interview configuration name cannot exceed 100 characters")]
    public string? InterviewConfigurationName { get; set; }
    
    public int? InterviewConfigurationVersion { get; set; }
    
    public DateTime? CompletedAfter { get; set; }
    public DateTime? CompletedBefore { get; set; }
    
    public bool? IsCompleted { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    
    public bool? IsRecent { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
