using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

/// <summary>
/// Specification to get UserProfile with all related entities, filtering out deleted records
/// </summary>
public sealed class UserProfileDetailsByIdSpec : Specification<Domain.Models.UserProfile>, ISingleResultSpecification<Domain.Models.UserProfile>
{
    public UserProfileDetailsByIdSpec(Guid id)
    {
        Query.Where(up => up.Id == id && !up.IsDeleted)
            .Include(up => up.Skills.Where(s => !s.IsDeleted))
            .Include(up => up.Experiences.Where(e => !e.IsDeleted))
            .Include(up => up.Educations.Where(ed => !ed.IsDeleted))
            .Include(up => up.Summaries.Where(s => !s.IsDeleted))
            .Include(up => up.AwardAchievements.Where(aa => !aa.IsDeleted))
            .Include(up => up.CertificationLicenses.Where(cl => !cl.IsDeleted))
            .Include(up => up.KeyStrengths.Where(ks => !ks.IsDeleted))
            .Include(up => up.VolunteerExtracurriculars.Where(ve => !ve.IsDeleted))
            .Include(up => up.ProjectResearches.Where(pr => !pr.IsDeleted));
    }
}

