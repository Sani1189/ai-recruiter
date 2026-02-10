using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class SectionByTemplateAndOrderSpec : Specification<QuestionnaireSection>, ISingleResultSpecification<QuestionnaireSection>
{
    public SectionByTemplateAndOrderSpec(string templateName, int templateVersion, int order)
    {
        Query
            .Where(s => s.QuestionnaireTemplateName == templateName 
                     && s.QuestionnaireTemplateVersion == templateVersion 
                     && s.Order == order);
    }
}
