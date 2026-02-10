using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Dashboard.Specifications;

public sealed class ActiveCandidatesSpec : Specification<Domain.Models.Candidate>
{
    public ActiveCandidatesSpec()
    {
        Query.Where(c => !c.IsDeleted);
    }
}

