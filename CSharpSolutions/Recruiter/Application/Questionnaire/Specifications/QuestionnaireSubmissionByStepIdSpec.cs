using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire.Specifications;

public sealed class QuestionnaireSubmissionByStepIdSpec : Specification<QuestionnaireCandidateSubmission>
{
    public QuestionnaireSubmissionByStepIdSpec(Guid jobApplicationStepId)
    {
        Query
            .Where(x => x.JobApplicationStepId == jobApplicationStepId && !x.IsDeleted)
            .Include(x => x.Answers)
                .ThenInclude(a => a.SelectedOptions);
    }
}

