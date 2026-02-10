using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfileByIdSpec : Specification<Domain.Models.UserProfile>, ISingleResultSpecification<Domain.Models.UserProfile>
{
    public UserProfileByIdSpec(Guid id)
    {
        Query.Where(up => up.Id == id);
    }
}
