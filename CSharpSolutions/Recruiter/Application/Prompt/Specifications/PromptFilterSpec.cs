using Ardalis.Specification;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class PromptFilterSpec : Specification<Domain.Models.Prompt>
{
    public PromptFilterSpec(PromptListQueryDto query)
    {
        // skip isDeleted prompts
        Query.Where(p => !p.IsDeleted);
        // Apply filters
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

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            switch (query.SortBy.ToLowerInvariant())
            {
                case "name":
                    if (query.SortDescending)
                        Query.OrderByDescending(p => p.Name);
                    else
                        Query.OrderBy(p => p.Name);
                    break;
                case "category":
                    if (query.SortDescending)
                        Query.OrderByDescending(p => p.Category);
                    else
                        Query.OrderBy(p => p.Category);
                    break;
                case "locale":
                    if (query.SortDescending)
                        Query.OrderByDescending(p => p.Locale);
                    else
                        Query.OrderBy(p => p.Locale);
                    break;
                case "version":
                    if (query.SortDescending)
                        Query.OrderByDescending(p => p.Version);
                    else
                        Query.OrderBy(p => p.Version);
                    break;
                case "createdat":
                default:
                    if (query.SortDescending)
                        Query.OrderByDescending(p => p.CreatedAt);
                    else
                        Query.OrderBy(p => p.CreatedAt);
                    break;
            }
        }
        else
        {
            Query.OrderByDescending(p => p.CreatedAt);
        }
        // Apply pagination
        Query.Skip((query.PageNumber - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
