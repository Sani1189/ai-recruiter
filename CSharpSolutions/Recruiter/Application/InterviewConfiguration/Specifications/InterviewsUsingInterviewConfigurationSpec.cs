using Ardalis.Specification;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewsUsingInterviewConfigurationSpec : Specification<Recruiter.Domain.Models.Interview>
{
    public InterviewsUsingInterviewConfigurationSpec(string configName, int configVersion)
    {
        Query.Where(i => i.InterviewConfigurationName == configName && i.InterviewConfigurationVersion == configVersion);
    }
}



