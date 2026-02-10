using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Gets the latest version of an option for read-only reference.
/// Uses AsNoTracking() to prevent entity tracking conflicts when used in versioning scenarios.
/// </summary>
public sealed class OptionLatestByNameSpec : Specification<QuestionnaireQuestionOption>, ISingleResultSpecification<QuestionnaireQuestionOption>
{
    public OptionLatestByNameSpec(string name)
    {
        Query
            .Where(o => o.Name == name && !o.IsDeleted)
            .OrderByDescending(o => o.Version)
            .Take(1)
            .AsNoTracking(); // Industry best practice: No tracking for read-only reference data
    }
}
