using Ardalis.Specification;
using Recruiter.Application.Interview.Dto;

namespace Recruiter.Application.Interview.Specifications;

public sealed class InterviewFilterCountSpec : Specification<Domain.Models.Interview>
{
    public InterviewFilterCountSpec(InterviewListQueryDto query)
    {
        ApplyFilters(query);
    }

    private void ApplyFilters(InterviewListQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(i => i.InterviewConfigurationName.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (query.JobApplicationStepId.HasValue)
        {
            Query.Where(i => i.JobApplicationStepId == query.JobApplicationStepId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.InterviewConfigurationName))
        {
            Query.Where(i => i.InterviewConfigurationName == query.InterviewConfigurationName);
        }

        if (query.InterviewConfigurationVersion.HasValue)
        {
            Query.Where(i => i.InterviewConfigurationVersion == query.InterviewConfigurationVersion.Value);
        }

        if (query.CompletedAfter.HasValue)
        {
            Query.Where(i => i.CompletedAt.HasValue && i.CompletedAt.Value >= query.CompletedAfter.Value);
        }

        if (query.CompletedBefore.HasValue)
        {
            Query.Where(i => i.CompletedAt.HasValue && i.CompletedAt.Value <= query.CompletedBefore.Value);
        }

        if (query.IsCompleted.HasValue)
        {
            Query.Where(i => i.CompletedAt.HasValue == query.IsCompleted.Value);
        }

        if (query.MinDuration.HasValue)
        {
            Query.Where(i => i.Duration.HasValue && i.Duration.Value >= query.MinDuration.Value);
        }

        if (query.MaxDuration.HasValue)
        {
            Query.Where(i => i.Duration.HasValue && i.Duration.Value <= query.MaxDuration.Value);
        }

        if (query.IsRecent.HasValue && query.IsRecent.Value)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            Query.Where(i => i.CreatedAt >= cutoffDate);
        }
    }
}
