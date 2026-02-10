using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class JobPostStepsUsingAssessmentTemplateSpec : Specification<JobPostStep>
{
    public JobPostStepsUsingAssessmentTemplateSpec(string name, int version)
    {
        Query.Where(s =>
            s.QuestionnaireTemplateName == name &&
            s.QuestionnaireTemplateVersion == version &&
            !s.IsDeleted);
    }
}


