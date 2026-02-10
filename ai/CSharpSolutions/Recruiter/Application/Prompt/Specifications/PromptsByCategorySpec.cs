using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptsByCategorySpec : Specification<Domain.Models.Prompt>
{
    public PromptsByCategorySpec(string category)
    {
        Query.Where(p => p.Category == category)
             .OrderByDescending(p => p.CreatedAt);
    }
}
