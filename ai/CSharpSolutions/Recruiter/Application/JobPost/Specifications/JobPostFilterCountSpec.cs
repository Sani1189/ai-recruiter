using Ardalis.Specification;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostFilterCountSpec : Specification<Domain.Models.JobPost>
{
    public JobPostFilterCountSpec(JobPostListQueryDto query)
    {
        // Exclude soft-deleted job posts
        Query.Where(jp => !jp.IsDeleted);

        // Apply only filters (no sorting or pagination for count)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(jp => jp.Name.Contains(query.SearchTerm) ||
                            jp.JobTitle.Contains(query.SearchTerm) ||
                            jp.JobDescription.Contains(query.SearchTerm) ||
                            jp.JobType.Contains(query.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(query.ExperienceLevel))
        {
            Query.Where(jp => jp.ExperienceLevel == query.ExperienceLevel);
        }

        if (!string.IsNullOrWhiteSpace(query.JobTitle))
        {
            Query.Where(jp => jp.JobTitle.Contains(query.JobTitle));
        }

        if (!string.IsNullOrWhiteSpace(query.JobType))
        {
            Query.Where(jp => jp.JobType == query.JobType);
        }

        if (query.PoliceReportRequired.HasValue)
        {
            Query.Where(jp => jp.PoliceReportRequired == query.PoliceReportRequired.Value);
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(jp => jp.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(jp => jp.CreatedAt <= query.CreatedBefore.Value);
        }
    }
}
