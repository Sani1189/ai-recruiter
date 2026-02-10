using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplicationStepFiles.Dto;

public class UploadResumeResultDto
{
    [Required]
    public Guid JobApplicationId { get; set; }

    [Required]
    public Guid JobApplicationStepId { get; set; }

    [Required]
    public Guid FileId { get; set; }

    [Required]
    public Guid JobApplicationStepFilesId { get; set; }

    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
}
