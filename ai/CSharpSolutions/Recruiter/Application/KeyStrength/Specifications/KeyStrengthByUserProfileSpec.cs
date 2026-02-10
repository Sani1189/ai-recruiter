using Ardalis.Specification;

namespace Recruiter.Application.KeyStrength.Specifications;

/// <summary>
/// Filters key strengths by user profile.
/// </summary>
public sealed class KeyStrengthByUserProfileSpec : Specification<Domain.Models.KeyStrength>
{
    public KeyStrengthByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(k => k.UserProfileId == userProfileId && k.IsDeleted == false);
    }
}

