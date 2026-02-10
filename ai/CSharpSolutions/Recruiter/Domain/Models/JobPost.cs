using System.ComponentModel.DataAnnotations;
using Recruiter.Domain.Enums;

namespace Recruiter.Domain.Models;

// Inherits from VersionedBaseDbModel for composite primary key (Name + Version)
public class JobPost : VersionedBaseDbModel
{
    [Required]
    public int MaxAmountOfCandidatesRestriction { get; set; }

    [Required]
    public List<string> MinimumRequirements { get; set; } = new();

    [Required][MaxLength(50)]
    public string ExperienceLevel { get; set; } = string.Empty;

    [Required][MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    // Type of job (e.g., "Software Engineer", "Data Scientist")
    [Required][MaxLength(100)]
    public string JobType { get; set; } = string.Empty;

    [Required]
    public string JobDescription { get; set; } = string.Empty;

    // NEW FIELDS FOR ENHANCED JOB POSTING
    [Required]
    public string Industry { get; set; } = string.Empty;

    [Required]
    public string IntroText { get; set; } = string.Empty;

    [Required]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    public string WhatWeOffer { get; set; } = string.Empty;

    [Required]
    public string CompanyInfo { get; set; } = string.Empty;

    /// <summary>Current board column this job is in (FK to KanbanBoardColumns)</summary>
    public Guid? CurrentBoardColumnId { get; set; }
    public virtual KanbanBoardColumn? CurrentBoardColumn { get; set; }

    public bool? PoliceReportRequired { get; set; }

    public Guid? TenantId { get; set; }

    /// <summary>Draft | Published | Archived. When deleted and has applications, treat as Archived. Stored as string in DB.</summary>
    public JobPostStatusEnum Status { get; set; } = JobPostStatusEnum.Draft;

    /// <summary>Recruiter's / job origin country. Optional (e.g. for draft).</summary>
    public string? OriginCountryCode { get; set; }
    public virtual Country? OriginCountry { get; set; }

    /// <summary>Country exposure set (deterministic by country codes). Null when no exposure countries set.</summary>
    public new Guid? CountryExposureSetId { get; set; }
    public new virtual CountryExposureSet? CountryExposureSet { get; set; }

    // Navigation properties
    public virtual ICollection<JobPostStepAssignment> StepAssignments { get; set; } = new List<JobPostStepAssignment>();
}
