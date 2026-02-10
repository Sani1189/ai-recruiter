using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public class LatestPromptByNameSpec : Specification<Domain.Models.Prompt>
{
    public LatestPromptByNameSpec(string name)
    {
        Query
            .Where(p => p.Name == name)
            .OrderByDescending(p => p.Version)
            .Take(1);
    }
}
