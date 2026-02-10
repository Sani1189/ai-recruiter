using Recruiter.Application.AwardAchievement.Dto;
using Recruiter.Application.CertificationLicense.Dto;
using Recruiter.Application.Education.Dto;
using Recruiter.Application.Experience.Dto;
using Recruiter.Application.KeyStrength.Dto;
using Recruiter.Application.ProjectResearch.Dto;
using Recruiter.Application.Skill.Dto;
using Recruiter.Application.Summary.Dto;
using Recruiter.Application.VolunteerExtracurricular.Dto;

namespace Recruiter.Application.UserProfile.Dto;
public class UserProfileDetailsDto
{
    public UserProfileDto UserProfile { get; set; } = null!;
    public ICollection<EducationDto>? Educations { get; set; }
    public ICollection<SkillDto>? Skills { get; set; }
    public ICollection<SummaryDto>? Summaries { get; set; }
    public ICollection<AwardAchievementDto>? AwardAchievements { get; set; }
    public ICollection<CertificationLicenseDto>? CertificationLicenses { get; set; }
    public ICollection<ExperienceDto>? Experiences { get; set; }
    public ICollection<ProjectResearchDto>? ProjectResearch { get; set; }
    public ICollection<KeyStrengthDto>? KeyStrengths { get; set; }
    public ICollection<VolunteerExtracurricularDto>? VolunteerExtracurriculars { get; set; }
}
