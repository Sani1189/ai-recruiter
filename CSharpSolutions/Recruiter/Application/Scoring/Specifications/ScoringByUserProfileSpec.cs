using Ardalis.Specification;

namespace Recruiter.Application.Scoring.Specifications;

/// <summary>
/// Fetch scorings by user profile via CvEvaluation relationship.
/// </summary>
public sealed class ScoringByUserProfileSpec : Specification<Domain.Models.Scoring>
{
    public ScoringByUserProfileSpec(Guid userProfileId)
    {
        Query.Include(s => s.CvEvaluation!)
             .Where(s => s.CvEvaluation != null && s.CvEvaluation.UserProfileId == userProfileId && s.IsDeleted == false);
    }
}

