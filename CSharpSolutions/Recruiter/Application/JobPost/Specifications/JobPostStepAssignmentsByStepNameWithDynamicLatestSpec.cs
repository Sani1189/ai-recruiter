using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepAssignmentsByStepNameWithDynamicLatestSpec : Specification<JobPostStepAssignment>
{
    public JobPostStepAssignmentsByStepNameWithDynamicLatestSpec(string stepName)
    {
        // StepVersion == null means "use latest version dynamically"
        Query.Where(a => a.StepName == stepName && a.StepVersion == null);
    }
}



