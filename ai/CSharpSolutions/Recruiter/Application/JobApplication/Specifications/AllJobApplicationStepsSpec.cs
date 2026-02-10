using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class AllJobApplicationStepsSpec : Specification<Domain.Models.JobApplicationStep>
{
    public AllJobApplicationStepsSpec()
    {
        Query.OrderByDescending(jas => jas.CreatedAt);
    }
}
