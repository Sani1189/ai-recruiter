using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.InterviewConfiguration.Dto;

/// <summary>
/// DTO for InterviewConfiguration operations
/// </summary>
public class InterviewConfigurationDto : VersionedBaseModelDto
{
    [Required]
    [MaxLength(50)]
    public string Modality { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Tone { get; set; }
    
    [MaxLength(100)]
    public string? ProbingDepth { get; set; }
    
    [MaxLength(100)]
    public string? FocusArea { get; set; }
    
    public int? Duration { get; set; }
    
    [MaxLength(50)]
    public string? Language { get; set; }
    
    [Required]
    public string InstructionPromptName { get; set; } = string.Empty;
    
    [Required]
    public string PersonalityPromptName { get; set; } = string.Empty;
    
    [Required]
    public string QuestionsPromptName { get; set; } = string.Empty;
    
    public int? InstructionPromptVersion { get; set; }
    
    public int? PersonalityPromptVersion { get; set; }
    
    public int? QuestionsPromptVersion { get; set; }
    
    [Required]
    public bool Active { get; set; } = true;
}
