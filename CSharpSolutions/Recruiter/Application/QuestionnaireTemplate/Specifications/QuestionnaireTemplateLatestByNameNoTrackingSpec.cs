using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Fetches the latest version of a template by name without tracking.
/// Uses AsNoTracking() to prevent entity tracking conflicts when used in versioning scenarios.
/// </summary>
public sealed class QuestionnaireTemplateLatestByNameNoTrackingSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateLatestByNameNoTrackingSpec(string name)
    {
        Query
            .Where(x => x.Name == name && !x.IsDeleted)
            .OrderByDescending(x => x.Version)
            .Take(1)
            .AsNoTracking(); // Industry best practice: No tracking for read-only reference data
    }
}
