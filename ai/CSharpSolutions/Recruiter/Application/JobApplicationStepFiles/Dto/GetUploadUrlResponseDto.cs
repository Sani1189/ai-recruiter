namespace Recruiter.Application.JobApplicationStepFiles.Dto;

public class GetUploadUrlResponseDto
{
    public string UploadUrl { get; set; } = string.Empty;
    public string BlobPath { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
}

