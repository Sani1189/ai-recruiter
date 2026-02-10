using Recruiter.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Base database model with (Name, Version) as composite primary key.
/// Inherits GDPR sync properties from GdprSyncBaseDbModel.
/// Used by: JobPost, JobPostStep, InterviewConfiguration, Prompt, etc.
/// </summary>
[NotMapped]
public abstract class VersionedBaseDbModel : GdprSyncBaseDbModel
{
    /// <summary>
    /// Name part of the composite primary key
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Version part of the composite primary key
    /// </summary>
    public int Version { get; set; }
    
    #region Audit Properties
    
    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// User who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
    
    #endregion
}
