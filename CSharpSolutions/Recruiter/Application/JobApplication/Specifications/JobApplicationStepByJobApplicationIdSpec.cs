using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationStepByJobApplicationIdSpec : Specification<Domain.Models.JobApplicationStep>
{
    public JobApplicationStepByJobApplicationIdSpec(Guid jobApplicationId)
    {
        Query.Where(jas => jas.JobApplicationId == jobApplicationId)
             .OrderBy(jas => jas.StepNumber);
    }
}
