using Ardalis.Specification;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptFilterCountSpec : Specification<Domain.Models.Prompt>
{
    public PromptFilterCountSpec(PromptListQueryDto query)
    {
        // skip isDeleted prompts
        Query.Where(p => !p.IsDeleted);
        // Apply only filters (no sorting or pagination for count)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(p => p.Name.Contains(query.SearchTerm) ||
                            p.Category.Contains(query.SearchTerm) ||
                            (p.Locale != null && p.Locale.Contains(query.SearchTerm)) ||
                            p.Content.Contains(query.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            Query.Where(p => p.Category == query.Category);
        }

        if (!string.IsNullOrWhiteSpace(query.Locale))
        {
            Query.Where(p => p.Locale == query.Locale);
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            Query.Where(p => p.Name == query.Name);
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(p => p.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(p => p.CreatedAt <= query.CreatedBefore.Value);
        }
    }
}
