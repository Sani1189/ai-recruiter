using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

public sealed class KanbanBoardColumnByRecruiterSpec : Specification<KanbanBoardColumn>
{
    public KanbanBoardColumnByRecruiterSpec(Guid recruiterId)
    {
        Query.Where(c => c.RecruiterId == recruiterId);
        Query.OrderBy(c => c.Sequence);
    }
}
