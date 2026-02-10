using Ardalis.Specification;

namespace Recruiter.Application.Experience.Specifications;

/// <summary>
/// Filters experience records by user profile, excluding deleted items.
/// </summary>
public sealed class ExperienceByUserProfileSpec : Specification<Domain.Models.Experience>
{
    public ExperienceByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(e => e.UserProfileId == userProfileId && !e.IsDeleted);
    }
}

