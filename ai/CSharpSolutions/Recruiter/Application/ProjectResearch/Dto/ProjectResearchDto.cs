using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.ProjectResearch.Dto;

/// <summary>
/// DTO for ProjectResearch operations
/// </summary>
public class ProjectResearchDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Role { get; set; }

    public string? TechnologiesUsed { get; set; }

    public string? Link { get; set; }
}
