using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Experience.Dto;

/// <summary>
/// DTO for Experience operations
/// </summary>
public class ExperienceDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    public string? Title { get; set; }

    public string? Organization { get; set; }

    public string? Industry { get; set; }

    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }
}
