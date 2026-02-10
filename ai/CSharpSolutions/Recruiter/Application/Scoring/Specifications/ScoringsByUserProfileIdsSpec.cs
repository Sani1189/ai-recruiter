using Ardalis.Specification;

namespace Recruiter.Application.Scoring.Specifications;

/// <summary>
/// Fetches all scoring rows for multiple user profiles via CvEvaluation relationship.
/// </summary>
public sealed class ScoringsByUserProfileIdsSpec : Specification<Domain.Models.Scoring>
{
    public ScoringsByUserProfileIdsSpec(IEnumerable<Guid> userProfileIds)
    {
        var ids = userProfileIds.ToList();
        if (!ids.Any())
        {
            Query.Where(s => false); // Return nothing if no IDs provided
            return;
        }

        Query.Include(s => s.CvEvaluation!)
             .Where(s => s.CvEvaluation != null && ids.Contains(s.CvEvaluation.UserProfileId) && s.IsDeleted == false);
    }
}
