using Recruiter.Application.Interview.Dto;

namespace Recruiter.Application.JobApplication.Dto;

/// <summary>
/// Job application step with its interviews
/// </summary>
public class JobApplicationStepWithInterviewsDto
{
    public JobApplicationStepDto Step { get; set; } = null!;
    public List<InterviewDto> Interviews { get; set; } = new();
}
