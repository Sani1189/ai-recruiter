using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.File.Dto;

/// File list query parameters DTO for filtering and pagination
public class FileListQueryDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }
    
    [StringLength(100, ErrorMessage = "Container filter cannot exceed 100 characters")]
    public string? Container { get; set; }
    
    [StringLength(10, ErrorMessage = "Extension filter cannot exceed 10 characters")]
    public string? Extension { get; set; }
    
    public int? MinSizeMb { get; set; }
    public int? MaxSizeMb { get; set; }
    
    [StringLength(100, ErrorMessage = "Storage account name filter cannot exceed 100 characters")]
    public string? StorageAccountName { get; set; }
    
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    
    public bool? IsRecent { get; set; }
    public bool? IsLarge { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
