using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

// JobPostStep entity - reusable step templates for job posts
[Table("JobPostSteps")]
public class JobPostStep : VersionedBaseDbModel
{

    [Required]
    public bool IsInterview { get; set; }

    [Required]
    [MaxLength(100)]
    public string StepType { get; set; } = string.Empty; // "Screening", "Technical", "Behavioral", "Assignment", "Interview"

    /// <summary>
    /// Defines who performs the step. Candidate steps are visible to candidates; recruiter steps are internal.
    /// Stored as a string for stable API payloads without requiring enum JSON converters.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Participant { get; set; } = "Candidate"; // "Candidate" | "Recruiter"

    [Required]
    public bool ShowStepForCandidate { get; set; } = true;

    [MaxLength(255)]
    public string? DisplayTitle { get; set; }

    public string? DisplayContent { get; set; }

    [Required]
    public bool ShowSpinner { get; set; } = false;

    [MaxLength(255)]
    public string? InterviewConfigurationName { get; set; }

    public int? InterviewConfigurationVersion { get; set; }

    // Optional prompt used for AI processing (e.g., Resume Upload extraction)
    [MaxLength(255)]
    public string? PromptName { get; set; }

    public int? PromptVersion { get; set; }

    // Optional questionnaire template (Name + Version)
    [MaxLength(255)]
    public string? QuestionnaireTemplateName { get; set; }

    public int? QuestionnaireTemplateVersion { get; set; }

    // Navigation properties
    public virtual ICollection<JobPostStepAssignment> StepAssignments { get; set; } = new List<JobPostStepAssignment>();
    public virtual InterviewConfiguration? InterviewConfiguration { get; set; }
    public virtual Prompt? Prompt { get; set; }
    public virtual QuestionnaireTemplate? QuestionnaireTemplate { get; set; }

    // Parameterless constructor for EF Core and object initialization
    public JobPostStep() { }

    public Guid? TenantId { get; set; }
}
