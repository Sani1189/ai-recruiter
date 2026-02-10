using Azure.Messaging.ServiceBus;
using Recruiter.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// Service for sending sync messages to Azure Service Bus queue
/// </summary>
public class SyncQueueService : ISyncQueueService, IAsyncDisposable
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<SyncQueueService> _logger;
    private readonly string _queueName;

    public SyncQueueService(
        IConfiguration configuration,
        ILogger<SyncQueueService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration["ServiceBus:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ServiceBus:ConnectionString is not configured in appsettings.json");
        }

        _queueName = configuration["ServiceBus:QueueName"] ?? "syncing-queue";
        
        _serviceBusClient = new ServiceBusClient(connectionString);
        _sender = _serviceBusClient.CreateSender(_queueName);
        
        _logger.LogInformation("SyncQueueService initialized for queue: {QueueName}", _queueName);
    }

    /// <summary>
    /// Sends a sync message to the queue
    /// </summary>
    public async Task SendSyncMessageAsync(SyncMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage)
            {
                MessageId = Guid.NewGuid().ToString(),
                Subject = message.EntityType,
                ApplicationProperties =
                {
                    ["EntityType"] = message.EntityType,
                    ["EntityId"] = message.EntityId,
                    ["SourceRegion"] = message.SourceRegion,
                    ["IsDeleted"] = message.IsDeleted
                }
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
            
            _logger.LogInformation(
                "Sent sync message to queue: {EntityType} {EntityId} from {SourceRegion} (Deleted={IsDeleted})",
                message.EntityType,
                message.EntityId,
                message.SourceRegion,
                message.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send sync message to queue: {EntityType} {EntityId}",
                message.EntityType,
                message.EntityId);
            throw;
        }
    }

    /// <summary>
    /// Sends multiple sync messages to the queue as a batch
    /// </summary>
    public async Task SendSyncMessagesAsync(IEnumerable<SyncMessage> messages, CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        if (!messageList.Any())
        {
            return;
        }

        ServiceBusMessageBatch? messageBatch = null;
        
        try
        {
            messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);
            int sentCount = 0;

            foreach (var message in messageList)
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(jsonMessage)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Subject = message.EntityType,
                    ApplicationProperties =
                    {
                        ["EntityType"] = message.EntityType,
                        ["EntityId"] = message.EntityId,
                        ["SourceRegion"] = message.SourceRegion,
                        ["IsDeleted"] = message.IsDeleted
                    }
                };

                if (!messageBatch.TryAddMessage(serviceBusMessage))
                {
                    // If batch is full, send it and create a new batch
                    await _sender.SendMessagesAsync(messageBatch, cancellationToken);
                    sentCount += messageBatch.Count;
                    
                    // Dispose old batch and create new one
                    messageBatch.Dispose();
                    messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);
                    
                    // Try adding the message again to the new batch
                    if (!messageBatch.TryAddMessage(serviceBusMessage))
                    {
                        _logger.LogError("Message too large to fit in batch: {EntityType} {EntityId}", 
                            message.EntityType, message.EntityId);
                        throw new InvalidOperationException($"Message too large for batch: {message.EntityType} {message.EntityId}");
                    }
                }
            }

            // Send remaining messages in the batch
            if (messageBatch.Count > 0)
            {
                await _sender.SendMessagesAsync(messageBatch, cancellationToken);
                sentCount += messageBatch.Count;
            }

            _logger.LogInformation("Sent {Count} sync messages to queue", sentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch of {Count} sync messages to queue", messageList.Count);
            throw;
        }
        finally
        {
            messageBatch?.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender != null)
        {
            await _sender.DisposeAsync();
        }
        
        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync();
        }
    }
}
