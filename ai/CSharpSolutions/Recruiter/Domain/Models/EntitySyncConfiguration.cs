using Recruiter.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Recruiter.Domain.Models;

/// <summary>
/// Defines GDPR and sync configuration for an entity type.
/// Stored in a configuration table - one row per entity type.
/// This is a configuration table, so it uses BasicBaseDbModel (no GDPR sync needed).
/// </summary>
public class EntitySyncConfiguration : BasicBaseDbModel
{
    /// <summary>
    /// The entity type name (e.g., "Candidate", "JobPost", "Country")
    /// </summary>
    [Required, MaxLength(128)]
    public required string EntityTypeName { get; set; }

    /// <summary>
    /// The database table name (if different from EntityTypeName)
    /// </summary>
    [MaxLength(128)]
    public string? TableName { get; set; }

    /// <summary>
    /// Data classification for this entity type
    /// </summary>
    [Required]
    public required DataClassification DataClassification { get; set; }

    /// <summary>
    /// Default sync scope for this entity type
    /// </summary>
    [Required]
    public required SyncScope SyncScope { get; set; }

    /// <summary>
    /// Legal basis for processing this type of data
    /// </summary>
    [Required]
    public required LegalBasisType LegalBasis { get; set; }

    /// <summary>
    /// Reference to legal documentation (e.g., "privacy-policy-v2", "dpa-section-4")
    /// </summary>
    [MaxLength(256)]
    public string? LegalBasisRef { get; set; }

    /// <summary>
    /// Purpose limitation description
    /// </summary>
    [Required, MaxLength(256)]
    public required string ProcessingPurpose { get; set; }

    /// <summary>
    /// Whether this entity type requires sanitization before global sync
    /// </summary>
    public bool RequiresSanitizationForGlobalSync { get; set; }

    /// <summary>
    /// Whether users can override sanitization requirement with explicit consent
    /// </summary>
    public bool AllowSanitizationOverrideConsent { get; set; }

    /// <summary>
    /// Entity types that must be synced before this one (FK dependencies)
    /// Comma-separated list (e.g., "Candidate,JobPost")
    /// </summary>
    [MaxLength(512)]
    public string? DependsOnEntities { get; set; }

    /// <summary>
    /// Whether this entity is enabled for syncing
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Notes for administrators
    /// </summary>
    [MaxLength(1024)]
    public string? Notes { get; set; }
}
