using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Comment.Dto;

/// <summary>
/// Comment list query parameters DTO for filtering, pagination, and sorting
/// </summary>
public class CommentListQueryDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }
    
    public Guid? EntityId { get; set; }
    
    public Domain.Enums.CommentableEntityType? EntityType { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    
    public bool? IncludeReplies { get; set; }
    
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

