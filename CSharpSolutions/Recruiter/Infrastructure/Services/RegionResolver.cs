// File: Domain/Services/RegionResolver.cs
using Recruiter.Domain.Enums;

namespace Recruiter.Infrastructure.Services;

/// Keep this mapping in one place.
/// You can swap it for a DB table later.
public static class RegionResolver
{
    public static string NormalizeCountryCode(string countryCode)
        => (countryCode ?? string.Empty).Trim().ToUpperInvariant();

    /// Simplified mapping. Extend as needed.
    public static bool IsEuCountry(string countryCode)
    {
        var cc = NormalizeCountryCode(countryCode);

        // EU/EEA-ish set (adjust to your policy; include EEA if you want).
        // NOTE: UK is not EU; CH is not EU; add/remove per your requirements.
        return cc is
            "AT" or "BE" or "BG" or "HR" or "CY" or "CZ" or "DK" or "EE" or "FI" or "FR" or "DE" or "GR" or "HU" or
            "IE" or "IT" or "LV" or "LT" or "LU" or "MT" or "NL" or "PL" or "PT" or "RO" or "SK" or "SI" or "ES" or "SE"
            // EEA (optional):  
            or "NO" or "IS" or "LI";
    }

    public static DataResidency ResidencyForCountry(string countryCode)
        => IsEuCountry(countryCode) ? DataResidency.EU : DataResidency.NonEU;
}
