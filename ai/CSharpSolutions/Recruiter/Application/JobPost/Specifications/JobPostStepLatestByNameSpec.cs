using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepLatestByNameSpec : Specification<Domain.Models.JobPostStep>, ISingleResultSpecification<Domain.Models.JobPostStep>
{
    public JobPostStepLatestByNameSpec(string name)
    {
        Query.Where(jps => jps.Name == name && !jps.IsDeleted)
             .OrderByDescending(jps => jps.Version)
             .Take(1);
    }
}
