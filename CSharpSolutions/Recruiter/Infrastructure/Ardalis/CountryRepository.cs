using Recruiter.Application.Country.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class CountryRepository : EfBasicRepository<Country>, ICountryRepository
{
    public CountryRepository(RecruiterDbContext context)
        : base(context)
    {
    }
}
