using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

/// <summary>
/// Comment on various entities (candidates, applications, etc.).
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
[Table("Comments")]
public class Comment : BaseDbModel
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid EntityId { get; set; }
    [Required]
    public CommentableEntityType EntityType { get; set; }

    public Guid? ParentCommentId { get; set; } // Optional: Support for nested comments or replies
}