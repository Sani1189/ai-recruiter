using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class FilesByContainerSpec : Specification<Domain.Models.File>
{
    public FilesByContainerSpec(string container)
    {
        Query.Where(f => f.Container == container)
             .OrderBy(f => f.FilePath);
    }
}
