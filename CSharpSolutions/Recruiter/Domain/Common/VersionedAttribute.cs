using System;

namespace Recruiter.Domain.Common;

[AttributeUsage(AttributeTargets.Class)]
public class VersionedAttribute : Attribute
{
    /// Whether to cascade versioning to parent entities when this entity is updated
    public bool CascadeToParent { get; set; } = false;

    /// Whether to cascade versioning to child entities when this entity is updated
    public bool CascadeToChildren { get; set; } = false;

    /// Properties to ignore during versioning (comma-separated)
    public string IgnoreProperties { get; set; } = "";
}
