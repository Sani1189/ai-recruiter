using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class FilesByExtensionSpec : Specification<Domain.Models.File>
{
    public FilesByExtensionSpec(string extension)
    {
        Query.Where(f => f.Extension == extension)
             .OrderByDescending(f => f.CreatedAt);
    }
}
