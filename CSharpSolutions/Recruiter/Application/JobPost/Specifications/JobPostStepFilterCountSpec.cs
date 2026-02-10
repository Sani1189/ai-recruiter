using Ardalis.Specification;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepFilterCountSpec : Specification<Domain.Models.JobPostStep>
{
    public JobPostStepFilterCountSpec(JobPostStepQueryDto query)
    {
        // Exclude soft-deleted job post steps
        Query.Where(jps => !jps.IsDeleted);

        // Apply only filters (no sorting or pagination for count)
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
    }
}
