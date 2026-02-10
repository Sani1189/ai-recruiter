using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class FileByIdSpec : Specification<Domain.Models.File>, ISingleResultSpecification<Domain.Models.File>
{
    public FileByIdSpec(Guid id)
    {
        Query.Where(f => f.Id == id);
    }
}
