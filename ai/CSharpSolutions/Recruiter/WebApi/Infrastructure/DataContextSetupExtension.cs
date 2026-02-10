using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Recruiter.Infrastructure.Interceptors;
using Recruiter.Infrastructure.Repository;
using System.Configuration;

namespace Recruiter.WebApi.Infrastructure;

/// Database context configuration
public static class DataContextSetupExtension
{
    /// Configure Entity Framework Core with SQL Server
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
         // Register the audit interceptor
        services.AddScoped<AuditInterceptor>();
       
        services.AddDbContext<RecruiterDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                         
                });
            
            // Add audit interceptor for automatic CreatedBy/UpdatedBy handling
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            options.AddInterceptors(auditInterceptor);

            // Enable sensitive data logging only in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                // options.EnableSensitiveDataLogging(); // Commented out to reduce console output
                options.EnableDetailedErrors(); // Commented out to reduce console output
            }
        });

        return services;
    }

    /// Ensure database is created and migrations are applied
    /// 
    public static async Task<WebApplication> EnsureDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RecruiterDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var timeoutSeconds = configuration
            .GetValue<int>("DatabaseSettings:TimeoutSeconds", 30);
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        try
        {
            logger.LogInformation($"Checking database connection... with timeout set at {timeoutSeconds} seconds.");
            logger.LogInformation($"Connection string: {connectionString}");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            await context.Database.CanConnectAsync(cts.Token);

            logger.LogInformation("Database connection successful.");

            // THIS IS ENOUGH
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date.");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Database connection timed out. Application will start without database verification.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database initialization failed. Application will start but database operations may fail.");
            throw;
        }

        return app;
    }

    // Comments by Rashed, previeous version occuring issues during migration
    
    //public static async Task<WebApplication> EnsureDatabaseAsync(this WebApplication app)
    //{
    //    using var scope = app.Services.CreateScope();
    //    var context = scope.ServiceProvider.GetRequiredService<RecruiterDbContext>();
    //    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    //    try
    //    {
    //        logger.LogInformation("Checking database connection...");

    //        // Test connection with timeout
    //        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    //        await context.Database.CanConnectAsync(cts.Token);

    //        logger.LogInformation("Database connection successful. Ensuring database exists...");

    //        // Ensure database is created
    //        await context.Database.EnsureCreatedAsync();

    //        // Apply migrations if any
    //        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    //        if (pendingMigrations.Any())
    //        {
    //            logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
    //            await context.Database.MigrateAsync();
    //            logger.LogInformation("Migrations applied successfully.");
    //        }
    //        else
    //        {
    //            logger.LogInformation("Database is up to date.");
    //        }
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        logger.LogWarning("Database connection timed out. Application will start without database verification.");
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogWarning(ex, "Database initialization failed. Application will start but database operations may fail.");
    //        // Don't throw - let the application start even if database is not available
    //    }

    //    return app;
    //}
}
