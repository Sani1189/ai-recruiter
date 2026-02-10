using Ardalis.Specification.EntityFrameworkCore;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

// Clean JobPostStepAssignment repository implementation
// Custom implementation for composite key entity
public class JobPostStepAssignmentRepository : EfRepository<Domain.Models.JobPostStepAssignment>, IJobPostStepAssignmentRepository
{
    public JobPostStepAssignmentRepository(RecruiterDbContext context) : base(context)
    {
        // That's it! All functionality provided by:
        // 1. RepositoryBase<T> (Ardalis base for custom entities)
        // 2. Ardalis.Specification (all query methods)
        // 3. Specifications (all business logic)
        
        // No manual LINQ queries needed!
        // No duplicate code!
    }
}