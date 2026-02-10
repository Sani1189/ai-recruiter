using Recruiter.Application.Country.Dto;

namespace Recruiter.Application.Country.Interfaces;

public interface ICountryListService
{
    /// <summary>
    /// Returns active countries for dropdowns (e.g. origin country, country exposure).
    /// </summary>
    Task<IReadOnlyList<CountryListItemDto>> GetActiveForDropdownAsync(CancellationToken cancellationToken = default);
}
