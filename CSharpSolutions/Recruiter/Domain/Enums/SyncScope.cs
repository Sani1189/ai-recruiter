// File: Domain/Enums/SyncScope.cs
namespace Recruiter.Domain.Enums;

public enum SyncScope
{
    ScopedByExposure = 0,   // follows Job Post country exposure
    GlobalSanitized = 1,    // sync everywhere (must be NonPersonal or SanitizationConsentOverride is not null)
    EUOnly = 2              // never leaves EU
}
