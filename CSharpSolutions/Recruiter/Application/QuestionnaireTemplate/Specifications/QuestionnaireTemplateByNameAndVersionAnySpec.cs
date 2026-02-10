using Ardalis.Specification;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Fetches a template version by identity regardless of soft-delete state.
/// Keep it lightweight (no includes) for administrative operations like restore.
/// </summary>
public sealed class QuestionnaireTemplateByNameAndVersionAnySpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateByNameAndVersionAnySpec(string name, int version)
    {
        Query.Where(x => x.Name == name && x.Version == version);
    }
}

