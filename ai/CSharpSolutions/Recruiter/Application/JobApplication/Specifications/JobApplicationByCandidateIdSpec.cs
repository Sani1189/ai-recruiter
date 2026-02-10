using Ardalis.Specification;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationByCandidateIdSpec : Specification<Domain.Models.JobApplication>
{
    public JobApplicationByCandidateIdSpec(Guid candidateId)
    {
        Query.Include(ja => ja.JobPost)
            .Where(ja => ja.CandidateId == candidateId)
            .OrderByDescending(ja => ja.CreatedAt);
    }
}
