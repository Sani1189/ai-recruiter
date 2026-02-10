using Recruiter.Application.Common.Interfaces;
using CountryEntity = Recruiter.Domain.Models.Country;

namespace Recruiter.Application.Country.Interfaces;

public interface ICountryRepository : IRepository<CountryEntity>
{
}
