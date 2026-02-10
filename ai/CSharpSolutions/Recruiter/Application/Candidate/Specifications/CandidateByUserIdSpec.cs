using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Candidate.Specifications;

/// <summary>
/// Specification to find candidate by user ID
/// </summary>
public sealed class CandidateByUserIdSpec : Specification<Domain.Models.Candidate>, ISingleResultSpecification<Domain.Models.Candidate>
{
    public CandidateByUserIdSpec(Guid userId)
    {
        Query.Include(c => c.UserProfile!);
        Query.Where(c => c.UserId == userId);
    }
}