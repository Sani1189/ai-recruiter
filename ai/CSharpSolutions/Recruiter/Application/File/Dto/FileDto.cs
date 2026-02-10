using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.File.Dto;

public class FileDto : BaseModelDto
{
    [Required]
    [MaxLength(100)]
    public string Container { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? FolderPath { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Extension { get; set; } = string.Empty;
    
    [Required]
    public int MbSize { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string StorageAccountName { get; set; } = string.Empty;
}
