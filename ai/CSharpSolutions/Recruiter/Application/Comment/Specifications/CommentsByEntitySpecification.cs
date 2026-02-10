using Ardalis.Specification;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Comment.Specifications;

public sealed class CommentsByEntitySpecification : Specification<Domain.Models.Comment>
{
    public CommentsByEntitySpecification(CommentableEntityType entityType, Guid entityId, bool includeReplies = false)
    {
        Query.Where(c => c.EntityType == entityType && c.EntityId == entityId)
             .Where(c => !c.IsDeleted)
             .OrderByDescending(c => c.CreatedAt);

        if (!includeReplies)
        {
            Query.Where(c => c.ParentCommentId == null);
        }
    }
}

