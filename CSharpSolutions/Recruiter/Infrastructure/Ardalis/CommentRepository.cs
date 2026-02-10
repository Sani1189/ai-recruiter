using Recruiter.Application.Comment.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class CommentRepository : EfRepository<Comment>, ICommentRepository
{
    public CommentRepository(RecruiterDbContext context) : base(context)
    {
    }
}

