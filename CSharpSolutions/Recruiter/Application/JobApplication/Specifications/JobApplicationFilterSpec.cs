using Ardalis.Specification;
using Recruiter.Application.JobApplication.Dto;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationFilterSpec : Specification<Domain.Models.JobApplication>
{
    public JobApplicationFilterSpec(JobApplicationListQueryDto query)
    {
        ApplyFilters(query);
        ApplySorting(query);
        ApplyPaging(query);
    }

    private void ApplyFilters(JobApplicationListQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(ja => ja.JobPostName.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.JobPostName))
        {
            Query.Where(ja => ja.JobPostName == query.JobPostName);
        }

        if (!string.IsNullOrWhiteSpace(query.JobPostVersion))
        {
            if (int.TryParse(query.JobPostVersion, out var version))
            {
                Query.Where(ja => ja.JobPostVersion == version);
            }
        }

        if (query.CandidateId.HasValue)
        {
            Query.Where(ja => ja.CandidateId == query.CandidateId.Value);
        }

        if (query.AppliedAfter.HasValue)
        {
            Query.Where(ja => ja.CreatedAt >= query.AppliedAfter.Value);
        }

        if (query.AppliedBefore.HasValue)
        {
            Query.Where(ja => ja.CreatedAt <= query.AppliedBefore.Value);
        }

        if (query.CompletedAfter.HasValue)
        {
            Query.Where(ja => ja.CompletedAt.HasValue && ja.CompletedAt.Value >= query.CompletedAfter.Value);
        }

        if (query.CompletedBefore.HasValue)
        {
            Query.Where(ja => ja.CompletedAt.HasValue && ja.CompletedAt.Value <= query.CompletedBefore.Value);
        }

        if (query.IsCompleted.HasValue)
        {
            Query.Where(ja => ja.CompletedAt.HasValue == query.IsCompleted.Value);
        }

        if (query.IsRecent.HasValue && query.IsRecent.Value)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            Query.Where(ja => ja.CreatedAt >= cutoffDate);
        }
    }

    private void ApplySorting(JobApplicationListQueryDto query)
    {
        var sortField = query.SortBy?.ToLower() ?? "createdat";
        
        if (query.SortDescending)
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderByDescending(ja => ja.Id);
                    break;
                case "jobpostname":
                    Query.OrderByDescending(ja => ja.JobPostName);
                    break;
                case "candidateid":
                    Query.OrderByDescending(ja => ja.CandidateId);
                    break;
                case "completedat":
                    Query.OrderByDescending(ja => ja.CompletedAt);
                    break;
                default:
                    Query.OrderByDescending(ja => ja.CreatedAt);
                    break;
            }
        }
        else
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderBy(ja => ja.Id);
                    break;
                case "jobpostname":
                    Query.OrderBy(ja => ja.JobPostName);
                    break;
                case "candidateid":
                    Query.OrderBy(ja => ja.CandidateId);
                    break;
                case "completedat":
                    Query.OrderBy(ja => ja.CompletedAt);
                    break;
                default:
                    Query.OrderBy(ja => ja.CreatedAt);
                    break;
            }
        }
    }

    private void ApplyPaging(JobApplicationListQueryDto query)
    {
        if (query.PageNumber > 0 && query.PageSize > 0)
        {
            Query.Skip((query.PageNumber - 1) * query.PageSize)
                 .Take(query.PageSize);
        }
    }
}
