using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Common.Dto;

// Base DTO with common properties
public class BaseModelDto
{
    public Guid? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}