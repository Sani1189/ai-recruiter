using Ardalis.Specification;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

/// <summary>
/// Latest version of JobPost by name with Status = Published only. Used for public/candidate endpoints.
/// </summary>
public sealed class JobPostLatestPublishedByNameSpec : Specification<Domain.Models.JobPost>, ISingleResultSpecification<Domain.Models.JobPost>
{
    public JobPostLatestPublishedByNameSpec(string name)
    {
        Query.Where(jp => jp.Name == name && !jp.IsDeleted && jp.Status == JobPostStatusEnum.Published)
            .OrderByDescending(jp => jp.Version)
            .Take(1)
            .Include(jp => jp.CountryExposureSet!)
            .ThenInclude(s => s.Countries);
    }
}
