using Ardalis.Specification;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationByIdSpec : Specification<Domain.Models.JobApplication>, ISingleResultSpecification<Domain.Models.JobApplication>
{
    public JobApplicationByIdSpec(Guid id)
    {
        Query.Where(ja => ja.Id == id);
    }
}
