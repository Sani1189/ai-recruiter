using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Base database model with Guid Id as primary key.
/// Inherits GDPR sync properties from GdprSyncBaseDbModel.
/// Provides common audit fields and concurrency control.
/// </summary>
[NotMapped]
public abstract class BaseDbModel : GdprSyncBaseDbModel
{   
    /// <summary>
    /// Unique identifier for the entity (Primary Key)
    /// </summary>
    public Guid Id { get; set; }
    
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
    public byte[] RowVersion { get; set; } = default!;
}
