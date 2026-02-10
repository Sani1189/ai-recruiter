using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionUsedInSubmissionsSpec : Specification<QuestionnaireCandidateSubmissionAnswer>
{
    public QuestionUsedInSubmissionsSpec(string questionName, int questionVersion)
    {
        Query.Where(a => 
            a.QuestionnaireQuestionName == questionName && 
            a.QuestionnaireQuestionVersion == questionVersion &&
            !a.IsDeleted);
    }
}
