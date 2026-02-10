using System.ComponentModel.DataAnnotations;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents a column on a recruiter's kanban board for organizing jobs
/// Each recruiter can have multiple columns with custom names and visibility settings
/// </summary>
public class KanbanBoardColumn : BasicBaseDbModel
{
    /// <summary>Recruiter ID (FK to UserProfile)</summary>
    [Required]
    public Guid RecruiterId { get; set; }
    public virtual UserProfile? Recruiter { get; set; }

    /// <summary>Column name (e.g., "Draft", "Open", "Custom Column")</summary>
    [Required]
    [MaxLength(255)]
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>Order/sequence of this column on the board</summary>
    [Required]
    public int Sequence { get; set; }

    /// <summary>Whether this column is visible on the board</summary>
    [Required]
    public bool IsVisible { get; set; } = true;

    // Navigation properties
    public virtual ICollection<JobPost> Jobs { get; set; } = new List<JobPost>();

    // UNIQUE CONSTRAINT: (RecruiterId, Sequence)
}
