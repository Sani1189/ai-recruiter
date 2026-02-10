using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Comment.Specifications;

public sealed class CommentThreadSpecification : Specification<Domain.Models.Comment>
{
    public CommentThreadSpecification(Guid parentCommentId)
    {
        Query.Where(c => (c.Id == parentCommentId || c.ParentCommentId == parentCommentId))
             .Where(c => !c.IsDeleted)
             .OrderBy(c => c.CreatedAt); // Oldest first for thread context
    }
}

