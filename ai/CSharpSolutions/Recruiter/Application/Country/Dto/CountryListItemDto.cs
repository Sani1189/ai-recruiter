namespace Recruiter.Application.Country.Dto;

/// <summary>
/// Country item for dropdowns (e.g. job post origin, country exposure).
/// </summary>
public class CountryListItemDto
{
    public string CountryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
