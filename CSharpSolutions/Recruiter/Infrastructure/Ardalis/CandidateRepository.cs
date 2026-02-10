using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class CandidateRepository : EfRepository<Candidate>, ICandidateRepository
{
    public CandidateRepository(RecruiterDbContext context) : base(context)
    {
    }
}
