using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class OptionVersionsByNameSpec : Specification<QuestionnaireQuestionOption>
{
    public OptionVersionsByNameSpec(string name)
    {
        Query
            .Where(o => o.Name == name)
            .OrderByDescending(o => o.Version);
    }
}
