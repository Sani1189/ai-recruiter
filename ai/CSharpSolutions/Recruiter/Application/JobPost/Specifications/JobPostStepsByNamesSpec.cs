using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

// Specification to fetch multiple JobPostSteps by a list of names (for batch loading)
public sealed class JobPostStepsByNamesSpec : Specification<JobPostStep>
{
    public JobPostStepsByNamesSpec(IEnumerable<string> names)
    {
        Query.Where(jps => names.Contains(jps.Name));
    }
}

