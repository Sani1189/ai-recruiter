using Microsoft.AspNetCore.Authorization;

namespace Recruiter.WebApi.Attributes;

/// Attribute to mark API endpoints as public (no authentication required)
/// This is more descriptive than AllowAnonymous and clearly indicates intentional public access
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PublicApiAttribute : Attribute, IAllowAnonymous
{
    public string? Description { get; set; }

    public PublicApiAttribute(string? description = null)
    {
        Description = description;
    }
}
