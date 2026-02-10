using Azure.Messaging.ServiceBus;
using DataSyncService.Models;
using System.Text.Json;

namespace DataSyncService.Examples;

/// <summary>
/// Examples of how to send sync messages from your API
/// </summary>
public class SyncMessageExamples
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _sender;

    public SyncMessageExamples(string serviceBusConnectionString)
    {
        _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
        _sender = _serviceBusClient.CreateSender("sync-central");
    }

    /// <summary>
    /// Send a sync message when a Candidate is created or updated.
    /// The sync service will fetch the candidate from the source region and determine
    /// target regions based on the entity's GDPR metadata.
    /// </summary>
    public async Task SendCandidateSyncAsync(string candidateId, string sourceRegion)
    {
        var syncMessage = new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = "Candidate",
            EntityId = candidateId,
            SourceRegion = sourceRegion,
            ChangeTimestamp = DateTimeOffset.UtcNow,
            TableName = "Candidates", // Optional: override if needed
            IsDeleted = false
        };

        await SendSyncMessageAsync(syncMessage);
    }

    /// <summary>
    /// Send a sync message when an entity is deleted.
    /// For deletions, we must mark IsDeleted=true because the entity no longer
    /// exists in the source database to fetch its metadata.
    /// </summary>
    public async Task SendDeleteSyncAsync(string entityType, string entityId, string sourceRegion)
    {
        var syncMessage = new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId,
            SourceRegion = sourceRegion,
            ChangeTimestamp = DateTimeOffset.UtcNow,
            IsDeleted = true // Critical: must be true for deletions
        };

        await SendSyncMessageAsync(syncMessage);
    }

    /// <summary>
    /// Generic sync trigger - just tells the sync service "this entity changed"
    /// </summary>
    public async Task SendEntityChangedAsync(string entityType, string entityId, string sourceRegion, bool isDeleted = false)
    {
        var syncMessage = new SyncMessage
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId,
            SourceRegion = sourceRegion,
            ChangeTimestamp = DateTimeOffset.UtcNow,
            IsDeleted = isDeleted
        };

        await SendSyncMessageAsync(syncMessage);
    }

    /// <summary>
    /// Helper to send any sync message
    /// </summary>
    private async Task SendSyncMessageAsync(SyncMessage syncMessage)
    {
        var messageBody = JsonSerializer.Serialize(syncMessage);
        var serviceBusMessage = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            MessageId = syncMessage.SyncEventId,
            Subject = syncMessage.EntityType
        };

        await _sender.SendMessageAsync(serviceBusMessage);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }
}

/// <summary>
/// Example of how to integrate sync in your repository layer.
/// The key principle: just tell the sync service "this entity changed in this region"
/// and it will handle fetching the data and determining targets.
/// </summary>
public class ExampleRepositoryWithSync
{
    private readonly SyncMessageExamples _syncExamples;

    public ExampleRepositoryWithSync(SyncMessageExamples syncExamples)
    {
        _syncExamples = syncExamples;
    }

    public async Task CreateOrUpdateCandidateAsync(string candidateId, object candidate, string currentRegion)
    {
        // 1. Save to local database
        // await _dbContext.Candidates.Update(candidate);
        // await _dbContext.SaveChangesAsync();

        // 2. Notify sync service about the change
        // The sync service will:
        //    - Fetch the candidate from this region
        //    - Read its GdprSyncBaseDbModel properties
        //    - Determine target regions based on SyncScope
        //    - Sync to those regions
        await _syncExamples.SendCandidateSyncAsync(candidateId, currentRegion);
    }

    public async Task DeleteCandidateAsync(string candidateId, string currentRegion)
    {
        // IMPORTANT: Send the deletion message BEFORE actually deleting!
        // The sync service needs to fetch the entity's GDPR metadata to know
        // which regions to delete from.
        
        // Option 1: Send message before delete (with IsDeleted=false)
        // await _syncExamples.SendEntityChangedAsync("Candidate", candidateId, currentRegion, isDeleted: false);
        
        // Then delete from database
        // await _dbContext.Candidates.Remove(candidate);
        // await _dbContext.SaveChangesAsync();

        // Option 2: If you've already deleted, send with IsDeleted=true
        // (the sync service will use default rules to determine targets)
        await _syncExamples.SendDeleteSyncAsync("Candidate", candidateId, currentRegion);
    }
}
