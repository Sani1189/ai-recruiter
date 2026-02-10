using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recruiter.Domain.Models;

/// <summary>
/// Represents file attachments for job application steps
/// </summary>
[Table("JobApplicationStepFiles")]
public class JobApplicationStepFiles : BaseDbModel
{
    [Required]
    public Guid FileId { get; set; }

    [Required]
    public Guid JobApplicationStepId { get; set; }

    public Guid? TenantId { get; set; }

    // Navigation properties
    public virtual File? File { get; set; }
    public virtual JobApplicationStep? JobApplicationStep { get; set; }
}
