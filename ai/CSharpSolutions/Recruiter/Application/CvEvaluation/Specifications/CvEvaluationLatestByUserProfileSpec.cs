using Ardalis.Specification;

namespace Recruiter.Application.CvEvaluation.Specifications;

/// <summary>
/// Returns the most recent CV evaluation for a user profile.
/// </summary>
public sealed class CvEvaluationLatestByUserProfileSpec : Specification<Domain.Models.CvEvaluation>, ISingleResultSpecification<Domain.Models.CvEvaluation>
{
    public CvEvaluationLatestByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(e => e.UserProfileId == userProfileId && e.IsDeleted == false)
             .OrderByDescending(e => e.CreatedAt)
             .Take(1);
    }
}

