using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class FilesBySizeSpec : Specification<Domain.Models.File>
{
    public FilesBySizeSpec(int minSizeMb = 1) // 1MB default
    {
        Query.Where(f => f.MbSize >= minSizeMb)
             .OrderByDescending(f => f.MbSize);
    }
}
