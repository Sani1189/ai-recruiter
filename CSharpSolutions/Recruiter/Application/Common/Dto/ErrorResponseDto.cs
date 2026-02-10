namespace Recruiter.Application.Common.Dto;

/// Standard error response DTO for API consistency
public class ErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Field { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> ValidationErrors { get; set; } = new();
}
