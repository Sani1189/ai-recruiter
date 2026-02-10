using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recruiter.Domain.Common;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents a prompt template
/// </summary>
[Table("Prompts")]
[Versioned(CascadeToChildren = true)]
public class Prompt : VersionedBaseDbModel
{
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty; // "instructions" | "tonality" | "rubric" | "question_bank"

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Locale { get; set; }

    public Guid? TenantId { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
}
