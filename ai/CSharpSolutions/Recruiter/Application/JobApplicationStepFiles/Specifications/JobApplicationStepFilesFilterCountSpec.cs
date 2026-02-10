using Ardalis.Specification;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Specifications;

public sealed class JobApplicationStepFilesFilterCountSpec : Specification<Domain.Models.JobApplicationStepFiles>
{
    public JobApplicationStepFilesFilterCountSpec(JobApplicationStepFilesListQueryDto query)
    {
        // Apply only filters (no sorting or pagination for count)
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
    }
}
