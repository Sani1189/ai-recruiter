using Ardalis.Specification;

namespace Recruiter.Application.ProjectResearch.Specifications;

/// <summary>
/// Filters project/research records by user profile, excluding deleted items.
/// </summary>
public sealed class ProjectResearchByUserProfileSpec : Specification<Domain.Models.ProjectResearch>
{
    public ProjectResearchByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(p => p.UserProfileId == userProfileId && !p.IsDeleted);
    }
}

