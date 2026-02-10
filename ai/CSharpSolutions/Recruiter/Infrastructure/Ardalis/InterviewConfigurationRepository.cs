using Recruiter.Application.InterviewConfiguration.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class InterviewConfigurationRepository : EfVersionedRepository<InterviewConfiguration>, IInterviewConfigurationRepository
{
    public InterviewConfigurationRepository(RecruiterDbContext context) : base(context)
    {
    }
}
