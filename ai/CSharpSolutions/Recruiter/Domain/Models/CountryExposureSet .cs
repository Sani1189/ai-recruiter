// File: Domain/Models/CountryExposureSet.cs
using System.ComponentModel.DataAnnotations;

namespace Recruiter.Domain.Models;

/// <summary>
/// Which countries a job ad is published/exposed to.
/// Use ISO-3166-1 alpha-2 codes (e.g. "NO", "IN", "US").
/// </summary>
public class CountryExposureSet : BasicBaseDbModel
{
    [Required]
    public Guid Id { get; set; } // Use DeterministicGuidHelper.CreateDeterministicGuid(canonical) to generate consistent GUIDs based on the Canonical string.

    [Required]
    public string Canonical { get; set; } = default!; // Countries abbreviations csv. For example: "IN,US,NO"

    public ICollection<CountryExposureSetCountry> Countries { get; set; } = new List<CountryExposureSetCountry>();
}
