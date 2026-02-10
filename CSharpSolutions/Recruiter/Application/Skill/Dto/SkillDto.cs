using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Skill.Dto;

/// <summary>
/// DTO for Skill operations
/// </summary>
public class SkillDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    public string? Category { get; set; }

    [Required]
    public string SkillName { get; set; } = string.Empty;

    public string? Proficiency { get; set; }

    public int? YearsExperience { get; set; }

    public string? Unit { get; set; }
}
