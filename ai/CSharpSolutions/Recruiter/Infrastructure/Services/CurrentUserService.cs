using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of current user service that extracts user information from JWT claims
/// This implementation belongs in Infrastructure layer as it depends on ASP.NET Core HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserProfileService _userProfileService;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserProfileService userProfileService)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
    }

    public UserInfoDto GetCurrentUserInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            return new UserInfoDto();
        }

        var email = GetUserEmail() ?? string.Empty;
        var name = GetUserName() ?? string.Empty;

        return new UserInfoDto
        {
            Email = email,
            Name = name,
            Roles = GetUserRoles().ToList()
        };
    }

    public async Task<UserProfileDto?> GetUserAsync()
    {
        var email = GetUserEmail();
        if (email == null)
        {
            return null;
        }
        var userProfile = await _userProfileService.GetByEmailAsync(email);
        if (userProfile.IsSuccess && userProfile.Value != null)
        {
            return userProfile.Value;
        }
        return null;
    }

    public string? GetUserId()
    {
        return GetClaimValue(ClaimTypes.NameIdentifier) ??
               GetClaimValue("sub") ??
               GetClaimValue("oid");
    }

    public string? GetUserEmail()
    {
        return GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn") ?? 
               GetClaimValue("upn") ?? 
               GetClaimValue("preferred_username") ?? 
               GetClaimValue(ClaimTypes.Email);
    }

    public string? GetUserName()
    {
        return GetClaimValue("name") ?? 
               GetClaimValue(ClaimTypes.Name) ?? 
               GetUserEmail();
    }

    public IEnumerable<string> GetUserRoles()
    {
        return _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" || 
                       c.Type == ClaimTypes.Role || 
                       c.Type == "roles")
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public bool IsInRole(string role)
    {
        return GetUserRoles().Contains(role);
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    public IEnumerable<Claim> GetAllClaims()
    {
        return _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    }

    private string DetermineProvider(ClaimsPrincipal user)
    {
        // Check for Microsoft-specific claims
        if (GetClaimValue("oid") != null || GetClaimValue("tid") != null)
        {
            return "Microsoft";
        }
        
        // Check for Google-specific claims
        if (GetClaimValue("sub") != null && GetClaimValue("iss")?.Contains("google") == true)
        {
            return "Google";
        }
        
        return "Unknown";
    }

    private string GetProviderId(ClaimsPrincipal user, string provider)
    {
        return provider switch
        {
            "Microsoft" => GetClaimValue("oid") ?? GetClaimValue("sub") ?? string.Empty,
            "Google" => GetClaimValue("sub") ?? string.Empty,
            _ => GetClaimValue("sub") ?? GetClaimValue("oid") ?? string.Empty
        };
    }

    private string? GetTenantId(ClaimsPrincipal user)
    {
        return GetClaimValue("tid");
    }
}
