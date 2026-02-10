using Ardalis.Specification;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class AllJobApplicationsWithIncludesSpec : Specification<Domain.Models.JobApplication>
{
    public AllJobApplicationsWithIncludesSpec()
    {
        Query.Include(ja => ja.Candidate!)
            .ThenInclude(c => c.UserProfile!)
            .OrderByDescending(ja => ja.CreatedAt);
    }
}

