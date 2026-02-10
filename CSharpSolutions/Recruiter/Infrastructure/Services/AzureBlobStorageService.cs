using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;
using Azure;

namespace Recruiter.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageOptions _options;

    public AzureBlobStorageService(
        ILoggerFactory loggerFactory, 
        BlobServiceClient blobServiceClient,
        AzureStorageOptions options)
    {
        _logger = loggerFactory.CreateLogger<AzureBlobStorageService>();
        _blobServiceClient = blobServiceClient;
        _options = options;
    }

    public string GetContainerName() => _options.ContainerName;

    public string GetStorageAccountName()
    {
        if (!string.IsNullOrWhiteSpace(_blobServiceClient.AccountName))
            return _blobServiceClient.AccountName;

        if (!string.IsNullOrWhiteSpace(_options.StorageAccountName))
            return _options.StorageAccountName;

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            var parts = _options.ConnectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
                    return part.Substring("AccountName=".Length).Trim();
            }
        }

        return string.Empty;
    }

    public BlobClient GetBlobClient(string containerName, string blobPath)
        => _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobPath);

    public BlobContainerClient GetBlobContainerClient(string containerName)
        => _blobServiceClient.GetBlobContainerClient(containerName);

    public async Task<string> UploadAsync(string containerName, string blobPath, Stream fileStream, string contentType)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };
            await client.UploadAsync(fileStream, uploadOptions);
            return client.Uri.ToString();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error uploading blob '{blobPath}' to container '{containerName}'!", blobPath, containerName);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobPath)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            var response = await client.DownloadStreamingAsync();
            if (response.Value.Content.CanSeek && response.Value.Content.Position > 0)
                response.Value.Content.Position = 0;
            return response.Value.Content;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 403)
        {
            _logger.LogError(ex, "Access denied to blob '{blobPath}' in container '{containerName}'", blobPath, containerName);
            throw new UnauthorizedAccessException("Access denied. Check Azure Storage permissions.", ex);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Blob '{blobPath}' not found in container '{containerName}'", blobPath, containerName);
            throw new FileNotFoundException($"Blob not found in container '{containerName}'", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error downloading blob '{blobPath}' from container '{containerName}'", blobPath, containerName);
            throw;
        }
    }

    public async Task DeleteAsync(string containerName, string blobPath)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            await client.DeleteIfExistsAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting blob '{blobPath}' in container '{containerName}'!", blobPath, containerName);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string containerName, string blobPath)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            return await client.ExistsAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking for existence of blob '{blobPath}' in container '{containerName}'!", blobPath, containerName);
            throw;
        }
    }

    public async Task<IDictionary<string, string>?> GetMetadataAsync(string containerName, string blobPath)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            var properties = await client.GetPropertiesAsync();
            return properties?.Value?.Metadata;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error get metadata for blob '{blobPath}' in container '{containerName}'!", blobPath, containerName);
            throw;
        }
    }

    /// <summary>
    /// Generates a secure upload URL for direct client-to-Azure uploads
    /// Supports both connection string (Account SAS) and DefaultAzureCredential (User Delegation SAS)
    /// </summary>
    public string GenerateUploadSasUrl(string containerName, string blobPath, int expirationMinutes = 20)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            
            // Check if we can use Account SAS (connection string) or need User Delegation SAS (DefaultAzureCredential)
            if (_options.UseConnectionString && !string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                // Use Account SAS with connection string
                var uri = client.GenerateSasUri(BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(expirationMinutes));
                return uri.ToString();
            }
            else
            {
                // Use User Delegation SAS with DefaultAzureCredential
                return GenerateUserDelegationSasUrl(containerName, blobPath, BlobSasPermissions.Write, expirationMinutes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload SAS URL for blob '{BlobPath}' in container '{Container}'", blobPath, containerName);
            throw;
        }
    }

    /// <summary>
    /// Generates a secure, short-lived download URL (read-once style with very short expiration)
    /// Supports both connection string (Account SAS) and DefaultAzureCredential (User Delegation SAS)
    /// </summary>
    public string GenerateSecureDownloadUrl(string containerName, string blobPath, int expirationMinutes = 5)
    {
        try
        {
            var client = GetBlobClient(containerName, blobPath);
            
            // Check if we can use Account SAS (connection string) or need User Delegation SAS (DefaultAzureCredential)
            if (_options.UseConnectionString && !string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                // Use Account SAS with connection string
                var uri = client.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(expirationMinutes));
                return uri.ToString();
            }
            else
            {
                // Use User Delegation SAS with DefaultAzureCredential
                return GenerateUserDelegationSasUrl(containerName, blobPath, BlobSasPermissions.Read, expirationMinutes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download SAS URL for blob '{BlobPath}' in container '{Container}'", blobPath, containerName);
            throw;
        }
    }

    /// <summary>
    /// Generates User Delegation SAS URL using DefaultAzureCredential
    /// This is required when using Managed Identity instead of connection strings
    /// </summary>
    private string GenerateUserDelegationSasUrl(string containerName, string blobPath, BlobSasPermissions permissions, int expirationMinutes)
    {
        try
        {
            var containerClient = GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);
            
            // Get user delegation key (valid for up to 7 days)
            var keyExpiry = DateTimeOffset.UtcNow.AddHours(1); // User delegation key expiry (max 7 days, but we use 1 hour for security)
            var userDelegationKey = _blobServiceClient.GetUserDelegationKey(DateTimeOffset.UtcNow, keyExpiry);
            
            // Create SAS builder
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobPath,
                Resource = "b", // 'b' for blob, 'c' for container
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Allow 5 minutes clock skew
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
                Protocol = SasProtocol.Https
            };
            
            sasBuilder.SetPermissions(permissions);
            
            // Generate SAS token using user delegation key
            var sasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName).ToString();
            
            // Build the full URI
            var uriBuilder = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken
            };
            
            return uriBuilder.ToString();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 403)
        {
            _logger.LogError(ex, "Access denied when generating user delegation key. User/Managed Identity needs 'Storage Blob Data Contributor' role on storage account '{Account}'. Current role might be 'Storage Account Contributor' which is insufficient.", _blobServiceClient.AccountName);
            throw new UnauthorizedAccessException(
                $"Access denied. To generate SAS URLs, you need 'Storage Blob Data Contributor' role (not just 'Storage Account Contributor') on storage account '{_blobServiceClient.AccountName}'. " +
                $"Error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user delegation SAS URL for blob '{BlobPath}' in container '{Container}'. Account: {Account}", blobPath, containerName, _blobServiceClient.AccountName);
            throw new InvalidOperationException(
                $"Failed to generate SAS URL with DefaultAzureCredential. " +
                $"Ensure you have 'Storage Blob Data Contributor' role on storage account '{_blobServiceClient.AccountName}'. " +
                $"For local development, make sure you're logged in to Visual Studio or Azure CLI. " +
                $"Error: {ex.Message}", ex);
        }
    }
}
