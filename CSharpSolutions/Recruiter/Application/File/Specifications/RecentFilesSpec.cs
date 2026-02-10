using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class RecentFilesSpec : Specification<Domain.Models.File>
{
    public RecentFilesSpec(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        Query.Where(f => f.CreatedAt >= cutoffDate)
             .OrderByDescending(f => f.CreatedAt);
    }
}
