using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Loads all versions for a question name including options (read-only).
/// </summary>
public sealed class QuestionsByNameWithOptionsSpec : Specification<QuestionnaireQuestion>
{
    public QuestionsByNameWithOptionsSpec(string name)
    {
        Query
            .Where(q => q.Name == name && !q.IsDeleted)
            .Include(q => q.Options.OrderBy(o => o.Order))
            .OrderByDescending(q => q.Version)
            .AsNoTracking();
    }
}

