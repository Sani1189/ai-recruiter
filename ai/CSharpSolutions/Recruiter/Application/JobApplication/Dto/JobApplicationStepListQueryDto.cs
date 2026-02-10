using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobApplication.Dto;

public class JobApplicationStepListQueryDto
{
    public string? SearchTerm { get; set; } // Searches in JobApplicationId, JobPostStepName, Status
    public Guid? JobApplicationId { get; set; }
    public string? JobPostStepName { get; set; }
    public int? JobPostStepVersion { get; set; }
    public string? Status { get; set; }
    public int? StepNumber { get; set; }
    public DateTime? StartedAfter { get; set; }
    public DateTime? StartedBefore { get; set; }
    public DateTime? CompletedAfter { get; set; }
    public DateTime? CompletedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
