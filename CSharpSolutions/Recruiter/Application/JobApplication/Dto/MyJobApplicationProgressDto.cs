namespace Recruiter.Application.JobApplication.Dto;

public sealed class MyJobApplicationProgressDto
{
    public JobApplicationDto? JobApplication { get; set; }

    public List<JobApplicationStepDto> Steps { get; set; } = new();
}



