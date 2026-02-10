namespace Recruiter.Application.Common.Options;

public class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";
    public string ConnectionString { get; set; } = string.Empty;

    public string StorageAccountName { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "cvfiles";

    /// <summary>
    /// Determines if connection string authentication should be used.
    /// If false, DefaultAzureCredential will be used.
    /// </summary>
    public bool UseConnectionString => !string.IsNullOrWhiteSpace(ConnectionString);
}

