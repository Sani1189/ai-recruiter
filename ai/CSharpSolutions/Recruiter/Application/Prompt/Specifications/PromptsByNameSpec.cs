using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptsByNameSpec : Specification<Domain.Models.Prompt>
{
    public PromptsByNameSpec(string name)
    {
        Query.Where(p => p.Name == name)
             .OrderByDescending(p => p.Version);
    }
}
