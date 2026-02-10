using Ardalis.Specification;
using Recruiter.Application.JobApplication.Dto;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationFilterCountSpec : Specification<Domain.Models.JobApplication>
{
    public JobApplicationFilterCountSpec(JobApplicationListQueryDto query)
    {
        ApplyFilters(query);
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
}
