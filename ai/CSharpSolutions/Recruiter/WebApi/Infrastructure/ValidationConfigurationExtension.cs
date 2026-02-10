using Microsoft.AspNetCore.Mvc;

namespace Recruiter.WebApi.Infrastructure;

/// <summary>
/// Extension methods for configuring validation and model binding error handling
/// </summary>
public static class ValidationConfigurationExtension
{
    public static IServiceCollection AddEnhancedValidation(this IServiceCollection services)
    {
        // For now, just return services without custom validation handling
        // This ensures normal FluentValidation and annotation validation works as expected
        services.AddControllers();
        return services;
    }
}
