using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Gets the latest version of a question for read-only reference.
/// Uses AsNoTracking() to prevent entity tracking conflicts when used in versioning scenarios.
/// </summary>
public sealed class QuestionLatestByNameSpec : Specification<QuestionnaireQuestion>, ISingleResultSpecification<QuestionnaireQuestion>
{
    public QuestionLatestByNameSpec(string name)
    {
        Query
            .Where(q => q.Name == name && !q.IsDeleted)
            .OrderByDescending(q => q.Version)
            .Take(1)
            .AsNoTracking(); // Industry best practice: No tracking for read-only reference data
    }
}
