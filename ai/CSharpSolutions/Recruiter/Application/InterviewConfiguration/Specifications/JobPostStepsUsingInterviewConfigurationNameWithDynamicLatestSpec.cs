using Ardalis.Specification;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class JobPostStepsUsingInterviewConfigurationNameWithDynamicLatestSpec : Specification<Recruiter.Domain.Models.JobPostStep>
{
    public JobPostStepsUsingInterviewConfigurationNameWithDynamicLatestSpec(string configName)
    {
        // InterviewConfigurationVersion == null means "use latest version dynamically"
        Query.Where(s => s.InterviewConfigurationName == configName && s.InterviewConfigurationVersion == null);
    }
}


