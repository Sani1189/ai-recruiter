using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class OptionByQuestionAndOrderSpec : Specification<QuestionnaireQuestionOption>, ISingleResultSpecification<QuestionnaireQuestionOption>
{
    public OptionByQuestionAndOrderSpec(string questionName, int questionVersion, int order)
    {
        Query
            .Where(o => o.QuestionnaireQuestionName == questionName 
                     && o.QuestionnaireQuestionVersion == questionVersion 
                     && o.Order == order 
                     && !o.IsDeleted)
            .AsNoTracking();
    }
}
