using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Common.Dto;

public class VersionedBaseQueryDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [Range(1, int.MaxValue)]
    public int? Version { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}
