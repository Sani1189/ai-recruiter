using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Option for an option-based question (versioned entity).
/// </summary>
[Table("QuestionnaireQuestionOptions")]
[Versioned(CascadeToChildren = false)]
public class QuestionnaireQuestionOption : VersionedBaseDbModel
{
    [Required]
    [MaxLength(255)]
    public string QuestionnaireQuestionName { get; set; } = string.Empty;

    public int? QuestionnaireQuestionVersion { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Label { get; set; } = string.Empty;

    public Guid? MediaFileId { get; set; }

    [MaxLength(1000)]
    public string? MediaUrl { get; set; }

    /// <summary>
    /// Quiz-only.
    /// </summary>
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// Quiz-only.
    /// </summary>
    public decimal? Score { get; set; }

    /// <summary>
    /// Optional generic weight (non-quiz).
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Likert-only.
    /// </summary>
    public decimal? Wa { get; set; }

    public virtual QuestionnaireQuestion? QuestionnaireQuestion { get; set; }

    public virtual File? MediaFile { get; set; }
}


