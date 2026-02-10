using System.Security.Claims;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current user information from JWT claims
/// This is a cross-cutting concern used throughout the application
/// </summary>
public interface ICurrentUserService
{
    UserInfoDto GetCurrentUserInfo();
    
    string? GetUserId();
    
    string? GetUserEmail();
    
    string? GetUserName();

    public Task<UserProfileDto?> GetUserAsync();
    
    IEnumerable<string> GetUserRoles();
    
    bool IsInRole(string role);
    
    bool IsAuthenticated();
    
    string? GetClaimValue(string claimType);
    
    IEnumerable<Claim> GetAllClaims();
}
