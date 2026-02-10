using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Gets the latest version of a question with options for read-only reference.
/// Uses AsNoTracking() to prevent entity tracking conflicts when reusing in versioning scenarios.
/// </summary>
public sealed class QuestionLatestByNameWithOptionsSpec : Specification<QuestionnaireQuestion>
{
    public QuestionLatestByNameWithOptionsSpec(string name)
    {
        Query
            .Where(q => q.Name == name && !q.IsDeleted)
            .OrderByDescending(q => q.Version)
            .Take(1)
            .Include(q => q.Options.OrderBy(o => o.Order))
            .AsNoTracking(); // Industry best practice: No tracking for read-only reference data
    }
}
