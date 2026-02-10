using Ardalis.Specification;
using Recruiter.Application.Candidate.Dto;

namespace Recruiter.Application.Candidate.Specifications;

public sealed class CandidateFilterCountSpec : Specification<Domain.Models.Candidate>
{
    public CandidateFilterCountSpec(CandidateListQueryDto query)
    {
        ApplyFilters(query);
    }

    private void ApplyFilters(CandidateListQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            // Search term can't search in Guid fields, so we'll skip this for now
            // or implement a different search strategy
        }

        if (query.UserId.HasValue)
        {
            Query.Where(c => c.UserId == query.UserId.Value);
        }

        if (query.CvFileId.HasValue)
        {
            Query.Where(c => c.CvFileId == query.CvFileId.Value);
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(c => c.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(c => c.CreatedAt <= query.CreatedBefore.Value);
        }

        if (query.IsRecent.HasValue && query.IsRecent.Value)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            Query.Where(c => c.CreatedAt >= cutoffDate);
        }
    }
}
