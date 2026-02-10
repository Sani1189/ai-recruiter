using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Recruiter.Application.Common.Dto;

public class VersionedBaseModelDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Version is required")] [Range(1, int.MaxValue, ErrorMessage = "Version must be greater than zero")]
    public int Version { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public byte[]? RowVersion { get; set; }
    [NotMapped]
    public bool? ShouldUpdateVersion { get; set; } = null;
}
