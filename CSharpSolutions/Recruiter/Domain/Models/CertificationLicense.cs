using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents certifications and licenses extracted from CV
/// </summary>
[Table("CertificationsLicenses")]
public class CertificationLicense : BaseDbModel
{
    [Required]
    public Guid UserProfileId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Issuer { get; set; }

    public DateTime? DateIssued { get; set; }

    public DateTime? ValidUntil { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
}
