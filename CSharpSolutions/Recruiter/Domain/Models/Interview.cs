using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Interview session for a job application step.
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
[Table("Interviews")]
public class Interview : BaseDbModel
{
    [Required]
    public Guid JobApplicationStepId { get; set; }

    [MaxLength(500)]
    public string? InterviewAudioUrl { get; set; }

    [Required]
    [MaxLength(255)]
    public string InterviewConfigurationName { get; set; } = string.Empty;

    [Required]
    public int InterviewConfigurationVersion { get; set; }

    [MaxLength(500)]
    public string? TranscriptUrl { get; set; }

    public DateTimeOffset? StartedAt { get; set; }
    
    public DateTimeOffset? CompletedAt { get; set; }

    public long? Duration { get; set; } // in milliseconds

    // Prompt references (snapshot of prompts used in this interview)
    [Required]
    [MaxLength(255)]
    public string InstructionPromptName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PersonalityPromptName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string QuestionsPromptName { get; set; } = string.Empty;

    [Required]
    public int InstructionPromptVersion { get; set; }

    [Required]
    public int PersonalityPromptVersion { get; set; }

    [Required]
    public int QuestionsPromptVersion { get; set; }

    // Navigation properties
    public virtual JobApplicationStep? JobApplicationStep { get; set; }
    public virtual InterviewConfiguration? InterviewConfiguration { get; set; }

    // Navigation properties for prompts
    public virtual Prompt? InstructionPrompt { get; set; }
    public virtual Prompt? PersonalityPrompt { get; set; }
    public virtual Prompt? QuestionsPrompt { get; set; }

    // 1:1 relationship with Score
    public virtual Score? Score { get; set; }

    // 1:1 relationship with Feedback
    public virtual Feedback? Feedback { get; set; }

    public Guid? TenantId { get; set; }
}
