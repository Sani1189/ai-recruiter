using System.ComponentModel.DataAnnotations;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents a file in the system (resumes, documents, etc.).
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// Files are referenced by: Candidate (1:1), JobApplicationStepFiles (1:1), cv_evaluation (M:1)
/// </summary>
public class File : BaseDbModel
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

    public Guid? TenantId { get; set; }

    [Required]
    [MaxLength(255)]
    public string StorageAccountName { get; set; } = string.Empty;

    // No inverse navigation properties - relationships defined in referencing tables
    // Candidate.cvFileId -> File.id (1:1)
    // JobApplicationStepFiles.fileId -> File.id (1:1)
    // cv_evaluation.file_id -> File.id (M:1)

    /// <summary>
    /// Helper to get full blob path: FolderPath/FilePath
    /// </summary>
    public string GetFullBlobPath() => string.IsNullOrEmpty(FolderPath) 
        ? FilePath 
        : $"{FolderPath}/{FilePath}";
}
