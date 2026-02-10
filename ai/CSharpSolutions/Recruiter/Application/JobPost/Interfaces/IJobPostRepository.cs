using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Interfaces;

// Repository interface for JobPost entity
// Inherits from IRepository<JobPost> for basic CRUD operations
// Can be extended with JobPost-specific methods in the future
public interface IJobPostRepository : IRepository<Domain.Models.JobPost>
{
    // Future JobPost-specific repository methods can be added here
    // For example: GetJobPostsByDepartment, GetActiveJobPosts, etc.
}
