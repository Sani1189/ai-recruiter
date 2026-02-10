using Ardalis.Specification;

namespace Recruiter.Application.Skill.Specifications;

/// <summary>
/// Filters skills by user profile, excluding deleted items.
/// </summary>
public sealed class SkillByUserProfileSpec : Specification<Domain.Models.Skill>
{
    public SkillByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(s => s.UserProfileId == userProfileId && !s.IsDeleted);
    }
}

