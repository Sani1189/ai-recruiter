using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Specifications;

public sealed class JobApplicationStepFilesByFileAndStepSpec : Specification<Domain.Models.JobApplicationStepFiles>, ISingleResultSpecification<Domain.Models.JobApplicationStepFiles>
{
    public JobApplicationStepFilesByFileAndStepSpec(Guid fileId, Guid jobApplicationStepId)
    {
        Query.Where(jasf => jasf.FileId == fileId && jasf.JobApplicationStepId == jobApplicationStepId);
    }
}
