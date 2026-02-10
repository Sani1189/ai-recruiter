using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionByNameAndVersionSpec : Specification<QuestionnaireQuestion>, ISingleResultSpecification<QuestionnaireQuestion>
{
    public QuestionByNameAndVersionSpec(string name, int version)
    {
        Query
            .Where(q => q.Name == name && q.Version == version && !q.IsDeleted)
            .AsNoTracking();
    }
}
