using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Dashboard.Specifications;

public sealed class ActiveInterviewsSpec : Specification<Domain.Models.Interview>
{
    public ActiveInterviewsSpec()
    {
        Query.Where(i => !i.IsDeleted);
    }
}

