using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Dto;

public class JobApplicationStepFilesListQueryDto
{
    public string? SearchTerm { get; set; } // Searches in FileId, JobApplicationStepId
    public Guid? FileId { get; set; }
    public Guid? JobApplicationStepId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
