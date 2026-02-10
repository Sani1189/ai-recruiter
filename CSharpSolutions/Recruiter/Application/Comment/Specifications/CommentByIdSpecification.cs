using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Comment.Specifications;

public sealed class CommentByIdSpecification : Specification<Domain.Models.Comment>, ISingleResultSpecification<Domain.Models.Comment>
{
    public CommentByIdSpecification(Guid id)
    {
        Query.Where(c => c.Id == id && !c.IsDeleted);
    }
}

