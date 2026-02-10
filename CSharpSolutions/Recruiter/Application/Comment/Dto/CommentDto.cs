using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.Comment.Dto;

public class CommentDto : BaseModelDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public CommentableEntityType EntityType { get; set; }

    public Guid? ParentCommentId { get; set; }
}

