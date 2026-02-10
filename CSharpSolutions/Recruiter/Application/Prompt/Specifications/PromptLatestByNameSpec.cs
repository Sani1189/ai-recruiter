using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptLatestByNameSpec : Specification<Domain.Models.Prompt>, ISingleResultSpecification<Domain.Models.Prompt>
{
    public PromptLatestByNameSpec(string name)
    {
        Query.Where(p => p.Name == name)
             .OrderByDescending(p => p.Version)
             .Take(1);
    }
}
