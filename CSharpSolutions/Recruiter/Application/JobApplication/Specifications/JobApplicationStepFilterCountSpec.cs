using Ardalis.Specification;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Domain.Models;
using Recruiter.Domain.Common;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationStepFilterCountSpec : Specification<Domain.Models.JobApplicationStep>
{
    public JobApplicationStepFilterCountSpec(JobApplicationStepListQueryDto query)
    {
        // Apply only filters (no sorting or pagination for count)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            Query.Where(jas => jas.JobPostStepName.Contains(query.SearchTerm) ||
                            jas.Status.ToString().ToLower().Contains(term));
        }

        if (query.JobApplicationId.HasValue)
        {
            Query.Where(jas => jas.JobApplicationId == query.JobApplicationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.JobPostStepName))
        {
            Query.Where(jas => jas.JobPostStepName == query.JobPostStepName);
        }

        if (query.JobPostStepVersion.HasValue)
        {
            Query.Where(jas => jas.JobPostStepVersion == query.JobPostStepVersion.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<JobApplicationStepStatusEnum>(query.Status, true, out var parsed))
            {
                Query.Where(jas => jas.Status == parsed);
            }
        }

        if (query.StepNumber.HasValue)
        {
            Query.Where(jas => jas.StepNumber == query.StepNumber.Value);
        }

        if (query.StartedAfter.HasValue)
        {
            Query.Where(jas => jas.StartedAt >= query.StartedAfter.Value);
        }

        if (query.StartedBefore.HasValue)
        {
            Query.Where(jas => jas.StartedAt <= query.StartedBefore.Value);
        }

        if (query.CompletedAfter.HasValue)
        {
            Query.Where(jas => jas.CompletedAt >= query.CompletedAfter.Value);
        }

        if (query.CompletedBefore.HasValue)
        {
            Query.Where(jas => jas.CompletedAt <= query.CompletedBefore.Value);
        }
    }
}
