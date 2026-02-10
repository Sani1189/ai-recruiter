namespace Recruiter.Infrastructure.Models;

/// <summary>
/// Message structure for sync requests sent to the Service Bus queue.
/// The message simply indicates "this entity changed in this region" - 
/// the sync service will fetch the entity and determine targets based on GDPR metadata.
/// </summary>
public class SyncMessage
{
    /// <summary>
    /// Unique identifier for this sync operation (idempotency key)
    /// </summary>
    public required string SyncEventId { get; set; }

    /// <summary>
    /// The entity type name (e.g., "Candidate", "JobPost")
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// The primary key of the entity (Guid as string)
    /// </summary>
    public required string EntityId { get; set; }

    /// <summary>
    /// Region where the change originated - used to fetch the source data
    /// </summary>
    public required string SourceRegion { get; set; }

    /// <summary>
    /// Timestamp when the change occurred
    /// </summary>
    public DateTimeOffset ChangeTimestamp { get; set; }

    /// <summary>
    /// Optional: Specific table name override (if different from EntityType)
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// True if this is a deletion event (entity no longer exists in source to fetch metadata)
    /// False/null means the entity exists and should be fetched and synced
    /// </summary>
    public bool IsDeleted { get; set; }
}
