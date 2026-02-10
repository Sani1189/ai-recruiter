using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

/// <summary>
/// CountryExposureSet by Id (for get-or-create lookup).
/// </summary>
public sealed class CountryExposureSetByIdSpec : Specification<CountryExposureSet>, ISingleResultSpecification<CountryExposureSet>
{
    public CountryExposureSetByIdSpec(Guid id)
    {
        Query.Where(s => s.Id == id);
    }
}
