using Ardalis.Specification;
using Recruiter.Application.Comment.Dto;

namespace Recruiter.Application.Comment.Specifications;

public sealed class CommentFilterCountSpec : Specification<Domain.Models.Comment>
{
    public CommentFilterCountSpec(CommentListQueryDto query)
    {
        ApplyFilters(query);
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
}

