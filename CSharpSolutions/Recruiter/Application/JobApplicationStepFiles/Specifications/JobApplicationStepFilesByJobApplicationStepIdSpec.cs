using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Specifications;

public sealed class JobApplicationStepFilesByJobApplicationStepIdSpec : Specification<Domain.Models.JobApplicationStepFiles>
{
    public JobApplicationStepFilesByJobApplicationStepIdSpec(Guid jobApplicationStepId)
    {
        Query.Where(jasf => jasf.JobApplicationStepId == jobApplicationStepId)
             .OrderByDescending(jasf => jasf.CreatedAt);
    }
}
