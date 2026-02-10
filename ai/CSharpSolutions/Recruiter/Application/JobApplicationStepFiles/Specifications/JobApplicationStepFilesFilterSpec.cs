using Ardalis.Specification;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Specifications;

public sealed class JobApplicationStepFilesFilterSpec : Specification<Domain.Models.JobApplicationStepFiles>
{
    public JobApplicationStepFilesFilterSpec(JobApplicationStepFilesListQueryDto query)
    {
        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            // Search term can't search in Guid fields, so we'll skip this for now
            // or implement a different search strategy
        }

        if (query.FileId.HasValue)
        {
            Query.Where(jasf => jasf.FileId == query.FileId.Value);
        }

        if (query.JobApplicationStepId.HasValue)
        {
            Query.Where(jasf => jasf.JobApplicationStepId == query.JobApplicationStepId.Value);
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(jasf => jasf.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(jasf => jasf.CreatedAt <= query.CreatedBefore.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            switch (query.SortBy.ToLowerInvariant())
            {
                case "fileid":
                    Query.OrderBy(jasf => jasf.FileId, query.SortDescending);
                    break;
                case "jobapplicationstepid":
                    Query.OrderBy(jasf => jasf.JobApplicationStepId, query.SortDescending);
                    break;
                case "createdat":
                default:
                    Query.OrderBy(jasf => jasf.CreatedAt, query.SortDescending);
                    break;
            }
        }
        else
        {
            Query.OrderByDescending(jasf => jasf.CreatedAt);
        }

        // Apply pagination
        Query.Skip((query.PageNumber - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
