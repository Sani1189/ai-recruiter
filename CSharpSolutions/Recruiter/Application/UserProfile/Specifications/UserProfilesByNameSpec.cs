using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfilesByNameSpec : Specification<Domain.Models.UserProfile>
{
    public UserProfilesByNameSpec(string name)
    {
        Query.Where(up => up.Name.ToLower().Contains(name.ToLower()))
             .OrderBy(up => up.Name);
    }
}
