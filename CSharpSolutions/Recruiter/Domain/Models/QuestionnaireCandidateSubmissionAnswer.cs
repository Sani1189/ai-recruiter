using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

/// <summary>
/// One answer per template question (per submission).
/// Stores exact question version used at submission time.
/// </summary>
[Table("QuestionnaireCandidateSubmissionAnswers")]
public class QuestionnaireCandidateSubmissionAnswer : BaseDbModel
{
    [Required]
    public Guid QuestionnaireCandidateSubmissionId { get; set; }

    [Required]
    [MaxLength(255)]
    public string QuestionnaireQuestionName { get; set; } = string.Empty;

    [Required]
    public int QuestionnaireQuestionVersion { get; set; }

    [Required]
    [MaxLength(50)]
    public QuestionnaireQuestionTypeEnum QuestionType { get; set; } = QuestionnaireQuestionTypeEnum.Text;

    [Required]
    public int QuestionOrder { get; set; }

    public string? AnswerText { get; set; }

    public decimal? ScoreAwarded { get; set; }

    public decimal? WaSum { get; set; }

    public DateTimeOffset? AnsweredAt { get; set; }

    public virtual QuestionnaireCandidateSubmission? QuestionnaireCandidateSubmission { get; set; }

    public virtual QuestionnaireQuestion? QuestionnaireQuestion { get; set; }

    public virtual ICollection<QuestionnaireCandidateSubmissionAnswerOption> SelectedOptions { get; set; } = new List<QuestionnaireCandidateSubmissionAnswerOption>();
}


