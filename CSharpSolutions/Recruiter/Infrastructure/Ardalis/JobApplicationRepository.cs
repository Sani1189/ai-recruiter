using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class JobApplicationRepository : EfRepository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(RecruiterDbContext context) : base(context)
    {
    }
}
