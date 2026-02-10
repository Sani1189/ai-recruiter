using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Section within a questionnaire template.
/// </summary>
[Table("QuestionnaireSections")]
public class QuestionnaireSection : BaseDbModel
{
    [Required]
    [MaxLength(255)]
    public string QuestionnaireTemplateName { get; set; } = string.Empty;

    [Required]
    public int QuestionnaireTemplateVersion { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public virtual QuestionnaireTemplate? QuestionnaireTemplate { get; set; }

    public virtual ICollection<QuestionnaireQuestion> Questions { get; set; } = new List<QuestionnaireQuestion>();
}


