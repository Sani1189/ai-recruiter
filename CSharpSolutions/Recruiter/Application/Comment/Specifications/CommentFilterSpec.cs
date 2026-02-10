using Ardalis.Specification;
using Recruiter.Application.Comment.Dto;

namespace Recruiter.Application.Comment.Specifications;

public sealed class CommentFilterSpec : Specification<Domain.Models.Comment>
{
    public CommentFilterSpec(CommentListQueryDto query)
    {
        ApplyFilters(query);
        ApplySorting(query);
        ApplyPaging(query);
    }

    private void ApplyFilters(CommentListQueryDto query)
    {
        Query.Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(c => c.Content.Contains(query.SearchTerm));
        }

        if (query.EntityId.HasValue)
        {
            Query.Where(c => c.EntityId == query.EntityId.Value);
        }

        if (query.EntityType.HasValue)
        {
            Query.Where(c => c.EntityType == query.EntityType.Value);
        }

        if (query.ParentCommentId.HasValue)
        {
            Query.Where(c => c.ParentCommentId == query.ParentCommentId.Value);
        }
        else if (query.IncludeReplies == false)
        {
            // If not explicitly including replies, only show top-level comments
            Query.Where(c => c.ParentCommentId == null);
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(c => c.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(c => c.CreatedAt <= query.CreatedBefore.Value);
        }
    }

    private void ApplySorting(CommentListQueryDto query)
    {
        var sortField = query.SortBy?.ToLower() ?? "createdat";
        
        if (query.SortDescending)
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderByDescending(c => c.Id);
                    break;
                case "content":
                    Query.OrderByDescending(c => c.Content);
                    break;
                case "entityid":
                    Query.OrderByDescending(c => c.EntityId);
                    break;
                case "entitytype":
                    Query.OrderByDescending(c => c.EntityType);
                    break;
                case "updatedat":
                    Query.OrderByDescending(c => c.UpdatedAt);
                    break;
                default:
                    Query.OrderByDescending(c => c.CreatedAt);
                    break;
            }
        }
        else
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderBy(c => c.Id);
                    break;
                case "content":
                    Query.OrderBy(c => c.Content);
                    break;
                case "entityid":
                    Query.OrderBy(c => c.EntityId);
                    break;
                case "entitytype":
                    Query.OrderBy(c => c.EntityType);
                    break;
                case "updatedat":
                    Query.OrderBy(c => c.UpdatedAt);
                    break;
                default:
                    Query.OrderBy(c => c.CreatedAt);
                    break;
            }
        }
    }

    private void ApplyPaging(CommentListQueryDto query)
    {
        if (query.PageNumber > 0 && query.PageSize > 0)
        {
            Query.Skip((query.PageNumber - 1) * query.PageSize)
                 .Take(query.PageSize);
        }
    }
}

