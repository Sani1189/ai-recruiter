using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptByNameAndVersionSpec : Specification<Domain.Models.Prompt>, ISingleResultSpecification<Domain.Models.Prompt>
{
    public PromptByNameAndVersionSpec(string name, int version)
    {
        Query.Where(p => p.Name == name && p.Version == version);
    }
}
