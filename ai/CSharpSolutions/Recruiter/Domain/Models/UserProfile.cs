using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents a user profile.
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
[Table("UserProfiles")]
public class UserProfile : BaseDbModel
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }

    public List<string> JobTypePreferences { get; set; } = new List<string>();

    public List<string> RemotePreferences { get; set; } = new List<string>();

    public List<string> Roles { get; set; } = new List<string>();

    public int? Age { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    // Nullable to handle NULL values from database (from older migrations)
    // AutoMapper converts NULL to false when mapping to DTO
    // No default value here - always set explicitly in code to ensure EF Core tracks it
    public bool? OpenToRelocation { get; set; }

    // Navigation properties
    public virtual Candidate? Candidate { get; set; }
    public virtual List<KeyStrength> KeyStrengths { get; set; } = new List<KeyStrength>();
    public virtual List<Skill> Skills { get; set; } = new List<Skill>();
    public virtual List<Experience> Experiences { get; set; } = new List<Experience>();
    public virtual List<Education> Educations { get; set; } = new List<Education>();
    public virtual List<CvEvaluation> CvEvaluations { get; set; } = new List<CvEvaluation>();
    public virtual List<AwardAchievement> AwardAchievements { get; set; } = new List<AwardAchievement>();
    public virtual List<CertificationLicense> CertificationLicenses { get; set; } = new List<CertificationLicense>();
    public virtual List<Summary> Summaries { get; set; } = new List<Summary>();
    public virtual List<VolunteerExtracurricular> VolunteerExtracurriculars { get; set; } = new List<VolunteerExtracurricular>();
    public virtual List<ProjectResearch> ProjectResearches { get; set; } = new List<ProjectResearch>();
}
