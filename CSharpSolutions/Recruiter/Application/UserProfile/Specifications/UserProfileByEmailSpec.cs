using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfileByEmailSpec : Specification<Domain.Models.UserProfile>, ISingleResultSpecification<Domain.Models.UserProfile>
{
    public UserProfileByEmailSpec(string email)
    {
        Query.Where(up => up.Email.ToLower() == email.ToLower());
    }
}
