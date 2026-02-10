using Ardalis.Specification;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionnaireTemplatesByNameSpec : Ardalis.Specification.Specification<Recruiter.Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplatesByNameSpec(string name)
    {
        Query
            .Where(x => x.Name == name && !x.IsDeleted)
            .OrderByDescending(x => x.Version);
    }
}


