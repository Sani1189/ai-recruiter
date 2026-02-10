using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents an interview configuration template
/// </summary>
[Table("InterviewConfigurations")]
[Versioned(CascadeToChildren = true)]
public class InterviewConfiguration : VersionedBaseDbModel
{
    [Required]
    [MaxLength(50)]
    public string Modality { get; set; } = string.Empty; // "voice" | "text" | "coding" | "assignment"

    [MaxLength(100)]
    public string? Tone { get; set; }

    [MaxLength(100)]
    public string? ProbingDepth { get; set; }

    [MaxLength(100)]
    public string? FocusArea { get; set; }

    public int? Duration { get; set; } // in minutes

    [MaxLength(50)]
    public string? Language { get; set; }

    [Required]
    [MaxLength(255)]
    public string InstructionPromptName { get; set; } = string.Empty;

    public int? InstructionPromptVersion { get; set; }

    [Required]
    [MaxLength(255)]
    public string PersonalityPromptName { get; set; } = string.Empty;

    public int? PersonalityPromptVersion { get; set; }

    [Required]
    [MaxLength(255)]
    public string QuestionsPromptName { get; set; } = string.Empty;

    public int? QuestionsPromptVersion { get; set; }

    [Required]
    public bool Active { get; set; } = true;
    

    // Navigation properties
    public virtual Prompt? InstructionPrompt { get; set; }
    public virtual Prompt? PersonalityPrompt { get; set; }
    public virtual Prompt? QuestionsPrompt { get; set; }
}
