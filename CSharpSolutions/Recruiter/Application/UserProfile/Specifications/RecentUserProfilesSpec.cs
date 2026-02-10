using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class RecentUserProfilesSpec : Specification<Domain.Models.UserProfile>
{
    public RecentUserProfilesSpec(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        Query.Where(up => up.CreatedAt >= cutoffDate)
             .OrderByDescending(up => up.CreatedAt);
    }
}
