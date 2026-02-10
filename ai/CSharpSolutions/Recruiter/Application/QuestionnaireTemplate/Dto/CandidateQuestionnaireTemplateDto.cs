namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class CandidateQuestionnaireTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }

    public string TemplateType { get; set; } = "Form";
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? TimeLimitSeconds { get; set; }

    public List<CandidateQuestionnaireSectionDto> Sections { get; set; } = new();
}


