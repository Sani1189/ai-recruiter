using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Specifications;

/// <summary>
/// Specification to find user by provider ID (for Microsoft/Google OAuth)
/// </summary>
public sealed class UserProfileByProviderIdSpec : Specification<Domain.Models.UserProfile>, ISingleResultSpecification<Domain.Models.UserProfile>
{
    public UserProfileByProviderIdSpec(string providerId)
    {
        // Note: Since UserProfile doesn't store ProviderId directly,
        // this spec would need to be enhanced if you add provider tracking
        // For now, this is a placeholder for future enhancement
        Query.Where(up => up.Id == Guid.Empty); // Placeholder - will be enhanced later
    }
}
