# SyncQueueService Implementation Summary

## Overview
Created a `SyncQueueService` in the Recruiter project to send sync messages to Azure Service Bus queue. This service will be used to trigger cross-region data synchronization.

## Files Created

### 1. ISyncQueueService Interface
**Location:** `Recruiter/Infrastructure/Services/ISyncQueueService.cs`

```csharp
public interface ISyncQueueService
{
    Task SendSyncMessageAsync(SyncMessage message, CancellationToken cancellationToken = default);
    Task SendSyncMessagesAsync(IEnumerable<SyncMessage> messages, CancellationToken cancellationToken = default);
}
```

### 2. SyncQueueService Implementation
**Location:** `Recruiter/Infrastructure/Services/SyncQueueService.cs`

**Features:**
- Sends single or batch sync messages to Azure Service Bus
- Automatic batch handling (creates new batch when full)
- Proper resource disposal with `IAsyncDisposable`
- Comprehensive logging
- Message metadata in `ApplicationProperties` for filtering
- Idempotency support via `MessageId`

**Key Methods:**
- `SendSyncMessageAsync()` - Send a single sync message
- `SendSyncMessagesAsync()` - Send multiple messages in optimized batches
- `DisposeAsync()` - Proper cleanup of Service Bus client resources

### 3. SyncMessage Model
**Location:** `Recruiter/Infrastructure/Models/SyncMessage.cs`

```csharp
public class SyncMessage
{
    public required string SyncEventId { get; set; }
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public required string SourceRegion { get; set; }
    public DateTimeOffset ChangeTimestamp { get; set; }
    public string? TableName { get; set; }
    public bool IsDeleted { get; set; }
}
```

## Configuration

### appsettings.json
```json
{
  "ServiceBus": {
    "ConnectionString": "",
    "QueueName": "syncing-queue"
  }
}
```

### appsettings.Development.json
```json
{
  "ServiceBus": {
    "ConnectionString": "",
    "QueueName": "syncing-queue"
  }
}
```

**Configuration Keys:**
- `ServiceBus:ConnectionString` - Azure Service Bus connection string (empty by default, needs to be set)
- `ServiceBus:QueueName` - Queue name (defaults to "syncing-queue")

## Dependency Injection

**Location:** `Recruiter/WebApi/Infrastructure/ServiceExtension.cs`

```csharp
// Sync queue service for sending sync messages to Service Bus
services.AddSingleton<ISyncQueueService, SyncQueueService>();
```

**Lifetime:** Singleton
- Service Bus client connections are expensive to create
- Thread-safe for concurrent use
- Proper disposal via `IAsyncDisposable`

## NuGet Package Added

**Package:** `Azure.Messaging.ServiceBus` v7.20.1
**Project:** `Recruiter.Infrastructure.csproj`

## Usage Example

```csharp
public class MyService
{
    private readonly ISyncQueueService _syncQueueService;

    public MyService(ISyncQueueService syncQueueService)
    {
        _syncQueueService = syncQueueService;
    }

    public async Task OnEntityChanged(Guid entityId, string entityType)
    {
        var syncMessage = new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId.ToString(),
            SourceRegion = "EU", // Get from configuration
            ChangeTimestamp = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await _syncQueueService.SendSyncMessageAsync(syncMessage);
    }
}
```

## Message Flow

1. **Entity changes** in Recruiter API (Create/Update/Delete)
2. **SyncQueueService** sends message to Service Bus queue
3. **DataSyncService** (Azure Function) picks up message
4. **SyncService** fetches entity data and syncs to target regions

## Message Structure

Messages sent to Service Bus include:

**Body:** JSON-serialized `SyncMessage`

**Application Properties:** (for filtering/routing)
- `EntityType` - e.g., "Candidate", "JobPost"
- `EntityId` - Guid as string
- `SourceRegion` - "EU", "US", "IN"
- `IsDeleted` - true/false

## Next Steps

1. **Set Connection String:**
   - Add Azure Service Bus connection string to configuration
   - Use Azure Key Vault or App Configuration for production

2. **Trigger Sync Messages:**
   - Create interceptor or repository decorator to auto-send sync messages
   - Or manually call `ISyncQueueService` after entity changes

3. **Testing:**
   - Unit test with mocked `ISyncQueueService`
   - Integration test with actual Service Bus (or emulator)

4. **Monitoring:**
   - Add Application Insights telemetry
   - Monitor queue depth and processing times
   - Alert on failed messages

## Error Handling

- Throws exceptions on failure (caller should handle/retry)
- Logs all operations (success and failure)
- Messages too large for batch will throw `InvalidOperationException`
- Configuration missing throws `InvalidOperationException` on startup

## Performance Considerations

- Batch sending is more efficient than individual messages
- Uses `ServiceBusMessageBatch` for optimal throughput
- Singleton lifetime reduces connection overhead
- Async/await throughout for non-blocking IO
