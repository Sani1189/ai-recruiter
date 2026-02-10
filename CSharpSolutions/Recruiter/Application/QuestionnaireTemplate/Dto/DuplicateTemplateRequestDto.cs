using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class DuplicateTemplateRequestDto
{
    [Required]
    public string NewName { get; set; } = string.Empty;
    
    public bool IncludeQuestions { get; set; } = true;
    public bool IncludeTitle { get; set; } = true;
    public bool IncludeDescription { get; set; } = true;
}
