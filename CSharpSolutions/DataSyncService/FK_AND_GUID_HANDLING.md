# Foreign Key Relationships & GUID Consistency

## ?? CRITICAL: GUID Preservation

**All entity IDs MUST be preserved exactly across regions!**

### Why This Matters

When syncing entities with foreign key relationships:
- `JobApplication.CandidateId` must reference the SAME Candidate GUID in all regions
- `JobApplication.JobPostId` must reference the SAME JobPost GUID in all regions
- If we generate new IDs in target regions, ALL foreign keys break

### Implementation Requirements

When implementing the dynamic upsert logic, you MUST:

1. **Extract the source ID from entityData**:
   ```csharp
   var sourceId = entityData["Id"]; // This is the GUID from source region
   ```

2. **Use explicit ID in INSERT statements**:
   ```sql
   INSERT INTO Candidates (Id, Name, Email, ...)
   VALUES (@SourceId, @Name, @Email, ...)
   -- Never use NEWID() or let database generate the ID!
   ```

3. **Use MERGE with ID matching**:
   ```sql
   MERGE INTO Candidates AS target
   USING (SELECT @SourceId AS Id, @Name AS Name, ...) AS source
   ON target.Id = source.Id
   WHEN MATCHED THEN UPDATE SET Name = source.Name, ...
   WHEN NOT MATCHED THEN INSERT (Id, Name, ...) VALUES (source.Id, source.Name, ...);
   ```

## Foreign Key Dependency Handling

### The Problem

Entities with foreign keys must be synced in the correct order:

```
Candidate (no dependencies)
  ?
JobPost (no dependencies)  
  ?
JobApplication (depends on Candidate + JobPost)
```

If `JobApplication` is synced before `Candidate`, the FK constraint fails.

### Current Solution: Retry with Exponential Backoff

1. **FK Violation Detected**: `SqlException` with error number 547
2. **Throw ForeignKeyConstraintException**: Signal that this is retriable
3. **Abandon Message**: Put it back on the queue
4. **Exponential Backoff**: Service Bus automatically delays retry (30s ? 60s ? 120s ? 240s)
5. **Eventually Succeeds**: Referenced entity is synced, FK constraint passes
6. **Dead Letter After 5 Retries**: If FK still fails, entity truly doesn't exist

### Example Flow

```
Time 0:00  - Candidate modified in EU ? sync message sent
Time 0:01  - JobApplication modified in EU ? sync message sent
Time 0:02  - JobApplication sync starts, but Candidate not synced yet ? FK violation
Time 0:02  - JobApplication message abandoned, goes back to queue
Time 0:32  - JobApplication sync retries (30s delay)
Time 0:32  - Candidate now exists ? JobApplication sync succeeds ?
```

### Limitations of Current Approach

? **No Guaranteed Ordering**: Messages may be processed in any order  
? **Retry Overhead**: Failed attempts waste resources  
? **Potential Deadlocks**: Circular dependencies could cause infinite retries  

### Future Improvements

1. **Dependency Graph**:
   - Define entity dependencies in configuration
   - Sync in topological order
   - Queue dependent entities only after dependencies complete

2. **Smart Queuing**:
   - Inspect entity data for FK references
   - Check if referenced entities exist in target
   - Queue referenced entities first

3. **Batch Sync**:
   - Group related entities together
   - Sync entire object graphs in dependency order
   - Use transactions for atomicity

4. **Sync Orchestrator**:
   - Separate service that manages sync order
   - Builds dependency chains
   - Schedules syncs in correct sequence

## Best Practices for Your API

### 1. Sync in Dependency Order

When modifying multiple related entities, send sync messages in dependency order:

```csharp
// ? Good: Parent first
await syncService.SendSyncMessage(candidate);
await syncService.SendSyncMessage(jobPost);
await syncService.SendSyncMessage(jobApplication); // Depends on both

// ? Bad: Child first
await syncService.SendSyncMessage(jobApplication); // Will retry multiple times
await syncService.SendSyncMessage(candidate);
```

### 2. Use Same Transaction

If possible, send sync messages in the same transaction as the database changes:

```csharp
using var transaction = await dbContext.Database.BeginTransactionAsync();
try
{
    // Save entities
    dbContext.Candidates.Update(candidate);
    await dbContext.SaveChangesAsync();
    
    // Send sync message
    await syncService.SendSyncMessage(candidate);
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 3. Consider Cascading Sync

When a parent entity changes, you might need to sync children too:

```csharp
// Candidate updated ? also sync their applications
await syncService.SendSyncMessage(candidate);

var applications = await dbContext.JobApplications
    .Where(a => a.CandidateId == candidate.Id)
    .ToListAsync();

foreach (var app in applications)
{
    await syncService.SendSyncMessage(app);
}
```

### 4. Delay Dependent Syncs

Add a small delay when syncing dependent entities:

```csharp
await syncService.SendSyncMessage(candidate);
await Task.Delay(1000); // Give candidate time to sync
await syncService.SendSyncMessage(jobApplication);
```

## Monitoring FK Failures

Watch for these log messages:

- **Warning**: `"FK constraint violation"` ? Normal, will retry
- **Error**: `"FK constraint still failing after 5 retries"` ? Investigate! Referenced entity missing
- **Dead Letter**: `"ForeignKeyConstraintPersistent"` ? Manual intervention required

## Troubleshooting

### FK Violation After 5 Retries

1. Check if referenced entity exists in source region
2. Check if referenced entity has `SyncScope` that excludes target region
3. Manually sync referenced entity
4. Resubmit dead-lettered message

### Circular Dependencies

If Entity A references Entity B, and Entity B references Entity A:
1. Break the cycle by making one FK nullable
2. Sync in two phases: first without FK, then update FK
3. Or: Temporarily disable FK constraints during sync (risky!)

## SQL Server MERGE Example (Future Implementation)

```sql
MERGE INTO Candidates AS target
USING (
    SELECT 
        @Id AS Id,
        @Name AS Name,
        @Email AS Email,
        @SyncScope AS SyncScope,
        @LastSyncEventId AS LastSyncEventId,
        @LastSyncedAt AS LastSyncedAt
) AS source
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        Name = source.Name,
        Email = source.Email,
        SyncScope = source.SyncScope,
        LastSyncEventId = source.LastSyncEventId,
        LastSyncedAt = source.LastSyncedAt
WHEN NOT MATCHED THEN
    INSERT (Id, Name, Email, SyncScope, LastSyncEventId, LastSyncedAt)
    VALUES (source.Id, source.Name, source.Email, source.SyncScope, source.LastSyncEventId, source.LastSyncedAt);
```

**Critical**: Notice how `source.Id` is used in the INSERT - never generate a new ID!
