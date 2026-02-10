using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire.Specifications;

/// <summary>
/// Minimal spec for updating a submission (no includes).
/// </summary>
public sealed class QuestionnaireSubmissionForUpdateByStepIdSpec : Specification<QuestionnaireCandidateSubmission>
{
    public QuestionnaireSubmissionForUpdateByStepIdSpec(Guid jobApplicationStepId)
    {
        Query.Where(x => x.JobApplicationStepId == jobApplicationStepId && !x.IsDeleted);
    }
}

