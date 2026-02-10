using Recruiter.Application.Common.Interfaces;

namespace Recruiter.Application.JobPost.Interfaces;

// JobPostStepAssignment repository interface inheriting from generic repository
// All queries handled by specifications
public interface IJobPostStepAssignmentRepository : IRepository<Domain.Models.JobPostStepAssignment>
{
    // No additional methods needed!
    // Ardalis.Specification provides:
    // - GetByIdAsync, GetBySpecAsync, ListAsync
    // - CountAsync, AnyAsync, FirstOrDefaultAsync
    // - AddAsync, UpdateAsync, DeleteAsync
    // - SaveChangesAsync
    // All with specification support!
}
