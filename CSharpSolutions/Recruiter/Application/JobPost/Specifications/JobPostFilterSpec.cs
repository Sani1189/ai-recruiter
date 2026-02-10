using Ardalis.Specification;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostFilterSpec : Specification<Domain.Models.JobPost>
{
    public JobPostFilterSpec(JobPostListQueryDto query)
    {
        // Exclude soft-deleted job posts
        Query.Where(jp => !jp.IsDeleted);
        Query.Include(jp => jp.CountryExposureSet!)
            .ThenInclude(s => s.Countries);

        // Apply filters
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

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            switch (query.SortBy.ToLowerInvariant())
            {
                case "name":
                    Query.OrderBy(jp => jp.Name, query.SortDescending);
                    break;
                case "version":
                    Query.OrderBy(jp => jp.Version, query.SortDescending);
                    break;
                case "jobtitle":
                    Query.OrderBy(jp => jp.JobTitle, query.SortDescending);
                    break;
                case "jobtype":
                    Query.OrderBy(jp => jp.JobType, query.SortDescending);
                    break;
                case "experiencelevel":
                    Query.OrderBy(jp => jp.ExperienceLevel, query.SortDescending);
                    break;
                case "createdat":
                default:
                    Query.OrderBy(jp => jp.CreatedAt, query.SortDescending);
                    break;
            }
        }
        else
        {
            Query.OrderByDescending(jp => jp.CreatedAt);
        }

        // Apply pagination
        Query.Skip((query.Page - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
