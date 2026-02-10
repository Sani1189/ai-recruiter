using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostLatestByNameSpec : Specification<Domain.Models.JobPost>, ISingleResultSpecification<Domain.Models.JobPost>
{
    public JobPostLatestByNameSpec(string name)
    {
        Query.Where(jp => jp.Name == name && !jp.IsDeleted)
             .OrderByDescending(jp => jp.Version)
             .Take(1);
    }
}
