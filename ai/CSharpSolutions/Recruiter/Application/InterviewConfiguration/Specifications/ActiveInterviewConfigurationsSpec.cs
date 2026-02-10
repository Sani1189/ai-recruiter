using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class ActiveInterviewConfigurationsSpec : Specification<Domain.Models.InterviewConfiguration>
{
    public ActiveInterviewConfigurationsSpec()
    {
        Query.Where(ic => ic.Active == true)
             .OrderByDescending(ic => ic.CreatedAt);
    }
}
