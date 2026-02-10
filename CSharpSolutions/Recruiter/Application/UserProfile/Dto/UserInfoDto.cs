using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.UserProfile.Dto;

/// <summary>
/// DTO for user information extracted from JWT claims (minimal for UserProfile model)
/// </summary>
public class UserInfoDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public List<string> Roles { get; set; } = new();
}
