using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Loads a template version for activating a specific question version.
/// - Tracking query (no AsNoTracking)
/// - Includes only the target section by order
/// - Includes ALL question versions for that section (no IsActive filter) + options
/// </summary>
public sealed class QuestionnaireTemplateByNameAndVersionForQuestionActivationSpec
    : Specification<Recruiter.Domain.Models.QuestionnaireTemplate>, ISingleResultSpecification<Recruiter.Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateByNameAndVersionForQuestionActivationSpec(string name, int version, int sectionOrder)
    {
        Query
            .Where(t => t.Name == name && t.Version == version && !t.IsDeleted)
            .Include(t => t.Sections.Where(s => s.Order == sectionOrder))
                .ThenInclude(s => s.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order));
    }
}

