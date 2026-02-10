using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepByNameSpec : Specification<JobPostStep>
{
    public JobPostStepByNameSpec(string name, bool includeDeleted = false)
    {
        Query.Where(jps => jps.Name == name);
        
        if (!includeDeleted)
        {
            Query.Where(jps => !jps.IsDeleted);
        }
        
        Query.OrderByDescending(jps => jps.Version);
    }
}