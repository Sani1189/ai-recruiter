using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewConfigurationsByNameSpec : Specification<Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationsByNameSpec(string name)
    {
        Query.Where(ic => ic.Name == name)
             .OrderByDescending(ic => ic.Version);
    }
}
