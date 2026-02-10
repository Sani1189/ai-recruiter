namespace Recruiter.Application.Common.Interfaces;

public interface IFileStorageService
{
    string GetContainerName();
    string GetStorageAccountName();
    Task<string> UploadAsync(string containerName, string blobPath, Stream fileStream, string contentType);
    Task<Stream> DownloadAsync(string containerName, string blobPath);
    Task DeleteAsync(string containerName, string blobPath);
    Task<bool> ExistsAsync(string containerName, string blobPath);
    Task<IDictionary<string, string>?> GetMetadataAsync(string containerName, string blobPath);
    /// <summary>
    /// Generates a secure upload URL for direct client-to-Azure uploads
    /// </summary>
    string GenerateUploadSasUrl(string containerName, string blobPath, int expirationMinutes = 20);
    /// <summary>
    /// Generates a secure, short-lived download URL (read-once style with very short expiration)
    /// </summary>
    string GenerateSecureDownloadUrl(string containerName, string blobPath, int expirationMinutes = 5);
}
