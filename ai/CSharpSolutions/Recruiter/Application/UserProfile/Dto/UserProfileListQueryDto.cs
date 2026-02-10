using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.UserProfile.Dto;

/// UserProfile list query parameters DTO for filtering and pagination
public class UserProfileListQueryDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }
    
    [StringLength(255, ErrorMessage = "Name filter cannot exceed 255 characters")]
    public string? Name { get; set; }
    
    [StringLength(255, ErrorMessage = "Email filter cannot exceed 255 characters")]
    public string? Email { get; set; }
    
    [StringLength(100, ErrorMessage = "Nationality filter cannot exceed 100 characters")]
    public string? Nationality { get; set; }
    
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    
    public bool? OpenToRelocation { get; set; }
    
    [StringLength(100, ErrorMessage = "Job type preference filter cannot exceed 100 characters")]
    public string? JobTypePreference { get; set; }
    
    [StringLength(100, ErrorMessage = "Remote preference filter cannot exceed 100 characters")]
    public string? RemotePreference { get; set; }
    
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
