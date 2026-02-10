# GlobalSanitized Validation - GDPR Compliance

## Overview

The `SyncScope.GlobalSanitized` setting allows data to be synced to ALL regions globally. However, this is only permitted under strict GDPR compliance conditions.

## The Rule

**GlobalSanitized data can ONLY sync if:**

```
IsSanitized = true 
  OR 
SanitizationOverrideConsentAt != null
```

### Condition 1: Data is Sanitized (`IsSanitized = true`)

The entity has been processed to remove all personal/sensitive information, making it safe for global distribution.

**Example:**
```csharp
var jobPost = new JobPost
{
    SyncScope = SyncScope.GlobalSanitized,
    IsSanitized = true,  // ? Sanitized - safe to sync globally
    Title = "Senior Developer",
    Description = "We are looking for...",
    // No personal data - candidate names, emails removed
};
```

### Condition 2: Explicit Override Consent (`SanitizationOverrideConsentAt` has value)

The user/entity owner has explicitly consented to share this data cross-region, acknowledging they have the legal authority to do so.

**Example:**
```csharp
var candidate = new Candidate
{
    SyncScope = SyncScope.GlobalSanitized,
    IsSanitized = false,  // Contains personal data
    SanitizationOverrideConsentAt = DateTimeOffset.UtcNow,  // ? User explicitly consents
    Name = "John Doe",
    Email = "john@example.com",
    // User insists this should be shared globally and has authority
};
```

## What Happens When Validation Fails?

If an entity is marked `SyncScope.GlobalSanitized` but **NEITHER** condition is met:

1. **Sync is BLOCKED** - No target regions are determined
2. **Warning is logged**:
   ```
   GlobalSanitized validation failed for Candidate abc-123: 
   IsSanitized=False, HasOverrideConsent=False. 
   Cannot sync - data must be sanitized OR have explicit override consent.
   ```
3. **No data leaves source region** - GDPR compliance maintained

## Implementation Details

### In SyncService.cs

```csharp
private List<string> ApplyGlobalSanitizedRules(EntityMetadata metadata, SyncMessage message)
{
    var isSanitized = metadata.IsSanitized == true;
    var hasOverrideConsent = metadata.SanitizationOverrideConsentAt.HasValue;

    if (!isSanitized && !hasOverrideConsent)
    {
        LogGlobalSanitizedValidationFailed(message, metadata);
        return new List<string>(); // BLOCKED - cannot sync
    }

    if (hasOverrideConsent)
    {
        LogUsingOverrideConsent(message, metadata.SanitizationOverrideConsentAt!.Value);
    }

    return _regionConfig.GetAllRegions().Keys.ToList(); // ? Allowed - sync globally
}
```

## Use Cases

### ? Use Case 1: Job Post (Sanitized)

Job posts typically don't contain personal data about candidates, so they can be sanitized and shared globally:

```csharp
var jobPost = new JobPost
{
    Id = Guid.NewGuid(),
    Title = "Backend Developer",
    Description = "5+ years experience...",
    SalaryRange = "$100k-$150k",
    SyncScope = SyncScope.GlobalSanitized,
    IsSanitized = true,  // No personal data
    DataClassification = DataClassification.NonPersonal
};

// Result: Syncs to EU, US, IN, EU-MAIN ?
```

### ? Use Case 2: Public Candidate Profile (Override Consent)

Candidate explicitly wants their profile visible globally:

```csharp
var candidate = new Candidate
{
    Id = Guid.NewGuid(),
    Name = "Jane Smith",
    Email = "jane@example.com",
    PublicProfile = true,
    SyncScope = SyncScope.GlobalSanitized,
    IsSanitized = false,  // Contains personal data
    SanitizationOverrideConsentAt = DateTimeOffset.UtcNow,  // Explicit consent
    DataClassification = DataClassification.Personal,
    LegalBasis = LegalBasisType.Consent,
    LegalBasisRef = "consent-record-xyz-456"
};

// Result: Syncs to EU, US, IN, EU-MAIN ?
// Log: "Using override consent: User explicitly consented on 2024-01-15"
```

### ? Use Case 3: Candidate Without Consent (BLOCKED)

Candidate profile marked as GlobalSanitized but neither condition met:

```csharp
var candidate = new Candidate
{
    Id = Guid.NewGuid(),
    Name = "John Doe",
    Email = "john@example.com",
    SyncScope = SyncScope.GlobalSanitized,  // Wants global sync
    IsSanitized = false,  // ? Not sanitized
    SanitizationOverrideConsentAt = null,  // ? No consent
    DataClassification = DataClassification.Personal
};

// Result: BLOCKED - no sync to any region ?
// Warning: "GlobalSanitized validation failed..."
```

### ? Use Case 4: Sanitized Candidate (Personal Data Removed)

Candidate data processed to remove personal information:

```csharp
var candidateStats = new CandidateStatistics
{
    Id = Guid.NewGuid(),
    YearsExperience = 7,
    SkillLevel = "Senior",
    Location = "Europe",  // Country level, not specific address
    // Name, email, phone removed
    SyncScope = SyncScope.GlobalSanitized,
    IsSanitized = true,  // ? All personal data removed
    DataClassification = DataClassification.NonPersonal
};

// Result: Syncs to EU, US, IN, EU-MAIN ?
```

## Setting Override Consent in Your API

When a user explicitly consents to global sharing:

```csharp
public async Task<IActionResult> EnableGlobalProfile(Guid candidateId)
{
    var candidate = await _candidateRepository.GetByIdAsync(candidateId);
    
    // Verify user has authority (e.g., owns the profile)
    if (!UserOwnsProfile(candidate)) 
        return Forbid();
    
    // Set consent timestamp
    candidate.SyncScope = SyncScope.GlobalSanitized;
    candidate.SanitizationOverrideConsentAt = DateTimeOffset.UtcNow;
    candidate.LegalBasis = LegalBasisType.Consent;
    candidate.LegalBasisRef = $"profile-consent-{candidateId}";
    
    await _candidateRepository.UpdateAsync(candidate);
    
    // Trigger sync
    await _syncService.SendSyncMessage(candidate);
    
    return Ok("Profile will be synced globally");
}
```

## Revoking Override Consent

If user changes their mind:

```csharp
public async Task<IActionResult> DisableGlobalProfile(Guid candidateId)
{
    var candidate = await _candidateRepository.GetByIdAsync(candidateId);
    
    // Remove consent
    candidate.SyncScope = SyncScope.EUOnly;  // Restrict to EU
    candidate.SanitizationOverrideConsentAt = null;
    
    await _candidateRepository.UpdateAsync(candidate);
    
    // Note: This won't remove already-synced data from other regions
    // You'd need a separate cleanup process for that
    
    return Ok("Profile restricted to EU only");
}
```

## Data Sanitization Process

Example of sanitizing a candidate for global distribution:

```csharp
public CandidateStatistics SanitizeCandidate(Candidate candidate)
{
    return new CandidateStatistics
    {
        Id = Guid.NewGuid(),  // New ID for sanitized version
        OriginalCandidateId = candidate.Id,  // Reference (if needed)
        YearsExperience = candidate.CalculateYearsExperience(),
        SkillLevel = candidate.SkillLevel,
        LocationCountry = candidate.Address?.Country,  // Country only, not full address
        Industry = candidate.Industry,
        // REMOVED: Name, Email, Phone, Exact Address, SSN, etc.
        SyncScope = SyncScope.GlobalSanitized,
        IsSanitized = true,
        DataClassification = DataClassification.NonPersonal
    };
}
```

## Monitoring

Key metrics to track:

1. **Blocked Syncs**: Count of `GlobalSanitized` entities that failed validation
2. **Override Consents**: Count of entities syncing via `SanitizationOverrideConsentAt`
3. **Sanitized Syncs**: Count of entities syncing via `IsSanitized=true`

Example query for blocked syncs:

```sql
SELECT EntityType, EntityId, IsSanitized, SanitizationOverrideConsentAt
FROM [Log_SyncAttempts]
WHERE SyncScope = 'GlobalSanitized'
  AND TargetRegionCount = 0
  AND Reason LIKE '%validation failed%'
```

## GDPR Compliance Notes

### Why This Matters

- **Article 5 GDPR**: Data minimization - only necessary data should be shared
- **Article 6 GDPR**: Legal basis required for processing personal data
- **Article 44-50 GDPR**: Restrictions on international data transfers

### Legal Bases for Override Consent

When using `SanitizationOverrideConsentAt`, ensure you have one of these legal bases:

1. **Consent** (`LegalBasisType.Consent`): User explicitly agrees
2. **Contract** (`LegalBasisType.Contract`): Necessary for contract performance
3. **Legitimate Interest** (`LegalBasisType.LegitimateInterest`): Balancing test passed

Always document the legal basis in `LegalBasisRef`:

```csharp
candidate.LegalBasis = LegalBasisType.Consent;
candidate.LegalBasisRef = "consent-record-2024-01-15-profile-sharing";
```

## Testing

### Test 1: Sanitized Entity

```csharp
[Fact]
public async Task GlobalSanitized_WithIsSanitizedTrue_SyncsToAllRegions()
{
    var entity = new TestEntity 
    { 
        SyncScope = SyncScope.GlobalSanitized, 
        IsSanitized = true 
    };
    
    var regions = await _syncService.DetermineTargetRegionsAsync(entity);
    
    Assert.Contains("EU", regions);
    Assert.Contains("US", regions);
    Assert.Contains("IN", regions);
}
```

### Test 2: Override Consent

```csharp
[Fact]
public async Task GlobalSanitized_WithOverrideConsent_SyncsToAllRegions()
{
    var entity = new TestEntity 
    { 
        SyncScope = SyncScope.GlobalSanitized, 
        IsSanitized = false,
        SanitizationOverrideConsentAt = DateTimeOffset.UtcNow
    };
    
    var regions = await _syncService.DetermineTargetRegionsAsync(entity);
    
    Assert.Contains("EU", regions);
    Assert.Contains("US", regions);
    Assert.Contains("IN", regions);
}
```

### Test 3: Blocked Sync

```csharp
[Fact]
public async Task GlobalSanitized_WithoutSanitizationOrConsent_BlocksSync()
{
    var entity = new TestEntity 
    { 
        SyncScope = SyncScope.GlobalSanitized, 
        IsSanitized = false,
        SanitizationOverrideConsentAt = null
    };
    
    var regions = await _syncService.DetermineTargetRegionsAsync(entity);
    
    Assert.Empty(regions);  // No sync allowed
}
```

## Summary

? **GlobalSanitized is powerful but requires validation**  
? **Two ways to enable: Sanitization OR Explicit Consent**  
? **Validation failures are logged and block sync**  
? **GDPR compliance is maintained automatically**  
? **Clear audit trail via `SanitizationOverrideConsentAt` timestamp**  

This ensures that personal data never accidentally leaks across regions without proper authorization!
