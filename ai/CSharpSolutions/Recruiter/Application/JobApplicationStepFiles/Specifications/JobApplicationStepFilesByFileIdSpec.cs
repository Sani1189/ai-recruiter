using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Specifications;

public sealed class JobApplicationStepFilesByFileIdSpec : Specification<Domain.Models.JobApplicationStepFiles>
{
    public JobApplicationStepFilesByFileIdSpec(Guid fileId)
    {
        Query.Where(jasf => jasf.FileId == fileId)
             .OrderByDescending(jasf => jasf.CreatedAt);
    }
}
