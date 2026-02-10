using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationStepByIdSpec : Specification<Domain.Models.JobApplicationStep>, ISingleResultSpecification<Domain.Models.JobApplicationStep>
{
    public JobApplicationStepByIdSpec(Guid id)
    {
        Query.Where(jas => jas.Id == id)
             .Include(jas => jas.Interview);
    }
}
