namespace Recruiter.Application.Questionnaire.Dto;

public sealed class CandidateQuestionnaireSubmitResponseDto
{
    public Guid QuestionnaireSubmissionId { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTimeOffset? SubmittedAt { get; set; }
}

