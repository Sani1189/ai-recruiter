using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents a candidate in the system.
/// Inherits from BaseDbModel which includes GDPR sync metadata.
/// </summary>
[Table("Candidates")]
public class Candidate : BaseDbModel
{
    [Required]
    [MaxLength(20)]
    public string CandidateId { get; set; } = string.Empty;
    
    public Guid? CvFileId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual UserProfile? UserProfile { get; set; }
    public virtual File? CvFile { get; set; }
}
