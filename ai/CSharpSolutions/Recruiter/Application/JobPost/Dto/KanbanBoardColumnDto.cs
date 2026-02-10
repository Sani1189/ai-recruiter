using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobPost.Dto;

public class KanbanBoardColumnDto : BaseModelDto
{
    /// <summary>Recruiter ID who owns this column</summary>
    [Required]
    public Guid RecruiterId { get; set; }

    /// <summary>Column name (e.g., "Draft", "Open", "Custom Column")</summary>
    [Required]
    [MaxLength(255)]
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>Order/sequence of this column on the board</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Sequence { get; set; }

    /// <summary>Whether this column is visible on the board</summary>
    [Required]
    public bool IsVisible { get; set; } = true;
}
