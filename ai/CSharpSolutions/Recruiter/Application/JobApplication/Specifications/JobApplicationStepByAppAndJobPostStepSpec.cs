using Ardalis.Specification;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationStepByAppAndJobPostStepSpec : Specification<Domain.Models.JobApplicationStep>, ISingleResultSpecification<Domain.Models.JobApplicationStep>
{
    public JobApplicationStepByAppAndJobPostStepSpec(Guid jobApplicationId, string jobPostStepName, int jobPostStepVersion)
    {
        Query.Where(s =>
                s.JobApplicationId == jobApplicationId
                && s.JobPostStepName == jobPostStepName
                && s.JobPostStepVersion == jobPostStepVersion)
            .Include(s => s.JobPostStep)
            .OrderByDescending(s => s.CreatedAt);
    }
}
