using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

/// <summary>
/// JobPost by name and version with CountryExposureSet and Countries included (for GET that returns CountryExposureCountryCodes).
/// </summary>
public sealed class JobPostByNameAndVersionWithCountryExposuresSpec : Specification<Domain.Models.JobPost>, ISingleResultSpecification<Domain.Models.JobPost>
{
    public JobPostByNameAndVersionWithCountryExposuresSpec(string name, int version)
    {
        Query.Where(jp => jp.Name == name && jp.Version == version && !jp.IsDeleted)
            .Include(jp => jp.CountryExposureSet!)
            .ThenInclude(s => s.Countries);
    }
}
