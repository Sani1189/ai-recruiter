namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class QuestionnaireSectionDto
{
    public Guid Id { get; set; }
    public int Order { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public List<QuestionnaireQuestionDto> Questions { get; set; } = new();
}


