using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Candidate.Dto;

/// Candidate list query parameters DTO for filtering and pagination
public class CandidateListQueryDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }
    
    public Guid? UserId { get; set; }
    
    public Guid? CvFileId { get; set; }

    public DateTime? CreatedAfter { get; set; }

    public DateTime? CreatedBefore { get; set; }
    
    public bool? IsRecent { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    
    public string? SortBy { get; set; } = "CreatedAt";
    
    public bool SortDescending { get; set; } = true;
}
