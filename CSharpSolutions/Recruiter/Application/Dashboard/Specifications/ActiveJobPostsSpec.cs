using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Dashboard.Specifications;

public sealed class ActiveJobPostsSpec : Specification<Domain.Models.JobPost>
{
    public ActiveJobPostsSpec()
    {
        Query.Where(jp => !jp.IsDeleted);
    }
}

