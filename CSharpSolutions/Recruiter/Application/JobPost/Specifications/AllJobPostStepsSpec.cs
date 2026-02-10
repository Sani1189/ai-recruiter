using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class AllJobPostStepsSpec : Specification<Domain.Models.JobPostStep>
{
    public AllJobPostStepsSpec()
    {
        Query.Where(jps => !jps.IsDeleted);
        Query.OrderByDescending(jps => jps.CreatedAt);
    }
}
