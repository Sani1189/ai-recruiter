using Ardalis.Specification;

namespace Recruiter.Application.Interview.Specifications;

public sealed class InterviewByJobApplicationStepIdSpec : Specification<Domain.Models.Interview>
{
    public InterviewByJobApplicationStepIdSpec(Guid jobApplicationStepId)
    {
        Query.Where(i => i.JobApplicationStepId == jobApplicationStepId)
             .OrderByDescending(i => i.CreatedAt);
    }
}
