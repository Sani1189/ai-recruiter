using Ardalis.Specification;
using Recruiter.Application.QuestionnaireTemplate.Dto;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionnaireTemplateFilterSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateFilterSpec(QuestionnaireTemplateListQueryDto query)
    {
        // This list endpoint powers the recruiter templates table.
        // We keep it reasonably light (no options include), but we DO include sections/questions so that
        // `QuestionsCount` / `SectionsCount` can be computed correctly in DTO mapping.
        Query.AsNoTracking()
            .Include(t => t.Sections)
                .ThenInclude(s => s.Questions.Where(q => q.IsActive));

        // Soft delete visibility
        if (query.OnlyDeleted)
        {
            Query.Where(t => t.IsDeleted);
        }
        else if (!query.IncludeDeleted)
        {
            Query.Where(t => !t.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            Query.Where(t =>
                t.Name.Contains(term) ||
                (t.Title != null && t.Title.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.TemplateType) && Enum.TryParse<Domain.Enums.QuestionnaireTemplateTypeEnum>(query.TemplateType, true, out var templateTypeEnum))
        {
            Query.Where(t => t.TemplateType == templateTypeEnum);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            Query.Where(t => t.Status == query.Status);
        }

        // Sorting
        var sortBy = (query.SortBy ?? "UpdatedAt").Trim().ToLowerInvariant();
        switch (sortBy)
        {
            case "name":
                if (query.SortDescending) Query.OrderByDescending(t => t.Name);
                else Query.OrderBy(t => t.Name);
                break;
            case "version":
                if (query.SortDescending) Query.OrderByDescending(t => t.Version);
                else Query.OrderBy(t => t.Version);
                break;
            case "templatetype":
                if (query.SortDescending) Query.OrderByDescending(t => t.TemplateType);
                else Query.OrderBy(t => t.TemplateType);
                break;
            case "status":
                if (query.SortDescending) Query.OrderByDescending(t => t.Status);
                else Query.OrderBy(t => t.Status);
                break;
            case "createdat":
                if (query.SortDescending) Query.OrderByDescending(t => t.CreatedAt);
                else Query.OrderBy(t => t.CreatedAt);
                break;
            case "updatedat":
            default:
                if (query.SortDescending) Query.OrderByDescending(t => t.UpdatedAt);
                else Query.OrderBy(t => t.UpdatedAt);
                break;
        }

        // Pagination
        Query.Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);
    }
}


