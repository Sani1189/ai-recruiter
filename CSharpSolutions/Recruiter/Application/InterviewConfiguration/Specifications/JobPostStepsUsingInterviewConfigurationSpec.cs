using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class JobPostStepsUsingInterviewConfigurationSpec : Specification<JobPostStep>
{
    public JobPostStepsUsingInterviewConfigurationSpec(string configName, int configVersion)
    {
        Query.Where(s => s.InterviewConfigurationName == configName && s.InterviewConfigurationVersion == configVersion);
    }
}



