using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Prompt.Dto;

/// <summary>
/// DTO for Prompt operations
/// </summary>
public class PromptDto : VersionedBaseModelDto
{
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string? Locale { get; set; }
    
    public List<string> Tags { get; set; } = new List<string>();
}
