using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfilesByAgeRangeSpec : Specification<Domain.Models.UserProfile>
{
    public UserProfilesByAgeRangeSpec(int minAge, int maxAge)
    {
        Query.Where(up => up.Age >= minAge && up.Age <= maxAge)
             .OrderBy(up => up.Name);
    }
}
