using Recruiter.Application.Country.Dto;
using Recruiter.Application.Country.Interfaces;
using Recruiter.Application.Country.Specifications;

namespace Recruiter.Application.Country;

public class CountryListService : ICountryListService
{
    private readonly ICountryRepository _repository;

    public CountryListService(ICountryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CountryListItemDto>> GetActiveForDropdownAsync(CancellationToken cancellationToken = default)
    {
        var spec = new ActiveCountriesSpec();
        var countries = await _repository.ListAsync(spec, cancellationToken);
        return countries
            .Select(c => new CountryListItemDto { CountryCode = c.CountryCode, Name = c.Name })
            .ToList();
    }
}
