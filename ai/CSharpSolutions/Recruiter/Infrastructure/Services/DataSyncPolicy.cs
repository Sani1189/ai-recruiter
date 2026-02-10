// File: Infrastructure/Services/DataSyncPolicy.cs
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// Runtime sync policy checks for row-level data.
/// Entity-level policies (SyncScope, DataClassification) are now in EntitySyncConfiguration table.
/// This class handles per-row checks like residency and sanitization status.
/// </summary>
public static class DataSyncPolicy
{
    /// <summary>
    /// Check if a row can be synced to a target region based on row-level data.
    /// Note: Entity-level checks (SyncScope, DataClassification) should be done via EntitySyncConfiguration.
    /// </summary>
    public static bool CanSyncToRegion(this GdprSyncBaseDbModel row, DataResidency targetResidency)
    {
        // EU data can always stay in EU
        if (targetResidency == DataResidency.EU)
            return true;

        // NonEU target requires checking DataResidency
        // If row is marked EU-only residency, it cannot go to NonEU regions
        if (row.DataResidency == DataResidency.EU)
        {
            // Row is EU-resident, check if it can leave EU
            // This depends on sanitization status
            return row.IsSanitized == true || row.SanitizationOverrideConsentAt.HasValue;
        }

        // NonEU resident data can go anywhere
        return true;
    }

    /// <summary>
    /// Check if a row is eligible for global sync based on sanitization requirements.
    /// </summary>
    public static bool IsEligibleForGlobalSync(
        this GdprSyncBaseDbModel row, 
        bool requiresSanitization,
        bool allowOverrideConsent)
    {
        if (!requiresSanitization)
            return true;

        if (row.IsSanitized == true)
            return true;

        if (allowOverrideConsent && row.SanitizationOverrideConsentAt.HasValue)
            return true;

        return false;
    }

    /// <summary>
    /// Determine target residency for a country code.
    /// </summary>
    public static DataResidency GetTargetResidency(string countryCode)
    {
        return RegionResolver.ResidencyForCountry(countryCode);
    }
}

