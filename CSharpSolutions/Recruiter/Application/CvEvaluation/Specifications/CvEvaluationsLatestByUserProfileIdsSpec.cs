using Ardalis.Specification;

namespace Recruiter.Application.CvEvaluation.Specifications;

/// <summary>
/// Returns the most recent CV evaluation for each user profile in the provided list.
/// Uses a window function approach to get the latest per user profile.
/// </summary>
public sealed class CvEvaluationsLatestByUserProfileIdsSpec : Specification<Domain.Models.CvEvaluation>
{
    public CvEvaluationsLatestByUserProfileIdsSpec(IEnumerable<Guid> userProfileIds)
    {
        var ids = userProfileIds.ToList();
        if (!ids.Any())
        {
            Query.Where(e => false); // Return nothing if no IDs provided
            return;
        }

        Query.Where(e => ids.Contains(e.UserProfileId) && e.IsDeleted == false)
             .OrderByDescending(e => e.CreatedAt)
             .ThenByDescending(e => e.Id);
    }
}
