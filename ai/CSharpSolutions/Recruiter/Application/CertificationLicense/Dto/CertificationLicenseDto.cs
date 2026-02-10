using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.CertificationLicense.Dto;

/// <summary>
/// DTO for CertificationLicense operations
/// </summary>
public class CertificationLicenseDto : BaseModelDto
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Issuer { get; set; }

    public DateTime? DateIssued { get; set; }

    public DateTime? ValidUntil { get; set; }
}
