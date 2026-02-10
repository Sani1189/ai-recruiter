using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Recruiter.Domain.Models;

namespace Recruiter.Infrastructure.Interceptors;

/// <summary>
/// Interceptor that automatically sets audit fields (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
/// on entity changes. Integrates with HTTP context for user tracking.
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetAuditFields(DbContext? context)
    {
        if (context == null) return;

        var currentUser = GetCurrentUser();
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (!IsAuditableEntity(entry)) continue;

            ApplyAudit(entry, utcNow, currentUser);
        }
    }

    private static bool IsAuditableEntity(EntityEntry entry)
    {
        return entry.Entity is BaseDbModel
            || entry.Entity is BasicBaseDbModel
            || entry.Entity is VersionedBaseDbModel;
    }

    private static void ApplyAudit(EntityEntry entry, DateTimeOffset utcNow, string? currentUser)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                SetProperty(entry, "CreatedAt", utcNow);
                SetProperty(entry, "UpdatedAt", utcNow);
                SetProperty(entry, "CreatedBy", currentUser);
                SetProperty(entry, "UpdatedBy", currentUser);
                break;

            case EntityState.Modified:
                SetProperty(entry, "UpdatedAt", utcNow);
                SetProperty(entry, "UpdatedBy", currentUser);
                entry.Property("CreatedAt").IsModified = false;
                entry.Property("CreatedBy").IsModified = false;
                break;
        }
    }

    private static void SetProperty(EntityEntry entry, string propertyName, object? value)
    {
        entry.Property(propertyName).CurrentValue = value;
    }

    private string? GetCurrentUser()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return GetUserIdentifier(user);
        }
        catch
        {
            // Background operations or missing HttpContext: skip audit user
            return null;
        }
    }

    /// <summary>Priority order: Email, UPN, preferred_username, Identity.Name, NameIdentifier, sub.</summary>
    private static string? GetUserIdentifier(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("upn")?.Value
            ?? user.FindFirst("preferred_username")?.Value
            ?? user.Identity?.Name
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
    }
}
