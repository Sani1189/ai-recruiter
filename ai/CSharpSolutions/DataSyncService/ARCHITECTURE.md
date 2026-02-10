# DataSync Architecture Refactoring Summary

## Overview

Multi-region data synchronization with GDPR compliance. Configuration is split between:
- **EntitySyncConfiguration table**: Per-entity-type settings (SyncScope, DataClassification, LegalBasis)
- **GdprSyncBaseDbModel**: Per-row settings (DataResidency, IsSanitized, etc.)

## Problem Statement

The original design had sync logic split between the message sender and the sync service:
- **OperationType** in message - redundant since we can always upsert
- **TargetRegions** in message - should be determined by the entity's GDPR metadata, not by the sender
- **EntityData** in message - increases message size and duplicates data already in the source database

## Architecture: Entity-Level vs Row-Level Configuration

### EntitySyncConfiguration Table (Per-Entity-Type)

Defines sync rules for each entity type:

| Column | Description |
|--------|-------------|
| `EntityTypeName` | "Candidate", "JobPost", "Country", etc. |
| `DataClassification` | NonPersonal, Personal, Sensitive |
| `SyncScope` | GlobalSanitized, EUOnly, ScopedByExposure |
| `LegalBasis` | None, Consent, Contract |
| `RequiresSanitizationForGlobalSync` | Whether sanitization is required for global sync |
| `AllowSanitizationOverrideConsent` | Whether users can override sanitization requirement |
| `DependsOnEntities` | FK dependencies (e.g., "Candidate,JobPost") |

### GdprSyncBaseDbModel (Per-Row)

Properties that vary per row:

| Property | Description |
|----------|-------------|
| `DataResidency` | EU or NonEU - where this row must reside |
| `DataOriginRegion` | EU, US, IN - where row was created |
| `IsSanitized` | Whether this row has been sanitized |
| `SanitizationOverrideConsentAt` | User consent timestamp |
| `LastSyncedAt`, `LastSyncEventId` | Sync status |

### Why This Design?

- **DataClassification** is constant per entity type: Candidate = Personal, Country = NonPersonal
- **SyncScope** is constant per entity type: Candidate = EUOnly, Country = GlobalSanitized
- **DataResidency** varies per row: One candidate may be EU-resident, another NonEU
- **IsSanitized** varies per row: Some rows may be sanitized for global sync

## New Architecture

### Simplified Message Design

The message now simply says: **"This entity changed in this region"**

```csharp
{
    "syncEventId": "guid",
    "entityType": "Candidate",
    "entityId": "123",
    "sourceRegion": "EU",
    "changeTimestamp": "2024-01-15T10:30:00Z",
    "isDeleted": false,  // Only true for deletions
    "tableName": "Candidates"  // Optional override
}
```

### Sync Flow

1. **Message Received** ? SyncCentral function picks up message from Service Bus
2. **Fetch Entity** ? SyncService fetches the entity from the source region database
3. **Read GDPR Metadata** ? Reads `GdprSyncBaseDbModel` properties (SyncScope, DataResidency, etc.) stored as string enums in the database
4. **Determine Targets** ? Applies GDPR rules to determine which regions should receive the data:
   - `SyncScope.GlobalSanitized` ? All regions (requires `IsSanitized=true` OR `SanitizationOverrideConsentAt` value)
   - `SyncScope.EUOnly` ? Only EU and EU-MAIN
   - `SyncScope.ScopedByExposure` ? EU-MAIN (placeholder for job post exposure logic)
   - Additional rule: US/IN data always goes to EU-MAIN
5. **Sync to Targets** ? Upserts the entity in each target region
6. **Idempotency** ? Uses `LastSyncEventId` to prevent duplicate processing

**Database Storage:** All enum values (SyncScope, DataResidency, DataOriginRegion, DataClassification) are stored as strings in the database (e.g., "GlobalSanitized", "EU", "NonPersonal") and parsed into strongly-typed enums by the sync service.

### Benefits

? **Single Source of Truth**: Sync rules are always applied based on the current state of the entity's GDPR metadata  
? **Smaller Messages**: No need to include full entity data in the message  
? **Flexibility**: Changing sync rules doesn't require updating all message senders  
? **Consistency**: Impossible for sender to override GDPR rules  
? **Simpler API Integration**: Just send "entity changed" - no need to determine targets or include data  

### Deletion Handling

**Best Practice**: Send the sync message **BEFORE** deleting from database
- Allows the sync service to fetch GDPR metadata
- Ensures correct target regions for deletion
- Use `isDeleted=false` in this case

**Fallback**: If already deleted, send with `isDeleted=true`
- Sync service will use default deletion rules
- Less accurate but still functional

### Key Changes

**SyncMessage.cs**:
- Removed: `OperationType`, `TargetRegions`, `EntityData`
- Added: `IsDeleted` (bool to indicate if entity no longer exists)
- Simplified to minimal required fields

**SyncService.cs**:
- New method: `ProcessDeletionAsync()` - handles deleted entities
- New method: `ProcessUpsertAsync()` - fetches and syncs entity data
- New method: `FetchEntityDataAsync()` - retrieves full entity from source
- Updated: `DetermineTargetRegionsAsync()` - now always fetches metadata from source
- Removed: switch/case on OperationType

**Examples/SyncMessageExamples.cs**:
- Simplified examples showing just entity ID and source region
- Added deletion best practices with timing guidance

**README.md**:
- Updated architecture philosophy section
- Simplified message format documentation
- Added deletion handling guidance

## Migration Guide

### Old Code:
```csharp
var syncMessage = new SyncMessage
{
    SyncEventId = Guid.NewGuid().ToString(),
    OperationType = SyncOperationType.Update,
    EntityType = nameof(Candidate),
    EntityId = candidate.Id.ToString(),
    SourceRegion = "EU",
    ChangeTimestamp = DateTimeOffset.UtcNow,
    EntityData = JsonSerializer.Serialize(candidate),  // ? No longer needed
    TargetRegions = new List<string> { "US", "IN" }     // ? No longer supported
};
```

### New Code:
```csharp
var syncMessage = new SyncMessage
{
    SyncEventId = Guid.NewGuid().ToString(),
    EntityType = nameof(Candidate),
    EntityId = candidate.Id.ToString(),
    SourceRegion = "EU",  // ? This is all we need!
    ChangeTimestamp = DateTimeOffset.UtcNow,
    IsDeleted = false
};
```

## Future Enhancements

1. **Dynamic Entity Mapping**: Implement reflection/source generation to map fetched data to SQL columns for upsert
2. **Deletion Metadata Cache**: Cache GDPR metadata before deletions to handle post-delete messages
3. **ScopedByExposure Logic**: Implement job post country exposure checking for granular sync control
4. **Performance**: Batch sync operations, connection pooling
5. **Monitoring**: Dashboard for sync lag, errors, throughput per region
