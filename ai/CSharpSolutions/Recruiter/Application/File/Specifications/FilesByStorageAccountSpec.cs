using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File.Specifications;

public sealed class FilesByStorageAccountSpec : Specification<Domain.Models.File>
{
    public FilesByStorageAccountSpec(string storageAccountName)
    {
        Query.Where(f => f.StorageAccountName.Contains(storageAccountName))
             .OrderByDescending(f => f.CreatedAt);
    }
}
