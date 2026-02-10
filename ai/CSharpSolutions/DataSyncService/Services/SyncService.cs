using DataSyncService.Configuration;
using DataSyncService.Exceptions;
using DataSyncService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruiter.Domain.Enums;
using System.Data;

namespace DataSyncService.Services;

public class SyncService : ISyncService
{
    private readonly ILogger<SyncService> _logger;
    private readonly RegionConfiguration _regionConfig;

    public SyncService(
        ILogger<SyncService> logger,
        IOptions<RegionConfiguration> regionConfig)
    {
        _logger = logger;
        _regionConfig = regionConfig.Value;
    }

    public async Task ProcessSyncMessageAsync(SyncMessage message, CancellationToken cancellationToken = default)
    {
        LogProcessingStart(message);

        var targetRegions = await DetermineTargetRegionsAsync(message, cancellationToken);
        if (!targetRegions.Any())
        {
            LogNoTargetRegions(message);
            return;
        }

        if (message.IsDeleted)
            await ProcessDeletionAsync(message, targetRegions, cancellationToken);
        else
            await ProcessUpsertAsync(message, targetRegions, cancellationToken);
    }

    public async Task<List<string>> DetermineTargetRegionsAsync(SyncMessage message, CancellationToken cancellationToken = default)
    {
        var sourceConnectionString = _regionConfig.GetConnectionString(message.SourceRegion);
        if (string.IsNullOrEmpty(sourceConnectionString))
        {
            LogMissingConnectionString(message.SourceRegion);
            return new List<string>();
        }

        try
        {
            var metadata = await FetchEntityMetadataAsync(sourceConnectionString, message.EntityType, message.EntityId, message.TableName, cancellationToken);
            if (metadata == null)
            {
                LogMissingMetadata(message);
                return new List<string>();
            }

            var targetRegions = ApplySyncRules(metadata, message);
            targetRegions = ApplyAdditionalRules(message, targetRegions);
            targetRegions.RemoveAll(r => r.Equals(message.SourceRegion, StringComparison.OrdinalIgnoreCase));

            LogTargetRegionsDetermined(message, metadata.SyncScope, targetRegions);
            return targetRegions;
        }
        catch (Exception ex)
        {
            LogTargetRegionError(message, ex);
            return new List<string>();
        }
    }

    private List<string> ApplySyncRules(EntityMetadata metadata, SyncMessage message)
    {
        return metadata.SyncScope switch
        {
            SyncScope.GlobalSanitized => ApplyGlobalSanitizedRules(metadata, message),
            SyncScope.EUOnly => new List<string> { "EU", "EU-MAIN" },
            SyncScope.ScopedByExposure => new List<string> { "EU-MAIN" }, // Todo: Use regions based on country exposure
            _ => new List<string>()
        };
    }

    private List<string> ApplyGlobalSanitizedRules(EntityMetadata metadata, SyncMessage message)
    {
        // Check if this entity type requires sanitization for global sync
        if (!metadata.RequiresSanitizationForGlobalSync)
        {
            // No sanitization required (e.g., Country table) - sync to all regions
            return _regionConfig.GetAllRegions().Keys.ToList();
        }

        // Sanitization is required - check conditions
        var isSanitized = metadata.IsSanitized == true;
        var hasOverrideConsent = metadata.AllowSanitizationOverrideConsent && 
                                  metadata.SanitizationOverrideConsentAt.HasValue;

        if (!isSanitized && !hasOverrideConsent)
        {
            LogGlobalSanitizedValidationFailed(message, metadata);
            return new List<string>(); // Cannot sync - not sanitized and no consent
        }

        if (hasOverrideConsent)
        {
            LogUsingOverrideConsent(message, metadata.SanitizationOverrideConsentAt!.Value);
        }

        // Sync to all regions
        return _regionConfig.GetAllRegions().Keys.ToList();
    }

    private List<string> ApplyAdditionalRules(SyncMessage message, List<string> targetRegions)
    {
        // US/IN data always goes to EU-MAIN (central aggregation)
        if ((message.SourceRegion.Equals("US", StringComparison.OrdinalIgnoreCase) ||
             message.SourceRegion.Equals("IN", StringComparison.OrdinalIgnoreCase)) &&
            !targetRegions.Contains("EU-MAIN"))
        {
            targetRegions.Add("EU-MAIN");
        }

        return targetRegions;
    }

    private async Task ProcessDeletionAsync(SyncMessage message, List<string> targetRegions, CancellationToken cancellationToken)
    {
        LogDeletionStart(message, targetRegions.Count);

        foreach (var region in targetRegions)
        {
            await DeleteEntityInRegionAsync(message, region, cancellationToken);
        }
    }

    private async Task ProcessUpsertAsync(SyncMessage message, List<string> targetRegions, CancellationToken cancellationToken)
    {
        LogUpsertStart(message, targetRegions.Count);

        var entityData = await FetchEntityFromSource(message, cancellationToken);
        if (entityData == null)
        {
            LogEntityNotFound(message);
            return;
        }

        foreach (var region in targetRegions)
        {
            await UpsertEntityInRegionAsync(message, region, entityData, cancellationToken);
        }
    }

    private async Task<Dictionary<string, object?>?> FetchEntityFromSource(SyncMessage message, CancellationToken cancellationToken)
    {
        var sourceConnectionString = _regionConfig.GetConnectionString(message.SourceRegion);
        if (string.IsNullOrEmpty(sourceConnectionString))
            throw new InvalidOperationException($"No connection string for source region {message.SourceRegion}");

        return await FetchEntityDataAsync(sourceConnectionString, message.EntityType, message.EntityId, message.TableName, cancellationToken);
    }

    private async Task DeleteEntityInRegionAsync(SyncMessage message, string targetRegion, CancellationToken cancellationToken)
    {
        try
        {
            var connectionString = GetConnectionStringOrWarn(targetRegion);
            if (connectionString == null) return;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var tableName = message.TableName ?? message.EntityType;
            var deleteCmd = new SqlCommand($"DELETE FROM {tableName} WHERE Id = @EntityId", connection);
            deleteCmd.Parameters.AddWithValue("@EntityId", message.EntityId);

            var rowsAffected = await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
            LogDeletionSuccess(message, targetRegion, rowsAffected);
        }
        catch (Exception ex)
        {
            LogDeletionFailure(message, targetRegion, ex);
            throw;
        }
    }

    private async Task UpsertEntityInRegionAsync(
        SyncMessage message,
        string targetRegion,
        Dictionary<string, object?> entityData,
        CancellationToken cancellationToken)
    {
        try
        {
            var connectionString = GetConnectionStringOrWarn(targetRegion);
            if (connectionString == null) return;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var tableName = message.TableName ?? message.EntityType;

            if (await IsAlreadySynced(connection, tableName, message, cancellationToken))
            {
                LogSkippingAlreadySynced(message, targetRegion);
                return;
            }

            ValidateEntityId(entityData, message.EntityType);

            await PerformUpsert(connection, tableName, message, entityData, cancellationToken);
            LogUpsertSuccess(message, targetRegion, entityData["Id"]!);
        }
        catch (SqlException ex) when (IsForeignKeyConstraintViolation(ex))
        {
            LogForeignKeyViolation(message, targetRegion, ex);
            throw new ForeignKeyConstraintException(
                $"FK constraint violation syncing {message.EntityType} {message.EntityId} to {targetRegion}", ex);
        }
        catch (Exception ex)
        {
            LogUpsertFailure(message, targetRegion, ex);
            throw;
        }
    }

    private async Task<bool> IsAlreadySynced(SqlConnection connection, string tableName, SyncMessage message, CancellationToken cancellationToken)
    {
        var checkCmd = new SqlCommand($"SELECT LastSyncEventId FROM {tableName} WHERE Id = @EntityId", connection);
        checkCmd.Parameters.AddWithValue("@EntityId", message.EntityId);
        var lastSyncEventId = await checkCmd.ExecuteScalarAsync(cancellationToken) as string;
        return lastSyncEventId == message.SyncEventId;
    }

    private async Task PerformUpsert(SqlConnection connection, string tableName, SyncMessage message, Dictionary<string, object?> entityData, CancellationToken cancellationToken)
    {
        // TODO: Implement dynamic entity mapping
        // CRITICAL: Must use entityData["Id"] as the GUID - never generate new!
        _logger.LogInformation(
            "Upsert placeholder: {Entity} {Id} in {Table} (SourceId={SourceId})",
            message.EntityType, message.EntityId, tableName, entityData["Id"]);

        // Update sync metadata
        var updateSyncCmd = new SqlCommand(
            $@"UPDATE {tableName} SET LastSyncedAt = @SyncedAt, LastSyncEventId = @EventId WHERE Id = @EntityId",
            connection);
        updateSyncCmd.Parameters.AddWithValue("@SyncedAt", DateTimeOffset.UtcNow);
        updateSyncCmd.Parameters.AddWithValue("@EventId", message.SyncEventId);
        updateSyncCmd.Parameters.AddWithValue("@EntityId", message.EntityId);
        await updateSyncCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private void ValidateEntityId(Dictionary<string, object?> entityData, string entityType)
    {
        if (!entityData.TryGetValue("Id", out var idValue) || idValue == null)
            throw new InvalidOperationException($"Entity {entityType} missing Id field");
    }

    private string? GetConnectionStringOrWarn(string region)
    {
        var connectionString = _regionConfig.GetConnectionString(region);
        if (string.IsNullOrEmpty(connectionString))
            _logger.LogWarning("No connection string for region {Region}", region);
        return connectionString;
    }

    private bool IsForeignKeyConstraintViolation(SqlException ex) => ex.Number == 547;

    private async Task<Dictionary<string, object?>?> FetchEntityDataAsync(
        string connectionString,
        string entityType,
        string entityId,
        string? tableName,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var table = tableName ?? entityType;

        // Fetch all columns for the entity
        var cmd = new SqlCommand(
            $"SELECT * FROM {table} WHERE Id = @EntityId",
            connection);

        cmd.Parameters.AddWithValue("@EntityId", entityId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var data = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                data[fieldName] = value;
            }
            return data;
        }

        return null;
    }

    private async Task<EntityMetadata?> FetchEntityMetadataAsync(
        string connectionString,
        string entityType,
        string entityId,
        string? tableName,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var table = tableName ?? entityType;

        // Fetch entity-level configuration from EntitySyncConfiguration table
        var configCmd = new SqlCommand(
            @"SELECT SyncScope, DataClassification, LegalBasis, RequiresSanitizationForGlobalSync, AllowSanitizationOverrideConsent
              FROM EntitySyncConfigurations
              WHERE EntityTypeName = @EntityType AND IsEnabled = 1",
            connection);
        configCmd.Parameters.AddWithValue("@EntityType", entityType);

        EntitySyncConfig? entityConfig = null;
        await using (var configReader = await configCmd.ExecuteReaderAsync(cancellationToken))
        {
            if (await configReader.ReadAsync(cancellationToken))
            {
                entityConfig = new EntitySyncConfig
                {
                    SyncScope = Enum.Parse<SyncScope>(configReader.GetString(0)),
                    DataClassification = Enum.Parse<DataClassification>(configReader.GetString(1)),
                    LegalBasis = Enum.Parse<LegalBasisType>(configReader.GetString(2)),
                    RequiresSanitizationForGlobalSync = configReader.GetBoolean(3),
                    AllowSanitizationOverrideConsent = configReader.GetBoolean(4)
                };
            }
        }

        if (entityConfig == null)
        {
            _logger.LogWarning("No sync configuration found for entity type {EntityType}", entityType);
            return null;
        }

        // Fetch per-row data (residency, sanitization status)
        var rowCmd = new SqlCommand(
            $@"SELECT DataResidency, DataOriginRegion, IsSanitized, SanitizationOverrideConsentAt
               FROM {table}
               WHERE Id = @EntityId",
            connection);
        rowCmd.Parameters.AddWithValue("@EntityId", entityId);

        await using var rowReader = await rowCmd.ExecuteReaderAsync(cancellationToken);

        if (await rowReader.ReadAsync(cancellationToken))
        {
            try
            {
                return new EntityMetadata
                {
                    // From EntitySyncConfiguration table (per-entity-type)
                    SyncScope = entityConfig.SyncScope,
                    DataClassification = entityConfig.DataClassification,
                    LegalBasis = entityConfig.LegalBasis,
                    RequiresSanitizationForGlobalSync = entityConfig.RequiresSanitizationForGlobalSync,
                    AllowSanitizationOverrideConsent = entityConfig.AllowSanitizationOverrideConsent,
                    
                    // From entity row (per-row)
                    DataResidency = Enum.Parse<DataResidency>(rowReader.GetString(0)),
                    DataOriginRegion = Enum.Parse<DataRegion>(rowReader.GetString(1)),
                    IsSanitized = rowReader.IsDBNull(2) ? null : rowReader.GetBoolean(2),
                    SanitizationOverrideConsentAt = rowReader.IsDBNull(3) ? null : rowReader.GetDateTimeOffset(3)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing metadata for {Entity} {Id}", entityType, entityId);
                return null;
            }
        }

        return null;
    }

    private class EntitySyncConfig
    {
        public SyncScope SyncScope { get; set; }
        public DataClassification DataClassification { get; set; }
        public LegalBasisType LegalBasis { get; set; }
        public bool RequiresSanitizationForGlobalSync { get; set; }
        public bool AllowSanitizationOverrideConsent { get; set; }
    }

    private class EntityMetadata
    {
        // Entity-level (from EntitySyncConfiguration)
        public SyncScope SyncScope { get; set; }
        public DataClassification DataClassification { get; set; }
        public LegalBasisType LegalBasis { get; set; }
        public bool RequiresSanitizationForGlobalSync { get; set; }
        public bool AllowSanitizationOverrideConsent { get; set; }
        
        // Row-level (from entity table)
        public DataResidency DataResidency { get; set; }
        public DataRegion DataOriginRegion { get; set; }
        public bool? IsSanitized { get; set; }
        public DateTimeOffset? SanitizationOverrideConsentAt { get; set; }
    }

    #region Logging Helpers

    private void LogProcessingStart(SyncMessage message) =>
        _logger.LogInformation("Processing: {Entity} {Id} from {Source} (Deleted={IsDeleted})", 
            message.EntityType, message.EntityId, message.SourceRegion, message.IsDeleted);

    private void LogNoTargetRegions(SyncMessage message) =>
        _logger.LogInformation("No target regions for {EventId}", message.SyncEventId);

    private void LogMissingConnectionString(string region) =>
        _logger.LogWarning("No connection string for region {Region}", region);

    private void LogMissingMetadata(SyncMessage message) =>
        _logger.LogWarning("Could not fetch metadata for {Entity} {Id}", message.EntityType, message.EntityId);

    private void LogTargetRegionsDetermined(SyncMessage message, SyncScope syncScope, List<string> regions) =>
        _logger.LogInformation("Targets for {Entity} {Id} (Scope={Scope}): {Regions}", 
            message.EntityType, message.EntityId, syncScope, string.Join(", ", regions));

    private void LogTargetRegionError(SyncMessage message, Exception ex) =>
        _logger.LogError(ex, "Error determining targets for {Entity} {Id}", message.EntityType, message.EntityId);

    private void LogDeletionStart(SyncMessage message, int regionCount) =>
        _logger.LogInformation("Deleting {Entity} {Id} in {Count} regions", message.EntityType, message.EntityId, regionCount);

    private void LogDeletionSuccess(SyncMessage message, string region, int rows) =>
        _logger.LogInformation("Deleted {Rows} row(s) of {Entity} {Id} in {Region}", rows, message.EntityType, message.EntityId, region);

    private void LogDeletionFailure(SyncMessage message, string region, Exception ex) =>
        _logger.LogError(ex, "Failed to delete {Entity} {Id} in {Region}", message.EntityType, message.EntityId, region);

    private void LogUpsertStart(SyncMessage message, int regionCount) =>
        _logger.LogInformation("Upserting {Entity} {Id} to {Count} regions", message.EntityType, message.EntityId, regionCount);

    private void LogEntityNotFound(SyncMessage message) =>
        _logger.LogWarning("Entity {Entity} {Id} not found in {Region}", message.EntityType, message.EntityId, message.SourceRegion);

    private void LogSkippingAlreadySynced(SyncMessage message, string region) =>
        _logger.LogDebug("Entity {Entity} {Id} already synced with {EventId} in {Region}", 
            message.EntityType, message.EntityId, message.SyncEventId, region);

    private void LogUpsertSuccess(SyncMessage message, string region, object sourceId) =>
        _logger.LogInformation("Synced {Entity} {Id} to {Region} (SourceId={SourceId})", 
            message.EntityType, message.EntityId, region, sourceId);

    private void LogUpsertFailure(SyncMessage message, string region, Exception ex) =>
        _logger.LogError(ex, "Failed to sync {Entity} {Id} to {Region}", message.EntityType, message.EntityId, region);

    private void LogForeignKeyViolation(SyncMessage message, string region, SqlException ex) =>
        _logger.LogWarning(ex, "FK violation for {Entity} {Id} in {Region}. Referenced entity not synced yet", 
            message.EntityType, message.EntityId, region);

    private void LogGlobalSanitizedValidationFailed(SyncMessage message, EntityMetadata metadata) =>
        _logger.LogWarning(
            "GlobalSanitized validation failed for {Entity} {Id}: IsSanitized={IsSanitized}, HasOverrideConsent={HasConsent}. " +
            "Cannot sync - data must be sanitized OR have explicit override consent.",
            message.EntityType, 
            message.EntityId, 
            metadata.IsSanitized, 
            metadata.SanitizationOverrideConsentAt.HasValue);

    private void LogUsingOverrideConsent(SyncMessage message, DateTimeOffset consentDate) =>
        _logger.LogInformation(
            "Using override consent for {Entity} {Id}: User explicitly consented to cross-region sharing on {ConsentDate}",
            message.EntityType,
            message.EntityId,
            consentDate);

    #endregion
}
