namespace Recruiter.Application.JobApplication.Dto;

/// <summary>
/// DTO for job application with steps and interviews
/// </summary>
public class JobApplicationWithStepsAndInterviewsDto
{
    public JobApplicationDto JobApplication { get; set; } = null!;
    public List<JobApplicationStepWithInterviewsDto> Steps { get; set; } = new();
}
