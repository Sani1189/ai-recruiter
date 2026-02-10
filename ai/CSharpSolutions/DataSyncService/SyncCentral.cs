using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using DataSyncService.Exceptions;
using DataSyncService.Models;
using DataSyncService.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DataSyncService;

public class SyncCentral
{
    private readonly ILogger<SyncCentral> _logger;
    private readonly ISyncService _syncService;

    public SyncCentral(
        ILogger<SyncCentral> logger,
        ISyncService syncService)
    {
        _logger = logger;
        _syncService = syncService;
    }

    [Function(nameof(SyncCentral))]
    public async Task Run(
        [ServiceBusTrigger("syncing-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        LogMessageReceived(message);
        try
        {
            var syncMessage = DeserializeSyncMessage(message);

            if (!ValidateSyncMessage(syncMessage))
            {
                await DeadLetterInvalidMessage(message, messageActions, "ValidationError", "Missing required fields");
                return;
            }

            LogProcessingStart(syncMessage);

            await _syncService.ProcessSyncMessageAsync(syncMessage);
            await messageActions.CompleteMessageAsync(message);

            LogProcessingSuccess(syncMessage);
        }
        catch (ForeignKeyConstraintException ex)
        {
            await HandleForeignKeyConstraintFailure(message, messageActions, ex);
            throw;
        }
        catch (JsonException ex)
        {
            await HandleDeserializationFailure(message, messageActions, ex);
            throw;
        }
        catch (Exception ex)
        {
            await HandleGeneralFailure(message, messageActions, ex);
            throw;
        }
    }

    private SyncMessage? DeserializeSyncMessage(ServiceBusReceivedMessage message)
    {
        try
        {
            return JsonSerializer.Deserialize<SyncMessage>(
                message.Body.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize sync message: {MessageId}", message.MessageId);
            return null;
        }
    }

    private bool ValidateSyncMessage(SyncMessage syncMessage)
    {
        var isValid = !string.IsNullOrEmpty(syncMessage.EntityType) &&
                      !string.IsNullOrEmpty(syncMessage.EntityId) &&
                      !string.IsNullOrEmpty(syncMessage.SourceRegion);

        if (!isValid)
        {
            _logger.LogError(
                "Invalid sync message: EntityType={EntityType}, EntityId={EntityId}, SourceRegion={SourceRegion}",
                syncMessage.EntityType,
                syncMessage.EntityId,
                syncMessage.SourceRegion);
        }

        return isValid;
    }

    private async Task DeadLetterInvalidMessage(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        string reason,
        string description)
    {
        _logger.LogError("Dead-lettering message {MessageId}: {Reason}", message.MessageId, reason);
        await messageActions.DeadLetterMessageAsync(message, deadLetterReason: reason, deadLetterErrorDescription: description);
    }

    private async Task HandleForeignKeyConstraintFailure(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        ForeignKeyConstraintException ex)
    {
        var delaySeconds = Math.Pow(2, message.DeliveryCount) * 30;

        _logger.LogWarning(
            ex,
            "FK constraint violation (Delivery #{DeliveryCount}). Will retry after {DelaySeconds}s",
            message.DeliveryCount,
            delaySeconds);

        if (message.DeliveryCount >= 5)
        {
            _logger.LogError("FK constraint persists after {DeliveryCount} retries. Dead-lettering message.", message.DeliveryCount);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "ForeignKeyConstraintPersistent", deadLetterErrorDescription: ex.Message);
        }
        else
        {
            await messageActions.AbandonMessageAsync(message);
        }
    }

    private async Task HandleDeserializationFailure(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        JsonException ex)
    {
        _logger.LogError(ex, "JSON parsing error for message: {MessageId}", message.MessageId);
        await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "JsonParsingError", deadLetterErrorDescription: ex.Message);
    }

    private async Task HandleGeneralFailure(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        Exception ex)
    {
        _logger.LogError(ex, "Error processing sync message: {MessageId}", message.MessageId);

        if (message.DeliveryCount >= 3)
        {
            _logger.LogError("Message exceeded max delivery count. Dead-lettering: {MessageId}", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "ProcessingError", deadLetterErrorDescription: ex.Message);
        }
        else
        {
            await messageActions.AbandonMessageAsync(message);
        }
    }

    private void LogMessageReceived(ServiceBusReceivedMessage message) =>
        _logger.LogInformation("Received: MessageId={MessageId}, DeliveryCount={DeliveryCount}", message.MessageId, message.DeliveryCount);

    private void LogProcessingStart(SyncMessage syncMessage) =>
        _logger.LogInformation("Processing: {Entity} {Id} from {Source} (Deleted={IsDeleted})", syncMessage.EntityType, syncMessage.EntityId, syncMessage.SourceRegion, syncMessage.IsDeleted);

    private void LogProcessingSuccess(SyncMessage syncMessage) =>
        _logger.LogInformation("Completed: {Entity} {Id} (EventId={EventId})", syncMessage.EntityType, syncMessage.EntityId, syncMessage.SyncEventId);
}