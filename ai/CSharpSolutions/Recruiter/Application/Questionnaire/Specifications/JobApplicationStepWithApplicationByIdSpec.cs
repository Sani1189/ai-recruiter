using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire.Specifications;

public sealed class JobApplicationStepWithApplicationByIdSpec : Specification<JobApplicationStep>
{
    public JobApplicationStepWithApplicationByIdSpec(Guid stepId)
    {
        Query
            .Where(s => s.Id == stepId && !s.IsDeleted)
            .Include(s => s.JobApplication);
    }
}

