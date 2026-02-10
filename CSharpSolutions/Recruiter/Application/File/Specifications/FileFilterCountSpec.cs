using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Application.File.Dto;

namespace Recruiter.Application.File.Specifications;

public sealed class FileFilterCountSpec : Specification<Domain.Models.File>
{
    public FileFilterCountSpec(FileListQueryDto query)
    {
        // Apply only filters (no sorting or pagination for count)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(f => f.FilePath.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.Container))
        {
            Query.Where(f => f.Container == query.Container);
        }

        if (!string.IsNullOrWhiteSpace(query.Extension))
        {
            Query.Where(f => f.Extension == query.Extension);
        }

        if (query.MinSizeMb.HasValue)
        {
            Query.Where(f => f.MbSize >= query.MinSizeMb.Value);
        }

        if (query.MaxSizeMb.HasValue)
        {
            Query.Where(f => f.MbSize <= query.MaxSizeMb.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.StorageAccountName))
        {
            Query.Where(f => f.StorageAccountName.Contains(query.StorageAccountName));
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(f => f.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(f => f.CreatedAt <= query.CreatedBefore.Value);
        }

        if (query.IsRecent.HasValue && query.IsRecent.Value)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            Query.Where(f => f.CreatedAt >= cutoffDate);
        }

        if (query.IsLarge.HasValue && query.IsLarge.Value)
        {
            Query.Where(f => f.MbSize >= 10);
        }
    }
}
