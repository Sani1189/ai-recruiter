using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Education.Dto;

/// <summary>
/// DTO for Education operations
/// </summary>
public class EducationDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    public string? Degree { get; set; }

    public string? Institution { get; set; }

    public string? FieldOfStudy { get; set; }

    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
