namespace Recruiter.Application.JobApplication.Dto;

public sealed class BeginJobApplicationStepResponseDto
{
    public Guid JobApplicationId { get; set; }

    public Guid JobApplicationStepId { get; set; }

    public Guid? InterviewId { get; set; }

    public string StepStatus { get; set; } = string.Empty;
}



