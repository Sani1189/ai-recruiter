using Ardalis.Specification;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Specifications;

/// <summary>
/// JobPost by name and version with Status = Published only. Used for public/candidate endpoints.
/// Includes CountryExposureSet for DTO mapping.
/// </summary>
public sealed class JobPostByNameVersionAndPublishedSpec : Specification<Domain.Models.JobPost>, ISingleResultSpecification<Domain.Models.JobPost>
{
    public JobPostByNameVersionAndPublishedSpec(string name, int version)
    {
        Query.Where(jp => jp.Name == name && jp.Version == version && !jp.IsDeleted && jp.Status == JobPostStatusEnum.Published)
            .Include(jp => jp.CountryExposureSet!)
            .ThenInclude(s => s.Countries);
    }
}
