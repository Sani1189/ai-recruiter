using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Lightweight existence check for a template by (Name, Version).
/// Uses AsNoTracking() to avoid EF Core change tracker collisions during versioning flows.
/// </summary>
public sealed class QuestionnaireTemplateExistsByNameAndVersionSpec : Specification<Recruiter.Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateExistsByNameAndVersionSpec(string name, int version)
    {
        Query
            .Where(x => x.Name == name && x.Version == version && !x.IsDeleted)
            .AsNoTracking();
    }
}

