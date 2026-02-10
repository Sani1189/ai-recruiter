using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptsByLocaleSpec : Specification<Domain.Models.Prompt>
{
    public PromptsByLocaleSpec(string locale)
    {
        Query.Where(p => p.Locale == locale)
             .OrderByDescending(p => p.CreatedAt);
    }
}
