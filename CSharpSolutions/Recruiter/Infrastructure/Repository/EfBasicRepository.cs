using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Infrastructure.Repository;

/// <summary>
/// EF repository for entities using BasicBaseDbModel (e.g. Country, EntitySyncConfiguration).
/// Use when the entity has neither Guid Id (BaseDbModel) nor Name+Version (VersionedBaseDbModel).
/// </summary>
public class EfBasicRepository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity>
    where TEntity : BasicBaseDbModel
{
    private readonly RecruiterDbContext _context;

    public EfBasicRepository(RecruiterDbContext context)
        : base(context)
    {
        _context = context;
    }

    public void Detach(TEntity entity)
    {
        if (entity != null)
            _context.Entry(entity).State = EntityState.Detached;
    }

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }
}
