using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class OptionUsedInSubmissionsSpec : Specification<QuestionnaireCandidateSubmissionAnswerOption>
{
    public OptionUsedInSubmissionsSpec(string optionName, int optionVersion)
    {
        Query.Where(o => 
            o.QuestionnaireQuestionOptionName == optionName && 
            o.QuestionnaireQuestionOptionVersion == optionVersion &&
            !o.IsDeleted);
    }
}
