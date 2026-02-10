using Ardalis.Specification;

namespace Recruiter.Application.Summary.Specifications;

/// <summary>
/// Filters summaries by user profile.
/// </summary>
public sealed class SummaryByUserProfileSpec : Specification<Domain.Models.Summary>
{
    public SummaryByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(s => s.UserProfileId == userProfileId && s.IsDeleted == false);
    }
}

