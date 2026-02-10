using Ardalis.Specification;

namespace Recruiter.Application.CvEvaluation.Specifications;

/// <summary>
/// Returns the most recent CV evaluation for a user profile (no IsDeleted filter).
/// </summary>
public sealed class CvEvaluationLatestAnyByUserProfileSpec : Specification<Domain.Models.CvEvaluation>, ISingleResultSpecification<Domain.Models.CvEvaluation>
{
    public CvEvaluationLatestAnyByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(e => e.UserProfileId == userProfileId)
             .OrderByDescending(e => e.CreatedAt)
             .Take(1);
    }
}

