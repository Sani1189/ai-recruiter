using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Interfaces;

// Repository interface for JobPostStep entity
// Inherits from IRepository<JobPostStep> for basic CRUD operations
// Can be extended with JobPostStep-specific methods in the future
public interface IJobPostStepRepository : IRepository<Domain.Models.JobPostStep>
{
    // Future JobPostStep-specific repository methods can be added here
    // For example: GetStepsByType, GetActiveSteps, etc.
}
