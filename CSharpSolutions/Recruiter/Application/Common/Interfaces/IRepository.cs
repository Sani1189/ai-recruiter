using Ardalis.Specification;

namespace Recruiter.Application.Common.Interfaces;

public interface IRepository<T> : IRepositoryBase<T> where T : class
{
    void Detach(T entity);

    void ClearChangeTracker();
}
