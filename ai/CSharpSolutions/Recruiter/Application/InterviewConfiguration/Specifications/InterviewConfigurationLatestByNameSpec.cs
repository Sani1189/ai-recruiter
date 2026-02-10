using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewConfigurationLatestByNameSpec : Specification<Domain.Models.InterviewConfiguration>, ISingleResultSpecification<Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationLatestByNameSpec(string name)
    {
        Query.Where(ic => ic.Name == name)
             .OrderByDescending(ic => ic.Version)
             .Take(1);
    }
}
