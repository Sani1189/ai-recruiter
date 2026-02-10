using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Infrastructure.Data;

/// <summary>
/// Seed data for EntitySyncConfiguration table.
/// Defines how each entity type should be synced across regions.
/// </summary>
public static class EntitySyncConfigurationSeed
{
    public static List<EntitySyncConfiguration> GetSeedData()
    {
        var now = DateTimeOffset.UtcNow;
        
        return new List<EntitySyncConfiguration>
        {
            // Reference data - syncs globally without sanitization
            new()
            {
                EntityTypeName = "Country",
                DataClassification = DataClassification.NonPersonal,
                SyncScope = SyncScope.GlobalSanitized,
                LegalBasis = LegalBasisType.None,
                ProcessingPurpose = "ReferenceData",
                RequiresSanitizationForGlobalSync = false,
                AllowSanitizationOverrideConsent = false,
                Notes = "Reference data - no personal information",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Job Posts - syncs based on country exposure
            new()
            {
                EntityTypeName = "JobPost",
                DataClassification = DataClassification.NonPersonal,
                SyncScope = SyncScope.ScopedByExposure,
                LegalBasis = LegalBasisType.None,
                ProcessingPurpose = "JobAdPublishing",
                RequiresSanitizationForGlobalSync = false,
                AllowSanitizationOverrideConsent = false,
                DependsOnEntities = "Country",
                Notes = "Sync based on JobPostCountryExposure settings",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Candidates - personal data, EU only by default
            new()
            {
                EntityTypeName = "Candidate",
                DataClassification = DataClassification.Personal,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                LegalBasisRef = "privacy-policy-candidate-data",
                ProcessingPurpose = "Recruitment",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = true,
                Notes = "Personal data - requires sanitization or explicit consent for global sync",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Job Applications - personal data, follows candidate rules
            new()
            {
                EntityTypeName = "JobApplication",
                DataClassification = DataClassification.Personal,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                LegalBasisRef = "privacy-policy-application-data",
                ProcessingPurpose = "Recruitment",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = true,
                DependsOnEntities = "Candidate,JobPost",
                Notes = "Depends on Candidate and JobPost - sync those first",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Interviews - sensitive data, EU only
            new()
            {
                EntityTypeName = "Interview",
                DataClassification = DataClassification.Sensitive,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                LegalBasisRef = "privacy-policy-interview-data",
                ProcessingPurpose = "Recruitment",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = false,
                DependsOnEntities = "JobApplication",
                Notes = "Sensitive interview data - no override consent allowed",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // User Profiles - personal data
            new()
            {
                EntityTypeName = "UserProfile",
                DataClassification = DataClassification.Personal,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                LegalBasisRef = "privacy-policy-user-profile",
                ProcessingPurpose = "AccountManagement",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = true,
                Notes = "User profile data",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Files - depends on context, default to personal
            new()
            {
                EntityTypeName = "File",
                DataClassification = DataClassification.Personal,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                LegalBasisRef = "privacy-policy-file-uploads",
                ProcessingPurpose = "DocumentStorage",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = true,
                Notes = "Uploaded files (resumes, documents) - typically contain personal data",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Comments - can be personal
            new()
            {
                EntityTypeName = "Comment",
                DataClassification = DataClassification.Personal,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                ProcessingPurpose = "Recruitment",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = true,
                DependsOnEntities = "Candidate,JobPost,JobApplication",
                Notes = "Comments may contain personal opinions about candidates",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Feedback - sensitive evaluation data
            new()
            {
                EntityTypeName = "Feedback",
                DataClassification = DataClassification.Sensitive,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                LegalBasisRef = "privacy-policy-feedback",
                ProcessingPurpose = "Recruitment",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = false,
                DependsOnEntities = "Interview",
                Notes = "Sensitive evaluation data - no override consent",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Interview Configuration - non-personal template data
            new()
            {
                EntityTypeName = "InterviewConfiguration",
                DataClassification = DataClassification.NonPersonal,
                SyncScope = SyncScope.GlobalSanitized,
                LegalBasis = LegalBasisType.None,
                ProcessingPurpose = "ConfigurationManagement",
                RequiresSanitizationForGlobalSync = false,
                AllowSanitizationOverrideConsent = false,
                Notes = "Interview templates - no personal data",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Job Post Steps - non-personal workflow data
            new()
            {
                EntityTypeName = "JobPostStep",
                DataClassification = DataClassification.NonPersonal,
                SyncScope = SyncScope.ScopedByExposure,
                LegalBasis = LegalBasisType.None,
                ProcessingPurpose = "WorkflowManagement",
                RequiresSanitizationForGlobalSync = false,
                AllowSanitizationOverrideConsent = false,
                DependsOnEntities = "JobPost",
                Notes = "Workflow configuration for job posts",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Job Application Steps - links personal data
            new()
            {
                EntityTypeName = "JobApplicationStep",
                DataClassification = DataClassification.Personal,
                SyncScope = SyncScope.EUOnly,
                LegalBasis = LegalBasisType.Consent,
                ProcessingPurpose = "Recruitment",
                RequiresSanitizationForGlobalSync = true,
                AllowSanitizationOverrideConsent = true,
                DependsOnEntities = "JobApplication,JobPostStep",
                Notes = "Application progress tracking",
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }
}
