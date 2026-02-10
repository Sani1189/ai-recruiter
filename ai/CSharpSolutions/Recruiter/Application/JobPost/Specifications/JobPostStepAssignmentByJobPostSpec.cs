using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepAssignmentByJobPostSpec : Specification<Domain.Models.JobPostStepAssignment>
{
    public JobPostStepAssignmentByJobPostSpec(string jobPostName, int jobPostVersion)
    {
        Query.Where(jpsa => jpsa.JobPostName == jobPostName && jpsa.JobPostVersion == jobPostVersion)
             // Don't eager load JobPostStep here - service will manually fetch based on whether StepVersion is null
             .OrderBy(jpsa => jpsa.StepNumber);
    }
}
