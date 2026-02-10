using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.VolunteerExtracurricular.Dto;

/// <summary>
/// DTO for VolunteerExtracurricular operations
/// </summary>
public class VolunteerExtracurricularDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    public string? Role { get; set; }

    public string? Organization { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }
}
