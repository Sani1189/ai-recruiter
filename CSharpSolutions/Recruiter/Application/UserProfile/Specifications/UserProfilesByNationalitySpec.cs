using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfilesByNationalitySpec : Specification<Domain.Models.UserProfile>
{
    public UserProfilesByNationalitySpec(string nationality)
    {
        Query.Where(up => up.Nationality == nationality)
             .OrderBy(up => up.Name);
    }
}
