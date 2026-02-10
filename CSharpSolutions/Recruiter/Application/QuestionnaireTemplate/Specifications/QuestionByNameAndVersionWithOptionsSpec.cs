using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Loads a specific question version including its options (read-only).
/// </summary>
public sealed class QuestionByNameAndVersionWithOptionsSpec
    : Specification<QuestionnaireQuestion>, ISingleResultSpecification<QuestionnaireQuestion>
{
    public QuestionByNameAndVersionWithOptionsSpec(string name, int version)
    {
        Query
            .Where(q => q.Name == name && q.Version == version && !q.IsDeleted)
            .Include(q => q.Options.OrderBy(o => o.Order))
            .AsNoTracking();
    }
}

