using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

// Clean JobPost repository implementation
// No business logic here - all handled by specifications
public class JobPostRepository : EfVersionedRepository<Domain.Models.JobPost>, IJobPostRepository
{
    public JobPostRepository(RecruiterDbContext context) : base(context)
    {
        // That's it! All functionality provided by:
        // 1. EfVersionedRepository<T> (versioned implementation)
        // 2. Ardalis.Specification (all query methods)
        // 3. Specifications (all business logic)
        
        // No manual LINQ queries needed!
        // No duplicate code!
        
        // Future JobPost-specific methods can be implemented here
        // For example: GetJobPostsByDepartment, GetActiveJobPosts, etc.
    }
}
