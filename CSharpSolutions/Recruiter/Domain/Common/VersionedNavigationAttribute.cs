using System;

namespace Recruiter.Domain.Common;

/// Marks navigation properties that should be versioned when the parent entity is versioned
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class VersionedNavigationAttribute : Attribute
{
    /// The type of the related entity
    public Type RelatedEntityType { get; }

    /// The foreign key property name that points to this entity
    public string ForeignKeyPropertyName { get; }

    public VersionedNavigationAttribute(Type relatedEntityType, string foreignKeyPropertyName)
    {
        RelatedEntityType = relatedEntityType ?? throw new ArgumentNullException(nameof(relatedEntityType));
        ForeignKeyPropertyName = foreignKeyPropertyName ?? throw new ArgumentNullException(nameof(foreignKeyPropertyName));
    }
}
