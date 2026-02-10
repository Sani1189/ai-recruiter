using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Questionnaire.Dto;

public sealed class CandidateQuestionnaireSubmitRequestDto
{
    [Required]
    public List<CandidateQuestionnaireAnswerDto> Answers { get; set; } = new();
}

public sealed class CandidateQuestionnaireAnswerDto
{
    [Required]
    [MaxLength(255)]
    public string QuestionName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int QuestionVersion { get; set; }

    public string? AnswerText { get; set; }

    public List<QuestionOptionReferenceDto> SelectedOptions { get; set; } = new();
}

public sealed class QuestionOptionReferenceDto
{
    [Required]
    [MaxLength(255)]
    public string OptionName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int OptionVersion { get; set; }
}

