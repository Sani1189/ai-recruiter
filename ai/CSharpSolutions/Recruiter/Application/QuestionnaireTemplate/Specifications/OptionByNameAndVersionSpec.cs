using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class OptionByNameAndVersionSpec : Specification<QuestionnaireQuestionOption>, ISingleResultSpecification<QuestionnaireQuestionOption>
{
    public OptionByNameAndVersionSpec(string name, int version)
    {
        Query
            .Where(o => o.Name == name && o.Version == version && !o.IsDeleted);
    }
}
