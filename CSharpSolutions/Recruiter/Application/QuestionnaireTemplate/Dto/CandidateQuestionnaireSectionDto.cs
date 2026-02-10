namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class CandidateQuestionnaireSectionDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public List<CandidateQuestionnaireQuestionDto> Questions { get; set; } = new();
}

