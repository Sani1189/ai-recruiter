using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recruiter.Domain.Models;

namespace Recruiter.Infrastructure.Extensions;

/// <summary>
/// Entity Framework configuration extensions for common database model properties
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Configures properties for BasicBaseDbModel (NO GDPR sync).
    /// Use for: Country, EntitySyncConfiguration, etc.
    /// </summary>
    public static void ConfigureBasicBaseDbModelProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BasicBaseDbModel
    {
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(null);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(null);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }

    /// <summary>
    /// Configures GDPR sync properties (shared by BaseDbModel and VersionedBaseDbModel).
    /// </summary>
    private static void ConfigureGdprSyncProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : GdprSyncBaseDbModel
    {
        builder.Property(e => e.DataOriginRegion)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(e => e.DataResidency)
            .IsRequired()
            .HasConversion<string>();

        // Configure CountryExposureSet FK relationship
        builder.Property(e => e.CountryExposureSetId)
            .IsRequired(false);

        builder.HasOne(e => e.CountryExposureSet)
            .WithMany()
            .HasForeignKey(e => e.CountryExposureSetId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.Property(e => e.LastSyncedAt)
            .IsRequired(false);
            
        builder.Property(e => e.LastSyncEventId)
            .HasMaxLength(128)
            .IsRequired(false);
            
        builder.Property(e => e.IsSanitized)
            .IsRequired(false);
            
        builder.Property(e => e.SanitizedAt)
            .IsRequired(false);
            
        builder.Property(e => e.SanitizationOverrideConsentAt)
            .IsRequired(false);
    }

    /// <summary>
    /// Configures common properties for entities that inherit from BaseDbModel.
    /// Sets up Guid Id, audit fields, concurrency control, and GDPR sync properties.
    /// </summary>
    public static void ConfigureBaseDbModelProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseDbModel
    {
        // Configure Id as required with NEWID() default
        builder.Property(e => e.Id)
            .IsRequired()
            .HasDefaultValueSql("NEWID()");

        // Configure audit properties
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(null);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(null);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Configure GDPR sync properties (inherited from GdprSyncBaseDbModel)
        builder.ConfigureGdprSyncProperties();
    }


    /// <summary>
    /// Configures common properties for entities that inherit from VersionedBaseDbModel.
    /// Sets up (Name, Version) composite key, audit fields, concurrency control, and GDPR sync properties.
    /// </summary>
    public static void ConfigureVersionedBaseDbModelProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : VersionedBaseDbModel
    {
        // Configure composite primary key (Name, Version)
        builder.HasKey(e => new { e.Name, e.Version });
        
        // Configure Name as required with max length
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        // Configure Version as required
        builder.Property(e => e.Version)
            .IsRequired();

        // Configure audit properties
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(null);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(null);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Configure GDPR sync properties (inherited from GdprSyncBaseDbModel)
        builder.ConfigureGdprSyncProperties();
    }
}
