using System.Security.Claims;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Application.UserProfile.Interfaces;

/// <summary>
/// Service for user registration from JWT claims
/// </summary>
public interface IUserRegistrationService
{
    Task<UserProfileDto> RegisterUserAsync(ClaimsPrincipal user);
    
    Task<bool> UserExistsAsync(string email);
}
