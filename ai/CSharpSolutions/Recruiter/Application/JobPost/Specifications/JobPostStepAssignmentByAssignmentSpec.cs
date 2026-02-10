using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepAssignmentByAssignmentSpec : Specification<Domain.Models.JobPostStepAssignment>, ISingleResultSpecification<Domain.Models.JobPostStepAssignment>
{
    public JobPostStepAssignmentByAssignmentSpec(string jobPostName, int jobPostVersion, string stepName, int? stepVersion)
    {
        Query.Where(jpsa => jpsa.JobPostName == jobPostName && 
                           jpsa.JobPostVersion == jobPostVersion &&
                           jpsa.StepName == stepName && 
                           jpsa.StepVersion == stepVersion);
             // Don't eager load JobPostStep - StepVersion might be null (use latest dynamically)
    }
}
