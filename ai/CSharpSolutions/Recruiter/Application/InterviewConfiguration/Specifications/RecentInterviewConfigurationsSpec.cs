using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class RecentInterviewConfigurationsSpec : Specification<Domain.Models.InterviewConfiguration>
{
    public RecentInterviewConfigurationsSpec(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        Query.Where(ic => ic.CreatedAt >= cutoffDate)
             .OrderByDescending(ic => ic.CreatedAt);
    }
}
