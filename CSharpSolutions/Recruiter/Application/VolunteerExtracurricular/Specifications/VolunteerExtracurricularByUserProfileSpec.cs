using Ardalis.Specification;

namespace Recruiter.Application.VolunteerExtracurricular.Specifications;

/// <summary>
/// Filters volunteer/extracurricular records by user profile, excluding deleted items.
/// </summary>
public sealed class VolunteerExtracurricularByUserProfileSpec : Specification<Domain.Models.VolunteerExtracurricular>
{
    public VolunteerExtracurricularByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(v => v.UserProfileId == userProfileId && !v.IsDeleted);
    }
}

