using Ardalis.Specification;

namespace Recruiter.Application.Candidate.Specifications;

public sealed class CandidateByCvFileIdSpec : Specification<Domain.Models.Candidate>
{
    public CandidateByCvFileIdSpec(Guid cvFileId)
    {
        Query.Include(c => c.UserProfile!);
        Query.Include(c => c.CvFile!);
        Query.Where(c => c.CvFileId == cvFileId)
             .OrderByDescending(c => c.CreatedAt);
    }
}
