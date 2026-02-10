using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.UserProfile.Dto;

/// <summary>
/// DTO for UserProfile operations
/// </summary>
public class UserProfileDto : BaseModelDto
{
    public string? ResumeUrl { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public int? Age { get; set; }
    
    [MaxLength(100)]
    public string? Nationality { get; set; }
    
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
    
    public string? Bio { get; set; }
    
    public List<string> JobTypePreferences { get; set; } = new List<string>();
    
    public bool? OpenToRelocation { get; set; }
    
    public List<string> RemotePreferences { get; set; } = new List<string>();
}
