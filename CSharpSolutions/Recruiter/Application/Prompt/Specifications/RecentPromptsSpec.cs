using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class RecentPromptsSpec : Specification<Domain.Models.Prompt>
{
    public RecentPromptsSpec(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        Query.Where(p => p.CreatedAt >= cutoffDate)
             .OrderByDescending(p => p.CreatedAt);
    }
}
