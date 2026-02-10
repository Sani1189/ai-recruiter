using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class AllJobPostsSpec : Specification<Domain.Models.JobPost>
{
    public AllJobPostsSpec()
    {
        Query.Where(jp => !jp.IsDeleted);
        Query.OrderByDescending(jp => jp.CreatedAt);
    }
}
