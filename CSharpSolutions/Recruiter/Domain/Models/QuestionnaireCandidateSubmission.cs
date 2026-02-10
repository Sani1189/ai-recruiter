using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

/// <summary>
/// Candidate questionnaire submission for a single JobApplicationStep (1:1).
/// Supports Draft/Submitted and scoring snapshots.
/// </summary>
[Table("QuestionnaireCandidateSubmissions")]
public class QuestionnaireCandidateSubmission : BaseDbModel
{
    [Required]
    public Guid JobApplicationStepId { get; set; }

    [Required]
    [MaxLength(255)]
    public string QuestionnaireTemplateName { get; set; } = string.Empty;

    [Required]
    public int QuestionnaireTemplateVersion { get; set; }

    [Required]
    [MaxLength(50)]
    public QuestionnaireTemplateTypeEnum TemplateType { get; set; } = QuestionnaireTemplateTypeEnum.Form;

    [Required]
    [MaxLength(50)]
    public QuestionnaireSubmissionStatusEnum Status { get; set; } = QuestionnaireSubmissionStatusEnum.Draft;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? LastSavedAt { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public decimal? TotalScore { get; set; }

    public decimal? MaxScore { get; set; }

    public string? PersonalityResultJson { get; set; }

    public virtual JobApplicationStep? JobApplicationStep { get; set; }

    public virtual QuestionnaireTemplate? QuestionnaireTemplate { get; set; }

    public virtual ICollection<QuestionnaireCandidateSubmissionAnswer> Answers { get; set; } = new List<QuestionnaireCandidateSubmissionAnswer>();
}


