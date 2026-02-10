using Ardalis.Specification;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepFilterSpec : Specification<Domain.Models.JobPostStep>
{
    public JobPostStepFilterSpec(JobPostStepQueryDto query)
    {
        // Exclude soft-deleted job post steps
        Query.Where(jps => !jps.IsDeleted);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(jps => jps.Name.Contains(query.SearchTerm) ||
                            jps.StepType.Contains(query.SearchTerm) ||
                            (jps.InterviewConfigurationName != null && jps.InterviewConfigurationName.Contains(query.SearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(query.StepType))
        {
            Query.Where(jps => jps.StepType == query.StepType);
        }

        if (query.CreatedAt.HasValue)
        {
            Query.Where(jps => jps.CreatedAt >= query.CreatedAt.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            switch (query.SortBy.ToLowerInvariant())
            {
                case "name":
                    if (query.SortDescending)
                        Query.OrderByDescending(jps => jps.Name);
                    else
                        Query.OrderBy(jps => jps.Name);
                    break;
                case "version":
                    if (query.SortDescending)
                        Query.OrderByDescending(jps => jps.Version);
                    else
                        Query.OrderBy(jps => jps.Version);
                    break;
                case "steptype":
                    if (query.SortDescending)
                        Query.OrderByDescending(jps => jps.StepType);
                    else
                        Query.OrderBy(jps => jps.StepType);
                    break;
                case "createdat":
                case "updatedat":
                    if (query.SortDescending)
                        Query.OrderByDescending(jps => jps.CreatedAt);
                    else
                        Query.OrderBy(jps => jps.CreatedAt);
                    break;
                default:
                    Query.OrderByDescending(jps => jps.CreatedAt);
                    break;
            }
        }
        else
        {
            Query.OrderByDescending(jps => jps.CreatedAt);
        }

        // Apply pagination
        Query.Skip((query.Page - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
