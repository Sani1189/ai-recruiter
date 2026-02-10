using Ardalis.Specification;

namespace Recruiter.Application.CertificationLicense.Specifications;

/// <summary>
/// Filters certification/license records by user profile, excluding deleted items.
/// </summary>
public sealed class CertificationLicenseByUserProfileSpec : Specification<Domain.Models.CertificationLicense>
{
    public CertificationLicenseByUserProfileSpec(Guid userProfileId)
    {
        Query.Where(c => c.UserProfileId == userProfileId && !c.IsDeleted);
    }
}

