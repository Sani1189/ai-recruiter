using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionVersionsByNameSpec : Specification<QuestionnaireQuestion>
{
    public QuestionVersionsByNameSpec(string name)
    {
        Query
            .Where(q => q.Name == name)
            .OrderByDescending(q => q.Version);
    }
}
