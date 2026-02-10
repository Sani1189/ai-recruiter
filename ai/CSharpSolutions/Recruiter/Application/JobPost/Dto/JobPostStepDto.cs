using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobPost.Dto;

public class JobPostStepDto : VersionedBaseModelDto
{


    [MaxLength(100)]
    public string StepType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [RegularExpression("^(Candidate|Recruiter)$", ErrorMessage = "Participant must be: Candidate or Recruiter")]
    public string Participant { get; set; } = "Candidate";

    [Required]
    public bool ShowStepForCandidate { get; set; } = true;

    [MaxLength(255)]
    public string? DisplayTitle { get; set; }

    public string? DisplayContent { get; set; }

    [Required]
    public bool ShowSpinner { get; set; } = false;

    [MaxLength(255)]
    public string? InterviewConfigurationName { get; set; }

    [Range(1, int.MaxValue)]
    public int? InterviewConfigurationVersion { get; set; }

    [MaxLength(255)]
    public string? PromptName { get; set; }

    [Range(1, int.MaxValue)]
    public int? PromptVersion { get; set; }

    [MaxLength(255)]
    public string? QuestionnaireTemplateName { get; set; }

    [Range(1, int.MaxValue)]
    public int? QuestionnaireTemplateVersion { get; set; }
}