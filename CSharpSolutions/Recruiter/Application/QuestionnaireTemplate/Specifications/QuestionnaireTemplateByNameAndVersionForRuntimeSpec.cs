using Ardalis.Specification;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Loads a template version for runtime consumption (candidate rendering).
/// Soft-deleted templates (IsDeleted=true) must still be loadable because they can be referenced by
/// existing JobPostSteps and/or historical submissions. "Delete" in that case means "archive/hide",
/// not "break ongoing interviews".
/// </summary>
public sealed class QuestionnaireTemplateByNameAndVersionForRuntimeSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateByNameAndVersionForRuntimeSpec(string name, int version)
    {
        Query
            .Where(x => x.Name == name && x.Version == version)
            .Include(x => x.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Questions.Where(q => q.IsActive).OrderBy(q => q.Order))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order));
    }
}

