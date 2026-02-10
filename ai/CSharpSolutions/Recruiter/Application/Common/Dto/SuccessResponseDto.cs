namespace Recruiter.Application.Common.Dto;

/// Success response DTO for consistent API responses
public class SuccessResponseDto<T>
{
    public bool Success { get; set; } = true;
    public T Data { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
