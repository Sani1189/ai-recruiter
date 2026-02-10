using Ardalis.Specification;
using Recruiter.Application.Candidate.Dto;

namespace Recruiter.Application.Candidate.Specifications;

public sealed class CandidateFilterSpec : Specification<Domain.Models.Candidate>
{
    public CandidateFilterSpec(CandidateListQueryDto query)
    {
        Query.Include(c => c.UserProfile!);
        
        ApplyFilters(query);
        ApplySorting(query);
        ApplyPaging(query);
    }

    private void ApplyFilters(CandidateListQueryDto query)
    {
       

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

    private void ApplySorting(CandidateListQueryDto query)
    {
        var sortField = query.SortBy?.ToLower() ?? "createdat";
        
        if (query.SortDescending)
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderByDescending(c => c.Id);
                    break;
                case "userid":
                    Query.OrderByDescending(c => c.UserId);
                    break;
                case "cvfileid":
                    Query.OrderByDescending(c => c.CvFileId);
                    break;
                case "updatedat":
                    Query.OrderByDescending(c => c.UpdatedAt);
                    break;
                default:
                    Query.OrderByDescending(c => c.CreatedAt);
                    break;
            }
        }
        else
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderBy(c => c.Id);
                    break;
                case "userid":
                    Query.OrderBy(c => c.UserId);
                    break;
                case "cvfileid":
                    Query.OrderBy(c => c.CvFileId);
                    break;
                case "updatedat":
                    Query.OrderBy(c => c.UpdatedAt);
                    break;
                default:
                    Query.OrderBy(c => c.CreatedAt);
                    break;
            }
        }
    }

    private void ApplyPaging(CandidateListQueryDto query)
    {
        if (query.PageNumber > 0 && query.PageSize > 0)
        {
            Query.Skip((query.PageNumber - 1) * query.PageSize)
                 .Take(query.PageSize);
        }
    }
}
