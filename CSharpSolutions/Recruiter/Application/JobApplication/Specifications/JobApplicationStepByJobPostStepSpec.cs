using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationStepByJobPostStepSpec : Specification<Domain.Models.JobApplicationStep>
{
    public JobApplicationStepByJobPostStepSpec(string jobPostStepName, int jobPostStepVersion)
    {
        Query.Where(jas => jas.JobPostStepName == jobPostStepName && jas.JobPostStepVersion == jobPostStepVersion)
             .OrderByDescending(jas => jas.CreatedAt);
    }
}
