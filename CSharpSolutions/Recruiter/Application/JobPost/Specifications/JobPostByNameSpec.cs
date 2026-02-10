using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostByNameSpec : Specification<Domain.Models.JobPost>
{
    public JobPostByNameSpec(string name)
    {
        Query.Where(jp => jp.Name == name && !jp.IsDeleted)
             .OrderByDescending(jp => jp.Version);
    }
}
