using Ardalis.Specification;

namespace Recruiter.Application.AwardAchievement.Specifications;

/// <summary>
/// Filters award/achievement records by user profile, excluding deleted items.
/// </summary>
public sealed class AwardAchievementByUserProfileSpec : Specification<Domain.Models.AwardAchievement>
{
    public AwardAchievementByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(a => a.UserProfileId == userProfileId && !a.IsDeleted);
    }
}

