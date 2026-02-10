using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostByNameAndVersionSpec : Specification<Domain.Models.JobPost>, ISingleResultSpecification<Domain.Models.JobPost>
{
    public JobPostByNameAndVersionSpec(string name, int version)
    {
        Query.Where(jp => jp.Name == name && jp.Version == version && !jp.IsDeleted);
        // Don't include StepAssignments - orchestrator will fetch them separately via GetByJobPostAsync
        // This avoids EF navigation property issues when StepVersion is null
    }
}
