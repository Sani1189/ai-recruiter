using Ardalis.Specification;

namespace Recruiter.Application.Candidate.Specifications;

public sealed class CandidateByIdSpec : Specification<Domain.Models.Candidate>, ISingleResultSpecification<Domain.Models.Candidate>
{
    public CandidateByIdSpec(Guid id)
    {
        Query.Include(c => c.UserProfile!);
        Query.Where(c => c.Id == id);
    }
}
