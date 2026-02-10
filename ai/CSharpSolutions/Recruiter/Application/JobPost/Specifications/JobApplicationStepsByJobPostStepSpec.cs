using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class JobApplicationStepsByJobPostStepSpec : Specification<JobApplicationStep>
{
    public JobApplicationStepsByJobPostStepSpec(string stepName, int stepVersion)
    {
        Query.Where(s => s.JobPostStepName == stepName && s.JobPostStepVersion == stepVersion);
    }
}



