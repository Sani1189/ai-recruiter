using Microsoft.Extensions.DependencyInjection;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Application.UserProfile.Services;
using Recruiter.Infrastructure.Services;

namespace Recruiter.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering user registration services
/// </summary>
public static class UserRegistrationExtensions
{
    /// <summary>
    /// Register user registration services
    /// </summary>
    public static IServiceCollection AddUserRegistration(this IServiceCollection services)
    {
        // Note: AddHttpContextAccessor() should be called in the WebApi project
        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Register user registration service
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        
        return services;
    }
}
