using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

/// <summary>
/// Question within a section (versioned entity).
/// </summary>
[Table("QuestionnaireQuestions")]
[Versioned(CascadeToChildren = true)]
public class QuestionnaireQuestion : VersionedBaseDbModel
{
    [Required]
    public Guid QuestionnaireSectionId { get; set; }

    [Required]
    public int Order { get; set; }

    /// <summary>
    /// Only one question version should be active per section/order.
    /// Older versions remain in the database for historical submissions but are not returned as part of the
    /// editable/candidate template read model.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    [MaxLength(50)]
    public QuestionnaireQuestionTypeEnum QuestionType { get; set; } = QuestionnaireQuestionTypeEnum.Text;

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    public bool IsRequired { get; set; } = false;

    [MaxLength(100)]
    public string? TraitKey { get; set; }

    public decimal? Ws { get; set; }

    public Guid? MediaFileId { get; set; }

    [MaxLength(1000)]
    public string? MediaUrl { get; set; }


    public virtual File? MediaFile { get; set; }

    public virtual ICollection<QuestionnaireQuestionOption> Options { get; set; } = new List<QuestionnaireQuestionOption>();
}


