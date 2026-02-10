using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.KeyStrength.Dto;

/// <summary>
/// DTO for KeyStrength operations
/// </summary>
public class KeyStrengthDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    public string StrengthName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
