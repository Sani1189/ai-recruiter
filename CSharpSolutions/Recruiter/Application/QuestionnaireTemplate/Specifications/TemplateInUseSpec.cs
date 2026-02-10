using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class TemplateInUseSpec : Specification<JobPostStep>
{
    public TemplateInUseSpec(string templateName, int templateVersion)
    {
        Query.Where(s => 
            s.QuestionnaireTemplateName == templateName && 
            s.QuestionnaireTemplateVersion == templateVersion &&
            !s.IsDeleted);
    }
}

public sealed class TemplateInUseBySubmissionsSpec : Specification<QuestionnaireCandidateSubmission>
{
    public TemplateInUseBySubmissionsSpec(string templateName, int templateVersion)
    {
        Query.Where(s => 
            s.QuestionnaireTemplateName == templateName && 
            s.QuestionnaireTemplateVersion == templateVersion &&
            !s.IsDeleted);
    }
}
