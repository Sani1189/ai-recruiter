using DataSyncService.Models;
using System.Text.Json;

namespace DataSyncService.Examples;

/// <summary>
/// Examples demonstrating FK relationship handling and GUID consistency
/// </summary>
public class ForeignKeyExamples
{
    /// <summary>
    /// Example: Syncing entities with FK relationships in the correct order
    /// </summary>
    public async Task SyncRelatedEntitiesExample(
        string candidateId,
        string jobPostId,
        string applicationId,
        string sourceRegion,
        Func<SyncMessage, Task> sendSyncMessage)
    {
        // ? CORRECT: Sync in dependency order (parents before children)
        
        // 1. Sync Candidate (no dependencies)
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "Candidate",
            EntityId = candidateId,  // CRITICAL: Same GUID will be used in all regions
            SourceRegion = sourceRegion,
            IsDeleted = false
        });

        // 2. Sync JobPost (no dependencies)
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "JobPost",
            EntityId = jobPostId,
            SourceRegion = sourceRegion,
            IsDeleted = false
        });

        // 3. Sync JobApplication (depends on Candidate + JobPost)
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "JobApplication",
            EntityId = applicationId,
            SourceRegion = sourceRegion,
            IsDeleted = false
        });

        // The sync service will:
        // - Fetch each entity from source using the exact GUID
        // - Insert/update in target regions using the SAME GUID
        // - FK relationships work because GUIDs match across regions
    }

    /// <summary>
    /// Example: What happens when syncing in WRONG order (still works due to retry)
    /// </summary>
    public async Task OutOfOrderSyncExample(
        string candidateId,
        string applicationId,
        string sourceRegion,
        Func<SyncMessage, Task> sendSyncMessage)
    {
        // ? BAD: Sync child before parent
        
        // 1. Sync JobApplication first
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "JobApplication",
            EntityId = applicationId,
            SourceRegion = sourceRegion,
            IsDeleted = false
        });

        // 2. Sync Candidate later
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "Candidate",
            EntityId = candidateId,
            SourceRegion = sourceRegion,
            IsDeleted = false
        });

        // What happens:
        // Time 0:00 - JobApplication sync starts
        // Time 0:01 - FK constraint fails (Candidate doesn't exist yet)
        // Time 0:01 - Message abandoned, goes back to queue
        // Time 0:05 - Candidate sync completes
        // Time 0:31 - JobApplication retries (30s delay)
        // Time 0:31 - Now succeeds because Candidate exists ?
        
        // This works but wastes resources - better to sync in order!
    }

    /// <summary>
    /// Example: Cascading sync - when parent changes, sync children too
    /// </summary>
    public async Task CascadingSyncExample(
        Guid candidateId,
        string sourceRegion,
        Func<string, Task<List<string>>> getApplicationIds,
        Func<SyncMessage, Task> sendSyncMessage)
    {
        // 1. Candidate was updated
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "Candidate",
            EntityId = candidateId.ToString(),
            SourceRegion = sourceRegion,
            IsDeleted = false
        });

        // 2. Also sync all their applications (they might reference updated fields)
        var applicationIds = await getApplicationIds(candidateId.ToString());
        
        foreach (var appId in applicationIds)
        {
            await sendSyncMessage(new SyncMessage
            {
                SyncEventId = Guid.NewGuid().ToString(),
                EntityType = "JobApplication",
                EntityId = appId,
                SourceRegion = sourceRegion,
                IsDeleted = false
            });
        }
    }

    /// <summary>
    /// Example: Bulk sync maintaining FK relationships
    /// </summary>
    public async Task BulkSyncWithDependenciesExample(
        List<(string Type, string Id)> entities,
        string sourceRegion,
        Func<SyncMessage, Task> sendSyncMessage)
    {
        // Group entities by type and sync in dependency order
        var byType = entities.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.ToList());

        // Define dependency order
        var syncOrder = new[]
        {
            "Candidate",           // No dependencies
            "JobPost",             // No dependencies
            "UserProfile",         // No dependencies
            "JobApplication",      // Depends on: Candidate, JobPost
            "InterviewSession",    // Depends on: JobApplication
            "Comment"              // Depends on: Candidate, JobPost, JobApplication
        };

        foreach (var entityType in syncOrder)
        {
            if (!byType.TryGetValue(entityType, out var entitiesOfType))
                continue;

            // Sync all entities of this type
            foreach (var (_, id) in entitiesOfType)
            {
                await sendSyncMessage(new SyncMessage
                {
                    SyncEventId = Guid.NewGuid().ToString(),
                    EntityType = entityType,
                    EntityId = id,
                    SourceRegion = sourceRegion,
                    IsDeleted = false
                });
            }

            // Optional: Add delay to ensure this batch completes before next
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>
    /// Example: Deletion with FK constraints
    /// </summary>
    public async Task DeleteWithDependenciesExample(
        string candidateId,
        string sourceRegion,
        Func<string, Task<List<string>>> getApplicationIds,
        Func<SyncMessage, Task> sendSyncMessage)
    {
        // When deleting a candidate, must delete dependent entities first
        // Otherwise FK constraint prevents deletion

        // 1. Get all applications for this candidate
        var applicationIds = await getApplicationIds(candidateId);

        // 2. Delete applications FIRST (children before parent)
        foreach (var appId in applicationIds)
        {
            await sendSyncMessage(new SyncMessage
            {
                SyncEventId = Guid.NewGuid().ToString(),
                EntityType = "JobApplication",
                EntityId = appId,
                SourceRegion = sourceRegion,
                IsDeleted = true
            });
        }

        // 3. Now delete candidate (parent after children)
        await sendSyncMessage(new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "Candidate",
            EntityId = candidateId,
            SourceRegion = sourceRegion,
            IsDeleted = true
        });

        // Note: Deletion order is REVERSE of creation order
    }

    /// <summary>
    /// Example: Monitoring FK retry metrics
    /// </summary>
    public class FKRetryMetrics
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public DateTimeOffset FirstAttempt { get; set; }
        public DateTimeOffset LastAttempt { get; set; }
        public string? LastError { get; set; }
        public bool Success { get; set; }

        public TimeSpan TotalRetryDuration => LastAttempt - FirstAttempt;

        public override string ToString()
        {
            if (Success)
                return $"{EntityType} {EntityId} succeeded after {RetryCount} retries in {TotalRetryDuration.TotalSeconds:F1}s";
            else
                return $"{EntityType} {EntityId} FAILED after {RetryCount} retries. Last error: {LastError}";
        }
    }
}

/// <summary>
/// Helper: Dependency graph for entity types
/// Use this to determine sync order
/// </summary>
public class EntityDependencyGraph
{
    private readonly Dictionary<string, List<string>> _dependencies = new()
    {
        ["Candidate"] = new List<string>(),                    // No dependencies
        ["JobPost"] = new List<string>(),                      // No dependencies
        ["UserProfile"] = new List<string>(),                  // No dependencies
        ["File"] = new List<string>(),                         // No dependencies
        ["JobApplication"] = new List<string> { "Candidate", "JobPost" },
        ["InterviewSession"] = new List<string> { "JobApplication" },
        ["Comment"] = new List<string> { "Candidate", "JobPost", "JobApplication" },
        ["JobPostStep"] = new List<string> { "JobPost" },
        ["JobPostStepAssignment"] = new List<string> { "JobApplication", "JobPostStep" }
    };

    /// <summary>
    /// Get sync order using topological sort
    /// </summary>
    public List<string> GetSyncOrder()
    {
        var order = new List<string>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var entityType in _dependencies.Keys)
        {
            Visit(entityType, visited, visiting, order);
        }

        return order;
    }

    private void Visit(string entityType, HashSet<string> visited, HashSet<string> visiting, List<string> order)
    {
        if (visited.Contains(entityType))
            return;

        if (visiting.Contains(entityType))
            throw new InvalidOperationException($"Circular dependency detected involving {entityType}");

        visiting.Add(entityType);

        foreach (var dependency in _dependencies[entityType])
        {
            Visit(dependency, visited, visiting, order);
        }

        visiting.Remove(entityType);
        visited.Add(entityType);
        order.Add(entityType);
    }

    /// <summary>
    /// Get entities that must be synced before this one
    /// </summary>
    public List<string> GetDependencies(string entityType)
    {
        return _dependencies.TryGetValue(entityType, out var deps) ? deps : new List<string>();
    }
}
