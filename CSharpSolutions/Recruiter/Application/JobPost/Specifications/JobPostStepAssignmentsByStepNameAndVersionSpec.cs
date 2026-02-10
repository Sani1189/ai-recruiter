using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobPostStepAssignmentsByStepNameAndVersionSpec : Specification<JobPostStepAssignment>
{
    public JobPostStepAssignmentsByStepNameAndVersionSpec(string stepName, int stepVersion)
    {
        Query.Where(a => a.StepName == stepName && a.StepVersion == stepVersion);
    }
}



