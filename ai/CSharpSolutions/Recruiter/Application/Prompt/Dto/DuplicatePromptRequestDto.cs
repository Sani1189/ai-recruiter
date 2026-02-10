using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Prompt.Dto;

public sealed class DuplicatePromptRequestDto
{
    [Required]
    [MaxLength(255)]
    public string NewName { get; set; } = string.Empty;
}












