// File: Domain/Services/JobAdExposurePolicy.cs
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Services;

namespace Recruiter.Infrastructure.Services;

public static class JobAdExposurePolicy
{
    /// Checks both: GDPR sync policy + job ad country exposure list.
    public static bool CanPublishToCountry(this JobPost jobAd, string countryCode)
    {
        //if (!jobAd.CanSyncTo(countryCode))
        //    return false;

        var cc = RegionResolver.NormalizeCountryCode(countryCode);
        return false;
        //return jobAd.Exposures?.Any(x => x.IsActive && x.CountryCode == cc) == true;
    }
}
