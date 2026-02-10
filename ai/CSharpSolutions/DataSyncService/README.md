# DataSyncService - Multi-Region Data Synchronization

## Overview

This Azure Function handles data synchronization across multiple regions (EU, US, IN, EU-MAIN) while respecting GDPR requirements.

## Architecture

### Configuration Split

| Level | Where | What |
|-------|-------|------|
| **Entity-Type** | `EntitySyncConfiguration` table | SyncScope, DataClassification, LegalBasis |
| **Per-Row** | `GdprSyncBaseDbModel` properties | DataResidency, IsSanitized, LastSyncedAt |

### Components

1. **SyncCentral** - Azure Function with Service Bus trigger
2. **SyncService** - Core synchronization logic
3. **SyncMessage** - Message format for sync requests
4. **RegionConfiguration** - Database connection management
5. **EntitySyncConfiguration** - Per-entity-type sync rules

### Sync Rules

The system follows these GDPR-based sync rules based on the `SyncScope` configured per entity type in `EntitySyncConfiguration`:

- **GlobalSanitized**: Syncs to all regions **ONLY IF**:
  - Entity config has `RequiresSanitizationForGlobalSync = false`, OR
  - Row has `IsSanitized = true`, OR
  - Entity config allows override AND row has `SanitizationOverrideConsentAt` value
  - ?? If none of these conditions are met, sync is **blocked**
  
- **EUOnly**: Only syncs between EU and EU-MAIN

- **ScopedByExposure**: Syncs based on job post country exposure (currently defaults to EU-MAIN)

- **US/IN to EU-MAIN**: All data from US and IN regions always syncs to EU-MAIN (central aggregation)

**Note:** Enum values are stored as strings in the database (e.g., "GlobalSanitized", "EUOnly", "ScopedByExposure")

**?? Important**: See [GLOBALSANITIZED_VALIDATION.md](GLOBALSANITIZED_VALIDATION.md) for details on GlobalSanitized validation requirements and GDPR compliance.

## Architecture Philosophy

**Two-Level Configuration:**
1. **EntitySyncConfiguration table** - Defines rules per entity type (Candidate, JobPost, etc.)
2. **GdprSyncBaseDbModel** - Per-row data (residency, sanitization status)

**Simplified Message Design:**
The message simply says "this entity changed in this region" - the sync service then:
1. Fetches entity-type configuration from `EntitySyncConfiguration`
2. Fetches per-row data from the entity table
3. Determines target regions based on `SyncScope` and other metadata
4. Syncs the entity to those regions

This ensures sync rules are always applied consistently based on the current state of the data.

### Message Format

Send messages to the `sync-central` Service Bus queue in this format:

```json
{
  "syncEventId": "unique-guid-or-id",
  "entityType": "Candidate",
  "entityId": "123",
  "sourceRegion": "EU",
  "changeTimestamp": "2024-01-15T10:30:00Z",
  "tableName": "Candidates",
  "isDeleted": false
}
```

**Required fields:**
- `syncEventId` - Unique identifier for idempotency
- `entityType` - Entity type name (e.g., "Candidate", "JobPost")
- `entityId` - Primary key of the entity (Guid as string)
- `sourceRegion` - Region where change originated (used to fetch the entity)

**Optional fields:**
- `tableName` - Override default table name (if different from EntityType)
- `isDeleted` - True if entity was deleted (default: false). When true, the entity can't be fetched, so default deletion rules apply
- `changeTimestamp` - When the change occurred

**Key Points:**
- No `OperationType` - we always fetch and upsert (unless `isDeleted=true`)
- No `TargetRegions` - determined by fetching the entity and reading its GDPR metadata
- No `EntityData` - fetched from source region on-demand

## Configuration

### local.settings.json (Development)

```json
{
  "Values": {
    "ServiceBusConnection": "<your-service-bus-connection-string>"
  },
  "RegionConnections": {
    "EUConnectionString": "Server=...",
    "EUMainConnectionString": "Server=...",
    "USConnectionString": "Server=...",
    "INConnectionString": "Server=..."
  }
}
```

### Azure Configuration (Production)

Set these Application Settings:

**Service Bus:**
- `ServiceBusConnection` - Connection string to Service Bus namespace

**Database Connections:**
- `RegionConnections__EUConnectionString`
- `RegionConnections__EUMainConnectionString`
- `RegionConnections__USConnectionString`
- `RegionConnections__INConnectionString`

## Features

### Idempotency

Each sync operation uses `SyncEventId` to prevent duplicate processing. The system checks `LastSyncEventId` on the target entity before applying changes.

### Error Handling

- **Validation Errors**: Dead-lettered immediately
- **Processing Errors**: Retried up to 3 times, then dead-lettered
- **JSON Errors**: Dead-lettered immediately

### Logging

Comprehensive logging at each step:
- Message reception and deserialization
- Entity data fetch from source region
- GDPR metadata evaluation and target region determination
- Sync operations per region
- Errors and warnings

## Usage Example

### From Your API

When you create/update/delete an entity that inherits from `GdprSyncBaseDbModel`:

```csharp
// After saving to database (insert or update)
var syncMessage = new SyncMessage
{
    SyncEventId = Guid.NewGuid().ToString(),
    EntityType = nameof(Candidate),
    EntityId = candidate.Id.ToString(),
    SourceRegion = "EU", // Current region
    ChangeTimestamp = DateTimeOffset.UtcNow,
    IsDeleted = false
};

// Send to Service Bus
await serviceBusClient.SendMessageAsync(new ServiceBusMessage(
    JsonSerializer.Serialize(syncMessage))
{
    ContentType = "application/json"
});

// For deletions - IMPORTANT: Send message BEFORE deleting from database
// so the sync service can fetch the entity's GDPR metadata
var deleteSyncMessage = new SyncMessage
{
    SyncEventId = Guid.NewGuid().ToString(),
    EntityType = nameof(Candidate),
    EntityId = candidate.Id.ToString(),
    SourceRegion = "EU",
    ChangeTimestamp = DateTimeOffset.UtcNow,
    IsDeleted = false // Still false because entity still exists
};

await serviceBusClient.SendMessageAsync(...);

// Now delete from database
await dbContext.Candidates.Remove(candidate);
await dbContext.SaveChangesAsync();
```

**Note on Deletions:** For best results, send the sync message BEFORE deleting from the database. This allows the sync service to fetch the entity's GDPR metadata and determine which regions to delete from. If you send after deletion with `IsDeleted=true`, the service will use default deletion rules.

## ?? CRITICAL: Foreign Keys & GUID Consistency

**READ THIS FIRST**: [FK_AND_GUID_HANDLING.md](FK_AND_GUID_HANDLING.md)

### Key Points

1. **GUID Preservation**: Entity IDs MUST be identical across all regions
   - When syncing `Candidate` with ID `abc-123`, it gets ID `abc-123` in ALL regions
   - Never generate new IDs in target regions - foreign keys will break!

2. **Foreign Key Dependencies**: Entities must be synced in dependency order
   - Sync `Candidate` before `JobApplication` (which references it)
   - Current solution: Retry with exponential backoff if FK constraint fails
   - Messages retry automatically up to 5 times with increasing delays

3. **Best Practice**: Send sync messages in dependency order from your API
   ```csharp
   // ? Good
   await SendSync(candidate);    // No dependencies
   await SendSync(jobPost);      // No dependencies  
   await SendSync(application);  // Depends on both
   ```

See [FK_AND_GUID_HANDLING.md](FK_AND_GUID_HANDLING.md) for complete details, troubleshooting, and future improvements.

## TODO / Future Enhancements

1. **Dynamic Entity Mapping**: Currently placeholder - needs implementation to map fetched data to SQL columns dynamically for upsert **with explicit GUID preservation**
2. **Dependency Graph Sync**: Implement topological ordering of entity syncs based on FK relationships
3. **Conflict Resolution**: Handle concurrent updates from multiple regions
4. **Deletion Metadata Cache**: Cache GDPR metadata before deletions to handle post-delete sync messages
5. **Monitoring Dashboard**: Track sync status, lag, errors per region, FK retry counts
6. **Dead Letter Processing**: Separate function to handle and retry failed messages
7. **Schema Version Management**: Handle entity schema evolution across regions
8. **Performance Optimization**: Batch operations, connection pooling, bulk fetch
9. **ScopedByExposure Logic**: Implement job post country exposure checking

## Development

### Prerequisites

- .NET 10.0 SDK
- Azure Functions Core Tools
- Azure Service Bus namespace
- SQL Server databases for each region

### Running Locally

1. Update `local.settings.json` with your connection strings
2. Start Azure Storage Emulator (for local development)
3. Run: `func start` or F5 in Visual Studio

### Testing

Send test messages to your Service Bus queue using Azure Portal or SDK:

```bash
# Using Azure CLI
az servicebus topic message send \
  --resource-group <rg> \
  --namespace-name <namespace> \
  --name sync-central \
  --body '{"syncEventId":"test-1","operationType":1,"entityType":"Candidate","entityId":"123","sourceRegion":"EU"}'
```
