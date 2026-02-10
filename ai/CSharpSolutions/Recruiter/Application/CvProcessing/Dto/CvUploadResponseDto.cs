using System.Text.Json.Serialization;

namespace Recruiter.Application.CvProcessing.Dto;

public class CvUploadResponseDto
{
    [JsonPropertyName("fileId")]
    public string FileId { get; set; } = string.Empty;
    
    [JsonPropertyName("userProfileId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

