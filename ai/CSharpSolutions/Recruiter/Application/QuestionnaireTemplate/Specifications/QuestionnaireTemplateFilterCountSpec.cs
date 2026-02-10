using Ardalis.Specification;
using Recruiter.Application.QuestionnaireTemplate.Dto;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionnaireTemplateFilterCountSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateFilterCountSpec(QuestionnaireTemplateListQueryDto query)
    {
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
    }
}


