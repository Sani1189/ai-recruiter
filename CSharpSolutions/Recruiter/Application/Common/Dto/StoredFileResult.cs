namespace Recruiter.Application.Common.Dto;

/// <summary>
/// Represents the result of a file persisted in external storage.
/// </summary>
public sealed class StoredFileResult
{
    public required string BlobUrl { get; init; }
    public required string BlobPath { get; init; }
    public required string Container { get; init; }
    public required string StorageAccountName { get; init; }
}

