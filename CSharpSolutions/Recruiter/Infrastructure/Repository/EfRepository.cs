using Ardalis.Specification.EntityFrameworkCore;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Recruiter.Infrastructure.Repository;

public class EfRepository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity> where TEntity : BaseDbModel
{
    private readonly RecruiterDbContext _context;

    public EfRepository(RecruiterDbContext context) : base(context)
    {
        _context = context;
    }

    public void Detach(TEntity entity)
    {
        if (entity != null)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
    }

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }
}
