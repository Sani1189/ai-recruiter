using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Application.File.Dto;

namespace Recruiter.Application.File.Specifications;

public sealed class FileFilterSpec : Specification<Domain.Models.File>
{
    public FileFilterSpec(FileListQueryDto query)
    {
        // Apply filters
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
            Query.Where(f => f.MbSize >= 10); // 10MB or larger
        }

        // Apply sorting
        var sortField = query.SortBy?.ToLower() ?? "createdat";
        
        if (query.SortDescending)
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderByDescending(f => f.Id);
                    break;
                case "container":
                    Query.OrderByDescending(f => f.Container);
                    break;
                case "filepath":
                    Query.OrderByDescending(f => f.FilePath);
                    break;
                case "extension":
                    Query.OrderByDescending(f => f.Extension);
                    break;
                case "mbsize":
                    Query.OrderByDescending(f => f.MbSize);
                    break;
                case "updatedat":
                    Query.OrderByDescending(f => f.UpdatedAt);
                    break;
                default:
                    Query.OrderByDescending(f => f.CreatedAt);
                    break;
            }
        }
        else
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderBy(f => f.Id);
                    break;
                case "container":
                    Query.OrderBy(f => f.Container);
                    break;
                case "filepath":
                    Query.OrderBy(f => f.FilePath);
                    break;
                case "extension":
                    Query.OrderBy(f => f.Extension);
                    break;
                case "mbsize":
                    Query.OrderBy(f => f.MbSize);
                    break;
                case "updatedat":
                    Query.OrderBy(f => f.UpdatedAt);
                    break;
                default:
                    Query.OrderBy(f => f.CreatedAt);
                    break;
            }
        }

        // Apply pagination
        Query.Skip((query.PageNumber - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
