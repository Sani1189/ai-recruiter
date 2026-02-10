// File: Domain/Models/Country.cs
using System.ComponentModel.DataAnnotations;

namespace Recruiter.Domain.Models;

/// <summary>
/// Reference data for countries. Uses BasicBaseDbModel (no GDPR sync needed).
/// Use ISO-3166-1 alpha-2 codes (e.g. "NO", "IN", "US").
/// </summary>
public class Country : BasicBaseDbModel
{
    [Required, MaxLength(2)]
    public string CountryCode { get; set; } = default!;

    [Required, MaxLength(60)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(60)]
    public string DataCenterRegion { get; set; } = default!;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsEuMember { get; set; } = false;
}
