using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire.Specifications;

/// <summary>
/// Optimized spec for checking submission status only (no includes needed)
/// </summary>
public sealed class QuestionnaireSubmissionStatusByStepIdSpec : Specification<QuestionnaireCandidateSubmission>
{
    public QuestionnaireSubmissionStatusByStepIdSpec(Guid jobApplicationStepId)
    {
        Query
            .Where(x => x.JobApplicationStepId == jobApplicationStepId && !x.IsDeleted)
            .AsNoTracking(); // Read-only, no tracking needed
    }
}
