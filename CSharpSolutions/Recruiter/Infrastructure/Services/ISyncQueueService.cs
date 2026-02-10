using Recruiter.Infrastructure.Models;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// Service for sending sync messages to the Azure Service Bus queue
/// </summary>
public interface ISyncQueueService
{
    /// <summary>
    /// Sends a sync message to the queue
    /// </summary>
    Task SendSyncMessageAsync(SyncMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple sync messages to the queue
    /// </summary>
    Task SendSyncMessagesAsync(IEnumerable<SyncMessage> messages, CancellationToken cancellationToken = default);
}
