using Ardalis.Specification;

namespace Recruiter.Application.Scoring.Specifications;

/// <summary>
/// Fetches all scoring rows for multiple CV evaluation IDs.
/// </summary>
public sealed class ScoringsByCvEvaluationIdsSpec : Specification<Domain.Models.Scoring>
{
    public ScoringsByCvEvaluationIdsSpec(IEnumerable<Guid> cvEvaluationIds)
    {
        var ids = cvEvaluationIds.ToList();
        if (!ids.Any())
        {
            Query.Where(s => false); // Return nothing if no IDs provided
            return;
        }

        Query.Where(s => ids.Contains(s.CvEvaluationId) && s.IsDeleted == false);
    }
}
