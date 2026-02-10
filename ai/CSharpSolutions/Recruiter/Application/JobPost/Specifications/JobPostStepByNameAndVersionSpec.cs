using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepByNameAndVersionSpec : Specification<Domain.Models.JobPostStep>, ISingleResultSpecification<Domain.Models.JobPostStep>
{
    public JobPostStepByNameAndVersionSpec(string name, int version)
    {
        Query.Where(jps => jps.Name == name && jps.Version == version && !jps.IsDeleted)
        .Include(jps => jps.Prompt);
    }
}
