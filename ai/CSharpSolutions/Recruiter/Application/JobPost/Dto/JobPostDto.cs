using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.JobPost.Dto;

public class JobPostDto : VersionedBaseModelDto
{
    
    [Required]
    [Range(1, 1000)]
    public int MaxAmountOfCandidatesRestriction { get; set; }
    
    [Required]
    [MinLength(1)]
    public List<string> MinimumRequirements { get; set; } = new List<string>();
    
    [Required]
    [MaxLength(50)]
    [RegularExpression("^(Entry|Mid|Senior|Lead|Executive)$", ErrorMessage = "Experience level must be: Entry, Mid, Senior, Lead, or Executive")]
    public string ExperienceLevel { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    [RegularExpression("^(FullTime|PartTime|Contract|Internship)$", ErrorMessage = "Job type must be: FullTime, PartTime, Contract, or Internship")]
    public string JobType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string JobDescription { get; set; } = string.Empty;

    // NEW FIELDS FOR ENHANCED JOB POSTING
    [Required]
    [MaxLength(100)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string IntroText { get; set; } = string.Empty;

    [Required]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    public string WhatWeOffer { get; set; } = string.Empty;

    [Required]
    public string CompanyInfo { get; set; } = string.Empty;

    /// <summary>Current kanban board column ID</summary>
    public Guid? CurrentBoardColumnId { get; set; }
    
    public bool? PoliceReportRequired { get; set; }

    /// <summary>Draft | Published | Archived. Serialized as string in API.</summary>
    public JobPostStatusEnum Status { get; set; } = JobPostStatusEnum.Draft;

    /// <summary>Recruiter's / job origin country (ISO 3166-1 alpha-2 code). Optional.</summary>
    [MaxLength(2)]
    public string? OriginCountryCode { get; set; }

    /// <summary>When Status is Published, list of country codes this job is exposed to. Optional.</summary>
    public List<string>? CountryExposureCountryCodes { get; set; }
    
    // Optional: Steps for frontend - can contain existing step references or new step data
    public List<JobPostStepRequestDto>? Steps { get; set; }
    
    // Read-only: Assigned steps with details (for GET operations)
    public List<JobPostStepAssignmentDto>? AssignedSteps { get; set; }
}
