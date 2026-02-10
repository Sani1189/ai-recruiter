# Summary: Foreign Key & GUID Handling Implementation

## ? Changes Made

### 1. GUID Preservation (CRITICAL)

**Updated `UpsertEntityInRegionAsync`**:
- Validates that `Id` field exists in entity data
- Logs the source ID to make GUID preservation visible
- Added TODO comments emphasizing:
  - MUST use exact GUID from source
  - Never generate new IDs in target regions
  - Use explicit ID in INSERT/MERGE statements

### 2. Foreign Key Constraint Handling

**Added `ForeignKeyConstraintException`**:
- Custom exception for FK violations
- Signals that the failure is retriable
- Indicates referenced entity hasn't been synced yet

**Updated `UpsertEntityInRegionAsync`**:
- Detects FK constraint violations (SQL Server error 547)
- Throws `ForeignKeyConstraintException` to trigger retry
- Logs warning with context about the failure

**Updated `SyncCentral`**:
- Special handling for `ForeignKeyConstraintException`
- Implements exponential backoff (30s ? 60s ? 120s ? 240s)
- Abandons message to put it back on queue
- Dead-letters after 5 retries if FK still fails

### 3. Documentation

**Created `FK_AND_GUID_HANDLING.md`**:
- Comprehensive guide on GUID preservation
- FK dependency handling strategies
- Current retry-based solution explained
- Future improvements outlined
- Best practices for API integration
- SQL MERGE examples showing explicit ID usage
- Troubleshooting guide

**Created `Examples/ForeignKeyExamples.cs`**:
- Correct vs incorrect sync order examples
- Cascading sync patterns
- Bulk sync with dependency ordering
- Deletion with FK constraints
- `EntityDependencyGraph` helper class
- Monitoring metrics example

**Updated `README.md`**:
- Prominent warning about FK and GUID handling
- Link to detailed FK documentation
- Updated TODO list with GUID-aware requirements

## How It Works

### Scenario: JobApplication references Candidate

```
Time 0:00 - Candidate modified in EU ? message sent
Time 0:01 - JobApplication modified in EU ? message sent
Time 0:02 - JobApplication processed first (wrong order)
Time 0:03 - FK constraint fails (Candidate not in target yet)
Time 0:03 - ForeignKeyConstraintException thrown
Time 0:03 - Message abandoned ? back to queue
Time 0:05 - Candidate processed ? synced to target
Time 0:33 - JobApplication retries (30s delay)
Time 0:33 - Candidate exists ? sync succeeds ?
```

### GUID Consistency

```csharp
// In source EU database:
Candidate.Id = "abc-123-def-456"
JobApplication.CandidateId = "abc-123-def-456"

// After sync to US database:
Candidate.Id = "abc-123-def-456"  // ? Same GUID!
JobApplication.CandidateId = "abc-123-def-456"  // ? FK works!
```

## Current Limitations

1. **No Guaranteed Order**: Messages processed in arbitrary order
2. **Retry Overhead**: Failed FK attempts waste resources  
3. **No Dependency Graph**: System doesn't know entity relationships
4. **5 Retry Limit**: If parent never syncs, child eventually dead-lettered

## Next Steps (TODO)

When implementing the dynamic upsert logic:

### ? Critical (Must Have)

1. **Preserve GUIDs**: Extract `entityData["Id"]` and use in INSERT
   ```sql
   INSERT INTO Table (Id, ...) VALUES (@SourceId, ...)
   ```

2. **Use MERGE**: Ensures idempotent upsert with explicit ID
   ```sql
   MERGE INTO Table AS target
   USING (...) AS source ON target.Id = source.Id
   WHEN MATCHED THEN UPDATE ...
   WHEN NOT MATCHED THEN INSERT (Id, ...) VALUES (source.Id, ...)
   ```

### ?? Important (Should Have)

3. **Dependency Graph**: Define entity relationships in config
4. **Topological Sync**: Use `EntityDependencyGraph` for ordering
5. **FK Validation**: Check if referenced entities exist before sync
6. **Smart Queuing**: Defer messages if dependencies not met

### ?? Nice to Have

7. **Batch Sync**: Group related entities, sync in dependency order
8. **Metrics Dashboard**: Track FK retry counts, success rates
9. **Circular Dependency Detection**: Prevent infinite retry loops
10. **Configurable Retry Policy**: Per-entity retry limits

## Testing Recommendations

1. **Test FK Retry Flow**:
   - Sync child before parent
   - Verify retry with exponential backoff
   - Confirm eventual success

2. **Test GUID Consistency**:
   - Sync entity with ID `X` to multiple regions
   - Verify ID is `X` in ALL regions
   - Verify FK relationships work

3. **Test Dead Letter**:
   - Sync entity referencing non-existent parent
   - Verify dead-letter after 5 retries
   - Process dead letter queue

4. **Test Deletion Order**:
   - Delete parent with children
   - Verify children deleted first
   - Verify FK constraints not violated

## Monitoring

Key metrics to track:
- FK retry count per entity type
- Average retry duration
- Dead letter rate for FK violations
- GUID consistency violations (should be zero!)

## Risk Mitigation

?? **Biggest Risk**: Implementing upsert without GUID preservation
- **Impact**: ALL foreign keys break across regions
- **Mitigation**: Multiple validation points in code, extensive testing
- **Detection**: Monitor FK constraint violation rates

? **Current State**: Foundation in place, GUID preservation emphasized in code comments

