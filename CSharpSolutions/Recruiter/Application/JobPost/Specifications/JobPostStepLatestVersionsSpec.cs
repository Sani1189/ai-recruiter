using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepLatestVersionsSpec : Specification<Domain.Models.JobPostStep>
{
    public JobPostStepLatestVersionsSpec()
    {
        Query.OrderBy(jps => jps.Name)
             .ThenByDescending(jps => jps.Version);
    }
}
