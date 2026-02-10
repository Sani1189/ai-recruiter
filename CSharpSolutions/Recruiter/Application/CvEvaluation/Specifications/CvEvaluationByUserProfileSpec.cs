using Ardalis.Specification;

namespace Recruiter.Application.CvEvaluation.Specifications;

/// <summary>
/// Filters CV evaluations by user profile, excluding deleted items.
/// </summary>
public sealed class CvEvaluationByUserProfileSpec : Specification<Domain.Models.CvEvaluation>
{
    public CvEvaluationByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(e => e.UserProfileId == userProfileId && !e.IsDeleted);
    }
}

