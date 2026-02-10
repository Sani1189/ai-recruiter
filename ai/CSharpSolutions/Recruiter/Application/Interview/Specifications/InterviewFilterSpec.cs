using Ardalis.Specification;
using Recruiter.Application.Interview.Dto;

namespace Recruiter.Application.Interview.Specifications;

public sealed class InterviewFilterSpec : Specification<Domain.Models.Interview>
{
    public InterviewFilterSpec(InterviewListQueryDto query)
    {
        ApplyFilters(query);
        ApplySorting(query);
        ApplyPaging(query);
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

    private void ApplySorting(InterviewListQueryDto query)
    {
        var sortField = query.SortBy?.ToLower() ?? "createdat";
        
        if (query.SortDescending)
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderByDescending(i => i.Id);
                    break;
                case "jobapplicationstepid":
                    Query.OrderByDescending(i => i.JobApplicationStepId);
                    break;
                case "interviewconfigurationname":
                    Query.OrderByDescending(i => i.InterviewConfigurationName);
                    break;
                case "duration":
                    Query.OrderByDescending(i => i.Duration);
                    break;
                case "completedat":
                    Query.OrderByDescending(i => i.CompletedAt);
                    break;
                default:
                    Query.OrderByDescending(i => i.CreatedAt);
                    break;
            }
        }
        else
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderBy(i => i.Id);
                    break;
                case "jobapplicationstepid":
                    Query.OrderBy(i => i.JobApplicationStepId);
                    break;
                case "interviewconfigurationname":
                    Query.OrderBy(i => i.InterviewConfigurationName);
                    break;
                case "duration":
                    Query.OrderBy(i => i.Duration);
                    break;
                case "completedat":
                    Query.OrderBy(i => i.CompletedAt);
                    break;
                default:
                    Query.OrderBy(i => i.CreatedAt);
                    break;
            }
        }
    }

    private void ApplyPaging(InterviewListQueryDto query)
    {
        if (query.PageNumber > 0 && query.PageSize > 0)
        {
            Query.Skip((query.PageNumber - 1) * query.PageSize)
                 .Take(query.PageSize);
        }
    }
}
