using Ardalis.Specification;

namespace Recruiter.Application.Interview.Specifications;

public sealed class InterviewByConfigurationSpec : Specification<Domain.Models.Interview>
{
    public InterviewByConfigurationSpec(string configurationName, int configurationVersion)
    {
        Query.Where(i => i.InterviewConfigurationName == configurationName && 
                        i.InterviewConfigurationVersion == configurationVersion)
             .OrderByDescending(i => i.CreatedAt);
    }
}
