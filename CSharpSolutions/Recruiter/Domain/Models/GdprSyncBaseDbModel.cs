// File: Domain/Models/GdprSyncBaseDbModel.cs
using Recruiter.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Base class containing ONLY GDPR sync properties.
/// Both BaseDbModel and VersionedBaseDbModel inherit from this to share sync metadata.
/// 
/// Note: Entity-level properties (DataClassification, SyncScope, LegalBasis) are configured
/// in the EntitySyncConfiguration table, not per-row.
/// </summary>
[NotMapped]
public abstract class GdprSyncBaseDbModel
{
    /// <summary>
    /// Where this data must reside (EU vs NonEU) - per row based on user location.
    /// Default: EU (safest default for GDPR compliance)
    /// </summary>
    public DataResidency DataResidency { get; set; } = DataResidency.EU;

    /// <summary>
    /// Region where this row was originally created.
    /// Default: EU (safest default for GDPR compliance)
    /// </summary>
    public DataRegion DataOriginRegion { get; set; } = DataRegion.EU;

    public Guid? CountryExposureSetId { get; set; }

    public CountryExposureSet? CountryExposureSet { get; set; }

    /// <summary>
    /// Last time this row was successfully synced INTO the current database
    /// </summary>
    public DateTimeOffset? LastSyncedAt { get; set; }

    /// <summary>
    /// Idempotency key for sync operations
    /// </summary>
    [MaxLength(128)]
    public string? LastSyncEventId { get; set; }

    /// <summary>
    /// Whether this specific row has been sanitized (PII removed)
    /// </summary>
    public bool? IsSanitized { get; set; }

    /// <summary>
    /// When this row was sanitized
    /// </summary>
    public DateTimeOffset? SanitizedAt { get; set; }

    /// <summary>
    /// If set, user explicitly consented to share this row globally without sanitization.
    /// Overrides the normal sanitization requirement for GlobalSanitized scope.
    /// </summary>
    public DateTimeOffset? SanitizationOverrideConsentAt { get; set; }
}
