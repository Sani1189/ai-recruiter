using Ardalis.Specification;

namespace Recruiter.Application.Interview.Specifications;

public sealed class InterviewByIdSpec : Specification<Domain.Models.Interview>, ISingleResultSpecification<Domain.Models.Interview>
{
    public InterviewByIdSpec(Guid id)
    {
        Query.Where(i => i.Id == id);
    }
}
