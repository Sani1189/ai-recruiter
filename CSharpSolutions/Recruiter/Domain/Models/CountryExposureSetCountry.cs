// File: Domain/Models/CountryExposureSetCountry.cs
using System.ComponentModel.DataAnnotations;

namespace Recruiter.Domain.Models;

/// <summary>
/// Which countries a job ad is published/exposed to.
/// Use ISO-3166-1 alpha-2 codes (e.g. "NO", "IN", "US").
/// </summary>
public class CountryExposureSetCountry : BasicBaseDbModel
{
    [Required]
    public Guid Id { get; set; }

    [Required] 
    public Guid CountryExposureSetId { get; set; }

    [Required, MaxLength(2)]
    public string CountryCode { get; set; } = default!;

    public Country Country { get; set; } = default!;
    public CountryExposureSet CountryExposureSet { get; set; } = default!;
}