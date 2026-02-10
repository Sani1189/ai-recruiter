using Ardalis.Specification;

namespace Recruiter.Application.Education.Specifications;

/// <summary>
/// Filters education records by user profile, excluding deleted items.
/// </summary>
public sealed class EducationByUserProfileSpec : Specification<Domain.Models.Education>
{
    public EducationByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(e => e.UserProfileId == userProfileId && !e.IsDeleted);
    }
}

