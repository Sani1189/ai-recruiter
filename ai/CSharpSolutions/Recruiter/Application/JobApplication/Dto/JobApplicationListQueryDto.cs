using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

/// JobApplication list query parameters DTO for filtering and pagination
public class JobApplicationListQueryDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }
    
    [StringLength(100, ErrorMessage = "Job post name filter cannot exceed 100 characters")]
    public string? JobPostName { get; set; }
    
    [StringLength(100, ErrorMessage = "Job post version filter cannot exceed 100 characters")]
    public string? JobPostVersion { get; set; }
    
    public Guid? CandidateId { get; set; }
    
    public DateTime? AppliedAfter { get; set; }
    public DateTime? AppliedBefore { get; set; }
    
    public DateTime? CompletedAfter { get; set; }
    public DateTime? CompletedBefore { get; set; }
    
    public bool? IsCompleted { get; set; }
    public bool? IsRecent { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
