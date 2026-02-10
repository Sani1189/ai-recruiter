using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Specifications;

public sealed class JobApplicationStepFilesByIdSpec : Specification<Domain.Models.JobApplicationStepFiles>, ISingleResultSpecification<Domain.Models.JobApplicationStepFiles>
{
    public JobApplicationStepFilesByIdSpec(Guid id)
    {
        Query.Where(jasf => jasf.Id == id);
    }
}
