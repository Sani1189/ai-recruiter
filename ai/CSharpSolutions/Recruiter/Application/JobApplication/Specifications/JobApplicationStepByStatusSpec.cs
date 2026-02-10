using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationStepByStatusSpec : Specification<Domain.Models.JobApplicationStep>
{
    public JobApplicationStepByStatusSpec(JobApplicationStepStatusEnum status)
    {
        Query.Where(jas => jas.Status == status)
             .OrderByDescending(jas => jas.CreatedAt);
    }
}
