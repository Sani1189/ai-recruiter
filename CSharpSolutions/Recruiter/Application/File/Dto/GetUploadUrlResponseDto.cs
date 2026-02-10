namespace Recruiter.Application.File.Dto;

public class GetUploadUrlResponseDto
{
    public string UploadUrl { get; set; } = string.Empty;
    public string Container { get; set; } = string.Empty;
    public string? FolderPath { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string StorageAccountName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
}


