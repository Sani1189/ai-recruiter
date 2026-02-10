using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

/// <summary>
/// Specification to find UserProfile by email
/// </summary>
public class UserProfileByEmailSpecification : Specification<Domain.Models.UserProfile>
{
    public UserProfileByEmailSpecification(string email)
    {
        Query.Where(up => up.Email == email);
    }
}
