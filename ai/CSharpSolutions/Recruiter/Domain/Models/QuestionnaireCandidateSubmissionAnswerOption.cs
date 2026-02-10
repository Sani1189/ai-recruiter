using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Selected option for an answer (1..N depending on question type).
/// Stores exact option version used at submission time.
/// </summary>
[Table("QuestionnaireCandidateSubmissionAnswerOptions")]
public class QuestionnaireCandidateSubmissionAnswerOption : BaseDbModel
{
    [Required]
    public Guid QuestionnaireCandidateSubmissionAnswerId { get; set; }

    [Required]
    [MaxLength(255)]
    public string QuestionnaireQuestionOptionName { get; set; } = string.Empty;

    [Required]
    public int QuestionnaireQuestionOptionVersion { get; set; }

    public bool? IsCorrect { get; set; }

    public decimal? Score { get; set; }

    public decimal? Wa { get; set; }

    public virtual QuestionnaireCandidateSubmissionAnswer? QuestionnaireCandidateSubmissionAnswer { get; set; }

    public virtual QuestionnaireQuestionOption? QuestionnaireQuestionOption { get; set; }
}


