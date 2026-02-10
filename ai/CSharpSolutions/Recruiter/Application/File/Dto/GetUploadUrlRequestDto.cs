using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.File.Dto;

public class GetUploadUrlRequestDto
{
    /// <summary>
    /// Original file name from client (used to preserve extension).
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ContentType { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long FileSize { get; set; }

    /// <summary>
    /// Optional folder prefix within the container (e.g. "assessment-templates").
    /// </summary>
    [MaxLength(200)]
    public string? FolderPrefix { get; set; }

    /// <summary>
    /// Optional container override. If not provided, server default container is used.
    /// </summary>
    [MaxLength(100)]
    public string? Container { get; set; }
}


