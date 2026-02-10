using Ardalis.Specification;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Comment.Specifications;

public sealed class CommentByEntitySpecification : Specification<Domain.Models.Comment>, ISingleResultSpecification<Domain.Models.Comment>
{
    public CommentByEntitySpecification(CommentableEntityType entityType, Guid entityId)
    {
        Query.Where(c => c.EntityType == entityType && c.EntityId == entityId)
             .Where(c => !c.IsDeleted)
             .Where(c => c.ParentCommentId == null) // Only top-level comments
             .OrderByDescending(c => c.CreatedAt); // Get the most recent one if multiple exist
    }
}

