using Ardalis.Specification;
using CountryEntity = Recruiter.Domain.Models.Country;

namespace Recruiter.Application.Country.Specifications;

public sealed class ActiveCountriesSpec : Specification<CountryEntity>
{
    public ActiveCountriesSpec()
    {
        Query.Where(c => c.IsActive)
            .OrderBy(c => c.Name);
    }
}
