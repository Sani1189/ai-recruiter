using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewConfigurationByNameAndVersionSpec : Specification<Domain.Models.InterviewConfiguration>, ISingleResultSpecification<Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationByNameAndVersionSpec(string name, int version)
    {
        Query.Where(ic => ic.Name == name && ic.Version == version);
    }
}
