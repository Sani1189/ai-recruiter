using DataSyncService.Models;

namespace DataSyncService.Services;

public interface ISyncService
{
    /// <summary>
    /// Process a sync message and replicate data to target regions
    /// </summary>
    Task ProcessSyncMessageAsync(SyncMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determine which regions should receive this sync based on GDPR rules
    /// </summary>
    Task<List<string>> DetermineTargetRegionsAsync(SyncMessage message, CancellationToken cancellationToken = default);
}
