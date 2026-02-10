using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Dashboard.Specifications;

public sealed class ActiveScoresSpec : Specification<Domain.Models.Score>
{
    public ActiveScoresSpec()
    {
        Query.Where(s => !s.IsDeleted);
    }
}

