using Ardalis.Specification;
using Recruiter.Domain.Models;
namespace Recruiter.Application.JobPost.Specifications;

// Fetches all non-deleted job steps ordered by name and version (descending)
// Note: Returns all versions - the query handler groups by name to get only latest versions
// This approach is necessary because Ardalis.Specification doesn't support GROUP BY operations
public sealed class JobStepDropdownSpec : Specification<JobPostStep>
{
    public JobStepDropdownSpec()
    {
        Query.Where(jps => !jps.IsDeleted)
             .OrderBy(jps => jps.Name)
             .ThenByDescending(jps => jps.Version);
    }
}