using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

/// <summary>
/// Questionnaire template (versioned aggregate root).
/// </summary>
[Table("QuestionnaireTemplates")]
[Versioned(CascadeToChildren = true)]
public class QuestionnaireTemplate : VersionedBaseDbModel
{
    [Required]
    [MaxLength(50)]
    public QuestionnaireTemplateTypeEnum TemplateType { get; set; } = QuestionnaireTemplateTypeEnum.Form;

    /// <summary>
    /// "Draft" | "Published" | "Archived"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public int? TimeLimitSeconds { get; set; }

    public virtual ICollection<QuestionnaireSection> Sections { get; set; } = new List<QuestionnaireSection>();
}


