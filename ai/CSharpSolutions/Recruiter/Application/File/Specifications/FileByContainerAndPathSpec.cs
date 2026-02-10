using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class FileByContainerAndPathSpec : Specification<Domain.Models.File>, ISingleResultSpecification<Domain.Models.File>
{
    public FileByContainerAndPathSpec(string container, string filePath)
    {
        Query.Where(f => f.Container == container && f.FilePath == filePath);
    }
}
